using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;  // ADDED: Import for database classes
using System.Collections.Generic;   // ADDED: For List<T>

namespace Car_Agency_Management.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        // ADDED: Database instance to fetch data from SQL Server
        private readonly DB _database;

        // ============================================
        // ADDED: Public properties to hold homepage data
        // These will be accessible in the Index.cshtml view
        // ============================================

        /// <summary>
        /// ADDED: Holds top 3 newest cars from database for "New Arrivals" section
        /// Populated by GetNewArrivals() query (ORDER BY DATE_ADDED DESC)
        /// </summary>
        public List<CarSummary> NewArrivals { get; set; } = new List<CarSummary>();

        /// <summary>
        /// ADDED: Holds top 3 most popular cars from last month for "Trending Now" section
        /// Populated by GetTrendingCars() query (based on BUYING_RENTING transactions)
        /// </summary>
        public List<CarSummary> TrendingCars { get; set; } = new List<CarSummary>();

        /// <summary>
        /// ADDED: Holds all active partners for the partners carousel section
        /// Populated by GetActivePartners() query (WHERE IS_ACTIVE = 1)
        /// </summary>
        public List<Partner> Partners { get; set; } = new List<Partner>();

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            // ADDED: Initialize database connection
            _database = new DB();
        }

        /// <summary>
        /// MODIFIED: OnGet method now loads dynamic data from database
        /// This method runs when the page loads (HTTP GET request)
        /// </summary>
        public void OnGet()
        {
            // ============================================
            // ADDED: Load all homepage data from database
            // ============================================

            // Load New Arrivals (Top 3 newest cars by DATE_ADDED)
            // Query: SELECT TOP 3 * FROM CAR ORDER BY DATE_ADDED DESC
            NewArrivals = _database.GetNewArrivals();

            // Load Trending Cars (Top 3 most rented/bought in last 30 days)
            // Query: Complex join with BUYING_RENTING, CUSTOMER, and PAYMENT tables
            // Counts transactions from last month and orders by popularity
            TrendingCars = _database.GetTrendingCars();

            // Load Active Partners (All brands with IS_ACTIVE = 1)
            // Query: SELECT BRAND_NAME, LOGO_URL FROM PARTNERS WHERE IS_ACTIVE = 1
            Partners = _database.GetActivePartners();
        }

        
        public IActionResult OnPost()
        {
            return RedirectToPage("profile");
        }

       
        public IActionResult OnPostViewGallery()
        {
            return RedirectToPage("/Car-gallary");
        }
    }
}

