using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;
using System.Collections.Generic;

namespace Car_Agency_Management.Pages
{
    public class CarDetailsModel : PageModel
    {
        private readonly DB _db;

        public CarDetailsModel()
        {
            _db = new DB();
        }

        // Car Properties
        public string CarId { get; set; } = "";
        public string CarName { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Year { get; set; } = "";
        public string Price { get; set; } = "";
        public string MainImage { get; set; } = "";
        public List<string> Images { get; set; } = new List<string>();

        // Specifications
        public string Transmission { get; set; } = "";
        public string FuelType { get; set; } = "";
        public string Engine { get; set; } = "";
        public string Seats { get; set; } = "";
        public string Color { get; set; } = "";
        public string Mileage { get; set; } = "";

        // Features
        public List<string> Features { get; set; } = new List<string>();

        // Payment Details
        public string MinDeposit { get; set; } = "";
        public string MonthlyInstallment { get; set; } = "";

        // Description
        public string Description { get; set; } = "";

        public void OnGet(string carId)
        {
            if (string.IsNullOrEmpty(carId))
            {
                Response.Redirect("/Car-gallery");
                return;
            }

            CarId = carId;
            LoadCarDetails(carId);
        }

        public IActionResult OnPostDelete(string carId)
        {
            if (!string.IsNullOrEmpty(carId))
            {
                // Delete car features
                _db.DeleteCarFeatures(carId);
                // Delete car images
                _db.DeleteCarImages(carId);
                // Delete car record (DB.DeleteCar needs implementation or check if simple DELETE FROM CAR works)
                _db.DeleteCar(carId);
            }
            return RedirectToPage("/Car-gallery");
        }

        private void LoadCarDetails(string carId)
        {
            // Fetch main details
            var carData = _db.GetCarDetails(carId);

            if (carData != null)
            {
                CarName = carData.CarName;
                Brand = carData.Brand;
                Year = carData.Year;
                Price = carData.Price;
                MainImage = carData.MainImage;
                Transmission = carData.Transmission;
                FuelType = carData.FuelType;
                Engine = carData.Engine;
                Seats = carData.Seats;
                Color = carData.Color;
                Mileage = carData.Mileage;
                MinDeposit = carData.MinDeposit;
                MonthlyInstallment = carData.MonthlyInstallment;
                Description = carData.Description;

                // Parse ID for methods that require int
                int.TryParse(carId, out int idInt);

                // Fetch images
                Images = _db.GetCarImagesById(idInt);
                // Ensure main image is in the list or handled
                if (Images == null) Images = new List<string>();
                if (!Images.Contains(MainImage) && !string.IsNullOrEmpty(MainImage))
                {
                    Images.Insert(0, MainImage);
                }

                // Fetch features
                Features = _db.GetCarFeaturesById(idInt);
                if (Features == null) Features = new List<string>();
            }
            else
            {
                // Car not found handling
                CarName = "Car Not Found";
            }
        }
    }
}