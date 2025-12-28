using Microsoft.Data.SqlClient;
using System.Data;

namespace Car_Agency_Management.Data
{
    public class DB
    {
        public readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=yarab;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
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
            //
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
            //
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

        public bool IsPhoneTaken(string phone)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM CUSTOMER_PHONE_NUMBERS WHERE PHONE_NUMBERS = @Phone";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking phone: {ex.Message}");
                return true;
            }
        }

        public bool VerifyPhoneLast4(string email, string last4)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // We join CUSTOMER with CUSTOMER_PHONE_NUMBERS to verify the last 4 digits
                    string query = @"
                        SELECT TOP 1 PN.PHONE_NUMBERS 
                        FROM CUSTOMER C
                        JOIN CUSTOMER_PHONE_NUMBERS PN ON C.CUSTOMER_ID = PN.CUSTOMER_ID
                        WHERE C.CUSTOMER_EMAIL = @Email";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        object result = cmd.ExecuteScalar();
                        
                        if (result != null)
                        {
                            string fullPhone = result.ToString();
                            // Handle cases where phone might have spaces or special characters
                            string cleanPhone = new string(fullPhone.Where(char.IsDigit).ToArray());
                            return cleanPhone.EndsWith(last4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error verifying phone digits: {ex.Message}");
            }
            return false;
        }

        public bool ResetPassword(string email, string newPassword)
        {
            try
            {
                string cleanEmail = email?.Trim() ?? "";
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE CUSTOMER SET CUSTOMER_PASSWORD = @Password WHERE CUSTOMER_EMAIL = @Email";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", cleanEmail);
                        cmd.Parameters.AddWithValue("@Password", newPassword);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        if (rowsAffected > 0)
                        {
                            LogActivity("Password Reset", $"Password reset successful for {email}", "info");
                            return true;
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ResetPassword: {ex.Message}");
                return false;
            }
        }

        public bool AddCustomer(string fname, string lname, string email, string password, string phone, string address)
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
                                INSERT INTO CUSTOMER (FNAME, LNAME, CUSTOMER_EMAIL, CUSTOMER_PASSWORD, ADDRESS, MNAME) 
                                VALUES (@Fname, @Lname, @Email, @Password, @Address, ''); 
                                SELECT SCOPE_IDENTITY();";
                                
                            int newCustomerId = 0;

                            using (SqlCommand cmd = new SqlCommand(insertCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Fname", fname);
                                cmd.Parameters.AddWithValue("@Lname", lname);
                                cmd.Parameters.AddWithValue("@Email", email);
                                cmd.Parameters.AddWithValue("@Password", password);
                                cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                                
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
                    MONTHLY_INSTALLMENT, DESCRIPTION, DATE_ADDED)
                    VALUES (@CarName, @Brand, @Year, @Price, @Color, @Transmission,
                    @FuelType, @Engine, @Seats, @Mileage, @MainImage, @MinDeposit,
                    @MonthlyInstallment, @Description, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarName", car.CarName ?? "");
                cmd.Parameters.AddWithValue("@Brand", car.Brand ?? "");
                cmd.Parameters.AddWithValue("@Year", car.Year ?? "");
                cmd.Parameters.AddWithValue("@Price", car.Price ?? "");
                cmd.Parameters.AddWithValue("@Color", car.Color ?? "");
                cmd.Parameters.AddWithValue("@Transmission", car.Transmission ?? "");
                cmd.Parameters.AddWithValue("@FuelType", car.FuelType ?? "");
                cmd.Parameters.AddWithValue("@Engine", car.Engine ?? "");
                cmd.Parameters.AddWithValue("@Seats", car.Seats ?? "");
                cmd.Parameters.AddWithValue("@Mileage", car.Mileage ?? "");
                cmd.Parameters.AddWithValue("@MainImage", car.MainImage ?? "");
                cmd.Parameters.AddWithValue("@MinDeposit", car.MinDeposit ?? "");
                cmd.Parameters.AddWithValue("@MonthlyInstallment", car.MonthlyInstallment ?? "");
                cmd.Parameters.AddWithValue("@Description", car.Description ?? "");

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    newCarId = result.ToString();
                    Console.WriteLine($" Car added successfully with ID: {newCarId}");
                }
                else
                {
                    Console.WriteLine(" AddCar: SCOPE_IDENTITY returned null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error in AddCar: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }

            // Log activity AFTER connection is closed
            if (!string.IsNullOrEmpty(newCarId))
            {
                try
                {
                    LogActivity("New Car Added", $"Car: {car.CarName} added to inventory", "success");
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Warning: Failed to log activity: {logEx.Message}");
                    // Don't fail the entire operation if logging fails
                }
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

        // NEW: Get customer history using TRANSACTION_LOG for accurate car mapping
        // UPDATED: Added fallback to PAYMENT table if TRANSACTION_LOG doesn't exist
public List<TransactionSummary> GetCustomerTransactions(int customerId)
{
    List<TransactionSummary> list = new List<TransactionSummary>();
    
    // First, try to use TRANSACTION_LOG for accurate car names
    string queryWithLog = @"SELECT T.CAR_NAME, P.PAYMENT_METHOD, P.AMOUNT, P.PAYMENT_DATE, P.PAYMENT_STATUS
            FROM TRANSACTION_LOG T
            JOIN PAYMENT P ON T.PAYMENT_ID = P.PAYMENT_ID
            WHERE P.CUSTOMER_ID = @Id
            ORDER BY P.PAYMENT_DATE DESC";
    
    // Fallback query if TRANSACTION_LOG doesn't exist
    string queryFallback = @"SELECT 
        ISNULL(C.CAR_NAME, 'Unknown Car') AS CAR_NAME,
        P.PAYMENT_METHOD, 
        P.AMOUNT, 
        P.PAYMENT_DATE, 
        P.PAYMENT_STATUS
    FROM PAYMENT P
    LEFT JOIN BUYING_RENTING BR ON P.CUSTOMER_ID = BR.CUSTOMER_ID
    LEFT JOIN CAR C ON BR.CAR_ID = C.CAR_ID
    WHERE P.CUSTOMER_ID = @Id
    ORDER BY P.PAYMENT_DATE DESC";
    
    try
    {
        _connection.Open();
        SqlCommand cmd = new SqlCommand(queryWithLog, _connection);
        cmd.Parameters.AddWithValue("@Id", customerId);
        
        try
        {
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TransactionSummary
                {
                    CarTitle = reader["CAR_NAME"].ToString() ?? "Unknown Car",
                    Method = reader["PAYMENT_METHOD"].ToString(),
                    Amount = reader["AMOUNT"].ToString(),
                    Date = Convert.ToDateTime(reader["PAYMENT_DATE"]).ToShortDateString(),
                    Status = reader["PAYMENT_STATUS"].ToString() ?? "N/A"
                });
            }
            reader.Close();
        }
        catch (SqlException sqlEx) when (sqlEx.Message.Contains("Invalid object name 'TRANSACTION_LOG'"))
        {
            // TRANSACTION_LOG doesn't exist, use fallback query
            Console.WriteLine("TRANSACTION_LOG table not found, using fallback query");
            _connection.Close();
            _connection.Open();
            
            SqlCommand fallbackCmd = new SqlCommand(queryFallback, _connection);
            fallbackCmd.Parameters.AddWithValue("@Id", customerId);
            SqlDataReader reader = fallbackCmd.ExecuteReader();
            
            while (reader.Read())
            {
                list.Add(new TransactionSummary
                {
                    CarTitle = reader["CAR_NAME"].ToString() ?? "Unknown Car",
                    Method = reader["PAYMENT_METHOD"].ToString(),
                    Amount = reader["AMOUNT"].ToString(),
                    Date = Convert.ToDateTime(reader["PAYMENT_DATE"]).ToShortDateString(),
                    Status = reader["PAYMENT_STATUS"].ToString() ?? "N/A"
                });
            }
            reader.Close();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in GetCustomerTransactions: {ex.Message}");
    }
    finally 
    { 
        if (_connection.State == ConnectionState.Open)
        {
            _connection.Close(); 
        }
    }
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
        /// Get count of active rentals with status 'Active'
        /// Query: SELECT COUNT(*) AS ActiveRentals FROM RENTALS WHERE STATUS = 'Active'
        /// </summary>
        public int GetActiveRentals()
        {
            int activeRentals = 0;
            string query = "SELECT COUNT(*) FROM RENTALS WHERE STATUS = 'Active'";
            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                activeRentals = Convert.ToInt32(cmd.ExecuteScalar());
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
        public CarPaymentInfo GetCarPaymentInfo(int carId)
        {
            CarPaymentInfo carInfo = null;

            string query = @"SELECT 
                    CAR_ID,
                    CAR_NAME,
                    PRICE,
                    MAIN_IMAGE,
                    MIN_DEPOSIT,
                    MONTHLY_INSTALLMENT
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
                    carInfo = new CarPaymentInfo
                    {
                        CarId = Convert.ToInt32(reader["CAR_ID"]),
                        CarName = reader["CAR_NAME"].ToString(),
                        Price = reader["PRICE"].ToString(),
                        MainImage = reader["MAIN_IMAGE"].ToString(),
                        MinDeposit = reader["MIN_DEPOSIT"].ToString(),
                        MonthlyInstallment = reader["MONTHLY_INSTALLMENT"].ToString()
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarPaymentInfo: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return carInfo;
        }

        /// <summary>
        /// Get insurance plans for selected car
        /// </summary>
        public List<InsurancePlan> GetInsurancePlans(int carId)
        {
            List<InsurancePlan> plans = new List<InsurancePlan>();

            string query = @"SELECT 
                    INSURANCE_ID,
                    COMPANY_COVERING_NAME,
                    COVERAGE_TYPE,
                    INSURANCE_START_DATE,
                    INSURANCE_END_DATE,
                    PRICE
                    FROM INSURANCE
                    WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    plans.Add(new InsurancePlan
                    {
                        InsuranceId = Convert.ToInt32(reader["INSURANCE_ID"]),
                        CompanyName = reader["COMPANY_COVERING_NAME"].ToString(),
                        CoverageType = reader["COVERAGE_TYPE"].ToString(),
                        StartDate = Convert.ToDateTime(reader["INSURANCE_START_DATE"]),
                        EndDate = Convert.ToDateTime(reader["INSURANCE_END_DATE"]),
                        Price = reader["PRICE"] != DBNull.Value ? Convert.ToDecimal(reader["PRICE"]) : 0
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInsurancePlans: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return plans;
        }

        /// <summary>
        /// Validate discount code
        /// </summary>
        public DiscountInfo ValidateDiscountCode(string discountCode)
        {
            DiscountInfo discount = null;

            string query = @"SELECT 
                    DISCOUNT_ID,
                    DISCOUNT_PERCENT,
                    EXPIRY_DATE
                    FROM DISCOUNTS
                    WHERE CODE = @DiscountCode
                    AND IS_ACTIVE = 1
                    AND EXPIRY_DATE >= GETDATE()";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@DiscountCode", discountCode);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    discount = new DiscountInfo
                    {
                        DiscountId = Convert.ToInt32(reader["DISCOUNT_ID"]),
                        DiscountPercent = Convert.ToDecimal(reader["DISCOUNT_PERCENT"]),
                        ExpiryDate = Convert.ToDateTime(reader["EXPIRY_DATE"])
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ValidateDiscountCode: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return discount;
        }

        /// <summary>
        /// Check car availability for selected dates
        /// </summary>
        public string CheckCarAvailability(int carId, DateTime startDate, DateTime endDate)
        {
            var availability = CheckRentalAvailability(carId, startDate, endDate);
            return availability.IsAvailable ? "Available" : "Not Available";
        }

        /// <summary>
        /// Create reservation and payment (Transaction)
        /// </summary>
        /// <summary>
        /// Create reservation and payment (Transaction)
        /// UPDATED: Now fetches and stores actual car name in transaction log
        /// </summary>
        public PaymentResult CreateReservationAndPayment(
            int customerId,
            int carId,
            DateTime startDate,
            DateTime endDate,
            string transactionType,
            string paymentMethod,
            decimal amountPaid)
        {
            PaymentResult result = new PaymentResult
            {
                Success = false,
                ErrorMessage = ""
            };

            SqlTransaction transaction = null;

            try
            {
                _connection.Open();
                transaction = _connection.BeginTransaction();

                // ============================================
                // ADDED: Get car name and customer name for transaction log
                // ============================================
                string carName = "Unknown Car";
                string customerName = "Unknown Customer";

                // Fetch car name
                string carQuery = "SELECT CAR_NAME FROM CAR WHERE CAR_ID = @CarId";
                using (SqlCommand carCmd = new SqlCommand(carQuery, _connection, transaction))
                {
                    carCmd.Parameters.AddWithValue("@CarId", carId);
                    object carResult = carCmd.ExecuteScalar();
                    if (carResult != null)
                    {
                        carName = carResult.ToString();
                    }
                }

                // Fetch customer name
                string customerQuery = "SELECT FNAME + ' ' + LNAME FROM CUSTOMER WHERE CUSTOMER_ID = @CustomerId";
                using (SqlCommand custCmd = new SqlCommand(customerQuery, _connection, transaction))
                {
                    custCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    object custResult = custCmd.ExecuteScalar();
                    if (custResult != null)
                    {
                        customerName = custResult.ToString();
                    }
                }

                // Step 1: Insert reservation (optional - wrapped in try-catch)
                int reservationId = 0;
                try
                {
                    string reservationQuery = @"INSERT INTO RESERVATIONS (
                                CUSTOMER_ID,
                                CAR_ID,
                                RESERVATION_STATUS,
                                RESERVATION_START_DATE,
                                RESERVATION_END_DATE
                                )
                                VALUES (
                                @CustomerId,
                                @CarId,
                                'Confirmed',
                                @StartDate,
                                @EndDate
                                );
                                SELECT SCOPE_IDENTITY();";

                    SqlCommand reservationCmd = new SqlCommand(reservationQuery, _connection, transaction);
                    reservationCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    reservationCmd.Parameters.AddWithValue("@CarId", carId);
                    reservationCmd.Parameters.AddWithValue("@StartDate", startDate);
                    reservationCmd.Parameters.AddWithValue("@EndDate", endDate);

                    reservationId = Convert.ToInt32(reservationCmd.ExecuteScalar());
                    result.ReservationId = reservationId;
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"⚠️ Warning: Could not insert into RESERVATIONS: {sqlEx.Message}");
                    Console.WriteLine("   Payment will continue without reservation record.");
                }

                // Step 2: Link customer with car (BUYING_RENTING)
                // Wrapped in try-catch to prevent payment failure if table schema is incorrect
                try
                {
                    string buyingRentingQuery = @"INSERT INTO BUYING_RENTING (
                                  CUSTOMER_ID,
                                  CAR_ID,
                                  TRANSACTION_TYPE,
                                  TRANSACTION_DATE
                                  )
                                  VALUES (
                                  @CustomerId,
                                  @CarId,
                                  @TransactionType,
                                  GETDATE()
                                  )";

                    SqlCommand buyingRentingCmd = new SqlCommand(buyingRentingQuery, _connection, transaction);
                    buyingRentingCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    buyingRentingCmd.Parameters.AddWithValue("@CarId", carId);
                    buyingRentingCmd.Parameters.AddWithValue("@TransactionType", transactionType);
                    buyingRentingCmd.ExecuteNonQuery();
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"⚠️ Warning: Could not insert into BUYING_RENTING: {sqlEx.Message}");
                    Console.WriteLine("   Payment will continue, but please reset database for full functionality.");
                }

                // Step 3: Insert payment record
                string paymentQuery = @"INSERT INTO PAYMENT (
                       CUSTOMER_ID,
                       PAYMENT_METHOD,
                       PAYMENT_STATUS,
                       PAYMENT_DATE,
                       AMOUNT
                       )
                       VALUES (
                       @CustomerId,
                       @PaymentMethod,
                       'Completed',
                       GETDATE(),
                       @AmountPaid
                       );
                       SELECT SCOPE_IDENTITY();";

                SqlCommand paymentCmd = new SqlCommand(paymentQuery, _connection, transaction);
                paymentCmd.Parameters.AddWithValue("@CustomerId", customerId);
                paymentCmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                paymentCmd.Parameters.AddWithValue("@AmountPaid", amountPaid);

                int paymentId = Convert.ToInt32(paymentCmd.ExecuteScalar());
                result.PaymentId = paymentId;

                // Commit transaction
                transaction.Commit();
                result.Success = true;

                Console.WriteLine($"✅ Reservation and payment created successfully!");
                Console.WriteLine($"   Reservation ID: {reservationId}");
                Console.WriteLine($"   Payment ID: {paymentId}");
                Console.WriteLine($"   Car Name: {carName}");
                Console.WriteLine($"   Customer Name: {customerName}");

                // Close connection before logging (LogTransaction opens its own connection)
                _connection.Close();
                
                // Log transaction with actual car name using existing method
                LogTransaction(paymentId, customerName, carName, amountPaid, "Completed");
                
                // Log activity
                LogActivity("Payment Completed", $"Payment of {amountPaid} EGP completed for {carName} by {customerName}", "success");
            }
            catch (Exception ex)
            {
                // Rollback on error
                transaction?.Rollback();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                Console.WriteLine($"❌ Error in CreateReservationAndPayment: {ex.Message}");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// Get payment receipt/confirmation details
        /// </summary>
        public PaymentReceipt GetPaymentReceipt(int paymentId)
        {
            PaymentReceipt receipt = null;

            string query = @"SELECT 
                    'TRX-' + RIGHT('000000' + CAST(p.PAYMENT_ID AS VARCHAR), 6) AS TransactionId,
                    p.AMOUNT,
                    p.PAYMENT_METHOD,
                    p.PAYMENT_STATUS,
                    p.PAYMENT_DATE,
                    c.FNAME + ' ' + c.LNAME AS CustomerName,
                    car.CAR_NAME
                    FROM PAYMENT p
                    INNER JOIN CUSTOMER c 
                        ON p.CUSTOMER_ID = c.CUSTOMER_ID
                    INNER JOIN BUYING_RENTING br 
                        ON c.CUSTOMER_ID = br.CUSTOMER_ID
                    INNER JOIN CAR car 
                        ON br.CAR_ID = car.CAR_ID
                    WHERE p.PAYMENT_ID = @PaymentId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@PaymentId", paymentId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    receipt = new PaymentReceipt
                    {
                        TransactionId = reader["TransactionId"].ToString(),
                        Amount = Convert.ToDecimal(reader["AMOUNT"]),
                        PaymentMethod = reader["PAYMENT_METHOD"].ToString(),
                        PaymentStatus = reader["PAYMENT_STATUS"].ToString(),
                        PaymentDate = Convert.ToDateTime(reader["PAYMENT_DATE"]),
                        CustomerName = reader["CustomerName"].ToString(),
                        CarName = reader["CAR_NAME"].ToString()
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPaymentReceipt: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return receipt;
        }

        /// <summary>
        /// Get customer info for payment page
        /// </summary>
        public CustomerPaymentInfo GetCustomerPaymentInfo(int customerId)
        {
            CustomerPaymentInfo customerInfo = null;

            string query = @"SELECT 
                    CUSTOMER_ID,
                    FNAME + ' ' + ISNULL(MNAME + ' ', '') + LNAME AS FullName,
                    CUSTOMER_EMAIL,
                    ADDRESS
                    FROM CUSTOMER
                    WHERE CUSTOMER_ID = @CustomerId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    customerInfo = new CustomerPaymentInfo
                    {
                        CustomerId = Convert.ToInt32(reader["CUSTOMER_ID"]),
                        FullName = reader["FullName"].ToString(),
                        Email = reader["CUSTOMER_EMAIL"].ToString(),
                        Address = reader["ADDRESS"].ToString()
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCustomerPaymentInfo: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return customerInfo;
        }

        public CarRentalInfo GetCarRentalInfo(int carId)
        {
            CarRentalInfo carInfo = null;
            string query = @"SELECT 
                    CAR_ID,
                    CAR_NAME,
                    ISNULL(DAILY_RENT_PRICE, CAST(REPLACE(MONTHLY_INSTALLMENT, ',', '') AS DECIMAL(18,2))/30.0) AS Price
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
                    carInfo = new CarRentalInfo
                    {
                        CarId = Convert.ToInt32(reader["CAR_ID"]),
                        CarName = reader["CAR_NAME"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"]).ToString("N2")
                    };
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCarRentalInfo: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return carInfo;
        }

        /// <summary>
        /// Get all booked date ranges for a car (to disable in calendar)
        /// UPDATED QUERY: Direct link via CAR_ID in RESERVATIONS table
        /// Query: SELECT RESERVATION_START_DATE, RESERVATION_END_DATE 
        ///        FROM RESERVATIONS WHERE CAR_ID = @CarId AND RESERVATION_STATUS = 'Confirmed'
        /// </summary>
        public List<DateRange> GetBookedDatesForCar(int carId)
        {
            List<DateRange> bookedDates = new List<DateRange>();
            string query = @"SELECT 
                    RESERVATION_START_DATE,
                    RESERVATION_END_DATE
                    FROM RESERVATIONS
                    WHERE CAR_ID = @CarId
                      AND RESERVATION_STATUS = 'Confirmed'
                    ORDER BY RESERVATION_START_DATE";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    bookedDates.Add(new DateRange
                    {
                        StartDate = Convert.ToDateTime(reader["RESERVATION_START_DATE"]),
                        EndDate = Convert.ToDateTime(reader["RESERVATION_END_DATE"])
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetBookedDatesForCar: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return bookedDates;
        }

        /// <summary>
        /// Check if selected date range is available for rental
        /// UPDATED QUERY: Direct check via CAR_ID in RESERVATIONS
        /// Query: Check if selected rental dates are available
        /// </summary>
        public RentalAvailability CheckRentalAvailability(int carId, DateTime startDate, DateTime endDate)
        {
            RentalAvailability availability = new RentalAvailability
            {
                IsAvailable = true,
                Message = "Car is available for selected dates"
            };

            string query = @"SELECT 
                    CASE 
                        WHEN EXISTS (
                            SELECT 1
                            FROM RESERVATIONS
                            WHERE CAR_ID = @CarId
                              AND RESERVATION_STATUS = 'Confirmed'
                              AND (
                                  @StartDate BETWEEN RESERVATION_START_DATE AND RESERVATION_END_DATE
                                  OR @EndDate BETWEEN RESERVATION_START_DATE AND RESERVATION_END_DATE
                                  OR RESERVATION_START_DATE BETWEEN @StartDate AND @EndDate
                              )
                        )
                        THEN 'Not Available'
                        ELSE 'Available'
                    END AS AvailabilityStatus";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                object result = cmd.ExecuteScalar();
                string status = result?.ToString() ?? "Available";

                availability.IsAvailable = (status == "Available");
                availability.Message = availability.IsAvailable
                    ? "Car is available for selected dates"
                    : "Car is not available for the selected date range. Please choose different dates.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckRentalAvailability: {ex.Message}");
                availability.IsAvailable = false;
                availability.Message = "Error checking availability. Please try again.";
            }
            finally
            {
                _connection.Close();
            }

            return availability;
        }

        /// <summary>
        /// Calculate rental duration in days
        /// Query: Calculate rental duration in days
        /// </summary>
        public int CalculateRentalDays(DateTime startDate, DateTime endDate)
        {
            int rentalDays = 0;
            string query = "SELECT DATEDIFF(DAY, @StartDate, @EndDate) AS RentalDays";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    rentalDays = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateRentalDays: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return rentalDays;
        }

        /// <summary>
        /// Calculate estimated rental cost (preview only)
        /// Query: Calculate estimated rental cost
        /// </summary>
        public decimal CalculateEstimatedRentalCost(int carId, DateTime startDate, DateTime endDate)
        {
            decimal estimatedCost = 0;
            string query = @"SELECT 
                    DATEDIFF(DAY, @StartDate, @EndDate) 
                    * (CAST(REPLACE(MONTHLY_INSTALLMENT, ',', '') AS DECIMAL(18,2))/30.0) AS EstimatedRentalCost
                    FROM CAR
                    WHERE CAR_ID = @CarId";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    estimatedCost = Convert.ToDecimal(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateEstimatedRentalCost: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return estimatedCost;
        }

        /// <summary>
        /// UPDATED: Create reservation with direct CAR_ID link
        /// Creates a reservation record linking car directly
        /// </summary>
        public int CreateCarReservation(int customerId, int carId, DateTime startDate, DateTime endDate)
        {
            int reservationId = 0;
            string query = @"INSERT INTO RESERVATIONS (
                        CUSTOMER_ID,
                        CAR_ID,
                        RESERVATION_START_DATE,
                        RESERVATION_END_DATE,
                        RESERVATION_STATUS
                    )
                    VALUES (
                        @CustomerId,
                        @CarId,
                        @StartDate,
                        @EndDate,
                        'Confirmed'
                    );
                    SELECT SCOPE_IDENTITY();";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@CarId", carId);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    reservationId = Convert.ToInt32(result);
                    Console.WriteLine($"✅ Reservation created with ID: {reservationId}");

                    // Log activity
                    LogActivity("Reservation Created",
                        $"Car #{carId} reserved from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}",
                        "info");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateCarReservation: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return reservationId;
        }

        // ============================================
        // USER MANAGEMENT OPERATIONS
        // ============================================

        /// <summary>
        /// Get all users for administration management
        /// Joins CUSTOMER with CUSTOMER_PHONE_NUMBERS
        /// </summary>
        public List<UserManagementModel> GetAllUsers()
        {
            List<UserManagementModel> users = new List<UserManagementModel>();
            string query = @"SELECT 
                            c.CUSTOMER_ID, 
                            c.FNAME, 
                            c.LNAME, 
                            c.CUSTOMER_EMAIL, 
                            c.ADDRESS,
                            (SELECT TOP 1 PHONE_NUMBERS FROM CUSTOMER_PHONE_NUMBERS WHERE CUSTOMER_ID = c.CUSTOMER_ID) as Phone,
                            (SELECT TOP 1 CAST(PAYMENT_DATE AS DATE) FROM PAYMENT WHERE CUSTOMER_ID = c.CUSTOMER_ID ORDER BY PAYMENT_DATE ASC) as JoinDate
                            FROM CUSTOMER c";

            try
            {
                _connection.Open();
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new UserManagementModel
                    {
                        Id = Convert.ToInt32(reader["CUSTOMER_ID"]),
                        FirstName = reader["FNAME"].ToString() ?? "",
                        LastName = reader["LNAME"].ToString() ?? "",
                        Email = reader["CUSTOMER_EMAIL"].ToString() ?? "",
                        Address = reader["ADDRESS"].ToString() ?? "",
                        PhoneNumber = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() ?? "" : "",
                        JoinDate = reader["JoinDate"] != DBNull.Value ? Convert.ToDateTime(reader["JoinDate"]).ToString("yyyy-MM-dd") : "N/A",
                        Status = "Active" // Default for now
                    });
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllUsers: {ex.Message}");
            }
            finally
            {
                _connection.Close();
            }

            return users;
        }

        public bool AddUser(string fname, string lname, string email, string password, string phone, string address)
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
                            string insertCustomerQuery = @"
                                INSERT INTO CUSTOMER (FNAME, LNAME, CUSTOMER_EMAIL, CUSTOMER_PASSWORD, ADDRESS, MNAME) 
                                VALUES (@Fname, @Lname, @Email, @Password, @Address, ''); 
                                SELECT SCOPE_IDENTITY();";

                            int newCustomerId = 0;
                            using (SqlCommand cmd = new SqlCommand(insertCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Fname", fname);
                                cmd.Parameters.AddWithValue("@Lname", lname);
                                cmd.Parameters.AddWithValue("@Email", email);
                                cmd.Parameters.AddWithValue("@Password", password);
                                cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);

                                object result = cmd.ExecuteScalar();
                                newCustomerId = Convert.ToInt32(result);
                            }

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
                            LogActivity("Admin: User Added", $"New user {fname} {lname} added by admin.", "success");
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddUser: {ex.Message}");
                return false;
            }
        }

        public bool UpdateUser(int id, string fname, string lname, string email, string phone, string address)
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
                            string updateCustomerQuery = @"
                                UPDATE CUSTOMER SET 
                                FNAME = @Fname, 
                                LNAME = @Lname, 
                                CUSTOMER_EMAIL = @Email, 
                                ADDRESS = @Address
                                WHERE CUSTOMER_ID = @Id";

                            using (SqlCommand cmd = new SqlCommand(updateCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.Parameters.AddWithValue("@Fname", fname);
                                cmd.Parameters.AddWithValue("@Lname", lname);
                                cmd.Parameters.AddWithValue("@Email", email);
                                cmd.Parameters.AddWithValue("@Address", address ?? (object)DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }

                            // Update phone: Delete old ones and insert new one (simplified)
                            string deletePhoneQuery = "DELETE FROM CUSTOMER_PHONE_NUMBERS WHERE CUSTOMER_ID = @Id";
                            using (SqlCommand cmd = new SqlCommand(deletePhoneQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            if (!string.IsNullOrEmpty(phone))
                            {
                                string insertPhoneQuery = "INSERT INTO CUSTOMER_PHONE_NUMBERS (CUSTOMER_ID, PHONE_NUMBERS) VALUES (@Id, @Phone)";
                                using (SqlCommand cmd = new SqlCommand(insertPhoneQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@Id", id);
                                    cmd.Parameters.AddWithValue("@Phone", phone);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            LogActivity("Admin: User Updated", $"User ID {id} updated by admin.", "info");
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateUser: {ex.Message}");
                return false;
            }
        }

        public bool DeleteUser(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Note: Cascading delete should handle phone numbers if set up, 
                    // otherwise we delete manually. Our schema has ON DELETE CASCADE for CUSTOMER_PHONE_NUMBERS.
                    // But other tables like PAYMENT and RESERVATIONS might not.
                    // Let's assume a simple delete for now, or we might need to handle dependencies.
                    
                    // Actually, looking at setup.sql, PAYMENT and RESERVATIONS references CUSTOMER without CASCADE.
                    // So we might need to delete those first or set them to NULL.
                    
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Delete from BUYING_RENTING
                            string deleteBRQuery = "DELETE FROM BUYING_RENTING WHERE CUSTOMER_ID = @Id";
                            using (SqlCommand cmd = new SqlCommand(deleteBRQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            // Delete from RESERVATIONS
                            string deleteResQuery = "DELETE FROM RESERVATIONS WHERE CUSTOMER_ID = @Id";
                            using (SqlCommand cmd = new SqlCommand(deleteResQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            // Delete from FINES
                            string deleteFinesQuery = "DELETE FROM FINES WHERE CUSTOMER_ID = @Id";
                            using (SqlCommand cmd = new SqlCommand(deleteFinesQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            // Delete from PAYMENT
                            string deletePaymentQuery = "DELETE FROM PAYMENT WHERE CUSTOMER_ID = @Id";
                            using (SqlCommand cmd = new SqlCommand(deletePaymentQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            // Finally delete CUSTOMER
                            string deleteCustomerQuery = "DELETE FROM CUSTOMER WHERE CUSTOMER_ID = @Id";
                            using (SqlCommand cmd = new SqlCommand(deleteCustomerQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Id", id);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            LogActivity("Admin: User Deleted", $"User ID {id} deleted by admin.", "warning");
                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteUser: {ex.Message}");
                return false;
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
        public string Status { get; set; }
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
    public class CarPaymentInfo
    {
        public int CarId { get; set; }
        public string CarName { get; set; } = "";
        public string Price { get; set; } = "";
        public string MainImage { get; set; } = "";
        public string MinDeposit { get; set; } = "";
        public string MonthlyInstallment { get; set; } = "";
    }

    public class InsurancePlan
    {
        public int InsuranceId { get; set; }
        public string CompanyName { get; set; } = "";
        public string CoverageType { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
    }

    public class DiscountInfo
    {
        public int DiscountId { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public int ReservationId { get; set; }
        public int PaymentId { get; set; }
        public string ErrorMessage { get; set; } = "";
    }

    public class PaymentReceipt
    {
        public string TransactionId { get; set; } = "";
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public DateTime PaymentDate { get; set; }
        public string CustomerName { get; set; } = "";
        public string CarName { get; set; } = "";
    }

    public class CustomerPaymentInfo
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
    }
    public class CarRentalInfo
    {
        public int CarId { get; set; }
        public string CarName { get; set; } = "";
        public string Price { get; set; } = "";
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UserManagementModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
        public string JoinDate { get; set; } = "";
        public string Status { get; set; } = "Active";
    }

    public class UserActionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    public class RentalAvailability
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = "";
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