using SQLite;

namespace TravelJournalApp.Data
{
    [Table("ImageDatabase")]
    public class ImageDatabase
    {
        public Guid Id { get; set; }
        public Guid TravelJournalId { get; set; } // Foreign key
        public string FilePath { get; set; }
        public int ImageIndex { get; set; }
    }
}
