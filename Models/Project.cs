using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace Proyectamos.Models
{
    [Table("project")]
    public class Project
    {
        [Key]
        public int Id { get; set; }
        public string? Name{ get; set; }
        public string? Description { get; set; }
        public byte Status { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }
        public List<DocumentProject> Files { get; set; }
        public List<PhotoProject>? Photos { get; set; }
        public List<Comment> Comments { get; set; }

        public int CategoryID { get; set; }
        [ForeignKey("CategoryID")]
        public Category Category { get; set; }
    }
}
