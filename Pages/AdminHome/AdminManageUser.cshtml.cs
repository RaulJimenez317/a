using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyectamos.Data;
using Proyectamos.Models;

namespace Proyectamos.Pages.Admin
{
    public class AdminManageUserModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public AdminManageUserModel(ApplicationDbContext context) => _context = context;

        public List<User> Users { get; set; } = new();

        public User SelectedUser { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        public Proyectamos.Models.Profile SelectedProfile { get; set; } = new Proyectamos.Models.Profile();

        public List<Project> CreatedProjects { get; set; } = new();

        public List<Project> SubscribedProjects { get; set; } = new();

        // Parámetro para identificar al usuario seleccionado
        [BindProperty(SupportsGet = true)]
        public int? UserId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || !role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/UserHome/Index");
            }

            var query = _context.Users.Include(u => u.Profile).AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var searchLower = Search.ToLower();
                query = query.Where(u => (u.Name + " " + u.LastName).ToLower().Contains(searchLower));
            }

            Users = await query.ToListAsync();

            if (UserId.HasValue)
            {
                SelectedUser = await _context.Users
                    .Include(u => u.Profile)
                    .FirstOrDefaultAsync(u => u.Id == UserId.Value);

                SelectedProfile = SelectedUser?.Profile ?? new Proyectamos.Models.Profile();

                CreatedProjects = await _context.Projects
                    .Include(p => p.Category)
                    .Where(p => p.UserID == UserId.Value && p.Status == 1)
                    .ToListAsync();

                SubscribedProjects = await _context.ProjectUsers
                    .Include(pu => pu.Project)
                        .ThenInclude(p => p.Category)
                    .Where(pu => pu.UserID == UserId.Value && pu.status == 1)
                    .Select(pu => pu.Project)
                    .ToListAsync();
            }
            return Page();
        }

        // Cambiar rol del usuario
        public async Task<IActionResult> OnPostChangeRoleAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Role = (user.Role.ToLower() == "admin") ? "user" : "admin";
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { UserId = userId });
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Status = 0;
                user.LastUpdate = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            TempData["Message"] = "Usuario Eliminado";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.Status == 0)
            {
                user.Status = 1;
                user.LastUpdate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            TempData["Message"] = "Usuario reactivado correctamente";
            return RedirectToPage(new { UserId = userId });

        }
    }
}
