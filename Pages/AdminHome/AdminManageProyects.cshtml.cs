using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyectamos.Data;
using Proyectamos.Models;

namespace Proyectamos.Pages.AdminHome
{
    public class AdminManageProyectsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminManageProyectsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> Categories { get; set; } = new();
        public List<Project> Projects { get; set; } = new();

        [BindProperty]
        public string CategoryName { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || !role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/UserHome/Index");
            }

            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            Projects = await _context.Projects
                .Include(p => p.Category)
                .Where(p => p.Status==1)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                TempData["Error"] = "El nombre de la categoría no puede estar vacío.";
                return RedirectToPage();
            }

            var existing = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == CategoryName.Trim());

            if (existing != null)
            {
                TempData["Error"] = "La categoría ya existe.";
                return RedirectToPage();
            }

            var newCategory = new Category
            {
                Name = CategoryName.Trim(),
                Status = 1,
                RegisterDate = DateTime.Now,
                UserID = 1 
            };

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Categoría añadida correctamente.";
            return RedirectToPage();
        }

        [BindProperty]
        public int CategoryId { get; set; }
        public async Task<IActionResult> OnPostToggleCategoryStatusAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToPage();
            }

            category.Status = category.Status = category.Status == 1 ? (byte)0 : (byte)1;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = category.Status == 1 ? "Categoría activada." : "Categoría desactivada.";
            return RedirectToPage();
        }
    }
}
