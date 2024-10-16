using SQLite;

namespace TravelJournalApp.Data
{
    [Table("ImageTable")]
    public class ImageTable
    {
        public Guid Id { get; set; }
        public Guid TravelJournalId { get; set; } // Foreign key
        public string FilePath { get; set; }
        public int ImageIndex { get; set; }
        public bool IsSelected { get; set; }
    }
}
