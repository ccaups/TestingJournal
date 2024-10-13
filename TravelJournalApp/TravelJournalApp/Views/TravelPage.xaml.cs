using System.Diagnostics;
using TravelJournalApp.Models;

namespace TravelJournalApp.Views
{
    public partial class TravelPage : ContentPage
    {
        public ViewModel Vm => BindingContext as ViewModel;

        public TravelPage()
        {
            InitializeComponent();

            // ViewModel-i sidumine
            BindingContext = new ViewModel(); 
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("TravelPage appeared - refreshing data.");
            Vm?.RefreshCommand.Execute(null);
        }

        private async void Add_Travel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddTravelPage());
        }
    }
}
