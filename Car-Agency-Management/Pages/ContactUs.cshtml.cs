using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;
using System;

namespace Car_Agency_Management.Pages
{
    public class ContactUsModel : PageModel
    {
        private readonly DB _db;

        [BindProperty]
        public string Message { get; set; }

        public string SuccessMessage { get; set; }

        public ContactUsModel()
        {
            _db = new DB();
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Message))
            {
                ModelState.AddModelError("Message", "Message cannot be empty.");
                return Page();
            }

            // For now passing null for CustomerId as we don't have session management fully clear yet
            // In future, this should take the logged-in user's ID
            bool success = _db.AddComplaint(Message, null);

            if (success)
            {
                SuccessMessage = "Your message has been sent successfully! Redirecting...";
                return Page();
            }
            else
            {
                ModelState.AddModelError("", "Failed to send message. Please try again.");
                return Page();
            }
        }
    }
}
