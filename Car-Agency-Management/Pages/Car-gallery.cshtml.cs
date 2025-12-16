using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Car_Agency_Management.Pages
{
    public class Car_galleryModel : PageModel
    {
        public List<CarSummary> Cars { get; set; } = new List<CarSummary>();

        public void OnGet()
        {
            LoadCars();
        }

        private void LoadCars()
        {
            // Load all available cars for the gallery
            Cars = new List<CarSummary>
            {
                new CarSummary
                {
                    CarId = "suzuki-spresso",
                    Name = "Suzuki S Presso Automatic 2024",
                    Brand = "Suzuki",
                    Year = "2024",
                    Price = "549,900",
                    Image = "/images/Suzuki_S-Presso_Exterior_01.png",
                    Transmission = "Automatic",
                    FuelType = "Petrol"
                },
                new CarSummary
                {
                    CarId = "nissan-sunny",
                    Name = "Nissan Sunny Manual / Baseline 2026",
                    Brand = "Nissan",
                    Year = "2026",
                    Price = "645,000",
                    Image = "/images/Nissan-sunny.png",
                    Transmission = "Manual",
                    FuelType = "Petrol"
                },
                new CarSummary
                {
                    CarId = "mercedes-c-class",
                    Name = "Mercedes-Benz C-Class 2025",
                    Brand = "Mercedes-Benz",
                    Year = "2025",
                    Price = "1,250,000",
                    Image = "/images/Mercedes-Benz.png",
                    Transmission = "Automatic",
                    FuelType = "Petrol"
                },
                new CarSummary
                {
                    CarId = "bmw-x3",
                    Name = "BMW X3 xDrive 2025",
                    Brand = "BMW",
                    Year = "2025",
                    Price = "1,450,000",
                    Image = "/images/Angebotsbild-bmw-x3-20d-xdrive-q1-2025.png",
                    Transmission = "Automatic",
                    FuelType = "Diesel"
                },
                new CarSummary
                {
                    CarId = "proton-saga",
                    Name = "Proton-Saga A/T Premium 2026",
                    Brand = "Proton",
                    Year = "2026",
                    Price = "649,900",
                    Image = "/images/Proton-saga.png",
                    Transmission = "Automatic",
                    FuelType = "Petrol"
                },
                new CarSummary
                {
                    CarId = "mg-zs",
                    Name = "MG ZS Standard 2025",
                    Brand = "MG",
                    Year = "2025",
                    Price = "685,000",
                    Image = "/images/MG-ZS-BLACK.png",
                    Transmission = "Automatic",
                    FuelType = "Hybrid"
                },
                new CarSummary
                {
                    CarId = "toyota-corolla",
                    Name = "Toyota Corolla GLi 2025",
                    Brand = "Toyota",
                    Year = "2025",
                    Price = "785,000",
                    Image = "/images/corolla.png",
                    Transmission = "Automatic",
                    FuelType = "Petrol"
                },
                new CarSummary
                {
                    CarId = "hyundai-tucson",
                    Name = "Hyundai Tucson 2025",
                    Brand = "Hyundai",
                    Year = "2025",
                    Price = "895,000",
                    Image = "/images/Tucson.png",
                    Transmission = "Automatic",
                    FuelType = "Diesel"
                },
                new CarSummary
                {
                    CarId = "kia-sportage",
                    Name = "Kia Sportage LX 2025",
                    Brand = "Kia",
                    Year = "2025",
                    Price = "920,000",
                    Image = "/images/kia-sportage.png",
                    Transmission = "Automatic",
                    FuelType = "Hybrid"
                },
                new CarSummary
                {
                    CarId = "chery-tiggo7",
                    Name = "Chery Tiggo 7 Comfort 2025",
                    Brand = "Chery",
                    Year = "2025",
                    Price = "740,000",
                    Image = "/images/Chery.png",
                    Transmission = "Automatic",
                    FuelType = "Petrol"
                },
                new CarSummary
                {
                    CarId = "renault-duster",
                    Name = "Renault Duster 2025",
                    Brand = "Renault",
                    Year = "2025",
                    Price = "670,000",
                    Image = "/images/Renault-duster.png",
                    Transmission = "Automatic",
                    FuelType = "Petrol"
                },
                new CarSummary
                {
                    CarId = "citroen-c3",
                    Name = "Citroen C3 2025",
                    Brand = "Citroen",
                    Year = "2025",
                    Price = "830,000",
                    Image = "/images/Citroen-c3.jpg",
                    Transmission = "Automatic",
                    FuelType = "Hybrid"
                }
            };
        }
    }

    // Helper class for car summary in gallery
    public class CarSummary
    {
        public string CarId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Year { get; set; } = "";
        public string Price { get; set; } = "";
        public string Image { get; set; } = "";
        public string Transmission { get; set; } = "";
        public string FuelType { get; set; } = "";
    }
}