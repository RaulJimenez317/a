using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Proyectamos.Pages.AdminHome
{
    public class AdminManageHomeModel : PageModel
    {
        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role) || !role.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPage("/UserHome/Index");
            }

            return Page();
        }
    }
}
