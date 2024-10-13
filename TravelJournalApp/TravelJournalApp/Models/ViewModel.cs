
using System.Collections.ObjectModel;
using TravelJournalApp.Data;
using TravelJournalApp.Views;

namespace TravelJournalApp.Models
{
    public class ViewModel
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

        public string PreviousImageCommand { get; set; }
        public string NextImageCommand {get; set; }
    }
}