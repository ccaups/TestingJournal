using TravelJournalApp.Data;
using TravelJournalApp.Models;

namespace TravelJournalApp.Views;

public partial class TravelDetailPage : ContentPage
{
    private readonly INavigation _navigation;
    public TravelDetailPage(TravelViewModel travelViewModel, INavigation navigation)
    {
        InitializeComponent();
        BindingContext = travelViewModel;
        _navigation = navigation;
    }




















































    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}