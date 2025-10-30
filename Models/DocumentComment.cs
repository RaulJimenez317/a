using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("documentcomment")]
    public class DocumentComment
    {
        [Key]
        public int Id { get; set; }
        public int CommentID { get; set; }
        public Comment Comment { get; set; }
        public string? File { get; set; }
        public string? Type { get; set; }
        public int UserID { get; set; }

    }
}
