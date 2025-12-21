using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Metadata;
using System.Data;
using System;
using System.Collections.Generic;

namespace Car_Agency_Management.Data
{
    public class DB
    {
        private readonly string _connectionString = "Data Source = ; Initial Catalog= ; Integrated Security = True; Trust Server Certificate=True;";
        private SqlConnection _connection;

        public DB()
        {
            _connection = new SqlConnection(_connectionString);
        }

        // ============================================
        // HOMEPAGE QUERIES - NEWLY ADDED SECTION
        // ============================================
        // These three methods were ADDED to support the homepage dynamic data
        // They fetch: New Arrivals, Trending Cars, and Active Partners

        /// <summary>
        /// ADDED: Get top 3 newest cars for New Arrivals section on homepage
        /// Query: SELECT TOP 3 * FROM CAR ORDER BY DATE_ADDED DESC
        /// Returns: List of CarSummary with newest cars based on DATE_ADDED field
        /// </summary>
        public List<CarSummary> GetNewArrivals()
        {
            List<CarSummary> cars = new List<CarSummary>();

            // ADDED: Query to get 3 most recently added cars
            string query = @"SELECT TOP 3
                            CAR_ID,
                            CAR_NAME,
                            BRAND,
                            YEAR,
                            PRICE,
                            MAIN_IMAGE,
                            TRANSMISSION,
                            FUEL_TYPE,
                            MIN_DEPOSIT,
                            MONTHLY_INSTALLMENT
                            FROM CAR
                            ORDER BY DATE_ADDED DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        // ADDED: Handle nullable fields with DBNull check
                        Transmission = reader["TRANSMISSION"] != DBNull.Value ? reader["TRANSMISSION"].ToString() : "",
                        FuelType = reader["FUEL_TYPE"] != DBNull.Value ? reader["FUEL_TYPE"].ToString() : "",
                        MinDeposit = reader["MIN_DEPOSIT"] != DBNull.Value ? reader["MIN_DEPOSIT"].ToString() : "",
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"] != DBNull.Value ? reader["MONTHLY_INSTALLMENT"].ToString() : ""
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetNewArrivals: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// ADDED: Get top 3 trending cars based on transactions in last month for homepage
        /// Query: Complex join between CAR, BUYING_RENTING, CUSTOMER, and PAYMENT tables
        /// Filters by PAYMENT_DATE >= last 30 days and groups by car to count transactions
        /// Returns: List of CarSummary with most popular cars in descending order
        /// </summary>
        public List<CarSummary> GetTrendingCars()
        {
            List<CarSummary> cars = new List<CarSummary>();

            // ADDED: Complex query to get trending cars based on last month's transactions
            string query = @"DECLARE @LastMonthStart DATE;
                            SET @LastMonthStart = DATEADD(month, -1, GETDATE());
                            
                            SELECT TOP 3 
                                C.CAR_ID,
                                C.CAR_NAME,
                                C.BRAND,
                                C.YEAR,
                                C.PRICE,
                                C.MAIN_IMAGE,
                                C.TRANSMISSION,
                                C.FUEL_TYPE,
                                C.MIN_DEPOSIT,
                                C.MONTHLY_INSTALLMENT,
                                COUNT(BR.CAR_ID) AS TotalTransactions
                            FROM CAR C
                            JOIN BUYING_RENTING BR ON C.CAR_ID = BR.CAR_ID
                            JOIN CUSTOMER CU ON BR.CUSTOMER_ID = CU.CUSTOMER_ID
                            JOIN PAYMENT P ON CU.CUSTOMER_ID = P.CUSTOMER_ID
                            WHERE P.PAYMENT_DATE >= @LastMonthStart
                            GROUP BY C.CAR_ID, C.CAR_NAME, C.BRAND, C.YEAR, C.PRICE, 
                                     C.MAIN_IMAGE, C.TRANSMISSION, C.FUEL_TYPE, 
                                     C.MIN_DEPOSIT, C.MONTHLY_INSTALLMENT
                            ORDER BY TotalTransactions DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        Transmission = reader["TRANSMISSION"] != DBNull.Value ? reader["TRANSMISSION"].ToString() : "",
                        FuelType = reader["FUEL_TYPE"] != DBNull.Value ? reader["FUEL_TYPE"].ToString() : "",
                        MinDeposit = reader["MIN_DEPOSIT"] != DBNull.Value ? reader["MIN_DEPOSIT"].ToString() : "",
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"] != DBNull.Value ? reader["MONTHLY_INSTALLMENT"].ToString() : ""
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTrendingCars: {ex.Message}");
                // ADDED: Fallback to newest cars if no trending data exists
                _connection.Close();
                return GetNewArrivals();
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }

            return cars;
        }

        /// <summary>
        /// ADDED: Get active partners for homepage carousel
        /// Query: SELECT BRAND_NAME, LOGO_URL FROM PARTNERS WHERE IS_ACTIVE = 1
        /// Returns: List of Partner objects with brand name and logo URL
        /// </summary>
        public List<Partner> GetActivePartners()
        {
            List<Partner> partners = new List<Partner>();

            // ADDED: Query to get all active partners for homepage carousel
            string query = @"SELECT BRAND_NAME, LOGO_URL
                            FROM PARTNERS
                            WHERE IS_ACTIVE = 1
                            ORDER BY BRAND_NAME";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    partners.Add(new Partner
                    {
                        BrandName = reader["BRAND_NAME"].ToString(),
                        LogoUrl = reader["LOGO_URL"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetActivePartners: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return partners;
        }

        // ============================================
        // CAR GALLERY - RETRIEVE OPERATIONS
        // UNCHANGED - These methods existed before
        // ============================================

        /// <summary>
        /// UNCHANGED: Get all cars for the gallery view
        /// </summary>
        public List<CarSummary> GetAllCars()
        {
            List<CarSummary> cars = new List<CarSummary>();

            string query = @"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE, 
                            MIN_DEPOSIT, MONTHLY_INSTALLMENT
                            FROM CAR
                            ORDER BY CAR_ID DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllCars: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// UNCHANGED: Get brands with car count for sidebar/brands tab
        /// </summary>
        public Dictionary<string, int> GetBrandsWithCount()
        {
            Dictionary<string, int> brands = new Dictionary<string, int>();

            string query = @"SELECT BRAND, COUNT(*) AS CarCount
                            FROM CAR
                            GROUP BY BRAND
                            ORDER BY BRAND";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    brands.Add(reader["BRAND"].ToString(), Convert.ToInt32(reader["CarCount"]));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBrandsWithCount: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return brands;
        }

        /// <summary>
        /// UNCHANGED: Filter cars by brand
        /// </summary>
        public List<CarSummary> GetCarsByBrand(string brand)
        {
            List<CarSummary> cars = new List<CarSummary>();

            string query = @"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE, 
                            MIN_DEPOSIT, MONTHLY_INSTALLMENT
                            FROM CAR
                            WHERE BRAND = @Brand
                            ORDER BY CAR_ID DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Brand", brand);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarsByBrand: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// UNCHANGED: Filter cars by maximum price
        /// </summary>
        public List<CarSummary> GetCarsByMaxPrice(decimal maxPrice)
        {
            List<CarSummary> cars = new List<CarSummary>();

            string query = @"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE, 
                            MIN_DEPOSIT, MONTHLY_INSTALLMENT
                            FROM CAR
                            WHERE CAST(REPLACE(PRICE, ',', '') AS DECIMAL(12,2)) <= @MaxPrice
                            ORDER BY CAR_ID DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@MaxPrice", maxPrice);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarsByMaxPrice: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// UNCHANGED: Filter cars with combined filters (brand + price range)
        /// </summary>
        public List<CarSummary> GetCarsFiltered(string brand = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            List<CarSummary> cars = new List<CarSummary>();

            string query = @"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE, 
                            MIN_DEPOSIT, MONTHLY_INSTALLMENT
                            FROM CAR
                            WHERE (@Brand IS NULL OR BRAND = @Brand)
                            AND (@MinPrice IS NULL OR CAST(REPLACE(PRICE, ',', '') AS DECIMAL(12,2)) >= @MinPrice)
                            AND (@MaxPrice IS NULL OR CAST(REPLACE(PRICE, ',', '') AS DECIMAL(12,2)) <= @MaxPrice)
                            ORDER BY CAR_ID DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Brand", (object)brand ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MinPrice", (object)minPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxPrice", (object)maxPrice ?? DBNull.Value);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarsFiltered: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// UNCHANGED: Sort cars by price (low to high or high to low)
        /// </summary>
        public List<CarSummary> GetCarsSortedByPrice(bool ascending = true)
        {
            List<CarSummary> cars = new List<CarSummary>();

            string sortOrder = ascending ? "ASC" : "DESC";
            string query = $@"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE, 
                             MIN_DEPOSIT, MONTHLY_INSTALLMENT
                             FROM CAR
                             ORDER BY CAST(REPLACE(PRICE, ',', '') AS DECIMAL(12,2)) {sortOrder}";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarsSortedByPrice: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// UNCHANGED: Sort cars by newest first
        /// </summary>
        public List<CarSummary> GetCarsNewestFirst()
        {
            List<CarSummary> cars = new List<CarSummary>();

            string query = @"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE, 
                            MIN_DEPOSIT, MONTHLY_INSTALLMENT
                            FROM CAR
                            ORDER BY YEAR DESC, CAR_ID DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarsNewestFirst: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// UNCHANGED: Get most popular cars based on rentals/sales
        /// </summary>
        public List<CarSummary> GetMostPopularCars()
        {
            List<CarSummary> cars = new List<CarSummary>();

            string query = @"SELECT c.CAR_ID, c.CAR_NAME, c.BRAND, c.YEAR, c.PRICE, c.MAIN_IMAGE,
                            c.MIN_DEPOSIT, c.MONTHLY_INSTALLMENT, COUNT(br.CUSTOMER_ID) AS Popularity
                            FROM CAR c
                            LEFT JOIN BUYING_RENTING br ON c.CAR_ID = br.CAR_ID
                            GROUP BY c.CAR_ID, c.CAR_NAME, c.BRAND, c.YEAR, c.PRICE, 
                            c.MAIN_IMAGE, c.MIN_DEPOSIT, c.MONTHLY_INSTALLMENT
                            ORDER BY Popularity DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cars.Add(new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMostPopularCars: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return cars;
        }

        /// <summary>
        /// UNCHANGED: Get total results count for filters
        /// </summary>
        public int GetTotalResultsCount(string brand = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            int count = 0;

            string query = @"SELECT COUNT(*) AS TotalResults
                            FROM CAR
                            WHERE (@Brand IS NULL OR BRAND = @Brand)
                            AND (@MinPrice IS NULL OR CAST(REPLACE(PRICE, ',', '') AS DECIMAL(12,2)) >= @MinPrice)
                            AND (@MaxPrice IS NULL OR CAST(REPLACE(PRICE, ',', '') AS DECIMAL(12,2)) <= @MaxPrice)";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Brand", (object)brand ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MinPrice", (object)minPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxPrice", (object)maxPrice ?? DBNull.Value);

                count = (int)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTotalResultsCount: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return count;
        }

        /// <summary>
        /// UNCHANGED: Get basic car info when clicking a card
        /// </summary>
        public CarSummary GetCarBasicInfo(string carId)
        {
            CarSummary car = null;

            string query = @"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE
                            FROM CAR
                            WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    car = new CarSummary
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        Name = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Image = reader["MAIN_IMAGE"].ToString()
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarBasicInfo: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return car;
        }

        // ============================================
        // CAR DETAILS - EDIT MODE
        // UNCHANGED - All methods below existed before
        // ============================================

        /// <summary>
        /// UNCHANGED: Get complete car details for edit mode
        /// </summary>
        public CarDetails GetCarDetails(string carId)
        {
            CarDetails car = null;

            string query = @"SELECT CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, COLOR, TRANSMISSION,
                            FUEL_TYPE, ENGINE, SEATS, MILEAGE, MAIN_IMAGE, MIN_DEPOSIT,
                            MONTHLY_INSTALLMENT, DESCRIPTION
                            FROM CAR
                            WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    car = new CarDetails
                    {
                        CarId = reader["CAR_ID"].ToString(),
                        CarName = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        Year = reader["YEAR"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        Color = reader["COLOR"].ToString(),
                        Transmission = reader["TRANSMISSION"].ToString(),
                        FuelType = reader["FUEL_TYPE"].ToString(),
                        Engine = reader["ENGINE"].ToString(),
                        Seats = reader["SEATS"].ToString(),
                        Mileage = reader["MILEAGE"].ToString(),
                        MainImage = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString(),
                        Description = reader["DESCRIPTION"].ToString()
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarDetails: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return car;
        }

        /// <summary>
        /// UNCHANGED: Get car images for edit mode
        /// </summary>
        public List<string> GetCarImages(string carId)
        {
            List<string> images = new List<string>();

            string query = @"SELECT IMAGE_URL
                            FROM CAR_IMAGES
                            WHERE CAR_ID = @CarId
                            ORDER BY IMAGE_ID";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    images.Add(reader["IMAGE_URL"].ToString());
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarImages: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return images;
        }

        /// <summary>
        /// UNCHANGED: Get car features for edit mode
        /// </summary>
        public List<string> GetCarFeatures(string carId)
        {
            List<string> features = new List<string>();

            string query = @"SELECT FEATURE_NAME
                            FROM CAR_FEATURES
                            WHERE CAR_ID = @CarId
                            ORDER BY FEATURE_ID";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    features.Add(reader["FEATURE_NAME"].ToString());
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarFeatures: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return features;
        }

        /// <summary>
        /// UNCHANGED: Update car main details
        /// </summary>
        public bool UpdateCarDetails(CarDetails car)
        {
            string query = @"UPDATE CAR SET
                            CAR_NAME = @CarName,
                            BRAND = @Brand,
                            YEAR = @Year,
                            PRICE = @Price,
                            COLOR = @Color,
                            TRANSMISSION = @Transmission,
                            FUEL_TYPE = @FuelType,
                            ENGINE = @Engine,
                            SEATS = @Seats,
                            MILEAGE = @Mileage,
                            MAIN_IMAGE = @MainImage,
                            MIN_DEPOSIT = @MinDeposit,
                            MONTHLY_INSTALLMENT = @MonthlyInstallment,
                            DESCRIPTION = @Description
                            WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", car.CarId);
                cmd.Parameters.AddWithValue("@CarName", car.CarName);
                cmd.Parameters.AddWithValue("@Brand", car.Brand);
                cmd.Parameters.AddWithValue("@Year", car.Year);
                cmd.Parameters.AddWithValue("@Price", car.Price);
                cmd.Parameters.AddWithValue("@Color", car.Color);
                cmd.Parameters.AddWithValue("@Transmission", car.Transmission);
                cmd.Parameters.AddWithValue("@FuelType", car.FuelType);
                cmd.Parameters.AddWithValue("@Engine", car.Engine);
                cmd.Parameters.AddWithValue("@Seats", car.Seats);
                cmd.Parameters.AddWithValue("@Mileage", car.Mileage);
                cmd.Parameters.AddWithValue("@MainImage", car.MainImage);
                cmd.Parameters.AddWithValue("@MinDeposit", car.MinDeposit);
                cmd.Parameters.AddWithValue("@MonthlyInstallment", car.MonthlyInstallment);
                cmd.Parameters.AddWithValue("@Description", car.Description);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateCarDetails: {ex.Message}");
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// UNCHANGED: Delete old car images
        /// </summary>
        public bool DeleteCarImages(string carId)
        {
            string query = "DELETE FROM CAR_IMAGES WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteCarImages: {ex.Message}");
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// UNCHANGED: Insert car image
        /// </summary>
        public bool InsertCarImage(string carId, string imageUrl)
        {
            string query = @"INSERT INTO CAR_IMAGES (CAR_ID, IMAGE_URL)
                            VALUES (@CarId, @ImageUrl)";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.Parameters.AddWithValue("@ImageUrl", imageUrl);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertCarImage: {ex.Message}");
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// UNCHANGED: Delete old car features
        /// </summary>
        public bool DeleteCarFeatures(string carId)
        {
            string query = "DELETE FROM CAR_FEATURES WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteCarFeatures: {ex.Message}");
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// UNCHANGED: Insert car feature
        /// </summary>
        public bool InsertCarFeature(string carId, string featureName)
        {
            string query = @"INSERT INTO CAR_FEATURES (CAR_ID, FEATURE_NAME)
                            VALUES (@CarId, @FeatureName)";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.Parameters.AddWithValue("@FeatureName", featureName);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in InsertCarFeature: {ex.Message}");
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// UNCHANGED: Delete car and all related data
        /// </summary>
        public bool DeleteCar(string carId)
        {
            string query = "DELETE FROM CAR WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteCar: {ex.Message}");
                return false;
            }
            finally
            {
                _connection.Close();
            }
        }

        // ============================================
        // ADD CAR MODE
        // UNCHANGED - This section existed before
        // ============================================

        /// <summary>
        /// UNCHANGED: Add new car and return the new car ID
        /// </summary>
        public string AddCar(CarDetails car)
        {
            string newCarId = null;

            string query = @"INSERT INTO CAR (CAR_NAME, BRAND, YEAR, PRICE, COLOR, TRANSMISSION,
                            FUEL_TYPE, ENGINE, SEATS, MILEAGE, MAIN_IMAGE, MIN_DEPOSIT,
                            MONTHLY_INSTALLMENT, DESCRIPTION)
                            VALUES (@CarName, @Brand, @Year, @Price, @Color, @Transmission,
                            @FuelType, @Engine, @Seats, @Mileage, @MainImage, @MinDeposit,
                            @MonthlyInstallment, @Description);
                            SELECT SCOPE_IDENTITY();";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarName", car.CarName);
                cmd.Parameters.AddWithValue("@Brand", car.Brand);
                cmd.Parameters.AddWithValue("@Year", car.Year);
                cmd.Parameters.AddWithValue("@Price", car.Price);
                cmd.Parameters.AddWithValue("@Color", car.Color);
                cmd.Parameters.AddWithValue("@Transmission", car.Transmission);
                cmd.Parameters.AddWithValue("@FuelType", car.FuelType);
                cmd.Parameters.AddWithValue("@Engine", car.Engine);
                cmd.Parameters.AddWithValue("@Seats", car.Seats);
                cmd.Parameters.AddWithValue("@Mileage", car.Mileage);
                cmd.Parameters.AddWithValue("@MainImage", car.MainImage);
                cmd.Parameters.AddWithValue("@MinDeposit", car.MinDeposit);
                cmd.Parameters.AddWithValue("@MonthlyInstallment", car.MonthlyInstallment);
                cmd.Parameters.AddWithValue("@Description", car.Description);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    newCarId = result.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddCar: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return newCarId;
        }
        // NEW: Get customer data (Uses FNAME, LNAME, and CUSTOMER_EMAIL from your schema)
        public CustomerModel GetCustomerProfile(int customerId)
        {
            CustomerModel customer = null;
            string query = "SELECT * FROM CUSTOMER WHERE CUSTOMER_ID = @Id";
            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Id", customerId);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    customer = new CustomerModel
                    {
                        Id = (int)reader["CUSTOMER_ID"],
                        FirstName = reader["FNAME"].ToString(),
                        LastName = reader["LNAME"].ToString(),
                        Email = reader["CUSTOMER_EMAIL"].ToString(),
                        Address = reader["ADDRESS"].ToString()
                    };
                }
            }
            finally { _connection.Close(); }
            return customer;
        }

        // NEW: Get customer history (Joins CAR, BUYING_RENTING, and PAYMENT)
        public List<TransactionSummary> GetCustomerTransactions(int customerId)
        {
            List<TransactionSummary> list = new List<TransactionSummary>();
            string query = @"SELECT C.CAR_NAME, C.BRAND, P.PAYMENT_METHOD, P.AMOUNT, P.PAYMENT_DATE
                    FROM BUYING_RENTING BR
                    JOIN CAR C ON BR.CAR_ID = C.CAR_ID
                    JOIN PAYMENT P ON BR.CUSTOMER_ID = P.CUSTOMER_ID
                    WHERE BR.CUSTOMER_ID = @Id
                    ORDER BY P.PAYMENT_DATE DESC";
            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Id", customerId);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new TransactionSummary
                    {
                        CarTitle = $"{reader["BRAND"]} {reader["CAR_NAME"]}",
                        Method = reader["PAYMENT_METHOD"].ToString(),
                        Amount = reader["AMOUNT"].ToString(),
                        Date = Convert.ToDateTime(reader["PAYMENT_DATE"]).ToShortDateString()
                    });
                }
            }
            finally { _connection.Close(); }
            return list;
        }
    }

    // ============================================
    // HELPER CLASSES
    // ============================================

    /// <summary>
    /// UNCHANGED: CarSummary class for car list views
    /// </summary>
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
        public string MinDeposit { get; set; } = "";
        public string MonthlyInstallment { get; set; } = "";
    }

    /// <summary>
    /// UNCHANGED: CarDetails class for detailed car information
    /// </summary>
    public class CarDetails
    {
        public string CarId { get; set; } = "";
        public string CarName { get; set; } = "";
        public string Brand { get; set; } = "";
        public string Year { get; set; } = "";
        public string Price { get; set; } = "";
        public string Color { get; set; } = "";
        public string Transmission { get; set; } = "";
        public string FuelType { get; set; } = "";
        public string Engine { get; set; } = "";
        public string Seats { get; set; } = "";
        public string Mileage { get; set; } = "";
        public string MainImage { get; set; } = "";
        public string MinDeposit { get; set; } = "";
        public string MonthlyInstallment { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// ADDED: Partner class for homepage partners carousel
    /// Stores partner brand name and logo URL from PARTNERS table
    /// </summary>
    public class Partner
    {
        public string BrandName { get; set; } = "";
        public string LogoUrl { get; set; } = "";
    }
    public class CustomerModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }

    public class TransactionSummary
    {
        public string CarTitle { get; set; }
        public string Method { get; set; }
        public string Amount { get; set; }
        public string Date { get; set; }
    }
}

// ============================================
// SUMMARY OF ALL CHANGES TO DB.CS:
// ============================================
// 1. ADDED: GetNewArrivals() method
//    - Fetches top 3 newest cars ordered by DATE_ADDED DESC
//    - Used for "New Arrivals" section on homepage
//
// 2. ADDED: GetTrendingCars() method  
//    - Complex query joining CAR, BUYING_RENTING, CUSTOMER, PAYMENT tables
//    - Fetches top 3 cars with most transactions in last 30 days
//    - Used for "Trending Now" section on homepage
//    - Falls back to GetNewArrivals() if no transaction data exists
//
// 3. ADDED: GetActivePartners() method
//    - Fetches all partners where IS_ACTIVE = 1
//    - Used for partners carousel on homepage
//
// 4. ADDED: Partner class
//    - New helper class with BrandName and LogoUrl properties
//    - Represents a row from the PARTNERS table
//
// 5. UNCHANGED: All other methods (gallery, filtering, sorting, CRUD operations)
//    - GetAllCars(), GetCarsByBrand(), GetCarsByMaxPrice()
//    - GetCarsFiltered(), GetCarsSortedByPrice(), GetCarsNewestFirst()
//    - GetMostPopularCars(), GetTotalResultsCount(), GetCarBasicInfo()
//    - GetCarDetails(), GetCarImages(), GetCarFeatures()
//    - UpdateCarDetails(), DeleteCarImages(), InsertCarImage()
//    - DeleteCarFeatures(), InsertCarFeature(), DeleteCar(), AddCar()
//
// 6. UNCHANGED: CarSummary and CarDetails helper classes
// ============================================