using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace Proyectamos.Pages.Register
{
    public class LoginRegisterModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginRegisterModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public LoginInputModel LoginData { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
                return Page();

            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                string query = "SELECT id, name, lastname, email, role, password, status FROM user WHERE email=@Email LIMIT 1;";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Email", LoginData.Email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int status = Convert.ToInt32(reader["status"]);
                            if (status == 0)
                            {
                                ErrorMessage = "Tu cuenta ha sido desactivada. No puedes iniciar sesión con este correo.";
                                return Page();
                            }

                            byte[] storedPassword = (byte[])reader["password"];
                            byte[] inputPassword = HashPassword(LoginData.Password);

                            if (storedPassword.SequenceEqual(inputPassword))
                            {
                                HttpContext.Session.SetInt32("UserId", Convert.ToInt32(reader["id"]));
                                HttpContext.Session.SetString("UserName", reader["name"].ToString());
                                HttpContext.Session.SetString("UserRole", reader["role"].ToString());

                                string role = reader["role"].ToString();
                                if (role == "admin")
                                    return RedirectToPage("/AdminHome/Index");
                                else
                                    return RedirectToPage("/UserHome/Index");
                            }
                        }
                    }
                }
            }

            ErrorMessage = "Correo o contraseña incorrectos.";
            return Page();
        }

        private byte[] HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }

    public class LoginInputModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo electrónico inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; }
    }
}
