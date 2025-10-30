using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("notification")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User User { get; set; }
        public int FromUserID { get; set; }

        [ForeignKey("FromUserID")]
        public User FromUser { get; set; }
        public int ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public Project Project { get; set; }
        public bool IsRead { get; set; } = false;
        public string? Message { get; set; }

    }
}
