// Profile.cshtml.cs

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AutoLux.Drive.Pages
{
    public class ProfileModel : PageModel
    {
        
        public string UserName { get; set; } = "Johnathan Doe";
        public string Email { get; set; } = "johnathan.doe@autolux.drive";
        public string UserId { get; set; } = "ALX980123";
        public class PaymentRecord
        {
            public string InvoiceId { get; set; }
            public string Service { get; set; }
            public decimal Amount { get; set; }
            public string Date { get; set; }
            public string Status { get; set; }
        }

        public List<PaymentRecord> PaymentHistory { get; set; } = new List<PaymentRecord>
        {
            new PaymentRecord { InvoiceId = "INV-1004", Service = "Mercedes S-Class Rental (5 days)", Amount = 2500.00M, Date = "2024-05-20", Status = "Paid" },
            new PaymentRecord { InvoiceId = "INV-1003", Service = "Airport Transfer Service", Amount = 180.00M, Date = "2024-04-12", Status = "Paid" },
            new PaymentRecord { InvoiceId = "INV-1002", Service = "Porsche 911 Rental (1 day)", Amount = 950.00M, Date = "2024-03-01", Status = "Paid" },
            new PaymentRecord { InvoiceId = "INV-1001", Service = "BMW 7-Series Rental (7 days)", Amount = 3200.00M, Date = "2024-01-15", Status = "Paid" },
            new PaymentRecord { InvoiceId = "INV-1000", Service = "Deposit for Future Booking", Amount = 500.00M, Date = "2023-12-01", Status = "Pending Refund" }
        };

        public void OnGet()
        {

        }
    }

   
    {
        public string InvoiceId { get; set; }
        public string Service { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
    }
}