using DocumentFormat.OpenXml.Drawing.Charts;
using DeviceDisplay = Microsoft.Maui.Devices.DeviceDisplay; // Add this for DeviceDisplay

namespace TravelJournalApp.Views;

public partial class StartPage : ContentPage
{
    

    public StartPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (this.AnimationIsRunning("TransitionAnimation"))
        {
            return;
        }

        var parentAnimation = new Animation();

        //// Get screen width and height dynamically
        //double screenWidth = DeviceDisplay.MainDisplayInfo.Width;
        //double screenHeight = DeviceDisplay.MainDisplayInfo.Height;

        double screenWidth = 540;
        double screenHeight = 1000;

        // Airplane image width, assuming it's already sized
        double imgWidth = imgAirplane.WidthRequest;
        double imgHeight = imgAirplane.HeightRequest;

        parentAnimation.Add(0, 0.4, new Animation(v => imgIcon.Opacity = v, 0, 1, Easing.CubicIn));
        parentAnimation.Add(0.1, 1, new Animation(v =>
        {
            // Moving to left
            imgAirplane.TranslationX = screenWidth - v * 1.7;

            // Moving to up
            imgAirplane.TranslationY = screenHeight + imgHeight - v * 3.4;
        }, 0, screenWidth + imgWidth, Easing.Linear));

        //Train moving
        parentAnimation.Add(0, 1, new Animation(v =>
        {
            // Moving to right
            imgTrain.TranslationX = 4 * v - 1000;
        }, 0, screenWidth, Easing.Linear));

        parentAnimation.Commit(this, "TransitionAnimation", 10, 4000, null, null);

        await TravelMain();
    }

    async Task TravelMain()
    {
        // Wait 5 seconds
        await Task.Delay(5000);

        // Redirecting page
        Application.Current.MainPage = new NavigationPage(new TravelPage());
    }
}