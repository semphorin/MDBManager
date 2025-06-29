using System.ComponentModel.DataAnnotations;

namespace MDBManager.Models
{
    public class MusicMetadata
    {
        [Key]
        public string FilePath { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
