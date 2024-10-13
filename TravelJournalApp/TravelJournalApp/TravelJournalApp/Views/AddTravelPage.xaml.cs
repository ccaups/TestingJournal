using System.Collections.ObjectModel;
using TravelJournalApp.Data;

namespace TravelJournalApp.Views
{
    public partial class AddTravelPage : ContentPage
    {
        private readonly DatabaseContext _databaseContext;
        private TravelJournal travelJournal;
        private ImageDatabase travelJournalImage;
        private List<string> selectedTempImagePaths = new List<string>(); // Initialize the temporary images path list
        private List<string> selectedImagePaths = new List<string>(); // To store selected image paths
        private ObservableCollection<ImageSource> imagePreviews = new ObservableCollection<ImageSource>();

        public AddTravelPage()
        {
            InitializeComponent();
            BindingContext = this;
            _databaseContext = new DatabaseContext();
            travelJournal = new TravelJournal();
            travelJournalImage = new ImageDatabase();
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
                    imagePreviews.Clear();
                    selectedImagePaths.Clear(); // Clear previous selections
                    selectedTempImagePaths.Clear(); // Clear previous temp selections

                    foreach (var image in pickResult)
                    {
                        // Store the temporary image path
                        selectedTempImagePaths.Add(image.FullPath);

                        // Display previews of the selected images (using CollectionView or ListView bound to `imagePreviews`)
                        imagePreviews.Add(ImageSource.FromFile(image.FullPath));
                    }
                    // Update UI (BindingContext or ItemSource in your UI for previews)
                    ImagesCollectionView.ItemsSource = imagePreviews;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error picking images: {ex.Message}";
            }
        }

        private async void SaveTravelClicked(object sender, EventArgs e)
        {
            int indexImages = 0;
            var title = TitleEntry.Text;
            var description = DescriptionEditor.Text;
            var location = LocationEntry.Text;

            if (string.IsNullOrWhiteSpace(title))
            {
                StatusLabel.Text = "Title is required.";
                return;
            }

            // Check if temporary image paths are available
            if (selectedTempImagePaths.Any())
            {
                foreach (var tempImagePath in selectedTempImagePaths)
                {
                    var newFilePath = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(tempImagePath));

                    try
                    {
                        // Copy the image to the target directory
                        File.Copy(tempImagePath, newFilePath, true);
                        selectedImagePaths.Add(newFilePath);
                    }
                    catch (Exception ex)
                    {
                        StatusLabel.Text = $"Error copying images: {ex.Message}";
                        return;
                    }
                }

                // Assign the first copied image to the travel journal image
                travelJournalImage.FilePath = selectedImagePaths.FirstOrDefault();
            }

            travelJournal = new TravelJournal
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = description,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now,
                Location = location,
            };

            bool result = await _databaseContext.AddItemAsync(travelJournal);
            bool result2 = true;

            foreach (var tempImagePath in selectedTempImagePaths)
            {
                // Move this inside the loop:
                var newFilePath = Path.Combine(FileSystem.AppDataDirectory, Path.GetFileName(tempImagePath));

                var travelJournalImage = new ImageDatabase
                {
                    Id = Guid.NewGuid(),
                    TravelJournalId = travelJournal.Id, // No need for .Value, travelJournal.Id is not nullable
                    FilePath = newFilePath,// Make sure to use the newFilePath after copying
                    ImageIndex = indexImages++
                };

                result2 = result2 && await _databaseContext.AddItemAsync(travelJournalImage);
            }

            if (result && result2)
            {
                StatusLabel.Text = "Travel saved successfully!";
                StatusLabel.TextColor = Color.FromArgb("#00FF00");
                TitleEntry.Text = string.Empty;
                DescriptionEditor.Text = string.Empty;
                LocationEntry.Text = string.Empty;
                selectedTempImagePaths.Clear();
                selectedImagePaths.Clear();
                imagePreviews.Clear(); // Clear the previews

                activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;

                await Task.Delay(2000);

                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;

                await Navigation.PopAsync();
            }
            else
            {
                StatusLabel.Text = "Failed to save travel.";
                StatusLabel.TextColor = Color.FromArgb("#FF0000");

                activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;

                await Task.Delay(2000);

                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
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
