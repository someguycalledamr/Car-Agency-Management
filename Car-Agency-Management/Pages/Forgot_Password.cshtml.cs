using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;

namespace Car_Agency_Management.Pages
{
    public class Forgot_PasswordModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; } = "";

        [BindProperty]
        public string PhoneLast4 { get; set; } = "";

        [BindProperty]
        public string NewPassword { get; set; } = "";

        [BindProperty]
        public string ConfirmPassword { get; set; } = "";

        [BindProperty]
        public bool IsVerified { get; set; } = false;

        public string? ErrorMessage { get; set; }
        public bool IsSuccess { get; set; } = false;

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            DB db = new DB();

            // STEP 1: Verification
            if (!IsVerified)
            {
                // Clean inputs
                Email = Email?.Trim() ?? "";
                PhoneLast4 = PhoneLast4?.Trim() ?? "";

                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(PhoneLast4))
                {
                    ErrorMessage = "Please enter your email and the last 4 digits of your phone.";
                    return Page();
                }

                if (PhoneLast4.Length != 4)
                {
                    ErrorMessage = "Please enter exactly 4 digits.";
                    return Page();
                }

                if (!db.IsEmailTaken(Email))
                {
                    ErrorMessage = "No account found with this email address.";
                    return Page();
                }

                if (!db.VerifyPhoneLast4(Email, PhoneLast4))
                {
                    ErrorMessage = "Security check failed: The phone digits are incorrect.";
                    return Page();
                }

                // Verification successful
                IsVerified = true;
                ModelState.Clear(); // Ensure the readonly fields reflect the trimmed values
                return Page();
            }

            // STEP 2: Password Reset
            Email = Email?.Trim() ?? "";
            if (string.IsNullOrEmpty(Email))
            {
                // This shouldn't happen if state is maintained, but just in case
                ErrorMessage = "Session error. Please start over.";
                IsVerified = false;
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return Page();
            }

            bool success = db.ResetPassword(Email, NewPassword);

            if (success)
            {
                IsSuccess = true;
                // Clear state for redirect or display
                IsVerified = false; 
                return Page();
            }
            else
            {
                ErrorMessage = "Failed to reset password. Please try again later.";
                return Page();
            }
        }
    }
}
