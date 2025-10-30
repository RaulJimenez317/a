using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyectamos.Data;
using Proyectamos.Models;
using System.Reflection.Metadata;

namespace Proyectamos.Pages.Projects
{
    public class AddDocumentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AddDocumentsModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public int ProjectId { get; set; }

        [BindProperty]
        public List<IFormFile> FilesAttached { get; set; }

        public Project Project { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Project = await _context.Projects
                .Include(p => p.Files)
                .FirstOrDefaultAsync(p => p.Id == id && p.Status != 0);

            if (Project == null)
                return NotFound();

            ProjectId = Project.Id;
            return Page();
        }


        [BindProperty]
        public string DocumentName { get; set; }
        public async Task<IActionResult> OnPostAsync()
        {
            if (FilesAttached == null || !FilesAttached.Any())
            {
                ModelState.AddModelError("", "Debe seleccionar al menos un archivo.");
                return Page();
            }

            var project = await _context.Projects.FindAsync(ProjectId);
            if (project == null || project.Status == 0)
                return NotFound();

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            foreach (var file in FilesAttached)
            {
                var extension = Path.GetExtension(file.FileName).ToLower();
                var uniqueName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(uploadsPath, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var document = new DocumentProject
                {
                    ProjectID = project.Id,
                    Name = !string.IsNullOrWhiteSpace(DocumentName) ? DocumentName : file.FileName,
                    File = "/uploads/" + uniqueName,
                    Type = extension,
                    Status = 1
                };
                _context.Documents.Add(document);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Projects/Project", new { id = project.Id });
        }
    }
}
