using System.Diagnostics;
using TravelJournalApp.Data;
using TravelJournalApp.Models;

namespace TravelJournalApp.Views
{
    public partial class TravelDeletePage : ContentPage
    {
        private readonly TravelViewModel _travelViewModel;
        private readonly DatabaseContext _dbContext = new DatabaseContext();

        public TravelDeletePage(TravelViewModel travel)
        {
            InitializeComponent();
            _travelViewModel = travel;
            BindingContext = _travelViewModel;
        }

        private async Task<bool> ConfirmDeleteAsync()
        {
            return await Application.Current.MainPage.DisplayAlert(
                "Delete Travel",
                "Are you sure you want to delete this travel entry?",
                "Yes",
                "No"
            );
        }

        private async Task<bool> DeleteImagesAsync()
        {
            try
            {
                // Get all images associated with the travel entry
                var images = await _dbContext.GetFilteredAsync<ImageTable>(img => img.TravelJournalId == _travelViewModel.Id);

                // Store the folder path based on the first image's file path
                string folderPath = null;

                foreach (var image in images)
                {
                    // Delete the image record from the database
                    await _dbContext.DeleteItemAsync(image);

                    // Delete the image file from the file system
                    if (File.Exists(image.FilePath))
                    {
                        File.Delete(image.FilePath);

                        // Set folderPath based on the current image file path
                        folderPath = Path.GetDirectoryName(image.FilePath);
                    }
                }

                // After deleting all images, check if the folder is empty and delete it
                if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
                {
                    var files = Directory.GetFiles(folderPath);
                    if (files.Length == 0) // Only delete if the folder is empty
                    {
                        Directory.Delete(folderPath);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting images: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete associated images.", "OK");
                return false;
            }
        }


        private async Task<bool> DeleteTravelEntryAsync()
        {
            try
            {
                var result = await _dbContext.DeleteItemByKeyAsync<TravelJournalTable>(_travelViewModel.Id);
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting travel entry: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete travel entry: {ex.Message}", "OK");
                return false;
            }
        }

        private async Task NavigateBackAsync()
        {
            // Check if we are on the TravelMainPage and pop appropriately
            var navigationStack = Application.Current.MainPage.Navigation.NavigationStack;
            if (navigationStack.Count > 1 && navigationStack[^2] is TravelMainPage)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            else
            {
                await Application.Current.MainPage.Navigation.PopToRootAsync();
            }
        }

        private async void DeleteTravelButton_Clicked(object sender, EventArgs e)
        {
            if (!await ConfirmDeleteAsync()) return;

            var travelExists = await _dbContext.GetItemByKeyAsync<TravelJournalTable>(_travelViewModel.Id) != null;
            if (!travelExists)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Travel entry not found.", "OK");
                return;
            }

            // Assuming that the images are stored in a folder associated with the travel entry
            string imageFolderPath = Path.GetDirectoryName(_travelViewModel.HeroImageFile) ?? string.Empty; // Adjust as necessary

            if (!await DeleteImagesAsync() || !await DeleteTravelEntryAsync())
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete travel entry.", "OK");
                return;
            }


            // Notify other pages to refresh
            MessagingCenter.Send(this, "RefreshTravelEntries");

            await Application.Current.MainPage.DisplayAlert("Success", "Travel entry deleted successfully!", "OK");
            await NavigateBackAsync();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}