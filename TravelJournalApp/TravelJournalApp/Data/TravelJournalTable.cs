using SQLite;
using System.ComponentModel.DataAnnotations;

namespace TravelJournalApp.Data
{
    //To DB Database
    [Table("TravelJournalTable")]
    public class TravelJournalTable
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string HeroImageFile { get; set; }
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
