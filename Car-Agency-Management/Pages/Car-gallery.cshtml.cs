using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Car_Agency_Management.Data;
using System.Collections.Generic;

namespace Car_Agency_Management.Pages
{
    public class Car_galleryModel : PageModel
    {
        private readonly DB _db;

        public List<CarSummary> Cars { get; set; } = new List<CarSummary>();
        public Dictionary<string, int> BrandsWithCount { get; set; } = new Dictionary<string, int>();
        public int TotalResults { get; set; }

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string SelectedBrand { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "default";

        public Car_galleryModel()
        {
            _db = new DB();
        }

        public void OnGet()
        {
            LoadCars();
            LoadBrands();
        }

        private void LoadCars()
        {
            // Apply filters and sorting based on query parameters
            if (!string.IsNullOrEmpty(SelectedBrand) || MaxPrice.HasValue)
            {
                // Apply filters
                Cars = _db.GetCarsFiltered(SelectedBrand, null, MaxPrice);
            }
            else if (SortBy == "price-low")
            {
                Cars = _db.GetCarsSortedByPrice(ascending: true);
            }
            else if (SortBy == "price-high")
            {
                Cars = _db.GetCarsSortedByPrice(ascending: false);
            }
            else if (SortBy == "newest")
            {
                Cars = _db.GetCarsNewestFirst();
            }
            else if (SortBy == "popular")
            {
                Cars = _db.GetMostPopularCars();
            }
            else
            {
                // Default: Get all cars
                Cars = _db.GetAllCars();
            }

            // Get total results count
            TotalResults = Cars.Count;
        }

        private void LoadBrands()
        {
            BrandsWithCount = _db.GetBrandsWithCount();
        }

        // Handler for brand filter
        public IActionResult OnGetFilterBrand(string brand)
        {
            SelectedBrand = brand;
            LoadCars();
            LoadBrands();
            return Page();
        }

        // Handler for price filter
        public IActionResult OnGetFilterPrice(decimal maxPrice)
        {
            MaxPrice = maxPrice;
            LoadCars();
            LoadBrands();
            return Page();
        }

        // Handler for combined filters
        public IActionResult OnGetFilter(string brand, decimal? maxPrice, string sortBy)
        {
            SelectedBrand = brand;
            MaxPrice = maxPrice;
            SortBy = sortBy ?? "default";
            LoadCars();
            LoadBrands();
            return Page();
        }

        // Handler for sorting
        public IActionResult OnGetSort(string sortBy)
        {
            SortBy = sortBy;
            LoadCars();
            LoadBrands();
            return Page();
        }

        // Handler for clearing filters
        public IActionResult OnGetClearFilters()
        {
            SelectedBrand = null;
            MaxPrice = null;
            SortBy = "default";
            LoadCars();
            LoadBrands();
            return Page();
        }
    }
}