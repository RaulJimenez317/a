using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyectamos.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public byte[] Password { get; set; }
        public int Status { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? Role { get; set; }

        public Profile? Profile { get; set; }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiration { get; set; }

    }
}
