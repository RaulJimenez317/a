using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyectamos.Data;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Proyectamos.Pages.Register
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string? Token { get; set; }

        [BindProperty]
        public string? NewPassword { get; set; }

        [BindProperty]
        public string? ConfirmPassword { get; set; }

        public string? Message { get; set; }

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(Token))
            {
                Message = "Token inválido o faltante.";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.ResetToken == Token && u.ResetTokenExpiration > DateTime.UtcNow);

            if (user == null)
            {
                Message = "El enlace de recuperación no es válido o ha expirado.";
                return Page();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (NewPassword != ConfirmPassword)
            {
                Message = "Las contraseñas no coinciden.";
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u => u.ResetToken == Token && u.ResetTokenExpiration > DateTime.Now);

            if (user == null)
            {
                Message = "El enlace de recuperación no es válido o ha expirado.";
                return Page();
            }

            // Hash de la nueva contraseña
            using (var sha = SHA256.Create())
            {
                user.Password = sha.ComputeHash(Encoding.UTF8.GetBytes(NewPassword!));
            }

            // Limpiar token
            user.ResetToken = null;
            user.ResetTokenExpiration = null;

            _context.SaveChanges();

            Message = "Tu contraseña ha sido restablecida correctamente. Ya puedes iniciar sesión.";

            return Page();
        }
    }
}
