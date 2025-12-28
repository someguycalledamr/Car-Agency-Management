using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;

namespace Car_Agency_Management.Pages
{
    public class sign_upModel : PageModel
    {
        [BindProperty]
        public string? FirstName { get; set; }

        [BindProperty]
        public string? LastName { get; set; }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string? Address { get; set; }

        [BindProperty]
        public string? Phone { get; set; }

        [BindProperty]
        public string? Password { get; set; }

        [BindProperty]
        public string? ConfirmPassword { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please fill in all required fields.";
                return Page();
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            DB db = new DB();
            
            // Ensure inputs are not null using null-coalescing operator
            string emailAddr = Email ?? "";
            
            if (db.IsEmailTaken(emailAddr))
            {
                ErrorMessage = "This email is already registered.";
                return Page();
            }

            if (!string.IsNullOrEmpty(Phone) && db.IsPhoneTaken(Phone))
            {
                ErrorMessage = "This phone number is already used.";
                return Page();
            }

            bool success = db.AddCustomer(
                FirstName ?? "", 
                LastName ?? "", 
                emailAddr, 
                Password ?? "", 
                Phone ?? "",
                Address ?? ""
            );

            if (!success)
            {
                ErrorMessage = "An error occurred during sign up. Please try again.";
                return Page();
            }

            return RedirectToPage("/Login");
        }
    }
}