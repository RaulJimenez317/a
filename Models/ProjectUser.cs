using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("projectuser")]
    public class ProjectUser
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int status { get; set; }
        public int UserID { get; set; }
        public int ProjectID { get; set; }
        public User User { get; set; }
        public Project Project { get; set; }
    }
}
