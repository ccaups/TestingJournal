using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Timers;
using TravelJournalApp.Data;
using TravelJournalApp.Models;
using Timer = System.Timers.Timer;

namespace TravelJournalApp.Views;

public partial class TravelDetailPage : ContentPage
{
	private Timer _scrollTimer; // Timer used for automatic scrolling of the title
	private bool _scrollingRight = true; // Direction of scrolling
	private bool _isScrollingPaused = false; // Indicates if scrolling is paused
	private List<string> _imageSources; // List of image file paths for the travel
	private int _currentImageIndex = 0; // Index of the currently displayed image

	public TravelDetailPage(TravelViewModel travelViewModel)
	{
		InitializeComponent();
		BindingContext = travelViewModel;
		StartScrolling(); // Start the title auto-scrolling
	}

	// Method to start the auto-scrolling timer
	private void StartScrolling()
	{
		_scrollTimer = new Timer(20); // Set the timer interval to 20ms
		_scrollTimer.Elapsed += OnScrollTimerElapsed; // Attach the event handler for timer ticks
		_scrollTimer.Start(); // Start the timer
	}

	// Event handler called on each timer tick for scrolling logic
	private async void OnScrollTimerElapsed(object sender, ElapsedEventArgs e)
	{
		// Use the dispatcher to safely update UI from the timer event
		this.Dispatcher.Dispatch(async () =>
		{
			// Exit early if scrolling is paused
			if (_isScrollingPaused)
				return;

			//Get the current horizontal scroll position and content width
			double currentScrollX = TitleScrollable.ScrollX;
			double contentWidth = ((Label)TitleScrollable.Content).Width;

			double scrollIncrement = 0.8; // How much to scroll each tick
			int delayBetweenScrolls = 100; // Delay between scroll increments

			if (_scrollingRight)
			{
				// If reached the end, pause scrolling and reset to the start after a delay
				if (currentScrollX >= contentWidth - TitleScrollable.Width)
				{
					_isScrollingPaused = true;
					await Task.Delay(1000); // Wait for 1 second
					TitleScrollable.ScrollToAsync(0, 0, false); // Scroll back to the start
					_isScrollingPaused = false;
				}
				else
				{
					// Scroll to the right and wait for the next increment
					TitleScrollable.ScrollToAsync(currentScrollX + scrollIncrement, 0, false);
					await Task.Delay(delayBetweenScrolls);
				}
			}
			else
			{
				// If reached the beginning, pause and scroll to the end after a delay
				if (currentScrollX <= 0)
				{
					_isScrollingPaused = true;
					await Task.Delay(1000); // Wait for 1 second
					TitleScrollable.ScrollToAsync(contentWidth, 0, false); // Scroll to the end
					_isScrollingPaused = false;
				}
				else
				{
					await Task.Delay(1000); // Wait for 1 second
					TitleScrollable.ScrollToAsync(currentScrollX - scrollIncrement, 0, false); // Scroll to the left
					await Task.Delay(delayBetweenScrolls);
				}
			}
		});
	}

	// Stop the timer when the page is disappearing to avoid running the timer when not needed
	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_scrollTimer?.Stop(); // Stop the timer if it was started
	}

	// Event handler for tapping the hero image to display it in the zoom overlay
	private void OnHeroImageTapped(object sender, EventArgs e)
	{
		var tappedImage = sender as Image;
		var tapGesture = tappedImage?.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
		var imageSource = tapGesture?.CommandParameter?.ToString(); // Get the image source from the command parameter

		if (imageSource != null)
		{
			ZoomedHeroImage.Source = imageSource; // Display the hero image in the zoom overlay
			ZoomHeroImageOverlay.IsVisible = true; // Show the zoom overlay
		}
	}

	// Event handler for tapping on an image in the gallery
	private void OnImageTapped(object sender, EventArgs e)
	{
		// Get the tapped image control and the associated tap gesture
		var tappedImage = sender as Image;
		var tapGesture = tappedImage.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();
		var imageSource = tapGesture?.CommandParameter?.ToString(); // Get the image source from the command parameter

		if (imageSource != null)
		{
			// Retrieve all image sources from the TravelViewModel
			_imageSources = (BindingContext as TravelViewModel)?.TravelImages.Select(img => img.FilePath).ToList();
			_currentImageIndex = _imageSources.IndexOf(imageSource); // Set the current image index to the tapped image

			ZoomedImage.Source = imageSource; // Display the tapped image in the zoom overlay

			UpdateImageIndexLabel(); // Update the label showing the current image index

			ZoomOverlay.IsVisible = true; // Show the zoom overlay
		}
	}

	// Update the label that shows the current image index
	private void UpdateImageIndexLabel()
	{
		ImageIndexLabel.Text = $"{_currentImageIndex + 1} / {_imageSources.Count}";
	}

	// Event handler for tapping on the zoom overlay to close it
	private void OnOverlayTapped(object sender, EventArgs e)
	{
		ZoomOverlay.IsVisible = false; // Hide the zoom overlay
	}

	// Event handler for tapping on the zoom overlay to close it
	private void OnHeroImageOverlayTapped(object sender, EventArgs e)
	{
		ZoomHeroImageOverlay.IsVisible = false; // Hide the zoom overlay
	}

	// Event handler for clicking the "Previous Image" button
	private void OnPreviousImageClicked(object sender, EventArgs e)
	{
		// Show the previous image if it exists
		if (_imageSources != null && _currentImageIndex > 0)
		{
			_currentImageIndex--; // Decrease the image index
			ZoomedImage.Source = _imageSources[_currentImageIndex]; // Display the previous image
			UpdateImageIndexLabel(); // Update the image index label
		}
	}

	// Event handler for clicking the "Next Image" button
	private void OnNextImageClicked(object sender, EventArgs e)
	{
		// Show the next image if it exists
		if (_imageSources != null && _currentImageIndex < _imageSources.Count - 1)
		{
			_currentImageIndex++; // Increase the image index
			ZoomedImage.Source = _imageSources[_currentImageIndex]; // Display the next image
			UpdateImageIndexLabel(); // Update the image index label
		}
	}

	// Event handler for clicking the back button to navigate to the previous page
	private async void OnBackButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync(); // Navigate back to the previous page
	}

	// Event handler for clicking the edit button to navigate to the travel update page
	private async void OnEditButtonClicked(object sender, EventArgs e)
	{
		var travel = (TravelViewModel)BindingContext; // Get the current travel entry from the binding context
		await Navigation.PushAsync(new TravelUpdatePage(travel)); // Navigate to the edit page
	}

	// Event handler for clicking the delete button to navigate to the travel delete confirmation page
	private async void OnDeleteButtonClicked(object sender, EventArgs e)
	{
		var travel = (TravelViewModel)BindingContext; // Get the current travel entry from the binding context
		await Navigation.PushAsync(new TravelDeletePage(travel)); // Navigate to the delete confirmation page
	}
}
