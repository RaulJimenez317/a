using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Proyectamos.Data;
using Proyectamos.Models;

namespace Proyectamos.Pages.Profiles
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public User? UserData { get; set; }

        [BindProperty]
        public Models.Profile ProfileData { get; set; } = new();

        public List<Project> CreatedProjects { get; set; } = new();
        public List<Project> SubscribedProjects { get; set; } = new();


        //notificaciones
        public List<Notification> Notifications { get; set; } = new();


        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToPage("/Register/LoginRegister");
            }

            UserData = _context.Users.FirstOrDefault(u => u.Id == id);
            if (UserData == null)
                return NotFound();

            ProfileData = _context.Profiles.FirstOrDefault(p => p.UserID == id) ?? new Models.Profile();

            CreatedProjects = _context.Projects
                .Include(p => p.Category)
                .Where(p => p.UserID == id && p.Status == 1)
                .ToList();

            SubscribedProjects = _context.ProjectUsers
                .Include(pu => pu.User)
                .Include(pu => pu.Project)
                    .ThenInclude(p => p.Category) 
                .Where(pu => pu.UserID == id && pu.Project.Status == 1)
                .Select(pu => pu.Project)
                .ToList();


            //notificaciones
            Notifications = _context.Notifications
                .Include(n => n.Project)
                .Include(n => n.FromUser)
                .Where(n => n.UserID == id && !n.IsRead)
                .OrderByDescending(n => n.Date)
                .ToList();


            return Page();
        }

        public async Task<IActionResult> OnPostAsync(IFormFile? ProfileImage, IFormFile? ProfileCurriculum)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToPage("/Login");

            UserData = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (UserData == null) return NotFound();

            var existingProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserID == userId.Value);

            if (existingProfile == null)
            {
                // IMPORTANTE: Id debe ser igual al UserId para respetar la FK
                existingProfile = new Models.Profile
                {
                    Id = userId.Value,
                    UserID = userId.Value
                };
                _context.Profiles.Add(existingProfile);
            }

            // Actualizamos los campos del perfil
            existingProfile.AboutMe = Request.Form["ProfileData.AboutMe"];
            existingProfile.Linkedin = Request.Form["ProfileData.Linkedin"];

            if (Request.Form["RemoveProfileImage"] == "true")
                existingProfile.Image = null;

            if (ProfileImage != null)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var extension = Path.GetExtension(ProfileImage.FileName);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var imagePath = Path.Combine(folderPath, fileName);

                using var stream = new FileStream(imagePath, FileMode.Create);
                await ProfileImage.CopyToAsync(stream);

                existingProfile.Image = $"/images/profiles/{fileName}";
            }

            if (Request.Form["RemoveCurriculum"] == "true")
                existingProfile.Curriculum = null;

            if (ProfileCurriculum != null)
            {
                var folderPathCV = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files/curriculums");
                if (!Directory.Exists(folderPathCV))
                    Directory.CreateDirectory(folderPathCV);

                var extensionCV = Path.GetExtension(ProfileCurriculum.FileName);
                var fileNameCV = $"{Guid.NewGuid()}{extensionCV}";
                var cvPath = Path.Combine(folderPathCV, fileNameCV);

                using var stream = new FileStream(cvPath, FileMode.Create);
                await ProfileCurriculum.CopyToAsync(stream);

                existingProfile.Curriculum = $"/files/curriculums/{fileNameCV}";
            }

            await _context.SaveChangesAsync();

            return RedirectToPage(new { id = userId.Value });
        }




        //notificaciones

        public async Task<IActionResult> OnPostAcceptNotificationAsync(int notificationId, int toUserId, string message)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return RedirectToPage("/Register/LoginRegister");


            var originalNotification = await _context.Notifications.FindAsync(notificationId);
            if (originalNotification == null)
            {
                TempData["Error"] = "La notificación no existe.";
                return RedirectToPage(new { id = currentUserId.Value });
            }

            originalNotification.IsRead = true;
            _context.Notifications.Update(originalNotification);

            var projectId = originalNotification.ProjectID;

            _context.Notifications.Remove(originalNotification);

            var existing = await _context.ProjectUsers
                .FirstOrDefaultAsync(pu => pu.ProjectID == projectId && pu.UserID == toUserId);

            if (existing == null)
            {
                var projectUser = new ProjectUser
                {
                    ProjectID = projectId,
                    UserID = toUserId,
                    Date = DateTime.Now,
                    status = 1 
                };

                _context.ProjectUsers.Add(projectUser);
            }

            var acceptance = new Notification
            {
                Type = "Solicitud aceptada",
                Message = message,
                Date = DateTime.Now,
                UserID = toUserId,               
                FromUserID = currentUserId.Value, 
                ProjectID = projectId,
                IsRead = false
            };

            _context.Notifications.Add(acceptance);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Has aceptado la solicitud y el usuario fue añadido al proyecto.";
            return RedirectToPage(new { id = currentUserId.Value });
        }

        public async Task<IActionResult> OnPostRejectNotificationAsync(int notificationId, int toUserId, string message)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return RedirectToPage("/Register/LoginRegister");

            var originalNotification = await _context.Notifications.FindAsync(notificationId);
            int projectId = 0;

            if (originalNotification != null)
            {
                projectId = originalNotification.ProjectID;
                _context.Notifications.Remove(originalNotification);
            }


            var rejection = new Notification
            {
                Type = "Solicitud rechazada",
                Message = message,
                Date = DateTime.Now,
                UserID = toUserId,
                FromUserID = currentUserId.Value,
                ProjectID = projectId,
                IsRead = false
            };

            _context.Notifications.Add(rejection);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Has rechazado la solicitud.";
            return RedirectToPage(new { id = currentUserId.Value });
        }

        public async Task<IActionResult> OnPostDeleteNotificationAsync(int notificationId)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return RedirectToPage("/Register/LoginRegister");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserID == currentUserId.Value);

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Notificación eliminada correctamente.";
            }

            return RedirectToPage(new { id = currentUserId.Value });
        }



    }
}
