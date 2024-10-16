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
        public ObservableCollection<TravelViewModel> Travels { get; set; } /*= new ObservableCollection<TravelViewModel>();*/
        public bool isRefreshing;
        public int _selectedImageIndex{get; set; }
        public int SelectedImageIndex { get; set; }
        private readonly INavigation _navigation;

        private TravelViewModel _selectedTravel;
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

        public ListViewModel(INavigation navigation)
        {
            _navigation = navigation;
            _databaseContext = new DatabaseContext();
            Travels = new ObservableCollection<TravelViewModel>();
            //LoadTravelEntries();
        }

        // Method to load travel entries and associated images from the database
        public async Task LoadTravelEntries()
        {
            try
            {
                var travels = await _databaseContext.GetAllAsync<TravelJournalTable>();

                if (travels != null)
                {
                    // Sorteeri reisid CreatedAt omaduse alusel kahanevas järjekorras
                    travels = travels.OrderByDescending(t => t.CreatedAt).ToList();

                    foreach (var travel in travels)
                    {
                        var images = await _databaseContext.GetFilteredAsync<ImageTable>(img => img.TravelJournalId == travel.Id);

                        var viewModel = new TravelViewModel(_navigation)
                        {
                            Id = travel.Id,
                            Title = travel.Title,
                            Description = travel.Description,
                            Location = travel.Location,
                            CreatedAt = travel.CreatedAt,
                            LastUpdatedAt = travel.LastUpdatedAt,
                            TravelStartDate = travel.TravelStartDate,
                            TravelEndDate = travel.TravelEndDate,
                            TravelImages = new ObservableCollection<ImageTable>(images),
                            
                        };

                        Travels.Add(viewModel);

                        SelectedTravel = Travels.First();  // Preselect the first travel item

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading travel entries: {ex.Message}");
            }
        }

        // Käsk detailvaatele navigeerimiseks

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
        public ICommand NavigateToDetailsCommand => new Command<TravelViewModel>(async (travel) =>
        {
            Debug.WriteLine($"Navigate to details");
            await _navigation.PushAsync(new TravelDetailPage(travel, _navigation));
        });

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}