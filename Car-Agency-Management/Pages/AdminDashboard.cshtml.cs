using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;
using System.Collections.Generic;

namespace Car_Agency_Management.Pages
{
    public class AdminDashboardModel : PageModel
    {
        // Database instance
        private readonly DB _database;

        // Dashboard Statistics - Now populated from database
        public int TotalCars { get; set; }
        public int TotalSales { get; set; }
        public int ActiveRentals { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Recent Activities - From database
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();

        // Sales Data for Charts - From database
        public List<MonthlyRevenue> MonthlyRevenueData { get; set; } = new List<MonthlyRevenue>();
        public List<CarSalesData> CarSalesData { get; set; } = new List<CarSalesData>();

        // Top Selling Cars - From database
        public List<TopCar> TopSellingCars { get; set; } = new List<TopCar>();

        // Recent Transactions - From database
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();

        public AdminDashboardModel()
        {
            // Initialize database connection
            _database = new DB();
        }

        public void OnGet()
        {
            // Load all dashboard data from database
            LoadDashboardDataFromDatabase();
        }

        public IActionResult OnPostResetDatabase()
        {
            string scriptPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data", "setup.sql");
            bool success = _database.ResetDatabase(scriptPath);
            
            if (success)
            {
                // Reload dashboard to reflect new data
                return RedirectToPage();
            }
            else
            {
                // Ideally show error message
                return RedirectToPage();
            }
        }

        /// <summary>
        /// Load all dashboard data from SQL Server database
        /// Replaces hardcoded sample data with real database queries
        /// </summary>
        private void LoadDashboardDataFromDatabase()
        {
            try
            {
                // ============================================
                // LOAD STATISTICS FROM DATABASE
                // ============================================

                // Get total cars count
                // Query: SELECT COUNT(*) FROM CAR
                TotalCars = _database.GetTotalCars();

                // Get total sales count
                // Query: SELECT COUNT(*) FROM BUYING_RENTING
                TotalSales = _database.GetTotalSales();

                // Get active rentals count (status = 'Confirmed')
                // Query: SELECT COUNT(*) FROM RESERVATIONS WHERE RESERVATION_STATUS = 'Confirmed'
                ActiveRentals = _database.GetActiveRentals();

                // Get total users count
                // Query: SELECT COUNT(*) FROM CUSTOMER
                TotalUsers = _database.GetTotalUsers();

                // Get total revenue (all completed payments)
                // Query: SELECT SUM(AMOUNT) FROM PAYMENT WHERE PAYMENT_STATUS = 'Completed'
                TotalRevenue = _database.GetTotalRevenue();

                // Get monthly revenue (current month)
                // Query: SELECT SUM(AMOUNT) FROM PAYMENT WHERE PAYMENT_STATUS = 'Completed' 
                //        AND MONTH(PAYMENT_DATE) = MONTH(GETDATE())
                MonthlyRevenue = _database.GetMonthlyRevenue();

                // ============================================
                // LOAD CHART DATA FROM DATABASE
                // ============================================

                // Load monthly revenue data for the chart
                // Query: SELECT DATENAME(MONTH, PAYMENT_DATE) AS Month, SUM(AMOUNT) AS Revenue
                //        FROM PAYMENT WHERE PAYMENT_STATUS = 'Completed' AND YEAR(PAYMENT_DATE) = YEAR(GETDATE())
                //        GROUP BY MONTH(PAYMENT_DATE), DATENAME(MONTH, PAYMENT_DATE)
                MonthlyRevenueData = _database.GetMonthlyRevenueData();

                // Load car sales data by brand
                // Query: SELECT c.BRAND, COUNT(br.CAR_ID) AS Sales
                //        FROM CAR c LEFT JOIN BUYING_RENTING br ON c.CAR_ID = br.CAR_ID
                //        GROUP BY c.BRAND ORDER BY Sales DESC
                CarSalesData = _database.GetCarSalesByBrand();

                // ============================================
                // LOAD TOP SELLING CARS FROM DATABASE
                // ============================================

                // Query: SELECT TOP 5 c.CAR_NAME, COUNT(br.CAR_ID) AS Sales,
                //        SUM(CAST(REPLACE(c.PRICE, ',', '') AS DECIMAL(10,2))) AS Revenue
                //        FROM CAR c LEFT JOIN BUYING_RENTING br ON c.CAR_ID = br.CAR_ID
                //        GROUP BY c.CAR_ID, c.CAR_NAME, c.PRICE ORDER BY Sales DESC
                TopSellingCars = _database.GetTopSellingCars();

                // ============================================
                // LOAD RECENT ACTIVITIES FROM DATABASE
                // ============================================

                // Combined query from new cars, sales, rentals, and payments
                // Query: SELECT TOP 10 * FROM (
                //        SELECT 'New Car Added' AS Action, 'Car: ' + CAR_NAME + ' added' AS Description, DATE_ADDED AS Timestamp
                //        UNION ALL SELECT 'Sale Completed'... UNION ALL SELECT 'Rental Started'... etc.)
                RecentActivities = _database.GetRecentActivities();

                // ============================================
                // LOAD RECENT TRANSACTIONS FROM DATABASE
                // ============================================

                // Query: SELECT TOP 10 'TRX-' + CAST(p.PAYMENT_ID AS VARCHAR) AS Id,
                //        c.FNAME + ' ' + c.LNAME AS Customer, car.CAR_NAME AS Car,
                //        p.AMOUNT, p.PAYMENT_DATE, p.PAYMENT_STATUS
                //        FROM PAYMENT p JOIN CUSTOMER c JOIN BUYING_RENTING br JOIN CAR car
                //        ORDER BY p.PAYMENT_DATE DESC
                RecentTransactions = _database.GetRecentTransactions();

                // Log success
                Console.WriteLine($"Dashboard data loaded successfully:");
                Console.WriteLine($"- Total Cars: {TotalCars}");
                Console.WriteLine($"- Total Sales: {TotalSales}");
                Console.WriteLine($"- Active Rentals: {ActiveRentals}");
                Console.WriteLine($"- Total Users: {TotalUsers}");
                Console.WriteLine($"- Total Revenue: EGP {TotalRevenue:N2}");
                Console.WriteLine($"- Monthly Revenue: EGP {MonthlyRevenue:N2}");
            }
            catch (Exception ex)
            {
                // Log error and use fallback data if database connection fails
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
                LoadFallbackData();
            }
        }

        /// <summary>
        /// Fallback data in case database connection fails
        /// Uses sample data to ensure dashboard still displays
        /// </summary>
        private void LoadFallbackData()
        {
            // Use sample statistics
            TotalCars = 12;
            TotalSales = 0;
            ActiveRentals = 0;
            TotalUsers = 2;
            TotalRevenue = 33433.00m;
            MonthlyRevenue = 33433.00m;

            // Sample monthly revenue data
            MonthlyRevenueData = new List<MonthlyRevenue>
            {
                new MonthlyRevenue { Month = "Jan", Revenue = 0 },
                new MonthlyRevenue { Month = "Feb", Revenue = 0 },
                new MonthlyRevenue { Month = "Mar", Revenue = 0 },
                new MonthlyRevenue { Month = "Apr", Revenue = 0 },
                new MonthlyRevenue { Month = "May", Revenue = 0 },
                new MonthlyRevenue { Month = "Jun", Revenue = 0 },
                new MonthlyRevenue { Month = "Jul", Revenue = 0 },
                new MonthlyRevenue { Month = "Aug", Revenue = 0 },
                new MonthlyRevenue { Month = "Sep", Revenue = 0 },
                new MonthlyRevenue { Month = "Oct", Revenue = 0 },
                new MonthlyRevenue { Month = "Nov", Revenue = 33433 },
                new MonthlyRevenue { Month = "Dec", Revenue = 0 }
            };

            // Sample car sales data
            CarSalesData = new List<CarSalesData>
            {
                new CarSalesData { Brand = "Suzuki", Sales= 0 },
                new CarSalesData { Brand = "Nissan", Sales = 0 },
                new CarSalesData { Brand = "Mercedes-Benz", Sales = 0 },
                new CarSalesData { Brand = "BMW", Sales = 0 },
                new CarSalesData { Brand = "Toyota", Sales = 0 }
            };

            // Sample top selling cars
            TopSellingCars = new List<TopCar>
            {
                new TopCar { Name = "No sales data yet", Sales = 0, Revenue = 0 }
            };

            // Sample recent activities
            RecentActivities = new List<ActivityLog>
            {
                new ActivityLog
                {
                    Action = "Database Connection",
                    Description = "Using fallback data - check database connection",
                    Timestamp = DateTime.Now,
                    Type = "info"
                }
            };

            // Sample recent transactions
            RecentTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = "TRX-000001",
                    Customer = "Robert Anderson",
                    Car = "Suzuki S Presso",
                    Amount = 15386.00m,
                    Date = new DateTime(2024, 11, 1),
                    Status = "Completed"
                },
                new Transaction
                {
                    Id = "TRX-000002",
                    Customer = "Jennifer Taylor",
                    Car = "Nissan Sunny",
                    Amount = 18047.00m,
                    Date = new DateTime(2024, 11, 5),
                    Status = "Completed"
                }
            };
        }
    }
}

