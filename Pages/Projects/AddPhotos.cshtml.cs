using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace Proyectamos.Pages.Projects
{
    public class AddPhotosModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public AddPhotosModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [BindProperty]
        public int ProjectId { get; set; }

        [BindProperty]
        public List<IFormFile> Fotos { get; set; }

        public void OnGet(int id)
        {
            ProjectId = id;
        }

        public IActionResult OnPost()
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
                return RedirectToPage("/Register/LoginRegister");

            if (Fotos != null && Fotos.Count > 5)
            {
                ModelState.AddModelError("Fotos", "No se pueden subir más de 5 imágenes a la vez.");
                return Page();
            }

            if (Fotos != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                foreach (var foto in Fotos)
                {
                    var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Fotos", $"El archivo {foto.FileName} no es una imagen válida.");
                        return Page();
                    }

                }
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");


            int existingCount = 0;
            using (var conn = new MySqlConnection(connString))
            {
                conn.Open();
                using var cmd = new MySqlCommand("SELECT COUNT(*) FROM photo WHERE projectID=@projectID", conn);
                cmd.Parameters.AddWithValue("@projectID", ProjectId);
                existingCount = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if ((existingCount + (Fotos?.Count ?? 0)) > 5)
            {
                ModelState.AddModelError("Fotos", $"Solo puedes tener un máximo de 5 fotos por proyecto. Actualmente tienes {existingCount}.");
                return Page();
            }

            string folder = Path.Combine(_environment.WebRootPath, "uploads", "createProyectImages");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                foreach (var foto in Fotos)
                {
                    if (foto.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                        string filePath = Path.Combine(folder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            foto.CopyTo(stream);
                        }

                        string query = @"INSERT INTO photo (projectID, image, status, userID) 
                                         VALUES (@projectID, @image, 1, @userID)";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@projectID", ProjectId);
                            cmd.Parameters.AddWithValue("@image", fileName);
                            cmd.Parameters.AddWithValue("@userID", userIdSession);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            return RedirectToPage("/Projects/Project", new { id = ProjectId });
        }
    }
}
