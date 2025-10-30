using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("photo")]
    public class PhotoProject
    {
        [Key]
        public int Id { get; set; }

        public int ProjectID { get; set; }
        public string? Image { get; set; }
    }
}
