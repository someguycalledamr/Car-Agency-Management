using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System;

namespace Car_Agency_Management.Pages
{
    public class DebugUsersModel : PageModel
    {
        public class UserInfo
        {
            public string Id { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public List<UserInfo> Users { get; set; } = new List<UserInfo>();

        public void OnGet()
        {
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=web;Integrated Security=True";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT CUSTOMER_ID, CUSTOMER_EMAIL, CUSTOMER_PASSWORD FROM CUSTOMER";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Users.Add(new UserInfo
                                {
                                    Id = reader["CUSTOMER_ID"].ToString(),
                                    Email = reader["CUSTOMER_EMAIL"].ToString(),
                                    Password = reader["CUSTOMER_PASSWORD"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ignore errors for debug page
            }
        }
    }
}
