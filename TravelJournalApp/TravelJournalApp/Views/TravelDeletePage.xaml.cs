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
                var images = await _dbContext.GetFilteredAsync<ImageTable>(img => img.TravelJournalId == _travelViewModel.Id);
                foreach (var image in images)
                {
                    await _dbContext.DeleteItemAsync(image);
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