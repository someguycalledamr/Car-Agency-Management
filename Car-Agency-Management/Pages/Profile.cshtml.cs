// Profile.cshtml.cs

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLux.Drive.Pages
{
    public class ProfileModel : PageModel
    {
        // Example structure for data
        public string UserName { get; set; } = "Johnathan Doe";
        public string Email { get; set; } = "johnathan.doe@autolux.drive";
        public string UserId { get; set; } = "ALX980123";

        public List<PaymentRecord> PaymentHistory { get; set; } = new List<PaymentRecord>
        {
            // ... (Your actual data structure)
        };

        public void OnGet()
        {
            // Logic to fetch user data from a database goes here
        }
    }

    public class PaymentRecord
    {
        public string InvoiceId { get; set; }
        public string Service { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
    }
}