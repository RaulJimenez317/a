using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("comment")]
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Date { get; set; }
        public byte Status { get; set; }
        public int ProjectID { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }

        public ICollection<DocumentComment> Documents { get; set; }

    }
}
