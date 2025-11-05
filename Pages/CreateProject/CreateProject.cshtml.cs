using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using System.ComponentModel.DataAnnotations;

namespace Proyectamos.Pages.CreateProject
{
    public class CreateProjectModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        public List<CategoryItem> Categorias { get; set; } = new();



        public CreateProjectModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [BindProperty]
        public ProyectoInputModel Proyecto { get; set; }

        public void OnGet()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                string query = "SELECT id, name FROM category WHERE status=1 ORDER BY name";
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Categorias.Add(new CategoryItem
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name")
                        });
                    }
                }
            }
        }

        public IActionResult OnPost()
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
            {
                return RedirectToPage("/Register/LoginRegister");
            }
            CargarCategorias();

            ModelState.Remove("Proyecto.Fotos");
            // Validaciones servidor
            if (!ModelState.IsValid)
                return Page();

            if (Proyecto.Fotos != null && Proyecto.Fotos.Count > 5)
            {
                ModelState.AddModelError("Proyecto.Fotos", "No se pueden subir más de 5 imágenes.");
                return Page();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (Proyecto.Fotos != null)
            {
                foreach (var foto in Proyecto.Fotos)
                {
                    var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Proyecto.Fotos", $"El archivo {foto.FileName} no es una imagen válida.");
                        return Page();
                    }

                }
            }

            string connString = _configuration.GetConnectionString("DefaultConnection");
            int projectId;

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();

                // Insertar proyecto
                string queryProject = @"INSERT INTO project (name, description, status, userID, categoryID) 
                        VALUES (@name, @description, 1, @userID, @categoryID);
                        SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(queryProject, connection))
                {
                    cmd.Parameters.AddWithValue("@name", Proyecto.Nombre);
                    cmd.Parameters.AddWithValue("@description", Proyecto.Descripcion);
                    cmd.Parameters.AddWithValue("@userID", userIdSession);
                    cmd.Parameters.AddWithValue("@categoryID", Proyecto.CategoriaID);
                    projectId = Convert.ToInt32(cmd.ExecuteScalar());
                }


                // Guardar fotos
                if (Proyecto.Fotos != null && Proyecto.Fotos.Count > 0)
                {
                    string folder = Path.Combine(_environment.WebRootPath, "uploads", "createProyectImages");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    foreach (var foto in Proyecto.Fotos)
                    {
                        if (foto.Length > 0)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                            string filePath = Path.Combine(folder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                foto.CopyTo(stream);
                            }

                            // Insertar en tabla photo
                            string queryPhoto = @"INSERT INTO photo (projectID, image, status, userID) 
                                                  VALUES (@projectID, @image, 1, @userID)";
                            using (var cmdPhoto = new MySqlCommand(queryPhoto, connection))
                            {
                                cmdPhoto.Parameters.AddWithValue("@projectID", projectId);
                                cmdPhoto.Parameters.AddWithValue("@image", fileName);
                                cmdPhoto.Parameters.AddWithValue("@userID", userIdSession);
                                cmdPhoto.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }

            return RedirectToPage("/Projects/Project", new { id = projectId });
        }

        private void CargarCategorias()
        {
            Categorias.Clear();
            string connString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                string query = "SELECT id, name FROM category ORDER BY name";
                using (var cmd = new MySqlCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Categorias.Add(new CategoryItem
                        {
                            Id = reader.GetInt32("id"),
                            Name = reader.GetString("name")
                        });
                    }
                }
            }
        }
    }

    public class ProyectoInputModel
    {
        [Required(ErrorMessage = "El nombre del proyecto es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría")]
        [Display(Name = "Categoría")]
        public int CategoriaID { get; set; }  

        [Display(Name = "Fotos del proyecto")]
        public List<IFormFile> Fotos { get; set; }
    }

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


    //

}
