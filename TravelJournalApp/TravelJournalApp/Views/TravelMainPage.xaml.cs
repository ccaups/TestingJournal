using System.Diagnostics;
using TravelJournalApp.Models;

namespace TravelJournalApp.Views
{
    public partial class TravelMainPage : ContentPage
    {
        public ListViewModel Vm => BindingContext as ListViewModel;

        public TravelMainPage()
        {
            InitializeComponent();

            // Set the BindingContext to the ListViewModel
            BindingContext = new ListViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Vm?.RefreshCommand.Execute(null);

            // Subscribe to the MessagingCenter to refresh entries when notified
            MessagingCenter.Subscribe<TravelDeletePage>(this, "RefreshTravelEntries", (sender) =>
            {
                Vm?.RefreshCommand.Execute(null);
            });
            Debug.WriteLine("TravelMainPage appeared - refreshing data.");

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Unsubscribe from the MessagingCenter to avoid memory leaks
            MessagingCenter.Unsubscribe<TravelDeletePage>(this, "RefreshTravelEntries");
        }

        private async void Add_Travel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TravelAddPage());
        }
    }
}
//code before DeleteView
//using DocumentFormat.OpenXml.InkML;
//using System.Diagnostics;
//using TravelJournalApp.Models;

//namespace TravelJournalApp.Views
//{
//    public partial class TravelMainPage : ContentPage
//    {
//        public ListViewModel Vm => BindingContext as ListViewModel;

//        public TravelMainPage()
//        {
//            InitializeComponent();

//            // ViewModel-i sidumine
//            BindingContext = new ListViewModel();
//        }

//        protected override void OnAppearing()
//        {
//            base.OnAppearing();
//            Console.WriteLine("TravelPage appeared - refreshing data.");
//            Vm?.RefreshCommand.Execute(null);
//        }

//        private async void Add_Travel_Clicked(object sender, EventArgs e)
//        {
//            await Navigation.PushAsync(new TravelAddPage());
//        }

//    }
//}
