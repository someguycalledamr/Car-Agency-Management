using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Car_Agency_Management.Pages
{
    public class IndexModel : PageModel
    {
        public IActionResult OnPostProfileRedirect()
        {
            return RedirectToPage("/Profile");
        }
    }
}
