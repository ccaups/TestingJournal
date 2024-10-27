using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Presentation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TravelJournalApp.Data;
using TravelJournalApp.Models;

namespace TravelJournalApp.Views
{
    public partial class TravelUpdatePage : ContentPage
    {
        private readonly DatabaseContext _databaseContext;
        private TravelViewModel _travelViewModel;
        public ObservableCollection<ImageViewModel> ImageViewModels { get; set; } = new ObservableCollection<ImageViewModel>();
        private List<string> selectedTempImagePaths = new List<string>();
        private string _heroImageFile; // Property to store the hero image file path

        // Constructor accepting a TravelViewModel parameter
        public TravelUpdatePage(TravelViewModel travel)
        {
            InitializeComponent();
            BindingContext = travel;
            _databaseContext = new DatabaseContext();
            _travelViewModel = travel;
            LoadImagePreviews();
        }

        // Load existing images for the travel entry
        private void LoadImagePreviews()
        {
            ImageViewModels.Clear(); // Clear the collection before adding new images

            foreach (var image in _travelViewModel.TravelImages)
            {
                if (File.Exists(image.FilePath)) // Check if the file exists
                {
                    var heroo = _travelViewModel.HeroImageFile;
                    var imageViewModel = new ImageViewModel(ImageViewModels, _databaseContext)
                    {
                        FilePath = image.FilePath,
                        ImageSource = ImageSource.FromFile(image.FilePath),
                        IsSelected = image.IsSelected,
                        IsHeroImage = image.FilePath == _travelViewModel.HeroImageFile // Check if the image is the hero image
                    };
                    ImageViewModels.Add(imageViewModel);
                }
                else
                {
                    Debug.WriteLine($"Image file not found: {image.FilePath}");
                }
            }
            ImagesCollectionView.ItemsSource = ImageViewModels; // Set collection as ItemsSource
        }

        // Event handler for picking photos
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
                    selectedTempImagePaths.Clear(); // Clear previous selection

                    foreach (var image in pickResult)
                    {
                        // Store temporary image path
                        selectedTempImagePaths.Add(image.FullPath);

                        // Show previously selected images
                        var imageViewModel = new ImageViewModel(ImageViewModels, _databaseContext)
                        {
                            FilePath = image.FullPath,
                            ImageSource = ImageSource.FromFile(image.FullPath),
                            IsSelected = true
                        };
                        ImageViewModels.Add(imageViewModel);
                    }

                    // Refresh UI
                    ImagesCollectionView.ItemsSource = ImageViewModels;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error picking images: {ex.Message}";
            }
        }

        // Event handler to update hero image selection
        private void OnButtonClickedUpdateHero(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is ImageViewModel selectedImage)
            {
                selectedImage.IsHeroImage = !selectedImage.IsHeroImage;

                foreach (var image in ImageViewModels)
                {
                    if (image != selectedImage)
                    {
                        image.IsHeroImage = false;
                    }
                    image.OnPropertyChanged(nameof(image.ButtonBackgroundColorHero));
                }

                OnPropertyChanged(nameof(ImageViewModels));
            }
        }

        // Event handler to mark image for deletion
        private void OnButtonClickedUpdateDelete(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.BindingContext is ImageViewModel selectedImage)
            {
                // Mark the image for deletion
                selectedImage.IsMarkedForDeletion = !selectedImage.IsMarkedForDeletion;
                OnPropertyChanged(nameof(ImageViewModels));
            }
        }

        private async void OnUpdateButtonClicked(object sender, EventArgs e)
        {
            ConfirmDeleteImages();

            // Find the existing travel journal
            var travelJournal = await _databaseContext.GetItemAsync(_travelViewModel.Id);
            if (travelJournal == null)
            {
                StatusLabel.Text = "Travel entry not found.";
                return;
            }

            // Get the hero image
            var heroImage = ImageViewModels.FirstOrDefault(image => image.IsHeroImage);

            // Update travel journal details
            travelJournal.Title = TitleEntry.Text;
            travelJournal.Description = DescriptionEditor.Text;
            travelJournal.Location = LocationEntry.Text;
            travelJournal.TravelStartDate = DateStartEntry.Date;
            travelJournal.TravelEndDate = DateEndEntry.Date;
            travelJournal.LastUpdatedAt = DateTime.Now;
            travelJournal.HeroImageFile = heroImage?.FilePath; // Save the hero image file path

            // Update travel journal entry
            bool result = await _databaseContext.UpdateItemAsync(travelJournal);

            // Find existing images for this travel journal
            var existingImages = await _databaseContext.GetImagesForTravelJournalAsync(travelJournal.Id);
            var existingImageFilePaths = existingImages.Select(img => img.FilePath).ToList();

            // Get the folder path from an existing image if available, otherwise create a new one
            string imageFolderPath = existingImages.FirstOrDefault()?.FilePath != null
                ? Path.GetDirectoryName(existingImages.FirstOrDefault().FilePath)
                : Path.Combine(FileSystem.AppDataDirectory, Guid.NewGuid().ToString());

            // Ensure the directory exists
            Directory.CreateDirectory(imageFolderPath);

            // Handle new images
            foreach (var tempImagePath in selectedTempImagePaths)
            {
                if (string.IsNullOrWhiteSpace(tempImagePath))
                {
                    Debug.WriteLine($"Temp image path is invalid: {tempImagePath}");
                    continue; // Skip if the path is null or empty
                }

                var newFilePath = Path.Combine(imageFolderPath, Path.GetFileName(tempImagePath));

                try
                {
                    // Only add image if it doesn't already exist in the database
                    if (!existingImageFilePaths.Contains(newFilePath))
                    {
                        File.Copy(tempImagePath, newFilePath, true);

                        var imageTable = new ImageTable
                        {
                            TravelJournalId = travelJournal.Id,
                            FilePath = newFilePath,
                            ImageIndex = existingImages.Count // Update ImageIndex for new images
                        };

                        await _databaseContext.AddItemAsync(imageTable);
                        existingImageFilePaths.Add(newFilePath); // Avoid duplicates
                    }
                }
                catch (Exception ex)
                {
                    StatusLabel.Text = $"Error copying images: {ex.Message}";
                    return;
                }
            }

            // Handle updating existing images logic...

            if (result)
            {
                // Notify the UI that the hero image source has changed
                OnPropertyChanged(nameof(_travelViewModel.HeroImageSource));

                // Get the ListViewModel from the Application.Current.MainPage
                if (Application.Current.MainPage is ContentPage mainPage &&
                    mainPage.BindingContext is ListViewModel listViewModel)
                {
                    await listViewModel.RefreshDataAsync();
                }

                await Navigation.PopToRootAsync();
            }
            else
            {
                StatusLabel.Text = "Failed to update travel.";
                StatusLabel.TextColor = Color.FromArgb("#FF6347");
            }
        }

        // Event handler for back navigation
        private async void BackTravelButton_Clicked(object sender, EventArgs e)
        {
            RestoreDeletedImages();
            await Navigation.PopAsync();
        }

        // Confirm deletion of marked images
        private async void ConfirmDeleteImages()
        {
            // If there are temporarily marked images, delete them permanently
            foreach (var imageViewModel in ImageViewModels.ToList()) // Use ToList() to avoid modifying the collection while iterating
            {
                if (imageViewModel.IsMarkedForDeletion) // Assuming you have a flag marking images for deletion
                {
                    try
                    {
                        // Delete image from the database
                        var imageRecord = await _databaseContext.GetImageByFilePathAsync(imageViewModel.FilePath);
                        if (imageRecord != null)
                        {
                            await _databaseContext.DeleteItemAsync(imageRecord);
                        }

                        // Delete image from the file system
                        if (File.Exists(imageViewModel.FilePath))
                        {
                            Debug.WriteLine($"Deleting file: {imageViewModel.FilePath}");
                            File.Delete(imageViewModel.FilePath);
                            Debug.WriteLine("File deleted successfully.");
                        }

                        // Remove image from UI
                        ImageViewModels.Remove(imageViewModel);

                        // Check if the deleted image is the hero image
                        if (imageViewModel.IsHeroImage)
                        {
                            // Set the hero image source to null or default image
                            _travelViewModel.HeroImageFile = null; // Or use "hero.png" as default
                            OnPropertyChanged(nameof(_travelViewModel.HeroImageSource)); // Notify that the hero image source has changed
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Text = $"Error deleting image {imageViewModel.FilePath}: {ex.Message}";
                        StatusLabel.TextColor = Color.FromArgb("#b01d0c");
                        StatusLabel.IsVisible = true;
                    }
                }
            }

            // Refresh UI after deletion
            ImagesCollectionView.ItemsSource = ImageViewModels;
        }

        // Restore temporarily deleted images back to the ImageViewModel collection
        private void RestoreDeletedImages()
        {
            // Check if there are temporarily removed images
            if (selectedTempImagePaths != null && selectedTempImagePaths.Count > 0)
            {
                foreach (var tempImagePath in selectedTempImagePaths)
                {
                    var imageViewModel = new ImageViewModel(ImageViewModels, _databaseContext)
                    {
                        FilePath = tempImagePath,
                        ImageSource = ImageSource.FromFile(tempImagePath),
                        IsMarkedForDeletion = false
                    };
                    ImageViewModels.Add(imageViewModel);
                }
            }
        }
    }
}
