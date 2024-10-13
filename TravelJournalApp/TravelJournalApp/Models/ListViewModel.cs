using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TravelJournalApp.Data;
using TravelJournalApp.Views;
using Microsoft.Maui.Controls;

namespace TravelJournalApp.Models
{
    public class ListViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseContext _databaseContext;
        public ObservableCollection<ViewModel> Travels { get; set; } = new ObservableCollection<ViewModel>();

        private ViewModel _travels;
        public bool isRefreshing;
        private int _selectedIndex;
        private string _selectedImagePath;
        public int ImageIndex { get; set; }

        public ObservableCollection<ImageDatabase> TravelImages
        {
            get
            {
                if (SelectedImageIndex >= 0 && Travels.Count > 0) // Check if Travels is not empty
                {
                    // Assuming you always want to work with the first travel entry (Travels[0])
                    var currentTravel = Travels[0];

                    if (SelectedImageIndex < currentTravel.TravelImages.Count)
                    {
                        // Return an ObservableCollection with the selected image
                        return new ObservableCollection<ImageDatabase> { currentTravel.TravelImages[SelectedImageIndex] };
                    }
                }
                return new ObservableCollection<ImageDatabase>();
            }
        }

        public ListViewModel()
        {
            _databaseContext = new DatabaseContext();
            Travels = new ObservableCollection<ViewModel>();
            LoadTravelEntries();
        }

        // Method to load travel entries and associated images from the database
        public async Task LoadTravelEntries()
        {
            try
            {
                var travels = await _databaseContext.GetAllAsync<TravelJournal>();

                if (travels != null)
                {
                    foreach (var travel in travels)
                    {
                        var images = await _databaseContext.GetFilteredAsync<ImageDatabase>(img => img.TravelJournalId == travel.Id);

                        var viewModel = new ViewModel
                        {
                            Id = travel.Id,
                            Title = travel.Title,
                            Description = travel.Description,
                            Location = travel.Location,
                            TravelDate = travel.TravelDate,
                            CreatedAt = travel.CreatedAt,
                            LastUpdatedAt = travel.LastUpdatedAt,
                            TravelImages = new ObservableCollection<ImageDatabase>(images),
                        };

                        Travels.Add(viewModel); // Add travel to collection
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading travel entries: {ex.Message}");
            }
        }

        // IsRefreshing property for pull-to-refresh functionality
        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                if (isRefreshing != value)
                {
                    isRefreshing = value;
                    OnPropertyChanged(nameof(IsRefreshing));
                }
            }
        }

        // Refresh command to reload travel entries
        public ICommand RefreshCommand => new Command(async () => await RefreshDataAsync());

        private async Task RefreshDataAsync()
        {
            IsRefreshing = true;
            Travels.Clear();
            await LoadTravelEntries();
            IsRefreshing = false;
        }

        public int SelectedImageIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(nameof(SelectedImageIndex));
                    // OnPropertyChanged(nameof(TravelImages)); // Ei pea seda vajalikuks muutma
                    OnPropertyChanged(nameof(TravelImages)); // Käivita TravelImages omaduse uuendamine
                }
            }
        }

        private async Task RefreshImages()
        {
            IsRefreshing = true;

            // Laadige pildid uuesti
            // ... Näiteks: Travels[0].TravelImages = await LoadImagesFromDatabase();

            IsRefreshing = false;
        }
        //Command to navigate to the next image
        //public ICommand NextImageCommand => new Command(async () =>
        //{
        //    if (Travels.Count > 0 && Travels[0].TravelImages.Count > 0)
        //        SelectedIndex = (SelectedIndex + 1) % Travels[0].TravelImages.Count; // Move to the next image
        //    Debug.WriteLine($"Next Button working: {SelectedIndex}");
        //    OnPropertyChanged(nameof(NextImageCommand));
        //});

        public ICommand NextImageCommand => new Command(() =>
        {
            if (SelectedImageIndex >= 0 && SelectedImageIndex < Travels.Count) // Kontrollib, kas SelectedIndex on kehtiv
            {
                var selectedTravel = Travels[0]; // Leiab õige reisi, ajutine
                if (selectedTravel.TravelImages.Count > 0) // Kontrollib, kas reisil on pilte
                {
                    if (SelectedImageIndex < selectedTravel.TravelImages.Count - 1) // Kontrollib, kas on viimane pilt
                    {
                        SelectedImageIndex++; // Liigutab järgmise pildi juurde ainult siis, kui see pole viimane
                        Debug.WriteLine($"Next Button working: {SelectedImageIndex}");
                    }
                }
            }
            OnPropertyChanged(nameof(NextImageCommand));
        });

        // Command to navigate to the previous image
        public ICommand PreviousImageCommand => new Command(async () =>
        {
            if (Travels.Count > 0 && Travels[0].TravelImages.Count > 0)
                SelectedImageIndex = (SelectedImageIndex - 1 + Travels[0].TravelImages.Count) % Travels[0].TravelImages.Count; // Move to the previous image
            Debug.WriteLine($"Back Button working: {SelectedImageIndex}");
            OnPropertyChanged(nameof(PreviousImageCommand));
        });


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
