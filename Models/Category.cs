using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("category")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public byte Status { get; set; } = 1;

        public DateTime RegisterDate { get; set; } = DateTime.Now;

        public int UserID { get; set; }

        public List<Project> Projects { get; set; }



    }
}
