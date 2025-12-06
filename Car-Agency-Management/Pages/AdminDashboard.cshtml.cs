using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Car_Agency_Management.Pages
{
    public class AdminDashboardModel : PageModel
    {
        // Dashboard Statistics
        public int TotalCars { get; set; }
        public int TotalSales { get; set; }
        public int ActiveRentals { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Recent Activities
        public List<ActivityLog> RecentActivities { get; set; } = new List<ActivityLog>();

        // Sales Data for Charts
        public List<MonthlyRevenue> MonthlyRevenueData { get; set; } = new List<MonthlyRevenue>();
        public List<CarSalesData> CarSalesData { get; set; } = new List<CarSalesData>();

        // Top Selling Cars
        public List<TopCar> TopSellingCars { get; set; } = new List<TopCar>();

        // Recent Transactions
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();

        public void OnGet()
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            // Load statistics
            TotalCars = 24;
            TotalSales = 156;
            ActiveRentals = 18;
            TotalUsers = 342;
            TotalRevenue = 45250000;
            MonthlyRevenue = 8750000;

            // Load monthly revenue data for chart
            MonthlyRevenueData = new List<MonthlyRevenue>
            {
                new MonthlyRevenue { Month = "Jan", Revenue = 5200000 },
                new MonthlyRevenue { Month = "Feb", Revenue = 6100000 },
                new MonthlyRevenue { Month = "Mar", Revenue = 5800000 },
                new MonthlyRevenue { Month = "Apr", Revenue = 7200000 },
                new MonthlyRevenue { Month = "May", Revenue = 6900000 },
                new MonthlyRevenue { Month = "Jun", Revenue = 8100000 },
                new MonthlyRevenue { Month = "Jul", Revenue = 7500000 },
                new MonthlyRevenue { Month = "Aug", Revenue = 8750000 },
                new MonthlyRevenue { Month = "Sep", Revenue = 7800000 },
                new MonthlyRevenue { Month = "Oct", Revenue = 8200000 },
                new MonthlyRevenue { Month = "Nov", Revenue = 8900000 },
                new MonthlyRevenue { Month = "Dec", Revenue = 9500000 }
            };

            // Load car sales data
            CarSalesData = new List<CarSalesData>
            {
                new CarSalesData { Brand = "Suzuki", Sales = 45 },
                new CarSalesData { Brand = "Nissan", Sales = 38 },
                new CarSalesData { Brand = "Mercedes", Sales = 28 },
                new CarSalesData { Brand = "BMW", Sales = 25 },
                new CarSalesData { Brand = "Toyota", Sales = 20 }
            };

            // Load top selling cars
            TopSellingCars = new List<TopCar>
            {
                new TopCar { Name = "Suzuki S-Presso", Sales = 45, Revenue = 24745500 },
                new TopCar { Name = "Nissan Sunny", Sales = 38, Revenue = 24510000 },
                new TopCar { Name = "Mercedes C-Class", Sales = 28, Revenue = 35000000 },
                new TopCar { Name = "BMW X3", Sales = 25, Revenue = 36250000 }
            };

            // Load recent activities
            RecentActivities = new List<ActivityLog>
            {
                new ActivityLog
                {
                    Action = "New Car Added",
                    Description = "BMW X5 2025 added to inventory",
                    Timestamp = DateTime.Now.AddMinutes(-15),
                    Type = "success"
                },
                new ActivityLog
                {
                    Action = "Sale Completed",
                    Description = "Mercedes C-Class sold to Ahmed Hassan",
                    Timestamp = DateTime.Now.AddHours(-2),
                    Type = "success"
                },
                new ActivityLog
                {
                    Action = "Rental Started",
                    Description = "Nissan Sunny rented by Sara Mohamed",
                    Timestamp = DateTime.Now.AddHours(-4),
                    Type = "info"
                },
                new ActivityLog
                {
                    Action = "Payment Received",
                    Description = "Payment of EGP 125,000 received",
                    Timestamp = DateTime.Now.AddHours(-6),
                    Type = "success"
                },
                new ActivityLog
                {
                    Action = "User Registered",
                    Description = "New user: Mona Ali registered",
                    Timestamp = DateTime.Now.AddHours(-8),
                    Type = "info"
                }
            };

            // Load recent transactions
            RecentTransactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = "TRX-001234",
                    Customer = "Ahmed Hassan",
                    Car = "Mercedes C-Class",
                    Amount = 1250000,
                    Date = DateTime.Now.AddDays(-1),
                    Status = "Completed"
                },
                new Transaction
                {
                    Id = "TRX-001235",
                    Customer = "Sara Mohamed",
                    Car = "Nissan Sunny",
                    Amount = 645000,
                    Date = DateTime.Now.AddDays(-2),
                    Status = "Completed"
                },
                new Transaction
                {
                    Id = "TRX-001236",
                    Customer = "Omar Ali",
                    Car = "BMW X3",
                    Amount = 1450000,
                    Date = DateTime.Now.AddDays(-3),
                    Status = "Pending"
                },
                new Transaction
                {
                    Id = "TRX-001237",
                    Customer = "Fatima Youssef",
                    Car = "Suzuki S-Presso",
                    Amount = 549900,
                    Date = DateTime.Now.AddDays(-4),
                    Status = "Completed"
                }
            };
        }
    }

    // Helper classes
    public class ActivityLog
    {
        public string Action { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = ""; // success, info, warning, error
    }

    public class MonthlyRevenue
    {
        public string Month { get; set; } = "";
        public decimal Revenue { get; set; }
    }

    public class CarSalesData
    {
        public string Brand { get; set; } = "";
        public int Sales { get; set; }
    }

    public class TopCar
    {
        public string Name { get; set; } = "";
        public int Sales { get; set; }
        public decimal Revenue { get; set; }
    }

    public class Transaction
    {
        public string Id { get; set; } = "";
        public string Customer { get; set; } = "";
        public string Car { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; } = "";
    }
}