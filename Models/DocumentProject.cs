using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("document")]
    public class DocumentProject
    {
        [Key]
        public int Id { get; set; }
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public Project Project { get; set; }
        public string? File { get; set; }
        public string? Type { get; set; }

        public int UserID { get; set; }
        public byte Status { get; set; }



    }
}
