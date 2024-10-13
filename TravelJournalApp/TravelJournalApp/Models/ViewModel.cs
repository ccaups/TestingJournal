
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TravelJournalApp.Data;
using TravelJournalApp.Views;

namespace TravelJournalApp.Models
{
    public class ViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public ObservableCollection<ImageDatabase> TravelImages { get; set; } = new ObservableCollection<ImageDatabase>();
        public int SelectedImageIndex { get; set; }


        public ICommand NextImageCommand => new Command(() =>
        {
            if (TravelImages.Count > 0)
            {
                SelectedImageIndex = (SelectedImageIndex + 1) % TravelImages.Count;
                Debug.WriteLine($"Next Button working: {SelectedImageIndex}");
                OnPropertyChanged(nameof(NextImageCommand));
                OnPropertyChanged(nameof(SelectedImageIndex)); // Notify UI of the change;
            }
        });


        public ICommand PreviousImageCommand => new Command(() =>
        {
            if (TravelImages.Count > 0)
            {
                SelectedImageIndex = (SelectedImageIndex - 1 + TravelImages.Count) % TravelImages.Count;
                Debug.WriteLine($"Back Button working: {SelectedImageIndex}");
                OnPropertyChanged(nameof(PreviousImageCommand));
                OnPropertyChanged(nameof(SelectedImageIndex)); // Notify UI of the change;
            }
        })
        {

        };

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}