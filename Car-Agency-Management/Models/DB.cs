using Microsoft.Data.SqlClient;
using System.Data.SqlTypes;
using System.Reflection.Metadata;
using System.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Car_Agency_Management.Data
{
    public class DB
    {
        private readonly string _connectionString = "Data Source = AMR; Initial Catalog= Car_agency ; Integrated Security = True; Trust Server Certificate=True;";
        private SqlConnection _connection;

        public DB()
        {
            _connection = new SqlConnection(_connectionString);
        }

        // ============================================
        // CAR GALLERY - RETRIEVE OPERATIONS
        // ============================================

        /// <summary>
        /// Get all cars for the gallery view
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
        /// Get brands with car count for sidebar/brands tab
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
        /// Filter cars by brand
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
        /// Filter cars by maximum price
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
        /// Filter cars with combined filters (brand + price range)
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
        /// Sort cars by price (low to high or high to low)
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
        /// Sort cars by newest first
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
        /// Get most popular cars based on rentals/sales
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
        /// Get total results count for filters
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
        /// Get basic car info when clicking a card
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
        // ============================================

        /// <summary>
        /// Get complete car details for edit mode
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
        /// Get car images for edit mode
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
        /// Get car features for edit mode
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
        /// Update car main details
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
        /// Delete old car images
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
        /// Insert car image
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
        /// Delete old car features
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
        /// Insert car feature
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
        /// Delete car and all related data
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
        // ============================================

        /// <summary>
        /// Add new car and return the new car ID
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
    }

    // ============================================
    // HELPER CLASSES
    // ============================================

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
}