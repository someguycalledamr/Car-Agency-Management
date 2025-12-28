using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;

namespace Car_Agency_Management.Pages
{
    public class user_manageModel : PageModel
    {
        private readonly DB _db;

        public user_manageModel()
        {
            _db = new DB();
        }

        public List<UserManagementModel> Users { get; set; } = new List<UserManagementModel>();

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            Users = _db.GetAllUsers();
        }

        public IActionResult OnPostAddUser(string firstName, string lastName, string email, string password, string phone, string address)
        {
            if (_db.IsEmailTaken(email))
            {
                ErrorMessage = "Email is already in use.";
                return RedirectToPage();
            }

            bool success = _db.AddUser(firstName, lastName, email, password, phone, address);
            if (success)
            {
                SuccessMessage = "User added successfully!";
            }
            else
            {
                ErrorMessage = "Failed to add user.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostEditUser(int userId, string firstName, string lastName, string email, string phone, string address)
        {
            bool success = _db.UpdateUser(userId, firstName, lastName, email, phone, address);
            if (success)
            {
                SuccessMessage = "User updated successfully!";
            }
            else
            {
                ErrorMessage = "Failed to update user.";
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDeleteUser(int userId)
        {
            bool success = _db.DeleteUser(userId);
            if (success)
            {
                SuccessMessage = "User deleted successfully!";
            }
            else
            {
                ErrorMessage = "Failed to delete user.";
            }

            return RedirectToPage();
        }
    }
}
