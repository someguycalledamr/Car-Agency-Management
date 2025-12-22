using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace Car_Agency_Management.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Clear all session data
            HttpContext.Session.Clear();
            
            // Redirect to Login page
            return RedirectToPage("/Login");
        }

        public IActionResult OnPost()
        {
            // Clear all session data
            HttpContext.Session.Clear();
            
            // Redirect to Login page
            return RedirectToPage("/Login");
        }
    }
}
