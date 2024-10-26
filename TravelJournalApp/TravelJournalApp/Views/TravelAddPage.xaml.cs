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
        private List<string> selectedTempImagePaths = new List<string>(); // Temporary images path list
        private List<string> selectedImagePaths = new List<string>(); // To store selected image paths
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }



        // Change the ObservableCollection to hold ImageOption objects
        public ObservableCollection<ImageViewModel> ImageOptions{ get; set; } = new ObservableCollection<ImageViewModel>();
        private string _heroImageFile; // Property to store the hero image file path

        public TravelAddPage()
        {
            InitializeComponent();
            TravelStartDate = DateTime.Now; // Seadista TravelStartDate praegune kuupäev
            TravelEndDate = DateTime.Now;
            BindingContext = this;
            _databaseContext = new DatabaseContext();
            travelJournal = new TravelJournalTable();
        }

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
                    ImageOptions.Clear(); // Clear previous selections
                    selectedImagePaths.Clear(); // Clear previous image paths
                    selectedTempImagePaths.Clear(); // Clear previous temp selections

                    foreach (var image in pickResult)
                    {
                        // Store the temporary image path
                        selectedTempImagePaths.Add(image.FullPath);

                        // Create an ImageViewModel for each image
                        var imageViewModel = new ImageViewModel(ImageOptions, _databaseContext) // Loo ImageViewModel objekt
                        {
                            FilePath = image.FullPath,
                            ImageSource = ImageSource.FromFile(image.FullPath),
                            IsHeroImage = false // Default to not selected as hero image
                        };
                        ImageOptions.Add(imageViewModel);
                    }

                    // Update the UI with the new ImageOptions
                    ImagesCollectionView.ItemsSource = ImageOptions;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error picking images: {ex.Message}";
            }
        }

        private async void SaveTravelClicked(object sender, EventArgs e)
        {
            var title = TitleEntry.Text?.Trim();
            var description = DescriptionEditor.Text?.Trim();
            var location = LocationEntry.Text?.Trim();
            var startDate = DateStartEntry.Date;
            var endDate = DateEndEntry.Date;

            // Validate title
            if (string.IsNullOrWhiteSpace(title))
            {
                StatusLabel.Text = "Title is required.";
                return;
            }

            // Set the hero image file path if selected
            foreach (var imageViewModel in ImageOptions)
            {
                if (imageViewModel.IsHeroImage)
                {
                    // Construct the correct path within the AppData directory
                    string appDataPath = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(imageViewModel.FilePath));
                    _heroImageFile = appDataPath; // Save the CORRECT hero image path
                    break;
                }
            }

            // Check if temporary images were selected and copy them to the app's local directory
            if (selectedTempImagePaths.Count > 0)
            {
                foreach (var tempImagePath in selectedTempImagePaths)
                {
                    var newFilePath = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(tempImagePath));

                    try
                    {
                        // Copy the image from temp location to app folder
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

            // Create and populate the TravelJournal entry
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

            // Save travel journal entry to the database
            bool journalSaved = await _databaseContext.AddItemAsync(travelJournal);
            bool imagesSaved = true;

            // Save each image associated with the journal
            for (int indexImages = 0; indexImages < selectedImagePaths.Count; indexImages++)
            {
                var newFilePath = selectedImagePaths[indexImages]; // Already copied to AppDataDirectory

                var travelJournalImage = new ImageTable
                {
                    Id = Guid.NewGuid(),
                    TravelJournalId = travelJournal.Id,
                    FilePath = newFilePath, // Use the saved path in the local folder
                    ImageIndex = indexImages
                };

                imagesSaved &= await _databaseContext.AddItemAsync(travelJournalImage);
            }

            // Update UI based on whether the save was successful
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

        private bool _isUpdating = false; // Temporary flag to avoid recursion

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
                    // Värskenda ka teiste nuppude värvi:
                    image.OnPropertyChanged(nameof(image.ButtonBackgroundColorHero));
                }

                OnPropertyChanged(nameof(ImageOptions));
            }
        }


        private async void DescriptionEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            await MyScrollView.ScrollToAsync(DescriptionEditor, ScrollToPosition.End, true);
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

}
