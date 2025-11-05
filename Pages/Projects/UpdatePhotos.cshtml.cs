using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;

namespace Proyectamos.Pages.Projects
{
    public class UpdatePhotosModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public UpdatePhotosModel(IConfiguration configuration, IWebHostEnvironment environment)
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
            string folder = Path.Combine(_environment.WebRootPath, "uploads", "createProyectImages");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();

                // 1?? Obtener fotos actuales
                string queryGet = "SELECT id, image, userID FROM photo WHERE projectID=@projectID";
                var photosToDelete = new List<(int Id, string File)>();

                using (var cmdGet = new MySqlCommand(queryGet, connection))
                {
                    cmdGet.Parameters.AddWithValue("@projectID", ProjectId);

                    using (var reader = cmdGet.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.GetInt32("userID") == userIdSession)
                            {
                                photosToDelete.Add((reader.GetInt32("id"), reader.GetString("image")));
                            }
                        }
                    }
                }

                foreach (var p in photosToDelete)
                {
                    string queryDel = "DELETE FROM photo WHERE id=@id";
                    using var cmdDel = new MySqlCommand(queryDel, connection);
                    cmdDel.Parameters.AddWithValue("@id", p.Id);
                    cmdDel.ExecuteNonQuery();

                    string filePath = Path.Combine(folder, p.File);
                    if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                }

                if (Fotos != null && Fotos.Count > 0)
                {
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

                            string queryInsert = @"INSERT INTO photo (projectID, image, status, userID) 
                                       VALUES (@projectID, @image, 1, @userID)";
                            using var cmdInsert = new MySqlCommand(queryInsert, connection);
                            cmdInsert.Parameters.AddWithValue("@projectID", ProjectId);
                            cmdInsert.Parameters.AddWithValue("@image", fileName);
                            cmdInsert.Parameters.AddWithValue("@userID", userIdSession);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                }
            }

            return RedirectToPage("/Projects/Project", new { id = ProjectId });
        }
    }
}
