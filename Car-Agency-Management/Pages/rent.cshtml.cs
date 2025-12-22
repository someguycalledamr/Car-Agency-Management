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
            string startDateStr = Request.Form["startDate"];
            string endDateStr = Request.Form["endDate"];

            if (!string.IsNullOrEmpty(startDateStr) && !string.IsNullOrEmpty(endDateStr))
            {
                DateTime startDate = DateTime.Parse(startDateStr);
                DateTime endDate = DateTime.Parse(endDateStr);

                // Check availability
                var availability = _db.CheckRentalAvailability(CarId, startDate, endDate);

                IsCheckingAvailability = true;
                IsAvailable = availability.IsAvailable;
                AvailabilityMessage = availability.Message;

                if (IsAvailable)
                {
                    // Calculate rental details
                    RentalDays = _db.CalculateRentalDays(startDate, endDate);
                    EstimatedCost = _db.CalculateEstimatedRentalCost(CarId, startDate, endDate);
                }
            }
            else
            {
                IsCheckingAvailability = true;
                IsAvailable = false;
                AvailabilityMessage = "Please select both start and end dates";
            }
        }

        /// <summary>
        /// Proceed to payment page with selected dates
        /// </summary>
        public IActionResult OnPostProceedToPayment()
        {
            string startDateStr = Request.Form["startDate"];
            string endDateStr = Request.Form["endDate"];

            if (string.IsNullOrEmpty(startDateStr) || string.IsNullOrEmpty(endDateStr))
            {
                TempData["ErrorMessage"] = "Please select both start and end dates";
                return RedirectToPage("/rent", new { carId = CarId });
            }

            DateTime startDate = DateTime.Parse(startDateStr);
            DateTime endDate = DateTime.Parse(endDateStr);

            // Validate dates
            if (startDate >= endDate)
            {
                TempData["ErrorMessage"] = "End date must be after start date";
                return RedirectToPage("/rent", new { carId = CarId });
            }

            // Check availability one more time
            var availability = _db.CheckRentalAvailability(CarId, startDate, endDate);

            if (!availability.IsAvailable)
            {
                TempData["ErrorMessage"] = availability.Message;
                return RedirectToPage("/rent", new { carId = CarId });
            }

            // Store dates in TempData for payment page
            TempData["RentalCarId"] = CarId;
            TempData["RentalStartDate"] = startDate.ToString("yyyy-MM-dd");
            TempData["RentalEndDate"] = endDate.ToString("yyyy-MM-dd");
            TempData["RentalDays"] = _db.CalculateRentalDays(startDate, endDate);
            TempData["EstimatedCost"] = _db.CalculateEstimatedRentalCost(CarId, startDate, endDate);

            return RedirectToPage("/Payment", new { carId = CarId });
        }
    }
}