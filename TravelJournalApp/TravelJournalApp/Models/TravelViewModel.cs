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
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }
        public ObservableCollection<ImageTable> TravelImages { get; set; } = new ObservableCollection<ImageTable>();
        private ObservableCollection<ImageViewModel> _imageViewModels;
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
        private readonly DatabaseContext _databaseContext; // Add this line

        public TravelViewModel(DatabaseContext databaseContext) // Modify the constructor
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            ConfirmDeleteImagesCommand = new Command(ConfirmDeleteImages);
            RestoreDeletedImagesCommand = new Command(RestoreDeletedImages);
        }

        private int _selectedImageIndex;
        private TravelViewModel _selectedTravel;

        private string _heroImageFile; // Property to store the hero image file path

        public ICommand ConfirmDeleteImagesCommand { get; }
        public ICommand RestoreDeletedImagesCommand { get; }

        private void ConfirmDeleteImages()
        {
            // Kontrolli, kas ImageViewModels on null
            if (ImageViewModels == null)
            {
                Debug.WriteLine("ImageViewModels is null in ConfirmDeleteImages method.");
                return; // Välju meetodist, kui see on null
            }

            foreach (var image in ImageViewModels)
            {
                // Kontrolli, et image ei ole null enne meetodi kutsumist
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

        private void RestoreDeletedImages()
        {
            if (ImageViewModels == null)
            {
                Debug.WriteLine("ImageViewModels is null, cannot restore deleted images.");
                return; // Välja minek, kui kollektsioon on null
            }

            foreach (var image in ImageViewModels)
            {
                image.RestoreDeletedImages(); // Veendu, et see meetod eksisteerib
            }
        }


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



        public ICommand NavigateToBigHeroCommand => new Command(() =>
        {
            var travelPage = (TravelMainPage)Application.Current.MainPage.Navigation.NavigationStack.FirstOrDefault(p => p is TravelMainPage);
            var listViewModel = (ListViewModel)travelPage.BindingContext;
            listViewModel.SelectedTravel = this;
            listViewModel.OnPropertyChanged(nameof(listViewModel.SelectedTravel));
        });

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

        // Property for the hero image source
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

                // Try to retrieve the hero image from the database if not already set
                UpdateHeroImageSourceAsync(); // Fire and forget
                return "hero.png"; // Default if still null
            }

        }

        // Async method to update the hero image source
        private async void UpdateHeroImageSourceAsync()
        {
            // Retrieve the hero image from the database using the current travel journal Id
            var heroImageFromDb = await _databaseContext.GetHeroImageFromDatabaseAsync(Id);

            // Check if HeroImageFile has a valid path from the database
            if (!string.IsNullOrEmpty(heroImageFromDb) && File.Exists(heroImageFromDb))
            {
                _heroImageSource = heroImageFromDb; // Set the hero image file path if it exists
            }
            else if (TravelImages != null && TravelImages.Count > 0 && SelectedImageIndex >= 0 && SelectedImageIndex < TravelImages.Count)
            {
                _heroImageSource = TravelImages[0].FilePath; // Use the selected image's file path
            }
            else
            {
                _heroImageSource = "hero.png"; // Default image path
            }

            // Notify property changed
            OnPropertyChanged(nameof(HeroImageSource));
        }

        // Example method to retrieve HeroImageFile from the database


        public string TravelDates => $"{TravelStartDate:dd.MM.yy} - {TravelEndDate:dd.MM.yy}";

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
