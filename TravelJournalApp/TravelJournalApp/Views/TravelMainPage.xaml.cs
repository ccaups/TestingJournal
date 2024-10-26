using System.Diagnostics;
using System.Timers;
using TravelJournalApp.Models;
using Timer = System.Timers.Timer;

namespace TravelJournalApp.Views
{
    public partial class TravelMainPage : ContentPage
    {
        private Timer _scrollTimer;
        private bool _scrollingRight = true;
        private bool _isScrollingPaused = false;
        public ListViewModel Vm => BindingContext as ListViewModel;

        public TravelMainPage()
        {
            InitializeComponent();
            // Set the BindingContext to the ListViewModel
            BindingContext = new ListViewModel();
            StartScrolling();
        }

        private void StartScrolling()
        {
            _scrollTimer = new Timer(20);
            _scrollTimer.Elapsed += OnScrollTimerElapsed;

        }

        private async void OnScrollTimerElapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Dispatch(async () =>
            {
                if (_isScrollingPaused)
                    return;

                double currentScrollX = TitleScrollable.ScrollX;
                double contentWidth = ((Label)TitleScrollable.Content).Width;

                // Define how much you want to scroll on each timer tick
                double scrollIncrement = 0.8; // Smaller increments for slower scrolling
                int delayBetweenScrolls = 100; // Increase the delay for slower movement

                if (_scrollingRight)
                {
                    if (currentScrollX >= contentWidth - TitleScrollable.Width)
                    {
                        _isScrollingPaused = true;
                        await Task.Delay(1000);
                        TitleScrollable.ScrollToAsync(0, 0, false);
                        _isScrollingPaused = false;
                    }
                    else
                    {
                        TitleScrollable.ScrollToAsync(currentScrollX + scrollIncrement, 0, false);
                        await Task.Delay(delayBetweenScrolls); // Slow down the scrolling
                    }
                }
                else
                {
                    if (currentScrollX <= 0)
                    {
                        _isScrollingPaused = true;
                        await Task.Delay(1000);
                        TitleScrollable.ScrollToAsync(contentWidth, 0, false);
                        _isScrollingPaused = false;
                    }
                    else
                    {
                        await Task.Delay(1000);
                        TitleScrollable.ScrollToAsync(currentScrollX - scrollIncrement, 0, false);
                        await Task.Delay(delayBetweenScrolls); // Slow down the scrolling
                    }
                }
            });
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

            _scrollTimer.Start();
            Debug.WriteLine("TravelMainPage appeared - refreshing data.");

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _scrollTimer?.Stop();
            // Unsubscribe from the MessagingCenter to avoid memory leaks
            //MessagingCenter.Unsubscribe<TravelDeletePage>(this, "RefreshTravelEntries");
            
        }

        private async void Add_Travel_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TravelAddPage());
        }
    }
}