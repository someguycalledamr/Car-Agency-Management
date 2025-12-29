using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;
using Microsoft.AspNetCore.Http; 

namespace Car_Agency_Management.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Please enter both email and password.";
                return Page();
            }

            DB db = new DB();

            if (!db.IsEmailTaken(Email ?? ""))
            {
                ErrorMessage = "Email not exist. Please Sign Up.";
                return Page();
            }

            int customerId = db.ValidateCustomer(Email ?? "", Password ?? "");
            if (customerId == 0)
            {
                ErrorMessage = "Invalid password.";
                return Page();
            }

            HttpContext.Session.SetInt32("UserId", customerId);
            HttpContext.Session.SetString("UserEmail", Email ?? "");
            
            // Administrative logic: Email containing a hyphen determines role
            string role = "User";
            string email = Email ?? "";
            
            if (email.StartsWith("m-"))
            {
                role = "Maintenance";
            }
            else if (email.Contains("-"))
            {
                role = "Admin";
            }
            
            HttpContext.Session.SetString("UserRole", role);

            return RedirectToPage("/Index");
        }
    }
}
