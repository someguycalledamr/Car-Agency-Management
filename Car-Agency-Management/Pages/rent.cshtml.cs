using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;

namespace Car_Agency_Management.Pages
{
    public class rentModel : PageModel
    {
        private readonly DB _db;

        public rentModel()
        {
            _db = new DB();
        }

        // Properties to bind from query string
        [BindProperty(SupportsGet = true)]
        public int CarId { get; set; }

        // Properties for the page
        public CarRentalInfo CarInfo { get; set; }
        public List<DateRange> BookedDates { get; set; }

        // Properties for availability check results
        public bool IsCheckingAvailability { get; set; }
        public bool IsAvailable { get; set; }
        public string AvailabilityMessage { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int RentalDays { get; set; }
        public decimal EstimatedCost { get; set; }

        public void OnGet()
        {
            // Get car ID from query string (e.g., /rent?carId=1)
            if (CarId > 0)
            {
                // Get car information
                CarInfo = _db.GetCarRentalInfo(CarId);

                // Get booked dates for this car
                BookedDates = _db.GetBookedDatesForCar(CarId);

                // Initialize availability check properties
                IsCheckingAvailability = false;
            }
            else
            {
                // Redirect to gallery if no car ID provided
                Response.Redirect("/Gallery");
            }
        }

        /// <summary>
        /// Check availability when user selects dates
        /// </summary>
        public void OnPost()
        {
            // Get carId from form if not in query string
            if (CarId == 0)
            {
                string carIdStr = Request.Form["carId"];
                if (!string.IsNullOrEmpty(carIdStr))
                {
                    CarId = int.Parse(carIdStr);
                }
            }

            // Get car information
            CarInfo = _db.GetCarRentalInfo(CarId);
            BookedDates = _db.GetBookedDatesForCar(CarId);

            // Get dates from form
            try
            {
                DateTime startDate, endDate;
                if (!DateTime.TryParseExact(Request.Form["startDate"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate) ||
                    !DateTime.TryParseExact(Request.Form["endDate"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
                {
                    ErrorMessage = "Please select valid start and end dates.";
                    AvailabilityMessage = ErrorMessage; // Added
                    IsAvailable = false; // Added
                    IsCheckingAvailability = true;
                    return;
                }

                if (startDate < DateTime.Today)
                {
                    ErrorMessage = "Start date cannot be in the past.";
                    AvailabilityMessage = ErrorMessage; // Added
                    IsAvailable = false; // Added
                    IsCheckingAvailability = true;
                    return;
                }

                if (endDate <= startDate)
                {
                    ErrorMessage = "End date must be at least one day after start date.";
                    AvailabilityMessage = ErrorMessage; // Added
                    IsAvailable = false; // Added
                    IsCheckingAvailability = true;
                    return;
                }

                var availability = _db.CheckRentalAvailability(CarId, startDate, endDate);
                if (availability.IsAvailable)
                {
                    IsCheckingAvailability = true; // Set to true to show results
                    IsAvailable = true; // Keep this for consistency with original logic if needed
                    AvailabilityMessage = availability.Message; // Keep this for consistency with original logic if needed
                    RentalDays = _db.CalculateRentalDays(startDate, endDate);
                    EstimatedCost = _db.CalculateEstimatedRentalCost(CarId, startDate, endDate);
                    
                    // Update model properties
                    StartDate = startDate;
                    EndDate = endDate;

                    Console.WriteLine($"✅ Car available for {CarId}: {startDate:d} to {endDate:d}. Cost: {EstimatedCost}");
                }
                else
                {
                    ErrorMessage = availability.Message;
                    IsCheckingAvailability = true;
                    IsAvailable = false; // Keep this for consistency with original logic if needed
                    AvailabilityMessage = availability.Message; // Keep this for consistency with original logic if needed
                    Console.WriteLine($"❌ Car NOT available for {CarId}: {availability.Message}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An unexpected error occurred while checking availability. Please try again.";
                IsCheckingAvailability = true;
                IsAvailable = false; // Keep this for consistency with original logic if needed
                AvailabilityMessage = ErrorMessage; // Keep this for consistency with original logic if needed
                Console.WriteLine($"Critical Error in rent OnPost: {ex.Message}");
            }
        }

        /// <summary>
        /// Proceed to payment page with selected dates
        /// </summary>
        public IActionResult OnPostProceedToPayment()
        {
            try
            {
                DateTime startDate, endDate;
                if (!DateTime.TryParseExact(Request.Form["startDate"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out startDate) ||
                    !DateTime.TryParseExact(Request.Form["endDate"], "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out endDate))
                {
                    TempData["ErrorMessage"] = "Please select valid start and end dates.";
                    return RedirectToPage("/rent", new { carId = CarId });
                }

                if (startDate < DateTime.Today)
                {
                    TempData["ErrorMessage"] = "Start date cannot be in the past.";
                    return RedirectToPage("/rent", new { carId = CarId });
                }

                if (endDate <= startDate)
                {
                    TempData["ErrorMessage"] = "End date must be at least one day after start date.";
                    return RedirectToPage("/rent", new { carId = CarId });
                }

                var availability = _db.CheckRentalAvailability(CarId, startDate, endDate);

                if (!availability.IsAvailable)
                {
                    TempData["ErrorMessage"] = availability.Message;
                    return RedirectToPage("/rent", new { carId = CarId });
                }

                decimal cost = _db.CalculateEstimatedRentalCost(CarId, startDate, endDate);
                
                // Store dates in TempData for payment page
                TempData["RentalCarId"] = CarId;
                TempData["RentalStartDate"] = startDate.ToString("dd/MM/yyyy");
                TempData["RentalEndDate"] = endDate.ToString("dd/MM/yyyy");
                TempData["RentalDays"] = _db.CalculateRentalDays(startDate, endDate);
                TempData["EstimatedCost"] = cost;

                return RedirectToPage("/Payment_page", new { CarId = CarId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while proceeding: " + ex.Message;
                return RedirectToPage("/rent", new { carId = CarId });
            }
        }
    }
}