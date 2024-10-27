using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using TravelJournalApp.Data;
using Microsoft.Maui.Graphics;
using TravelJournalApp.Models;

namespace TravelJournalApp.Views
{
    public partial class TravelAddPage : ContentPage
    {
        private readonly DatabaseContext _databaseContext;
        private TravelJournalTable travelJournal;
        private List<string> selectedTempImagePaths = new List<string>(); // Temporary paths for selected images
        private List<string> selectedImagePaths = new List<string>(); /// Paths for images to be saved
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }

        // ObservableCollection to hold ImageOption objects for image selection UI
        public ObservableCollection<ImageViewModel> ImageOptions{ get; set; } = new ObservableCollection<ImageViewModel>();
        private string _heroImageFile; // Path for the selected hero image file

        public TravelAddPage()
        {
            InitializeComponent();
            TravelStartDate = DateTime.Now; // Default start date to today
            TravelEndDate = DateTime.Now; // Default end date to today
            BindingContext = this;
            _databaseContext = new DatabaseContext();
            travelJournal = new TravelJournalTable();
        }

        // Event handler for selecting photos
        private async void OnPickPhotosClicked(object sender, EventArgs e)
        {
            try
            {
                var pickResult = await FilePicker.PickMultipleAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select Images"
                });

                if (pickResult != null)
                {
                    ImageOptions.Clear(); // Clear previous image selection
                    selectedImagePaths.Clear(); // Reset saved image paths
                    selectedTempImagePaths.Clear(); // Reset temporary image paths

                    foreach (var image in pickResult)
                    {
                        // Store temporary image path
                        selectedTempImagePaths.Add(image.FullPath);

                        // Create an ImageViewModel instance for each image
                        var imageViewModel = new ImageViewModel(ImageOptions, _databaseContext)
                        {
                            FilePath = image.FullPath,
                            ImageSource = ImageSource.FromFile(image.FullPath),
                            IsHeroImage = false // Default to not selected as hero image
                        };
                        ImageOptions.Add(imageViewModel); // Add to collection for display
                    }

                    // Set CollectionView's item source to updated ImageOptions collection
                    ImagesCollectionView.ItemsSource = ImageOptions;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error picking images: {ex.Message}";
            }
        }

        // Event handler to save the travel entry
        private async void SaveTravelClicked(object sender, EventArgs e)
        {
            var title = TitleEntry.Text?.Trim();
            var description = DescriptionEditor.Text?.Trim();
            var location = LocationEntry.Text?.Trim();
            var startDate = DateStartEntry.Date;
            var endDate = DateEndEntry.Date;

            // Validate title input
            if (string.IsNullOrWhiteSpace(title))
            {
                StatusLabel.Text = "Title is required.";
                return;
            }

            // Create a new folder for the images using a new GUID
            string imageFolderName = Guid.NewGuid().ToString();
            string imageFolderPath = Path.Combine(FileSystem.AppDataDirectory, imageFolderName);
            Directory.CreateDirectory(imageFolderPath); // Create the folder

            // Set hero image file path if selected
            foreach (var imageViewModel in ImageOptions)
            {
                if (imageViewModel.IsHeroImage)
                {
                    // Construct the correct path within the new image folder
                    _heroImageFile = Path.Combine(imageFolderPath, Path.GetFileName(imageViewModel.FilePath));
                    break;
                }
            }

            // Copy selected temporary images to the new folder
            if (selectedTempImagePaths.Count > 0)
            {
                foreach (var tempImagePath in selectedTempImagePaths)
                {
                    var newFilePath = Path.Combine(imageFolderPath, Path.GetFileName(tempImagePath));

                    try
                    {
                        // Copy the image from temp location to the new folder
                        File.Copy(tempImagePath, newFilePath, true);
                        selectedImagePaths.Add(newFilePath); // Add to the list of copied images
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Text = $"Error copying images: {ex.Message}";
                        return;
                    }
                }
            }

            // Create and populate TravelJournal entry
            travelJournal = new TravelJournalTable
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now,
                Location = location,
                TravelStartDate = startDate,
                TravelEndDate = endDate,
                HeroImageFile = _heroImageFile
            };

            // Save travel journal entry to database
            bool journalSaved = await _databaseContext.AddItemAsync(travelJournal);
            bool imagesSaved = true;

            // Save each image in the journal entry
            for (int indexImages = 0; indexImages < selectedImagePaths.Count; indexImages++)
            {
                var newFilePath = selectedImagePaths[indexImages]; // Use the saved path in the new folder

                var travelJournalImage = new ImageTable
                {
                    Id = Guid.NewGuid(),
                    TravelJournalId = travelJournal.Id,
                    FilePath = newFilePath, // Save the new path in the database
                    ImageIndex = indexImages
                };

                imagesSaved &= await _databaseContext.AddItemAsync(travelJournalImage);
            }

            // Update UI based on save result
            if (journalSaved && imagesSaved)
            {
                StatusLabel.Text = "Travel saved successfully!";
                StatusLabel.TextColor = Color.FromArgb("#00FF00");
                ClearInputs();
                await Task.Delay(2000);
                await Navigation.PopAsync(); // Navigate back
            }
            else
            {
                StatusLabel.Text = "Failed to save travel.";
                StatusLabel.TextColor = Color.FromArgb("#FF0000");
                await Task.Delay(2000);
            }
        }


        // Method to clear input fields after saving or resetting
        private void ClearInputs()
        {
            TitleEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            LocationEntry.Text = string.Empty;
            StatusLabel.Text = string.Empty; // Clear status label
            selectedTempImagePaths.Clear();
            selectedImagePaths.Clear();
            ImageOptions.Clear();
        }

        private bool _isUpdating = false; // Flag to prevent recursion during updates

        // Event handler to toggle hero image selection on button click
        private void OnButtonClickedHero(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is ImageViewModel selectedImage)
            {
                selectedImage.IsHeroImage = !selectedImage.IsHeroImage;

                foreach (var image in ImageOptions)
                {
                    if (image != selectedImage)
                    {
                        image.IsHeroImage = false;
                    }
                    // Update button color for other images
                    image.OnPropertyChanged(nameof(image.ButtonBackgroundColorHero));
                }

                OnPropertyChanged(nameof(ImageOptions));
            }
        }

        // Scroll to the end of DescriptionEditor upon text change
        private async void DescriptionEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            await MyScrollView.ScrollToAsync(DescriptionEditor, ScrollToPosition.End, true);
        }

        // Event handler for back navigation
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

}
