using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.ComponentModel.DataAnnotations;

namespace Proyectamos.Pages.Register
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ForgotPasswordModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        public string? Message { get; set; }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();

                // Buscar usuario por email
                string query = "SELECT id FROM user WHERE email=@Email LIMIT 1;";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);
                    var userId = cmd.ExecuteScalar();

                    if (userId == null)
                    {
                        Message = "No se encontró ninguna cuenta con ese correo.";
                        return;
                    }
                }

                // Generar token y guardar en BD
                string token = Guid.NewGuid().ToString();
                DateTime expiration = DateTime.Now.AddMinutes(30);

                string updateQuery = @"UPDATE user 
                                       SET resetToken=@Token, resetTokenExpiration=@Exp 
                                       WHERE email=@Email;";
                using (var updateCmd = new MySqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@Token", token);
                    updateCmd.Parameters.AddWithValue("@Exp", expiration);
                    updateCmd.Parameters.AddWithValue("@Email", Email);
                    updateCmd.ExecuteNonQuery();
                }

                // Mostrar enlace simulado
                string resetLink = $"{Request.Scheme}://{Request.Host}/Register/ResetPassword?token={token}";

                Message = $"Enlace de recuperación (simulado): <a href='{resetLink}'>{resetLink}</a>";
            }
        }
    }
}
