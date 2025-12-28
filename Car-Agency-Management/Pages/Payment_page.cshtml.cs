using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;
using System;
using System.Collections.Generic;

namespace Car_Agency_Management.Pages
{
    public class Payment_pageModel : PageModel
    {
        private readonly DB _db;

        public Payment_pageModel()
        {
            _db = new DB();
        }

        // ============================================
        // PROPERTIES FOR PAGE DATA
        // ============================================

        // Car Information
        public CarPaymentInfo CarInfo { get; set; }

        // Insurance Plans
        public List<InsurancePlan> InsurancePlans { get; set; } = new List<InsurancePlan>();

        // Customer Information
        public CustomerPaymentInfo CustomerInfo { get; set; }

        // URL Parameters
        [BindProperty(SupportsGet = true)]
        public int CarId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CustomerId { get; set; } = 1; // Default for testing

        // ============================================
        // FORM SUBMISSION PROPERTIES
        // ============================================

        [BindProperty]
        public string SelectedInsurancePlan { get; set; } = "1-year";

        [BindProperty]
        public string DiscountCode { get; set; }

        [BindProperty]
        public string CardNumber { get; set; }

        [BindProperty]
        public string ExpirationDate { get; set; }

        [BindProperty]
        public string CVV { get; set; }

        [BindProperty]
        public string CardName { get; set; }

        [BindProperty]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [BindProperty]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        [BindProperty]
        public string TransactionType { get; set; } = "Rent"; // "Rent" or "Buy"

        [BindProperty]
        public decimal TotalAmount { get; set; }

        // ============================================
        // PAGE LOAD
        // ============================================

        public IActionResult OnGet()
        {
            Console.WriteLine($"=== Payment Page Load ===");
            Console.WriteLine($"CarId: {CarId}");
            Console.WriteLine($"CustomerId: {CustomerId}");

            // Validate required parameters
            if (CarId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid car selection. Please select a car first.";
                return RedirectToPage("/Car-gallery");
            }

            // Load car information
            CarInfo = _db.GetCarPaymentInfo(CarId);

            if (CarInfo == null)
            {
                TempData["ErrorMessage"] = "Car not found.";
                return RedirectToPage("/Car-gallery");
            }

            Console.WriteLine($"✅ Car loaded: {CarInfo.CarName}");

            // Load insurance plans for this car
            InsurancePlans = _db.GetInsurancePlans(CarId);
            Console.WriteLine($"✅ Found {InsurancePlans.Count} insurance plans");

            // Load customer information from session
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId != null)
            {
                CustomerId = sessionUserId.Value;
                CustomerInfo = _db.GetCustomerPaymentInfo(CustomerId);
                if (CustomerInfo != null)
                {
                    Console.WriteLine($"✅ Customer loaded from session: {CustomerInfo.FullName}");
                }
            }

            // Restore dates from TempData (passed from /rent page)
            if (TempData["RentalStartDate"] != null)
            {
                StartDate = DateTime.Parse(TempData["RentalStartDate"].ToString());
                TempData.Keep("RentalStartDate"); // Keep for postback if needed
            }
            if (TempData["RentalEndDate"] != null)
            {
                EndDate = DateTime.Parse(TempData["RentalEndDate"].ToString());
                TempData.Keep("RentalEndDate");
            }
            if (TempData["EstimatedCost"] != null)
            {
                TotalAmount = Convert.ToDecimal(TempData["EstimatedCost"]);
                TempData.Keep("EstimatedCost");
            }

            // Check car availability
            string availability = _db.CheckCarAvailability(CarId, StartDate, EndDate);
            Console.WriteLine($"Car availability: {availability}");

            if (availability == "Not Available")
            {
                TempData["WarningMessage"] = "This car may not be available for the selected dates. Please check availability.";
            }

            return Page();
        }

        // ============================================
        // AJAX HANDLERS
        // ============================================

        /// <summary>
        /// Handler to validate discount code via AJAX
        /// </summary>
        public JsonResult OnGetValidateDiscount(string code)
        {
            Console.WriteLine($"Validating discount code: {code}");

            if (string.IsNullOrWhiteSpace(code))
            {
                return new JsonResult(new { valid = false, message = "Please enter a discount code" });
            }

            var discount = _db.ValidateDiscountCode(code);

            if (discount != null)
            {
                Console.WriteLine($"✅ Valid discount: {discount.DiscountPercent}%");
                return new JsonResult(new
                {
                    valid = true,
                    discountPercent = discount.DiscountPercent,
                    message = $"Discount applied: {discount.DiscountPercent}% off!"
                });
            }
            else
            {
                Console.WriteLine("❌ Invalid discount code");
                return new JsonResult(new { valid = false, message = "Invalid or expired discount code" });
            }
        }

        /// <summary>
        /// Handler to check car availability via AJAX
        /// </summary>
        public JsonResult OnGetCheckAvailability(int carId, DateTime startDate, DateTime endDate)
        {
            Console.WriteLine($"Checking availability for car {carId} from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            string availability = _db.CheckCarAvailability(carId, startDate, endDate);

            return new JsonResult(new
            {
                available = availability == "Available",
                message = availability == "Available"
                    ? "Car is available for selected dates"
                    : "Car is not available for selected dates"
            });
        }

        // ============================================
        // PAYMENT SUBMISSION
        // ============================================

        public IActionResult OnPost()
        {
            Console.WriteLine("=== PAYMENT SUBMISSION ===");
            Console.WriteLine($"CarId: {CarId}");
            Console.WriteLine($"CustomerId: {CustomerId}");
            Console.WriteLine($"Transaction Type: {TransactionType}");
            Console.WriteLine($"Total Amount: {TotalAmount}");
            Console.WriteLine($"Card Name: {CardName}");

            // Validate form
            if (string.IsNullOrWhiteSpace(CardNumber))
            {
                ModelState.AddModelError("CardNumber", "Card number is required");
            }

            if (string.IsNullOrWhiteSpace(CardName))
            {
                ModelState.AddModelError("CardName", "Card name is required");
            }

            if (string.IsNullOrWhiteSpace(CVV))
            {
                ModelState.AddModelError("CVV", "CVV is required");
            }

            if (TotalAmount <= 0)
            {
                ModelState.AddModelError("TotalAmount", "Invalid payment amount");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Form validation failed");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }

                // Reload page data
                CarInfo = _db.GetCarPaymentInfo(CarId);
                InsurancePlans = _db.GetInsurancePlans(CarId);
                CustomerInfo = _db.GetCustomerPaymentInfo(CustomerId);

                return Page();
            }

            // Check car availability one more time before booking
            string availability = _db.CheckCarAvailability(CarId, StartDate, EndDate);

            if (availability != "Available")
            {
                ModelState.AddModelError("", "Sorry, this car is no longer available for the selected dates.");

                // Reload page data
                CarInfo = _db.GetCarPaymentInfo(CarId);
                InsurancePlans = _db.GetInsurancePlans(CarId);
                CustomerInfo = _db.GetCustomerPaymentInfo(CustomerId);

                return Page();
            }

            // Process payment and create reservation
            Console.WriteLine("Processing payment...");

            var result = _db.CreateReservationAndPayment(
                customerId: CustomerId,
                carId: CarId,
                startDate: StartDate,
                endDate: EndDate,
                transactionType: TransactionType,
                paymentMethod: "Credit Card",
                amountPaid: TotalAmount
            );

            if (result.Success)
            {
                Console.WriteLine($"✅ Payment successful!");
                Console.WriteLine($"   Reservation ID: {result.ReservationId}");
                Console.WriteLine($"   Payment ID: {result.PaymentId}");

                TempData["SuccessMessage"] = "Payment successful! Your booking has been confirmed.";
                TempData["PaymentId"] = result.PaymentId;
                TempData["TransactionId"] = $"TRX-{result.PaymentId:D6}";
                TempData["Amount"] = TotalAmount;

                // Redirect to confirmation/receipt page
                return RedirectToPage("/PaymentConfirmation", new { paymentId = result.PaymentId });
            }
            else
            {
                Console.WriteLine($"❌ Payment failed: {result.ErrorMessage}");
                ModelState.AddModelError("", $"Payment failed: {result.ErrorMessage}");

                // Reload page data
                CarInfo = _db.GetCarPaymentInfo(CarId);
                InsurancePlans = _db.GetInsurancePlans(CarId);
                CustomerInfo = _db.GetCustomerPaymentInfo(CustomerId);

                return Page();
            }
        }
    }
}