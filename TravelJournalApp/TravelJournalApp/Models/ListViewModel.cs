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
        private int _selectedImageIndex;
        private string _selectedImagePath;

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
            get => _selectedImageIndex;
            set
            {
                if (_selectedImageIndex != value)
                {
                    _selectedImageIndex = value;
                    OnPropertyChanged(nameof(SelectedImageIndex));
                    OnPropertyChanged(nameof(TravelImages)); // Käivita TravelImages omaduse uuendamine
                }
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
