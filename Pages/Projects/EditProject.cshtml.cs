using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyectamos.Data;
using Proyectamos.Models;

namespace Proyectamos.Pages.Projects
{
    public class EditProjectModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditProjectModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Project Project { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Project = await _context.Projects.FindAsync(id);
            if (Project == null || Project.Status==0)
                return NotFound();


            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            if (Project.UserID != userId && userRole != "admin")
            {
                TempData["Error"] = "No tienes permiso para editar este proyecto.";
                return RedirectToPage("/LookForProject/");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostEditProjectAsync(int id, string name, string description)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId"); 

            project.Name = name;
            project.Description = description;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Projects/Project", new { id = project.Id });
        }
    }
}
