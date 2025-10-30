using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Proyectamos.Pages.Register
{
    public class RegisterModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public RegisterModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public UserInputModel NewUser { get; set; }

        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor, completa todos los campos correctamente.";
                return Page();
            }

            // Verificar la contraseña
            if (!IsValidPassword(NewUser.Password))
            {
                ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.";
                return Page();
            }

            NewUser.Email = NewUser.Email.ToLower();

            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();

                // Verificar si ya existe el correo
                string checkQuery = "SELECT status FROM user WHERE email=@Email LIMIT 1;";
                using (var checkCmd = new MySqlCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Email", NewUser.Email);
                    var existingStatus = checkCmd.ExecuteScalar();

                    if (existingStatus != null)
                    {
                        int status = Convert.ToInt32(existingStatus);
                        if (status == 1)
                        {
                            ErrorMessage = "Este correo ya está registrado.";
                            return Page();
                        }
                        else
                        {
                            // status 0 (borrado logico), reactivar usuario
                            string updateQuery = @"UPDATE user 
                                                   SET name=@Name, lastname=@LastName, password=@Password, status=1, lastUpdate=NOW()
                                                   WHERE email=@Email;";
                            using (var updateCmd = new MySqlCommand(updateQuery, connection))
                            {
                                updateCmd.Parameters.AddWithValue("@Name", NewUser.Name);
                                updateCmd.Parameters.AddWithValue("@LastName", NewUser.LastName);
                                updateCmd.Parameters.AddWithValue("@Email", NewUser.Email);
                                updateCmd.Parameters.AddWithValue("@Password", HashPassword(NewUser.Password));
                                updateCmd.ExecuteNonQuery();
                            }

                            Message = "Tu cuenta ha sido reactivada correctamente ??";
                            return Page();
                        }
                    }
                }

                string insertQuery = @"INSERT INTO user 
                      (name, lastname, email, password, role, status, registerDate, lastUpdate, userID)
                      VALUES 
                      (@Name, @LastName, @Email, @Password, 'user', 1, NOW(), NULL, 1);
                      SELECT LAST_INSERT_ID();";

                int newUserId;

                using (var cmd = new MySqlCommand(insertQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", NewUser.Name);
                    cmd.Parameters.AddWithValue("@LastName", NewUser.LastName);
                    cmd.Parameters.AddWithValue("@Email", NewUser.Email);
                    cmd.Parameters.AddWithValue("@Password", HashPassword(NewUser.Password));

                    newUserId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                HttpContext.Session.SetInt32("UserId", newUserId);
                HttpContext.Session.SetString("UserName", NewUser.Name);
                HttpContext.Session.SetString("UserRole", "user");

                return RedirectToPage("/UserHome/Index");
            }
        }

        //validaciones
        private bool IsValidPassword(string password)
        {
            if (password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => "!@#$%^&*()_+-=[]{}|;:'\",.<>?/`~".Contains(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        //contraseña
        private byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }

    public class UserInputModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; }
    }
}
