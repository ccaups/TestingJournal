using SQLite;

namespace TravelJournalApp.Data
{
    //To DB Database
    [Table("TravelJournalTable")]
    public class TravelJournalTable
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public int HeroFilePath { get; set; }
        public DateTime TravelStartDate { get; set; }
        public DateTime TravelEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
