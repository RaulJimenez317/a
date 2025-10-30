using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("profile")]
    public class Profile
    {
        [Key]
        public int Id { get; set; }
        public string? Image { get; set; }
        public string? AboutMe { get; set; }
        public string? Linkedin { get; set; }
        public string? Curriculum { get; set; }   
        public int UserID { get; set; }
    }
}

