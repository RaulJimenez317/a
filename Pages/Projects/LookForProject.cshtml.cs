using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyectamos.Data;
using Proyectamos.Models;

namespace Proyectamos.Pages.Projects
{
    public class LookForProjectModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public LookForProjectModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Project> Projects { get; set; } = new List<Project>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public HashSet<int> ProjectsUserBelongsTo { get; set; } = new HashSet<int>();
        public async Task OnGetAsync()
        {

            int? userIdSession = HttpContext.Session.GetInt32("UserId");


            var query = _context.Projects
                                .Include(p => p.Category)
                                .Include(p => p.User)
                                .Where(p => p.Status == 1)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(p => p.Name.Contains(SearchTerm) ||
                                         (p.Category != null && p.Category.Name.Contains(SearchTerm)));
            }

            Projects = await query.ToListAsync();

            if (userIdSession != null)
            {
                ProjectsUserBelongsTo = await _context.ProjectUsers
                    .Where(pu => pu.UserID == userIdSession.Value)
                    .Select(pu => pu.ProjectID)
                    .ToHashSetAsync();
            }

        }





        //enviar solicitud
        public async Task<IActionResult> OnPostSolicitarUnionAsync(int idProyecto)
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
                return RedirectToPage("/Register/LoginRegister");


            if (idProyecto <= 0)
            {
                TempData["Message"] = "Debes seleccionar un proyecto válido.";
                return RedirectToPage("/Projects/LookForProject");
            }
            // Buscar el proyecto al que se quiere unir
            var proyecto = await _context.Projects.FirstOrDefaultAsync(p => p.Id == idProyecto);
            if (proyecto == null)
            {
                TempData["Message"] = "El proyecto seleccionado no existe.";
                return RedirectToPage("/Projects/LookForProject");
            }

            // Crear la notificacion para el dueño del proyecto
            var notificacion = new Notification
            {
                Type = "Solicitud de unión",
                Date = DateTime.Now,
                UserID = proyecto.UserID,   // destinatario: el dueño del proyecto
                FromUserID = userIdSession.Value,  // quien envía la solicitud
                ProjectID = proyecto.Id,
                IsRead = false
            };

            _context.Notifications.Add(notificacion);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Tu solicitud fue enviada correctamente.";

            //pagina del proyecto
            return RedirectToPage("/Projects/LookForProject");
        }

    }
}
