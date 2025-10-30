using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Proyectamos.Pages.UserHome
{
    public class IndexModel : PageModel
    {
        public IActionResult OnGet()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToPage("/Register/LoginRegister");
            }

            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role) || !role.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/AdminHome/Index");
            }

            return Page();
        }
    }
}
