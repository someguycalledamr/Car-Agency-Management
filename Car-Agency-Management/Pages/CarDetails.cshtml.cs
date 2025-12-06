using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Car_Agency_Management.Pages
{
    public class CarDetailsModel : PageModel
    {
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
            CarId = carId;
            LoadCarDetails(carId);
        }

        private void LoadCarDetails(string carId)
        {
            // This is a sample data structure. Replace with actual database calls
            var carData = GetCarDataById(carId);

            if (carData != null)
            {
                CarName = carData.Name;
                Brand = carData.Brand;
                Year = carData.Year;
                Price = carData.Price;
                MainImage = carData.MainImage;
                Images = carData.Images;
                Transmission = carData.Transmission;
                FuelType = carData.FuelType;
                Engine = carData.Engine;
                Seats = carData.Seats;
                Color = carData.Color;
                Mileage = carData.Mileage;
                Features = carData.Features;
                MinDeposit = carData.MinDeposit;
                MonthlyInstallment = carData.MonthlyInstallment;
                Description = carData.Description;
            }
        }

        private CarData? GetCarDataById(string carId)
        {
            // Complete Sample car database - Replace this with actual database queries
            var cars = new Dictionary<string, CarData>
    {
        {
            "suzuki-spresso", new CarData
            {
                Name = "Suzuki S Presso Automatic 2024",
                Brand = "Suzuki",
                Year = "2024",
                Price = "549,900",
                MainImage = "/images/Suzuki_S-Presso_Exterior_01.png",
                Images = new List<string>
                {
                    "/images/suzuki 2.jpg",
                    "/images/suzuki interior.jpg",
                    "/images/runnning suzuki.jpg",
                    "/images/Suzuki_S-Presso_Exterior_01.png"
                },
                Transmission = "Automatic",
                FuelType = "Petrol",
                Engine = "1.0L 3-Cylinder",
                Seats = "5",
                Color = "White, Yellow, Orange, Grey",
                Mileage = "0",
                Features = new List<string>
                {
                    "Air Conditioning",
                    "Power Windows",
                    "ABS Brakes",
                    "Airbags (Driver & Passenger)",
                    "Central Locking",
                    "USB & Bluetooth",
                    "Touchscreen Display",
                    "Rear Parking Sensors"
                },
                MinDeposit = "82,485",
                MonthlyInstallment = "15,386",
                Description = "The Suzuki S-Presso is a compact and efficient urban car designed for city driving. With its automatic transmission and modern features, it offers excellent fuel economy and reliability. Perfect for daily commutes and city adventures."
            }
        },
        {
            "nissan-sunny", new CarData
            {
                Name = "Nissan Sunny Manual / Baseline 2026",
                Brand = "Nissan",
                Year = "2026",
                Price = "645,000",
                MainImage = "/images/Nissan-sunny.png",
                Images = new List<string>
                {
                    "/images/sunny1.jpg",
                    "/images/sunny interior 2.jpg",
                    "/images/sunny interior 1.jpg",
                    "/images/Nissan-sunny.png"
                },
                Transmission = "Manual",
                FuelType = "Petrol",
                Engine = "1.5L 4-Cylinder",
                Seats = "5",
                Color = "Silver",
                Mileage = "0",
                Features = new List<string>
                {
                    "Air Conditioning",
                    "Power Steering",
                    "ABS Brakes",
                    "Dual Airbags",
                    "Electric Windows",
                    "MP3 Player",
                    "Fabric Seats",
                    "Remote Central Locking"
                },
                MinDeposit = "96,750",
                MonthlyInstallment = "18,047",
                Description = "The Nissan Sunny offers reliability and practicality with its spacious interior and efficient performance. A perfect choice for families and business professionals seeking a dependable sedan."
            }
        },
        {
            "mercedes-c-class", new CarData
            {
                Name = "Mercedes-Benz C-Class 2025",
                Brand = "Mercedes-Benz",
                Year = "2025",
                Price = "1,250,000",
                MainImage = "/images/Mercedes-Benz.png",
                Images = new List<string>
                {
                    "/images/merc1.jpg",
                    "/images/merc2.jpg",
                    "/images/merc3.jpg",
                    "/images/Mercedes-Benz.png"
                },
                Transmission = "Automatic",
                FuelType = "Petrol",
                Engine = "2.0L Turbo 4-Cylinder",
                Seats = "5",
                Color = "Black",
                Mileage = "0",
                Features = new List<string>
                {
                    "Leather Seats",
                    "Panoramic Sunroof",
                    "Navigation System",
                    "Premium Sound System",
                    "Adaptive Cruise Control",
                    "LED Headlights",
                    "Wireless Charging",
                    "360° Camera",
                    "Heated Seats",
                    "Parking Assist"
                },
                MinDeposit = "187,500",
                MonthlyInstallment = "35,000",
                Description = "Experience luxury and performance with the Mercedes-Benz C-Class. Featuring cutting-edge technology, premium materials, and exceptional driving dynamics, this sedan represents the pinnacle of automotive excellence."
            }
        },
        {
            "bmw-x3", new CarData
            {
                Name = "BMW X3 xDrive 2025",
                Brand = "BMW",
                Year = "2025",
                Price = "1,450,000",
                MainImage = "/images/Angebotsbild-bmw-x3-20d-xdrive-q1-2025.png",
                Images = new List<string>
                {
                    "/images/bm1.jpg",
                    "/images/bm2.jpg",
                    "/images/bm3.jpg",
                    "/images/Angebotsbild-bmw-x3-20d-xdrive-q1-2025.png"
                },
                Transmission = "Automatic",
                FuelType = "Diesel",
                Engine = "2.0L Turbo Diesel",
                Seats = "5",
                Color = "Blue",
                Mileage = "0",
                Features = new List<string>
                {
                    "All-Wheel Drive (xDrive)",
                    "Premium Leather Interior",
                    "Digital Instrument Cluster",
                    "iDrive Infotainment",
                    "Adaptive LED Headlights",
                    "Harman Kardon Sound",
                    "Gesture Control",
                    "Head-Up Display",
                    "Ambient Lighting",
                    "Power Tailgate"
                },
                MinDeposit = "217,500",
                MonthlyInstallment = "40,600",
                Description = "The BMW X3 combines luxury SUV comfort with sporty performance. Its xDrive all-wheel-drive system ensures confident handling in all conditions, while premium features provide an exceptional driving experience."
            }
        },
        {
            "proton-saga", new CarData
            {
                Name = "Proton-Saga A/T Premium 2026",
                Brand = "Proton",
                Year = "2026",
                Price = "649,900",
                MainImage = "/images/Proton-saga.png",
                Images = new List<string>
                {
                    "/images/Proton-saga.png",
                    "/images/saga1.jpg",
                    "/images/saga2.jpg",
                    "/images/saga3.jpg",
                },
                Transmission = "Automatic",
                FuelType = "Petrol",
                Engine = "1.3L 4-Cylinder",
                Seats = "5",
                Color = "Red, White, Black",
                Mileage = "0",
                Features = new List<string>
                {
                    "Air Conditioning",
                    "Power Windows",
                    "ABS with EBD",
                    "Dual Airbags",
                    "Central Locking",
                    "Audio System",
                    "Fabric Seats",
                    "Rear Parking Sensors",
                    "Electric Mirrors"
                },
                MinDeposit = "97,485",
                MonthlyInstallment = "18,186",
                Description = "The Proton Saga offers exceptional value with its automatic transmission and premium features. Built for reliability and comfort, it's an ideal choice for those seeking an affordable yet well-equipped sedan."
            }
        },
        {
            "mg-zs", new CarData
            {
                Name = "MG ZS Standard 2025",
                Brand = "MG",
                Year = "2025",
                Price = "685,000",
                MainImage = "/images/MG-ZS-BLACK.png",
                Images = new List<string>
                {
                    "/images/mg1.jpg",
                    "/images/mg2.jpg",
                    "/images/mg3.jpg",
                    "/images/MG-ZS-BLACK.png"
                },
                Transmission = "Automatic",
                FuelType = "Hybrid",
                Engine = "1.5L Hybrid",
                Seats = "5",
                Color = "Black, Silver, Red",
                Mileage = "0",
                Features = new List<string>
                {
                    "Hybrid Technology",
                    "Touchscreen Infotainment",
                    "Apple CarPlay & Android Auto",
                    "Cruise Control",
                    "Rear Camera",
                    "LED Headlights",
                    "Automatic Climate Control",
                    "Keyless Entry",
                    "Push Start Button"
                },
                MinDeposit = "102,750",
                MonthlyInstallment = "19,167",
                Description = "The MG ZS combines modern hybrid technology with SUV versatility. Offering excellent fuel efficiency and advanced features, it's perfect for eco-conscious drivers who don't want to compromise on style or comfort."
            }
        },
        {
            "toyota-corolla", new CarData
            {
                Name = "Toyota Corolla GLi 2025",
                Brand = "Toyota",
                Year = "2025",
                Price = "785,000",
                MainImage = "/images/corolla.png",
                Images = new List<string>
                {
                    "/images/corolla.png",
                    "/images/toy1.jpg",
                    "/images/toy2.jpg",
                    "/images/toy3.jpg"
                },
                Transmission = "Automatic",
                FuelType = "Petrol",
                Engine = "1.6L 4-Cylinder",
                Seats = "5",
                Color = "White, Silver, Blue",
                Mileage = "0",
                Features = new List<string>
                {
                    "Air Conditioning",
                    "Power Windows & Mirrors",
                    "ABS with EBD",
                    "Multiple Airbags",
                    "Cruise Control",
                    "Touchscreen Display",
                    "Bluetooth Connectivity",
                    "Rear Camera",
                    "LED Daytime Running Lights"
                },
                MinDeposit = "117,750",
                MonthlyInstallment = "21,972",
                Description = "The Toyota Corolla GLi is renowned for its legendary reliability and fuel efficiency. With a spacious interior and comprehensive safety features, it's the perfect sedan for families and professionals alike."
            }
        },
        {
           "hyundai-tucson", new CarData
            {
                Name = "Hyundai Tucson 2025",
                Brand = "Hyundai",
                Year = "2025",
                Price = "895,000",
                MainImage = "/images/Tucson.png",
                Images = new List<string>
                {
                    "/images/tusc1.jpg",
                    "/images/tusc2.jpg",
                    "/images/tusc 3.jpg",
                    "/images/Tucson.png"
                },
                Transmission = "Automatic",
                FuelType = "Diesel",
                Engine = "2.0L Turbo Diesel",
                Seats = "5",
                Color = "Grey, White, Black",
                Mileage = "0",
                Features = new List<string>
                {
                    "Panoramic Sunroof",
                    "Leather Seats",
                    "Digital Instrument Cluster",
                    "Wireless Charging",
                    "360° Camera",
                    "Adaptive Cruise Control",
                    "Lane Keep Assist",
                    "Blind Spot Monitoring",
                    "Premium Sound System",
                    "Smart Tailgate"
                },
                MinDeposit = "134,250",
                MonthlyInstallment = "25,042",
                Description = "The Hyundai Tucson delivers a premium SUV experience with its striking design and advanced technology. Featuring a powerful diesel engine and luxurious interior, it's built for both city driving and long journeys."
            }
        },
        {
            "kia-sportage", new CarData
            {
                Name = "Kia Sportage LX 2025",
                Brand = "Kia",
                Year = "2025",
                Price = "920,000",
                MainImage = "/images/kia-sportage.jpg",
                Images = new List<string>
                {
                    "/images/kia1.jpg",
                    "/images/kia2.jpg",
                    "/images/kia3.jpg",
                    "/images/kia-sportage.jpg"
                },
                Transmission = "Automatic",
                FuelType = "Hybrid",
                Engine = "1.6L Turbo Hybrid",
                Seats = "5",
                Color = "Red, Silver, Black",
                Mileage = "0",
                Features = new List<string>
                {
                    "Hybrid Powertrain",
                    "Premium Interior",
                    "Digital Cockpit",
                    "Panoramic Sunroof",
                    "Wireless Phone Charging",
                    "Dual-Zone Climate Control",
                    "Smart Cruise Control",
                    "Forward Collision Avoidance",
                    "LED Headlights",
                    "Power Tailgate"
                },
                MinDeposit = "138,000",
                MonthlyInstallment = "25,744",
                Description = "The Kia Sportage LX offers a perfect blend of hybrid efficiency and SUV capability. With its bold design, cutting-edge technology, and eco-friendly performance, it stands out in the competitive SUV market."
            }
        },
        {
           "chery-tiggo7", new CarData
            {
                Name = "Chery Tiggo 7 Comfort 2025",
                Brand = "Chery",
                Year = "2025",
                Price = "740,000",
                MainImage = "/images/Chery.png",
                Images = new List<string>
                {
                    "/images/tig1.jpg",
                    "/images/tig2.jpg",
                    "/images/tig3.jpg",
                    "/images/Chery.png"
                },
                Transmission = "Automatic",
                FuelType = "Petrol",
                Engine = "1.5L Turbo",
                Seats = "5",
                Color = "Blue, White, Grey",
                Mileage = "0",
                Features = new List<string>
                {
                    "Turbocharged Engine",
                    "Touchscreen Infotainment",
                    "Apple CarPlay",
                    "Rear Camera",
                    "Cruise Control",
                    "Leather Seats",
                    "Sunroof",
                    "Automatic Climate Control",
                    "Electric Parking Brake"
                },
                MinDeposit = "111,000",
                MonthlyInstallment = "20,711",
                Description = "The Chery Tiggo 7 Comfort combines affordability with modern features. Its turbocharged engine provides spirited performance, while the spacious cabin ensures comfort for all passengers on every journey."
            }
        },
        {
            "renault-duster", new CarData
            {
                Name = "Renault Duster 2025",
                Brand = "Renault",
                Year = "2025",
                Price = "670,000",
                MainImage = "/images/Renault-duster.jpg",
                Images = new List<string>
                {
                    "/images/ren1.jpg",
                    "/images/ren2.jpg",
                    "/images/ren3.jpg",
                    "/images/Renault-duster.png"
                },
                Transmission = "Automatic",
                FuelType = "Petrol",
                Engine = "1.6L 4-Cylinder",
                Seats = "5",
                Color = "Orange, White, Grey",
                Mileage = "0",
                Features = new List<string>
                {
                    "High Ground Clearance",
                    "Air Conditioning",
                    "ABS with EBD",
                    "Dual Airbags",
                    "Touchscreen System",
                    "Rear Parking Sensors",
                    "Roof Rails",
                    "Fabric Seats",
                    "Electric Windows"
                },
                MinDeposit = "100,500",
                MonthlyInstallment = "18,750",
                Description = "The Renault Duster is built for adventure with its rugged design and high ground clearance. Perfect for both urban commuting and off-road adventures, it offers versatility and reliability at an attractive price point."
            }
        },
        {
           "citroen-c3", new CarData
            {
                Name = "Citroen C3 2025",
                Brand = "Citroen",
                Year = "2025",
                Price = "830,000",
                MainImage = "/images/Cirtoen-c3.jpeg",
                Images = new List<string>
                {
                    "/images/cit1.jpg",
                    "/images/cit2.jpg",
                    "/images/cit3.jpg",
                    "/images/Cirtoen-c3.jpeg"
                },
                Transmission = "Automatic",
                FuelType = "Hybrid",
                Engine = "1.2L PureTech Hybrid",
                Seats = "5",
                Color = "Yellow, White, Red",
                Mileage = "0",
                Features = new List<string>
                {
                    "Hybrid Technology",
                    "Unique French Design",
                    "Touchscreen Infotainment",
                    "Reverse Camera",
                    "Automatic Climate Control",
                    "LED Headlights",
                    "Cruise Control",
                    "Keyless Entry",
                    "Electric Mirrors",
                    "USB Connectivity"
                },
                MinDeposit = "124,500",
                MonthlyInstallment = "23,233",
                Description = "The Citroen C3 brings French flair and hybrid efficiency together in a stylish package. With its distinctive design, comfortable ride, and eco-friendly technology, it offers a unique driving experience in the compact car segment."
            }
        }
    };

            return cars.ContainsKey(carId) ? cars[carId] : null;
        }
    }

    // Helper class for car data
    public class CarData
    {
        public string Name { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Year { get; set; } = "";
        public string Price { get; set; } = "";
        public string MainImage { get; set; } = "";
        public List<string> Images { get; set; } = new List<string>();
        public string Transmission { get; set; } = "";
        public string FuelType { get; set; } = "";
        public string Engine { get; set; } = "";
        public string Seats { get; set; } = "";
        public string Color { get; set; } = "";
        public string Mileage { get; set; } = "";
        public List<string> Features { get; set; } = new List<string>();
        public string MinDeposit { get; set; } = "";
        public string MonthlyInstallment { get; set; } = "";
        public string Description { get; set; } = "";
    }
}