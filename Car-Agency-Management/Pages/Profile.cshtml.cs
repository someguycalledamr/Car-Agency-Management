// Profile.cshtml.cs

using Car_Agency_Management.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace AutoLux.Drive.Pages
{
    public class ProfileModel : PageModel
    {

        public CustomerModel Customer { get; set; }
        public List<TransactionSummary> Transactions { get; set; }

        public IActionResult OnGet(int? id)
        {
            return LoadProfile(id);
        }

        public IActionResult OnGetView(int id)
        {
            return LoadProfile(id);
        }

        private IActionResult LoadProfile(int? id)
        {
            int? userIdToView = id;

            // If no ID provided, show the logged-in user's profile
            if (userIdToView == null)
            {
                userIdToView = HttpContext.Session.GetInt32("UserId");
            }

            // If still no ID (not logged in and no ID param), redirect to login
            if (userIdToView == null)
            {
                return RedirectToPage("/Login");
            }

            var db = new DB();
            Customer = db.GetCustomerProfile(userIdToView.Value);
            Transactions = db.GetCustomerTransactions(userIdToView.Value);

             // Fallback if customer not found
            if (Customer == null)
            {
               // If it was a deep link, maybe rediret back or show error
                return RedirectToPage("/Index");
            }
            
            return Page();
        }

        public string InvoiceId { get; set; }
        public string Service { get; set; }
        public decimal Amount { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
    }
}
        
    
    


   
  