using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelJournalApp.Data;
using TravelJournalApp.Views;

namespace TravelJournalApp.Models
{
    public class TravelViewModel : INotifyPropertyChanged
    {
        // Unique identifier for each travel entry
        public Guid Id { get; set; }

        // Title of the travel journal entry
        public string Title { get; set; }

        // Description of the travel journal entry
        public string Description { get; set; }

        // Location of the travel journal entry
        public string Location { get; set; }

        // Date when the entry was created
        public DateTime CreatedAt { get; set; }

        // Date when the entry was last updated
        public DateTime LastUpdatedAt { get; set; }

        // Start date of the travel
        public DateTime TravelStartDate { get; set; }

        // End date of the travel
        public DateTime TravelEndDate { get; set; }

        // Collection of images associated with this travel entry, stored as ImageTable objects
        public ObservableCollection<ImageTable> TravelImages { get; set; } = new ObservableCollection<ImageTable>();

        // Backing field for ImageViewModels property
        private ObservableCollection<ImageViewModel> _imageViewModels;

        // Collection of ImageViewModel objects representing images, used for binding in UI
        public ObservableCollection<ImageViewModel> ImageViewModels
        {
            get => _imageViewModels;
            set
            {
                if (_imageViewModels != value)
                {
                    _imageViewModels = value;
                    OnPropertyChanged(nameof(ImageViewModels));
                }
            }
        }

        // Instance of the database context for accessing database operations
        private readonly DatabaseContext _databaseContext; // Add this line

        // Constructor that initializes the TravelViewModel with a database context and sets up commands
        public TravelViewModel(DatabaseContext databaseContext) // Modify the constructor
        {
            // Ensure that the database context is not null
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));

            // Initialize commands for deleting and restoring images
            ConfirmDeleteImagesCommand = new Command(ConfirmDeleteImages);
            RestoreDeletedImagesCommand = new Command(RestoreDeletedImages);
        }

        // Property to track the index of the currently selected image
        private int _selectedImageIndex;

        // Property to hold a reference to the selected travel entry in a list
        private TravelViewModel _selectedTravel;

        // Private field to store the hero image file path
        private string _heroImageFile; // Property to store the hero image file path

        // Command to confirm deletion of selected images
        public ICommand ConfirmDeleteImagesCommand { get; }

        // Command to restore deleted images to the collection
        public ICommand RestoreDeletedImagesCommand { get; }

        // Method to confirm deletion of images, looping through ImageViewModels collection
        private void ConfirmDeleteImages()
        {
            // Check if ImageViewModels is null and log an error if so
            if (ImageViewModels == null)
            {
                Debug.WriteLine("ImageViewModels is null in ConfirmDeleteImages method.");
                return; // Välju meetodist, kui see on null
            }

            // Loop through each image in ImageViewModels
            foreach (var image in ImageViewModels)
            {
                // Confirm deletion for non-null images only
                if (image != null)
                {
                    image.ConfirmDeleteImages();
                }
                else
                {
                    Debug.WriteLine("Found a null image in ImageViewModels.");
                }
            }
        }

        // Method to restore deleted images in ImageViewModels collection
        private void RestoreDeletedImages()
        {
            // Check if ImageViewModels is null and log an error if so
            if (ImageViewModels == null)
            {
                Debug.WriteLine("ImageViewModels is null, cannot restore deleted images.");
                return; // Välja minek, kui kollektsioon on null
            }

            // Loop through each image in ImageViewModels and call RestoreDeletedImages method
            foreach (var image in ImageViewModels)
            {
                image.RestoreDeletedImages(); // Veendu, et see meetod eksisteerib
            }
        }

        // Property to store and track the path of the hero image file
        public string HeroImageFile
        {
            get => _heroImageFile;
            set
            {
                if (_heroImageFile != value)
                {
                    _heroImageFile = value;
                    OnPropertyChanged(nameof(HeroImageFile));
                }
            }
        }

        // Field and property for indicating whether this travel entry is selected
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        // Command to navigate to a large view of the hero image
        public ICommand NavigateToBigHeroCommand => new Command(() =>
        {
            // Find the first TravelMainPage in the navigation stack
            var travelPage = (TravelMainPage)Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault(p => p is TravelMainPage);

            // Get the ListViewModel associated with this page
            var listViewModel = (ListViewModel)travelPage.BindingContext;

            // Set the selected travel in the ListViewModel and notify change
            listViewModel.SelectedTravel = this;
            listViewModel.OnPropertyChanged(nameof(listViewModel.SelectedTravel));
        });

        // Property for tracking the currently selected travel entry
        public TravelViewModel SelectedTravel
        {
            get => _selectedTravel;
            set
            {
                if (_selectedTravel != value)
                {
                    _selectedTravel = value;
                    OnPropertyChanged(nameof(SelectedTravel));
                }
            }
        }

        // Index of the selected image in the collection
        public int SelectedImageIndex
        {
            get => _selectedImageIndex;
            set
            {
                if (_selectedImageIndex != value)
                {
                    _selectedImageIndex = value;
                    OnPropertyChanged(nameof(SelectedImageIndex));
                    OnPropertyChanged(nameof(HeroImageSource));
                    OnPropertyChanged(nameof(HeroImageFile)); // Notify change for HeroImageFile
                }
            }
        }

        // Field and property for the source of the hero image
        private string _heroImageSource;

        public string HeroImageSource
        {
            get
            {
                // Return the cached hero image source if it exists
                if (!string.IsNullOrEmpty(_heroImageSource))
                {
                    return _heroImageSource;
                }

                // Return the cached hero image source if it exists
                UpdateHeroImageSourceAsync(); // Fire and forget
                return null; // Default if still null
            }

        }

        // Async method to retrieve and update the hero image source
        private async void UpdateHeroImageSourceAsync()
        {
            try
            {
                // Retrieve the hero image path from the database
                var heroImageFromDb = await _databaseContext.GetHeroImageFromDatabaseAsync(Id);

                // Validate and set the hero image path if it exists
                if (!string.IsNullOrEmpty(heroImageFromDb) && File.Exists(heroImageFromDb))
                {
                    _heroImageSource = heroImageFromDb;
                }
                // Fallback to the first image in TravelImages collection if hero image is unavailable
                else if (TravelImages != null && TravelImages.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < TravelImages.Count)
                {
                    _heroImageSource = TravelImages[0].FilePath;
                }
                else
                {
                    // Use default image if no hero image or fallback is available
                    _heroImageSource = "hero.png";
                }

                // Notify property changed for HeroImageSource
                OnPropertyChanged(nameof(HeroImageSource));
            }
            catch (Exception ex)
            {
                // Log any exception that occurs
                Debug.WriteLine($"Error updating hero image source: {ex.Message}");

                // Set to default hero image if an error occurs
                _heroImageSource = "hero.png";
                OnPropertyChanged(nameof(HeroImageSource));
            }
        }

        // Computed property to display travel dates in a formatted string
        public string TravelDates => $"{TravelStartDate:dd.MM.yy} - {TravelEndDate:dd.MM.yy}";

        // INotifyPropertyChanged implementation for notifying UI about property changes
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
