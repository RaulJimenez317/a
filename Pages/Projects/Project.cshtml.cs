using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Proyectamos.Data;
using Proyectamos.Models;
using System;

namespace Proyectamos.Pages.Projects
{
    public class ProjectModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public Project ProjectTest { get; set; }

        [BindProperty]
        public int ProjectId { get; set; }

        [BindProperty]
        public string CommentText { get; set; }

        [BindProperty]
        public List<IFormFile> PhotosAttached { get; set; }

        [BindProperty]
        public List<IFormFile> FilesAttached { get; set; }

        public bool IsUserInProject { get; set; }


        public List<ProjectUser> ProjectMembers { get; set; } = new();
        public ProjectModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync(int id)
        {
            ProjectTest = await _context.Projects
                        .Include(p => p.Files)
                        .Include(p => p.Photos)
                        .Include(p => p.User)
                            .ThenInclude(u => u.Profile)
                        .Include(p => p.Category)
                        .Include(p => p.Comments)
                            .ThenInclude(c => c.User)
                                    .ThenInclude(u => u.Profile)
                        .Include(p => p.Comments)
                            .ThenInclude(c => c.Documents)
                        .FirstOrDefaultAsync(p => p.Id == id);

            var currentUserId = HttpContext.Session.GetInt32("UserId");

            if (currentUserId != null)
            {
                // Verifica si el usuario pertenece al proyecto
                IsUserInProject = await _context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectID == id && pu.UserID == currentUserId.Value);
            }

            ProjectMembers = await _context.ProjectUsers
                .Include(pu => pu.User)
                    .ThenInclude(u => u.Profile)
                .Where(pu => pu.ProjectID == id)
                .ToListAsync();

        }

        public async Task<IActionResult> OnPostAsync()
        {

            int? userIdSession = HttpContext.Session.GetInt32("UserId");

            if (userIdSession == null)
            {
                return RedirectToPage("/Register/LoginRegister");
            }

            if (string.IsNullOrWhiteSpace(CommentText))
            {
                ModelState.AddModelError("", "El comentario no puede estar vacío.");
                return RedirectToPage(new { id = ProjectId });
            }

            var comment = new Comment
            {
                Content = CommentText,
                UserID = userIdSession.Value, 
                ProjectID = ProjectId,
                Date = DateTime.Now,
                Status = 1,
                Documents = new List<DocumentComment>()
            };

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            if (PhotosAttached != null)
            {
                foreach (var file in PhotosAttached)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var uniqueName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsPath, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    comment.Documents.Add(new DocumentComment
                    {
                        File = "/uploads/" + uniqueName,
                        Type = extension,
                        UserID = 1
                    });
                }
            }

            if (FilesAttached != null)
            {
                foreach (var file in FilesAttached)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var uniqueName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(uploadsPath, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    comment.Documents.Add(new DocumentComment
                    {
                        File = "/uploads/" + uniqueName,
                        Type = extension,
                        UserID = userIdSession.Value
                    });
                }
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = ProjectId });
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(int id)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
            if (comment == null)
                return NotFound();

            comment.Status = 0;
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = comment.ProjectID });
        }

        public async Task<IActionResult> OnPostDeleteProjectAsync(int id)
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
            {
                return RedirectToPage("/Register/LoginRegister");
            }
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound();

            var userId = userIdSession; 

            project.Status = 0;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Projects/LookForProject");
        }

        public IActionResult OnPostDeletePhoto(int photoId)
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
                return RedirectToPage("/Register/LoginRegister");

            var photo = _context.Photos.FirstOrDefault(p => p.Id == photoId);
            if (photo == null)
                return NotFound();


            var project = _context.Projects.FirstOrDefault(p => p.Id == photo.ProjectID);
            if (project == null || project.UserID != userIdSession)
                return Forbid();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "createProyectImages", photo.Image);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Photos.Remove(photo);
            _context.SaveChanges();

            return RedirectToPage("/Projects/Project", new { id = photo.ProjectID });
        }


        public async Task<IActionResult> OnPostSolicitarUnionAsync(int idProyecto)
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
                return RedirectToPage("/Register/LoginRegister");

            if (idProyecto <= 0)
            {
                TempData["Message"] = "Debes seleccionar un proyecto válido.";
                return RedirectToPage("/Projects/Project", new { id = idProyecto });
            }

            var proyecto = await _context.Projects.FirstOrDefaultAsync(p => p.Id == idProyecto);
            if (proyecto == null)
            {
                TempData["Message"] = "El proyecto seleccionado no existe.";
                return RedirectToPage("/Projects/Project", new { id = idProyecto });
            }


            bool yaPertenece = await _context.ProjectUsers.AnyAsync(pu => pu.ProjectID == idProyecto && pu.UserID == userIdSession.Value);
            bool yaSolicito = await _context.Notifications.AnyAsync(n =>
                n.ProjectID == idProyecto &&
                n.FromUserID == userIdSession.Value &&
                n.Type == "Solicitud de unión" &&
                !n.IsRead);

            if (yaPertenece)
            {
                TempData["Message"] = "Ya formas parte de este proyecto.";
                return RedirectToPage("/Projects/Project", new { id = idProyecto });
            }

            if (yaSolicito)
            {
                TempData["Message"] = "Ya enviaste una solicitud para este proyecto.";
                return RedirectToPage("/Projects/Project", new { id = idProyecto });
            }

            var utcNow = DateTime.UtcNow;
            var boliviaTime = utcNow.AddHours(-4);

            var notificacion = new Notification
            {
                Type = "Solicitud de unión",
                Date = boliviaTime,
                UserID = proyecto.UserID, 
                FromUserID = userIdSession.Value,
                ProjectID = proyecto.Id,
                IsRead = false
            };

            _context.Notifications.Add(notificacion);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Tu solicitud fue enviada correctamente.";
            return RedirectToPage("/Projects/Project", new { id = idProyecto });
        }

        public async Task<IActionResult> OnPostRemoveMemberAsync(int memberId)
        {
            int? userIdSession = HttpContext.Session.GetInt32("UserId");
            if (userIdSession == null)
                return RedirectToPage("/Register/LoginRegister");

            var member = await _context.ProjectUsers
                .Include(pu => pu.Project)
                .FirstOrDefaultAsync(pu => pu.Id == memberId);

            if (member == null)
                return NotFound();


            if (member.Project.UserID != userIdSession)
                return Forbid();

            _context.ProjectUsers.Remove(member);
            await _context.SaveChangesAsync();

            TempData["Message"] = "El miembro fue eliminado del proyecto correctamente.";
            return RedirectToPage("/Projects/Project", new { id = member.ProjectID });
        }

        public async Task<IActionResult> OnPostDeleteDocumentAsync(int DocumentId)
        {
            var document = await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == DocumentId);

            if (document == null)
                return NotFound();

            var project = await _context.Projects.FindAsync(document.ProjectID);
            var userIdSession = HttpContext.Session.GetInt32("UserId");
            if (project == null || userIdSession != project.UserID)
                return Forbid();

            if (!string.IsNullOrEmpty(document.File))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.File.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Projects/Project", new { id = project.Id });
        }
    }
}
