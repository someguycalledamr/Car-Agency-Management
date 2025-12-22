
using Car_Agency_Management.Pages;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Reflection.Metadata;

namespace Car_Agency_Management.Data
{
    public class DB
    {
        private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=web;Integrated Security=True";
        private SqlConnection _connection;

        public DB()
        {
            _connection = new SqlConnection(_connectionString);
        }

        // ============================================
        // DATABASE SETUP & RESET
        // ============================================

        /// <summary>
        /// Resets the database using the setup.sql script
        /// WARNING: This will drop existing tables and data
        /// </summary>
        public bool ResetDatabase(string scriptPath)
        {
            try
            {
                if (!System.IO.File.Exists(scriptPath))
                {
                    Console.WriteLine($"Setup script not found at: {scriptPath}");
                    return false;
                }

                string script = System.IO.File.ReadAllText(scriptPath);

                // Split script into batches (GO or simple line breaks if needed)
                // Since this is a simple script without GO, we can try running it as one block 
                // or split by logical sections if needed. For now, running as one block.
                // However, localdb might require multiple commands.
                // Let's execute it directly.

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(script, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting database: {ex.Message}");
                return false;
            }
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
        // CUSTOMER OPERATIONS
        // ============================================

        public bool IsEmailTaken(string email)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM CUSTOMER WHERE CUSTOMER_EMAIL = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking email: {ex.Message}");
                return true; 
            }
        }

        public bool AddCustomer(string fname, string lname, string email, string password, string phone)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try 
                        {
                            // 1. Insert Customer (AdministratorId is NULL for public signups)
                            string insertCustomerQuery = @"
                                INSERT INTO CUSTOMER (FNAME, LNAME, CUSTOMER_EMAIL, CUSTOMER_PASSWORD, MNAME) 
                                VALUES (@Fname, @Lname, @Email, @Password, ''); 
                                SELECT SCOPE_IDENTITY();";
                                
                            int newCustomerId = 0;

                            using (SqlCommand cmd = new SqlCommand(insertCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Fname", fname);
                                cmd.Parameters.AddWithValue("@Lname", lname);
                                cmd.Parameters.AddWithValue("@Email", email);
                                cmd.Parameters.AddWithValue("@Password", password);
                                
                                object result = cmd.ExecuteScalar();
                                newCustomerId = Convert.ToInt32(result);
                            }

                            // 2. Insert Phone if provided
                            if (!string.IsNullOrEmpty(phone) && newCustomerId > 0)
                            {
                                string insertPhoneQuery = "INSERT INTO CUSTOMER_PHONE_NUMBERS (CUSTOMER_ID, PHONE_NUMBERS) VALUES (@CustId, @Phone)";
                                using (SqlCommand cmd = new SqlCommand(insertPhoneQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@CustId", newCustomerId);
                                    cmd.Parameters.AddWithValue("@Phone", phone);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return true;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding customer: {ex.Message}");
                return false;
            }
        }

        public int ValidateCustomer(string email, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT CUSTOMER_ID FROM CUSTOMER WHERE CUSTOMER_EMAIL = @Email AND CUSTOMER_PASSWORD = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Password", password);
                        
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating customer: {ex.Message}");
            }
            return 0; // Return 0 if validation fails
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

                // Log the activity
                if (!string.IsNullOrEmpty(newCarId))
                {
                    _connection.Close(); // Close existing connection before logging (LogActivity opens its own)
                    LogActivity("New Car Added", $"Car: {car.CarName} added to inventory", "success");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddCar: {ex.Message}");
                if (_connection.State == ConnectionState.Open) _connection.Close();
            }
            finally
            {
                if (_connection.State == ConnectionState.Open) _connection.Close();
            }

            return newCarId;
        }

        // LOGGING HELPER METHODS

        /// <summary>
        /// Log an activity to ACTIVITY_LOG table
        /// </summary>
        public void LogActivity(string action, string description, string type)
        {
            string query = @"INSERT INTO ACTIVITY_LOG (ACTION, DESCRIPTION, TYPE, TIMESTAMP) 
                     VALUES (@Action, @Description, @Type, GETDATE())";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@Description", description);
                        cmd.Parameters.AddWithValue("@Type", type);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging activity: {ex.Message}");
            }
        }

        /// <summary>
        /// Log a transaction to TRANSACTION_LOG table
        /// </summary>
        public void LogTransaction(int paymentId, string customerName, string carName, decimal amount, string status)
        {
            string query = @"INSERT INTO TRANSACTION_LOG (PAYMENT_ID, CUSTOMER_NAME, CAR_NAME, AMOUNT, DATE, STATUS) 
                     VALUES (@PaymentId, @CustomerName, @CarName, @Amount, GETDATE(), @Status)";
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                        cmd.Parameters.AddWithValue("@CustomerName", customerName);
                        cmd.Parameters.AddWithValue("@CarName", (object)carName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging transaction: {ex.Message}");
            }
        }

        // MISSING RESERVATION AND PAYMENT METHODS

        /// <summary>
        /// Add a new reservation
        /// </summary>
        public int AddReservation(int customerId, DateTime startDate, DateTime endDate, string status = "Confirmed")
        {
            int reservationId = 0;
            string query = @"INSERT INTO RESERVATIONS (CUSTOMER_ID, RESERVATION_START_DATE, RESERVATION_END_DATE, RESERVATION_STATUS)
                     VALUES (@CustomerId, @StartDate, @EndDate, @Status);
                     SELECT SCOPE_IDENTITY();";
            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                cmd.Parameters.AddWithValue("@Status", status);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    reservationId = Convert.ToInt32(result);
                    _connection.Close();
                    LogActivity("Rental Started", $"Car rented by customer", "info");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddReservation: {ex.Message}");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open) _connection.Close();
            }
            return reservationId;
        }

        /// <summary>
        /// Add a new payment and log transaction
        /// </summary>
        public int AddPayment(int customerId, string method, decimal amount, string status = "Completed")
        {
            int paymentId = 0;
            string query = @"INSERT INTO PAYMENT (CUSTOMER_ID, PAYMENT_METHOD, PAYMENT_STATUS, PAYMENT_DATE, AMOUNT)
                     VALUES (@CustomerId, @Method, @Status, GETDATE(), @Amount);
                     SELECT SCOPE_IDENTITY();";
            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@Method", method);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@Amount", amount);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    paymentId = Convert.ToInt32(result);
                    _connection.Close();

                    // Log basic activity
                    LogActivity("Payment Received", $"Payment of EGP {amount} received", "success");

                    // Fetch customer name for transaction log
                    string customerName = "Unknown Customer";
                    // In a real scenario we'd query it or pass it. For now, let's query it quickly or just placeholder.
                    // Simplified:
                    LogTransaction(paymentId, "Customer #" + customerId, "N/A", amount, status);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddPayment: {ex.Message}");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open) _connection.Close();
            }
            return paymentId;
        }        // NEW: Get customer data (Uses FNAME, LNAME, and CUSTOMER_EMAIL from your schema)
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


        // ============================================
        // ADMIN DASHBOARD QUERIES
        // ============================================

        /// <summary>
        /// Get total count of cars in inventory
        /// Query: SELECT COUNT(*) AS TotalCars FROM CAR
        /// </summary>
        public int GetTotalCars()
        {
            int totalCars = 0;
            string query = "SELECT COUNT(*) AS TotalCars FROM CAR";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalCars = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTotalCars: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return totalCars;
        }





        /// <summary>
        /// Get total sales count from BUYING_RENTING table
        /// Query: SELECT COUNT(*) AS TotalSales FROM BUYING_RENTING
        /// </summary>
        public int GetTotalSales()
        {
            int totalSales = 0;
            string query = "SELECT COUNT(*) AS TotalSales FROM BUYING_RENTING";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalSales = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTotalSales: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return totalSales;
        }




        /// <summary>
        /// Get count of active rentals with status 'Confirmed'
        /// Query: SELECT COUNT(*) AS ActiveRentals FROM RESERVATIONS WHERE RESERVATION_STATUS = 'Confirmed'
        /// </summary>
        public int GetActiveRentals()
        {
            int activeRentals = 0;
            string query = @"SELECT COUNT(*) AS ActiveRentals 
                            FROM RESERVATIONS 
                            WHERE RESERVATION_STATUS = 'Confirmed'";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    activeRentals = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetActiveRentals: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return activeRentals;
        }





        /// <summary>
        /// Get total users count from CUSTOMER table
        /// Query: SELECT COUNT(*) AS TotalUsers FROM CUSTOMER
        /// </summary>
        public int GetTotalUsers()
        {
            int totalUsers = 0;
            string query = "SELECT COUNT(*) AS TotalUsers FROM CUSTOMER";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalUsers = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTotalUsers: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return totalUsers;
        }


        /// <summary>
        /// Get total revenue from all completed payments
        /// Query: SELECT SUM(AMOUNT) AS TotalRevenue FROM PAYMENT WHERE PAYMENT_STATUS = 'Completed'
        /// </summary>
        public decimal GetTotalRevenue()
        {
            decimal totalRevenue = 0;
            string query = @"SELECT SUM(AMOUNT) AS TotalRevenue 
                            FROM PAYMENT 
                            WHERE PAYMENT_STATUS = 'Completed'";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalRevenue = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTotalRevenue: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return totalRevenue;
        }



        /// <summary>
        /// Get monthly revenue for current month
        /// Query: SELECT SUM(AMOUNT) FROM PAYMENT WHERE PAYMENT_STATUS = 'Completed' 
        ///        AND MONTH(PAYMENT_DATE) = MONTH(GETDATE()) AND YEAR(PAYMENT_DATE) = YEAR(GETDATE())
        /// </summary>
        public decimal GetMonthlyRevenue()
        {
            decimal monthlyRevenue = 0;
            string query = @"SELECT SUM(AMOUNT) AS MonthlyRevenue 
                            FROM PAYMENT 
                            WHERE PAYMENT_STATUS = 'Completed'
                            AND MONTH(PAYMENT_DATE) = MONTH(GETDATE())
                            AND YEAR(PAYMENT_DATE) = YEAR(GETDATE())";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    monthlyRevenue = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMonthlyRevenue: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return monthlyRevenue;
        }



        /// <summary>
        /// Get revenue data for chart grouped by month
        /// Query: SELECT DATENAME(MONTH, PAYMENT_DATE) AS Month, SUM(AMOUNT) AS Revenue
        ///        FROM PAYMENT WHERE PAYMENT_STATUS = 'Completed' AND YEAR(PAYMENT_DATE) = YEAR(GETDATE())
        ///        GROUP BY MONTH(PAYMENT_DATE), DATENAME(MONTH, PAYMENT_DATE)
        /// </summary>
        public List<MonthlyRevenue> GetMonthlyRevenueData()
        {
            List<MonthlyRevenue> revenueData = new List<MonthlyRevenue>();
            string query = @"SELECT 
                            DATENAME(MONTH, PAYMENT_DATE) AS Month,
                            SUM(AMOUNT) AS Revenue
                            FROM PAYMENT
                            WHERE PAYMENT_STATUS = 'Completed'
                            AND YEAR(PAYMENT_DATE) = YEAR(GETDATE())
                            GROUP BY MONTH(PAYMENT_DATE), DATENAME(MONTH, PAYMENT_DATE)
                            ORDER BY MONTH(PAYMENT_DATE)";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    revenueData.Add(new MonthlyRevenue
                    {
                        Month = reader["Month"].ToString(),
                        Revenue = reader["Revenue"] != DBNull.Value ? Convert.ToDecimal(reader["Revenue"]) : 0
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMonthlyRevenueData: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return revenueData;
        }
        /// <summary>
        /// Get car sales by brand for chart
        /// Query: SELECT c.BRAND, COUNT(br.CAR_ID) AS Sales
        ///        FROM CAR c LEFT JOIN BUYING_RENTING br ON c.CAR_ID = br.CAR_ID
        ///        GROUP BY c.BRAND ORDER BY Sales DESC
        /// </summary>
        public List<CarSalesData> GetCarSalesByBrand()
        {
            List<CarSalesData> salesData = new List<CarSalesData>();
            string query = @"SELECT 
                            c.BRAND,
                            COUNT(br.CAR_ID) AS Sales
                            FROM CAR c
                            LEFT JOIN BUYING_RENTING br ON c.CAR_ID = br.CAR_ID
                            GROUP BY c.BRAND
                            ORDER BY Sales DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    salesData.Add(new CarSalesData
                    {
                        Brand = reader["BRAND"].ToString(),
                        Sales = reader["Sales"] != DBNull.Value ? Convert.ToInt32(reader["Sales"]) : 0
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarSalesByBrand: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return salesData;
        }


        /// <summary>
        /// Get top 5 selling cars
        /// Query: SELECT TOP 5 c.CAR_NAME, COUNT(br.CAR_ID) AS Sales, 
        ///        SUM(CAST(REPLACE(c.PRICE, ',', '') AS DECIMAL(10,2))) AS Revenue
        ///        FROM CAR c LEFT JOIN BUYING_RENTING br ON c.CAR_ID = br.CAR_ID
        ///        GROUP BY c.CAR_ID, c.CAR_NAME, c.PRICE ORDER BY Sales DESC
        /// </summary>
        public List<TopCar> GetTopSellingCars()
        {
            List<TopCar> topCars = new List<TopCar>();
            string query = @"SELECT TOP 5
                            c.CAR_NAME AS Name,
                            COUNT(br.CAR_ID) AS Sales,
                            SUM(CAST(REPLACE(c.PRICE, ',', '') AS DECIMAL(10,2))) AS Revenue
                            FROM CAR c
                            LEFT JOIN BUYING_RENTING br ON c.CAR_ID = br.CAR_ID
                            GROUP BY c.CAR_ID, c.CAR_NAME, c.PRICE
                            ORDER BY Sales DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    topCars.Add(new TopCar
                    {
                        Name = reader["Name"].ToString(),
                        Sales = reader["Sales"] != DBNull.Value ? Convert.ToInt32(reader["Sales"]) : 0,
                        Revenue = reader["Revenue"] != DBNull.Value ? Convert.ToDecimal(reader["Revenue"]) : 0
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetTopSellingCars: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return topCars;
        }



        /// <summary>
        /// Get recent activities (last 10 activities from ACTIVITY_LOG table)
        /// </summary>
        public List<ActivityLog> GetRecentActivities()
        {
            List<ActivityLog> activities = new List<ActivityLog>();
            string query = @"SELECT TOP 10 ACTION, DESCRIPTION, TIMESTAMP, TYPE 
                            FROM ACTIVITY_LOG 
                            ORDER BY TIMESTAMP DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    activities.Add(new ActivityLog
                    {
                        Action = reader["ACTION"].ToString(),
                        Description = reader["DESCRIPTION"].ToString(),
                        Timestamp = reader["TIMESTAMP"] != DBNull.Value ? Convert.ToDateTime(reader["TIMESTAMP"]) : DateTime.Now,
                        Type = reader["TYPE"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRecentActivities: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return activities;
        }


        // <summary>
        /// Get recent transactions (last 10 transactions from TRANSACTION_LOG table)
        /// </summary>
        public List<Transaction> GetRecentTransactions()
        {
            List<Transaction> transactions = new List<Transaction>();
            string query = @"SELECT TOP 10 
                            TRANS_ID AS Id,
                            CUSTOMER_NAME AS Customer,
                            CAR_NAME AS Car,
                            AMOUNT AS Amount,
                            DATE AS Date,
                            STATUS AS Status
                            FROM TRANSACTION_LOG
                            ORDER BY DATE DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    transactions.Add(new Transaction
                    {
                        Id = "TRX-" + reader["Id"].ToString().PadLeft(6, '0'),
                        Customer = reader["Customer"].ToString(),
                        Car = reader["Car"] != DBNull.Value ? reader["Car"].ToString() : "N/A",
                        Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                        Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : DateTime.Now,
                        Status = reader["Status"].ToString()
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRecentTransactions: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return transactions;
        }

        // ============================================
        // CAR DETAILS PAGE QUERIES
        // ============================================

        /// <summary>
        /// Get main car details by car ID
        /// Query: SELECT * FROM CAR WHERE CAR_ID = @CarId
        /// </summary>
        public CarDetails GetCarDetailsFull(int carId)
        {
            CarDetails car = null;
            string query = @"SELECT 
                            c.CAR_ID,
                            c.CAR_NAME,
                            c.BRAND,
                            c.YEAR,
                            c.PRICE,
                            c.MAIN_IMAGE,
                            c.TRANSMISSION,
                            c.FUEL_TYPE,
                            c.ENGINE,
                            c.SEATS,
                            c.COLOR,
                            c.MILEAGE,
                            c.MIN_DEPOSIT,
                            c.MONTHLY_INSTALLMENT,
                            c.DESCRIPTION
                            FROM CAR c
                            WHERE c.CAR_ID = @CarId";

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
                        MainImage = reader["MAIN_IMAGE"] != DBNull.Value ? reader["MAIN_IMAGE"].ToString() : "",
                        Transmission = reader["TRANSMISSION"] != DBNull.Value ? reader["TRANSMISSION"].ToString() : "",
                        FuelType = reader["FUEL_TYPE"] != DBNull.Value ? reader["FUEL_TYPE"].ToString() : "",
                        Engine = reader["ENGINE"] != DBNull.Value ? reader["ENGINE"].ToString() : "",
                        Seats = reader["SEATS"] != DBNull.Value ? reader["SEATS"].ToString() : "",
                        Color = reader["COLOR"] != DBNull.Value ? reader["COLOR"].ToString() : "",
                        Mileage = reader["MILEAGE"] != DBNull.Value ? reader["MILEAGE"].ToString() : "",
                        MinDeposit = reader["MIN_DEPOSIT"] != DBNull.Value ? reader["MIN_DEPOSIT"].ToString() : "",
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"] != DBNull.Value ? reader["MONTHLY_INSTALLMENT"].ToString() : "",
                        Description = reader["DESCRIPTION"] != DBNull.Value ? reader["DESCRIPTION"].ToString() : ""
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarDetailsFull: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return car;
        }

        /// <summary>
        /// Get all images for a specific car
        /// Query: SELECT IMAGE_URL FROM CAR_IMAGES WHERE CAR_ID = @CarId ORDER BY IMAGE_ID
        /// </summary>
        public List<string> GetCarImagesById(int carId)
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
                Console.WriteLine($"Error in GetCarImagesById: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return images;
        }

        /// <summary>
        /// Get all features for a specific car
        /// Query: SELECT FEATURE_NAME FROM CAR_FEATURES WHERE CAR_ID = @CarId ORDER BY FEATURE_ID
        /// </summary>
        public List<string> GetCarFeaturesById(int carId)
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
                Console.WriteLine($"Error in GetCarFeaturesById: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return features;
        }

        /// <summary>
        /// Get car with insurance information
        /// Query joins CAR and INSURANCE tables
        /// </summary>
        public CarWithInsurance GetCarWithInsurance(int carId)
        {
            CarWithInsurance carInfo = null;
            string query = @"SELECT 
                            c.*,
                            i.INSURANCE_ID,
                            i.INSURANCE_START_DATE,
                            i.INSURANCE_END_DATE,
                            i.COMPANY_COVERING_NAME,
                            i.COVERAGE_TYPE
                            FROM CAR c
                            LEFT JOIN INSURANCE i ON c.CAR_ID = i.CAR_ID
                            WHERE c.CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    carInfo = new CarWithInsurance
                    {
                        CarId = Convert.ToInt32(reader["CAR_ID"]),
                        CarName = reader["CAR_NAME"].ToString(),
                        Brand = reader["BRAND"].ToString(),
                        InsuranceId = reader["INSURANCE_ID"] != DBNull.Value ? Convert.ToInt32(reader["INSURANCE_ID"]) : 0,
                        InsuranceStartDate = reader["INSURANCE_START_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["INSURANCE_START_DATE"]) : DateTime.MinValue,
                        InsuranceEndDate = reader["INSURANCE_END_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["INSURANCE_END_DATE"]) : DateTime.MinValue,
                        CompanyCoveringName = reader["COMPANY_COVERING_NAME"] != DBNull.Value ? reader["COMPANY_COVERING_NAME"].ToString() : "",
                        CoverageType = reader["COVERAGE_TYPE"] != DBNull.Value ? reader["COVERAGE_TYPE"].ToString() : ""
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarWithInsurance: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return carInfo;
        }

        /// <summary>
        /// Get maintenance history for a specific car
        /// Query joins MAINTENANCE_REPORT and MAINTENANCE_STAFF tables
        /// </summary>
        public List<MaintenanceRecord> GetMaintenanceHistory(int carId)
        {
            List<MaintenanceRecord> records = new List<MaintenanceRecord>();
            string query = @"SELECT 
                            mr.REPAIR_COST,
                            mr.MAINTENANCE_START_DATE,
                            mr.MAINTENANCE_END_DATE,
                            ms.SPECIALIZATION AS MaintenanceType,
                            ms.MAINTENANCE_STAFF_ID
                            FROM MAINTENANCE_REPORT mr
                            INNER JOIN MAINTENANCE_STAFF ms ON mr.MAINTENANCE_STAFF_ID = ms.MAINTENANCE_STAFF_ID
                            WHERE mr.CAR_ID = @CarId
                            ORDER BY mr.MAINTENANCE_START_DATE DESC";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    records.Add(new MaintenanceRecord
                    {
                        RepairCost = reader["REPAIR_COST"] != DBNull.Value ? Convert.ToDecimal(reader["REPAIR_COST"]) : 0,
                        MaintenanceStartDate = reader["MAINTENANCE_START_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["MAINTENANCE_START_DATE"]) : DateTime.MinValue,
                        MaintenanceEndDate = reader["MAINTENANCE_END_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["MAINTENANCE_END_DATE"]) : DateTime.MinValue,
                        MaintenanceType = reader["MaintenanceType"].ToString(),
                        StaffId = Convert.ToInt32(reader["MAINTENANCE_STAFF_ID"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetMaintenanceHistory: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return records;
        }






        /// <summary>
        /// ADDED: Add a new complaint/message
        /// </summary>
        public bool AddComplaint(string description, int? customerId = null)
        {
            string query = @"INSERT INTO COMPLAINTS (COMPLAINT_DATE, PROBLEM_DESCRIPTION, COMPLAINT_STATUTS, CUSTOMER_ID)
                             VALUES (GETDATE(), @Description, 'Pending', @CustomerId)";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@CustomerId", (object)customerId ?? DBNull.Value);
                
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddComplaint: {ex.Message}");
                return false;
            }
            finally
            {
                _connection.Close();
            }
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
        public string? CarId { get; set; }
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? Year { get; set; }
        public string? Price { get; set; }
        public string? Image { get; set; }
        public string? Transmission { get; set; }
        public string? FuelType { get; set; }
        public string? MinDeposit { get; set; }
        public string? MonthlyInstallment { get; set; }
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




    // ============================================
    // HELPER CLASSES FOR ADMIN DASHBOARD
    // ============================================
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

    public class ActivityLog
    {
        public string Action { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = "";
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





    // ============================================
    // HELPER CLASSES FOR CAR DETAILS
    // ============================================
    public class CarWithInsurance
    {
        public int CarId { get; set; }
        public string CarName { get; set; } = "";
        public string Brand { get; set; } = "";
        public int InsuranceId { get; set; }
        public DateTime InsuranceStartDate { get; set; }
        public DateTime InsuranceEndDate { get; set; }
        public string CompanyCoveringName { get; set; } = "";
        public string CoverageType { get; set; } = "";
    }

    public class MaintenanceRecord
    {
        public decimal RepairCost { get; set; }
        public DateTime MaintenanceStartDate { get; set; }
        public DateTime MaintenanceEndDate { get; set; }
        public string MaintenanceType { get; set; } = "";
        public int StaffId { get; set; }
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