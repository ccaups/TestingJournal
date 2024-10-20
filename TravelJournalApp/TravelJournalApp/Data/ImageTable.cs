using SQLite;

namespace TravelJournalApp.Data
{
    [Table("ImageTable")]
    public class ImageTable
    {
        [PrimaryKey]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Indexed]
        public Guid TravelJournalId { get; set; } // Foreign key
        public string FilePath { get; set; }
        public int ImageIndex { get; set; }
        public bool IsSelected { get; set; }
    }
}
