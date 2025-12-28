-- ############################################
-- 1. DROP EXISTING TABLES (Reverse Dependencies)
-- ############################################

IF OBJECT_ID('BUYING_RENTING', 'U') IS NOT NULL DROP TABLE BUYING_RENTING;
IF OBJECT_ID('CAR_REPAIRINGS', 'U') IS NOT NULL DROP TABLE CAR_REPAIRINGS;
IF OBJECT_ID('ADMINISTRATOR_PHONE_NUMBERS', 'U') IS NOT NULL DROP TABLE ADMINISTRATOR_PHONE_NUMBERS;
IF OBJECT_ID('COMPLAINTS', 'U') IS NOT NULL DROP TABLE COMPLAINTS;
IF OBJECT_ID('RESERVATIONS', 'U') IS NOT NULL DROP TABLE RESERVATIONS;
IF OBJECT_ID('INSURANCE', 'U') IS NOT NULL DROP TABLE INSURANCE;
IF OBJECT_ID('FINES', 'U') IS NOT NULL DROP TABLE FINES;
IF OBJECT_ID('PAYMENT', 'U') IS NOT NULL DROP TABLE PAYMENT;
IF OBJECT_ID('MAINTENANCE_REPORT', 'U') IS NOT NULL DROP TABLE MAINTENANCE_REPORT;
IF OBJECT_ID('MAINTENANCE_STAFF', 'U') IS NOT NULL DROP TABLE MAINTENANCE_STAFF;
IF OBJECT_ID('STAFF_DEPARTEMENT', 'U') IS NOT NULL DROP TABLE STAFF_DEPARTEMENT;
IF OBJECT_ID('CUSTOMER_PHONE_NUMBERS', 'U') IS NOT NULL DROP TABLE CUSTOMER_PHONE_NUMBERS;
IF OBJECT_ID('CUSTOMER', 'U') IS NOT NULL DROP TABLE CUSTOMER;
IF OBJECT_ID('ADMINISTRATOR', 'U') IS NOT NULL DROP TABLE ADMINISTRATOR;
IF OBJECT_ID('CAR_IMAGES', 'U') IS NOT NULL DROP TABLE CAR_IMAGES;
IF OBJECT_ID('CAR_FEATURES', 'U') IS NOT NULL DROP TABLE CAR_FEATURES;
IF OBJECT_ID('CAR', 'U') IS NOT NULL DROP TABLE CAR;
IF OBJECT_ID('PARTNERS', 'U') IS NOT NULL DROP TABLE PARTNERS;
IF OBJECT_ID('TRANSACTION_LOG', 'U') IS NOT NULL DROP TABLE TRANSACTION_LOG;
IF OBJECT_ID('ACTIVITY_LOG', 'U') IS NOT NULL DROP TABLE ACTIVITY_LOG;
IF OBJECT_ID('RENTALS', 'U') IS NOT NULL DROP TABLE RENTALS;

-- ############################################
-- 2. CREATE TABLES
-- ############################################

CREATE TABLE CAR (
    CAR_ID INT PRIMARY KEY IDENTITY(1,1),
    CAR_NAME NVARCHAR(200) NOT NULL,
    BRAND NVARCHAR(100) NOT NULL,
    YEAR NVARCHAR(10) NOT NULL,
    PRICE NVARCHAR(50) NOT NULL,
    MAIN_IMAGE NVARCHAR(MAX),
    TRANSMISSION NVARCHAR(50),
    FUEL_TYPE NVARCHAR(50),
    ENGINE NVARCHAR(100),
    SEATS NVARCHAR(10),
    COLOR NVARCHAR(100),
    MILEAGE NVARCHAR(20),
    MIN_DEPOSIT NVARCHAR(50),
    MONTHLY_INSTALLMENT NVARCHAR(50),
    DESCRIPTION NVARCHAR(MAX),
    DATE_ADDED DATE NOT NULL 
);

CREATE TABLE PARTNERS (
    PARTNER_ID INT PRIMARY KEY IDENTITY(1,1),
    BRAND_NAME NVARCHAR(100) UNIQUE NOT NULL, 
    LOGO_URL NVARCHAR(MAX) NOT NULL,        
    IS_ACTIVE BIT DEFAULT 1
);

CREATE TABLE CAR_IMAGES (
    IMAGE_ID INT PRIMARY KEY IDENTITY(1,1),
    CAR_ID INT NOT NULL,
    IMAGE_URL NVARCHAR(MAX) NOT NULL,
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID) ON DELETE CASCADE
); 

CREATE TABLE CAR_FEATURES (
    FEATURE_ID INT PRIMARY KEY IDENTITY(1,1),
    CAR_ID INT NOT NULL,
    FEATURE_NAME NVARCHAR(200) NOT NULL,
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID) ON DELETE CASCADE
);

CREATE TABLE ADMINISTRATOR (
    ADMINISTRATOR_ID INT PRIMARY KEY IDENTITY(1,1),
    ADMINISTRATOR_FNAME NVARCHAR(50) NOT NULL,
    ADMINISTRATOR_MNAME NVARCHAR(50),
    ADMINISTRATOR_LNAME NVARCHAR(50) NOT NULL,
    ADMINISTRATOR_EMAIL NVARCHAR(100) UNIQUE NOT NULL,
    ADMINISTRATOR_PASSWORD NVARCHAR(255) NOT NULL,
    ROLE NVARCHAR(50) NOT NULL
);

CREATE TABLE CUSTOMER (
    CUSTOMER_ID INT PRIMARY KEY IDENTITY(1,1),
    FNAME NVARCHAR(50) NOT NULL,
    MNAME NVARCHAR(50),
    LNAME NVARCHAR(50) NOT NULL,
    CUSTOMER_EMAIL NVARCHAR(100) UNIQUE NOT NULL,
    CUSTOMER_PASSWORD NVARCHAR(255) NOT NULL,
    ADDRESS NVARCHAR(255),
    ADMINISTRATOR_ID INT,
    FOREIGN KEY (ADMINISTRATOR_ID) REFERENCES ADMINISTRATOR(ADMINISTRATOR_ID)
);

CREATE TABLE CUSTOMER_PHONE_NUMBERS (
    CUSTOMER_ID INT,
    PHONE_NUMBERS NVARCHAR(20),
    PRIMARY KEY (CUSTOMER_ID, PHONE_NUMBERS),
    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMER(CUSTOMER_ID) ON DELETE CASCADE
);

CREATE TABLE STAFF_DEPARTEMENT (
    STAFF_MEMBER_ID INT PRIMARY KEY IDENTITY(1,1),
    STAFF_MEMBER_FNAME NVARCHAR(50) NOT NULL,
    STAFF_MEMBER_MNAME NVARCHAR(50),
    STAFF_MEMBER_LNAME NVARCHAR(50) NOT NULL,
    SALARY DECIMAL(10,2),
    WORKING_HOURS INT,
    STAFF_MEMBER_EMAIL NVARCHAR(100) UNIQUE NOT NULL,
    STAFF_MEMBER_PASSWORD NVARCHAR(255) NOT NULL
);

CREATE TABLE MAINTENANCE_STAFF (
    MAINTENANCE_STAFF_ID INT PRIMARY KEY IDENTITY(1,1),
    SPECIALIZATION NVARCHAR(100) NOT NULL,
    CAR_ID INT,
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID)
);

CREATE TABLE MAINTENANCE_REPORT (
    CAR_ID INT,
    MAINTENANCE_STAFF_ID INT,
    REPAIR_COST DECIMAL(10,2),
    MAINTENANCE_START_DATE DATE,
    MAINTENANCE_END_DATE DATE,
    PRIMARY KEY (CAR_ID, MAINTENANCE_STAFF_ID),
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID),
    FOREIGN KEY (MAINTENANCE_STAFF_ID) REFERENCES MAINTENANCE_STAFF(MAINTENANCE_STAFF_ID)
);

CREATE TABLE PAYMENT (
    CUSTOMER_ID INT,
    PAYMENT_ID INT PRIMARY KEY IDENTITY(1,1),
    PAYMENT_METHOD NVARCHAR(50) NOT NULL,
    PAYMENT_STATUS NVARCHAR(50) NOT NULL,
    PAYMENT_DATE DATE NOT NULL,
    AMOUNT DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMER(CUSTOMER_ID)
);

CREATE TABLE FINES (
    CUSTOMER_ID INT,
    FINE_ID INT PRIMARY KEY IDENTITY(1,1),
    PAYMENT_ID INT,
    PAYMENT_METHOD NVARCHAR(50),
    PAYMENT_STATUS NVARCHAR(50),
    PAYMENT_DATE DATE,
    AMOUNT DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMER(CUSTOMER_ID),
    FOREIGN KEY (PAYMENT_ID) REFERENCES PAYMENT(PAYMENT_ID)
);

CREATE TABLE INSURANCE (
    CAR_ID INT,
    INSURANCE_ID INT PRIMARY KEY IDENTITY(1,1),
    INSURANCE_START_DATE DATE NOT NULL,
    INSURANCE_END_DATE DATE NOT NULL,
    COMPANY_COVERING_NAME NVARCHAR(100) NOT NULL,
    COVERAGE_TYPE NVARCHAR(100) NOT NULL,
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID)
);

CREATE TABLE RESERVATIONS (
    CUSTOMER_ID INT,
    RESERVATION_ID INT PRIMARY KEY IDENTITY(1,1),
    RESERVATION_STATUS NVARCHAR(50) NOT NULL,
    RESERVATION_START_DATE DATE NOT NULL,
    RESERVATION_END_DATE DATE NOT NULL,
    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMER(CUSTOMER_ID)
);

CREATE TABLE COMPLAINTS (
    COMPLAINT_ID INT PRIMARY KEY IDENTITY(1,1),
    COMPLAINT_DATE DATE NOT NULL,
    PROBLEM_DESCRIPTION NVARCHAR(MAX) NOT NULL,
    COMPLAINT_STATUTS NVARCHAR(50) NOT NULL,
    ADMINISTRATOR_ID INT,
    CUSTOMER_ID INT,
    FOREIGN KEY (ADMINISTRATOR_ID) REFERENCES ADMINISTRATOR(ADMINISTRATOR_ID),
    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMER(CUSTOMER_ID)
);

CREATE TABLE ADMINISTRATOR_PHONE_NUMBERS (
    ADMINISTRATOR_ID INT,
    PHONE_NUMBERS NVARCHAR(20),
    PRIMARY KEY (ADMINISTRATOR_ID, PHONE_NUMBERS),
    FOREIGN KEY (ADMINISTRATOR_ID) REFERENCES ADMINISTRATOR(ADMINISTRATOR_ID) ON DELETE CASCADE
);

CREATE TABLE CAR_REPAIRINGS (
    MAINTENANCE_STAFF_ID INT,
    CAR_ID INT,
    PRIMARY KEY (MAINTENANCE_STAFF_ID, CAR_ID),
    FOREIGN KEY (MAINTENANCE_STAFF_ID) REFERENCES MAINTENANCE_STAFF(MAINTENANCE_STAFF_ID),
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID)
);


CREATE TABLE BUYING_RENTING (
    CAR_ID INT,
    CUSTOMER_ID INT,
    PRIMARY KEY (CAR_ID, CUSTOMER_ID),
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID),
    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMER(CUSTOMER_ID)
);

-- ############################################
-- NEW: LOGGING TABLES
-- ############################################

CREATE TABLE ACTIVITY_LOG (
    LOG_ID INT PRIMARY KEY IDENTITY(1,1),
    ACTION NVARCHAR(100),
    DESCRIPTION NVARCHAR(MAX),
    TIMESTAMP DATETIME DEFAULT GETDATE(),
    TYPE NVARCHAR(50) -- 'success', 'info', 'warning', 'error'
);

CREATE TABLE TRANSACTION_LOG (
    TRANS_ID INT PRIMARY KEY IDENTITY(1,1),
    PAYMENT_ID INT,
    CUSTOMER_NAME NVARCHAR(100),
    CAR_NAME NVARCHAR(200),
    AMOUNT DECIMAL(10,2),
    DATE DATETIME,
    STATUS NVARCHAR(50),
    FOREIGN KEY (PAYMENT_ID) REFERENCES PAYMENT(PAYMENT_ID)
);

CREATE TABLE RENTALS (
    RENTAL_ID INT PRIMARY KEY IDENTITY(1,1),
    CAR_ID INT,
    CUSTOMER_ID INT,
    RENTAL_DATE DATE,
    RETURN_DATE DATE,
    STATUS NVARCHAR(50), -- 'Active', 'Returned', 'Overdue'
    FOREIGN KEY (CAR_ID) REFERENCES CAR(CAR_ID),
    FOREIGN KEY (CUSTOMER_ID) REFERENCES CUSTOMER(CUSTOMER_ID)
);

-- ############################################
-- 3. INSERT SAMPLE DATA
-- ############################################

SET IDENTITY_INSERT CAR ON;
INSERT INTO CAR (CAR_ID, CAR_NAME, BRAND, YEAR, PRICE, MAIN_IMAGE, TRANSMISSION, FUEL_TYPE, ENGINE, SEATS, COLOR, MILEAGE, MIN_DEPOSIT, MONTHLY_INSTALLMENT, DESCRIPTION, DATE_ADDED) VALUES
(1, 'Suzuki S Presso Automatic 2024', 'Suzuki', '2024', '549,900', 'https://th.bing.com/th/id/R.25774d40b1c35cb48942f0539711919a?rik=M5aPe82bLpYx2g&pid=ImgRaw&r=0', 'Automatic', 'Petrol', '1.0L 3-Cylinder', '5', 'White, Yellow, Orange, Grey', '0', '82,485', '15,386', 'The Suzuki S-Presso is a compact urban car...', '2025-09-01'),
(2, 'Nissan Sunny Manual / Baseline 2026', 'Nissan', '2026', '645,000', 'https://th.bing.com/th/id/R.b02f1a726bea6c3c8cf11b64ebf2b814?rik=7Om8yQcvA3XXLQ&pid=ImgRaw&r=0', 'Manual', 'Petrol', '1.5L 4-Cylinder', '5', 'Silver', '0', '96,750', '18,047', 'The Nissan Sunny offers reliability and practicality...', '2025-09-15'),
(3, 'Mercedes-Benz C-Class 2025', 'Mercedes-Benz', '2025', '1,250,000', 'https://th.bing.com/th/id/R.ea2598311ae4fcfa4bc6f38f28339f59?rik=mYZ6%2f%2fZOVFKjAw%26pid=ImgRaw%26r=0', 'Automatic', 'Petrol', '2.0L Turbo 4-Cylinder', '5', 'Black', '0', '187,500', '35,000', 'Experience luxury and performance...', '2025-10-01'),
(4, 'BMW X3 xDrive 2025', 'BMW', '2025', '1,450,000', 'https://th.bing.com/th/id/R.beac5fc8db7e5d99126a87faee56c1f8?rik=GHEnRCJ4EcjnbA%26pid=ImgRaw%26r=0', 'Automatic', 'Diesel', '2.0L Turbo Diesel', '5', 'Blue', '0', '217,500', '40,600', 'The BMW X3 combines luxury and sport...', '2025-10-15'),
(5, 'Proton-Saga A/T Premium 2026', 'Proton', '2026', '649,900', 'https://th.bing.com/th/id/R.5b76cb5528ea10c993d03a20a70a0fdf?rik=DEhv1u8GzBNLJw%26pid=ImgRaw%26r=0', 'Automatic', 'Petrol', '1.3L 4-Cylinder', '5', 'Red, White, Black', '0', '97,485', '18,186', 'The Proton Saga offers exceptional value...', '2025-11-01'),
(6, 'MG ZS Standard 2025', 'MG', '2025', '685,000', 'https://static.carsguide.com.au/static/images/car-logos/mg-logo.png', 'Automatic', 'Hybrid', '1.5L Hybrid', '5', 'Black, Silver, Red', '0', '102,750', '19,167', 'The MG ZS combines modern hybrid technology...', '2025-11-10'),
(7, 'Toyota Corolla GLi 2025', 'Toyota', '2025', '785,000', 'https://www.toyota.com.eg/media/gamma/models/corolla/exterior/colors/white.png', 'Automatic', 'Petrol', '1.6L 4-Cylinder', '5', 'White, Silver, Blue', '0', '117,750', '21,972', 'Renowned for its legendary reliability...', '2025-11-20'),
(8, 'Hyundai Tucson 2025', 'Hyundai', '2025', '895,000', 'https://www.hyundai.com/content/dam/hyundai/eg/en/data/vehicle-thumbnail/tucson-2024-thumbnail.png', 'Automatic', 'Diesel', '2.0L Turbo Diesel', '5', 'Grey, White, Black', '0', '134,250', '25,042', 'Premium SUV experience...', '2025-11-25'),
(9, 'Kia Sportage LX 2025', 'Kia', '2025', '920,000', 'https://www.kia.com/content/dam/kwp/eg/en/data/vehicles/sportage-nq5/kia-sportage-nq5-front-view.png', 'Automatic', 'Hybrid', '1.6L Turbo Hybrid', '5', 'Red, Silver, Black', '0', '138,000', '25,744', 'Perfect blend of hybrid efficiency...', '2025-12-01'),
(10, 'Chery Tiggo 7 Comfort 2025', 'Chery', '2025', '740,000', 'https://tse2.mm.bing.net/th/id/OIP.PtZLPetK2-EnEJ9HfF9IKgHaE7?cb=ucfimg2&ucfimg=1&rs=1&pid=ImgDetMain&o=7&rm=3', 'Automatic', 'Petrol', '1.5L Turbo', '5', 'Blue, White, Grey', '0', '111,000', '20,711', 'Combines affordability and power...', '2025-12-08'),
(11, 'Renault Duster 2025', 'Renault', '2025', '670,000', 'https://th.bing.com/th/id/OIP.PieiSRtjHUFCHokPbrAfRgHaEo?w=228&h=180&c=7&r=0&o=7&cb=ucfimg2&dpr=1.4&pid=1.7&rm=3&ucfimg=1', 'Automatic', 'Petrol', '1.6L 4-Cylinder', '5', 'Orange, White, Grey', '0', '100,500', '18,750', 'Built for adventure...', '2025-12-11'),
(12, 'Citroen C3 2025', 'Citroen', '2025', '830,000', 'https://tse3.mm.bing.net/th/id/OIP.DYQmziuKuuaHqe0P7lkgIgHaE8?cb=ucfimg2&ucfimg=1&rs=1&pid=ImgDetMain&o=7&rm=3', 'Automatic', 'Hybrid', '1.2L PureTech Hybrid', '5', 'Yellow, White, Red', '0', '124,500', '23,233', 'French flair and efficiency...', '2025-12-13');
SET IDENTITY_INSERT CAR OFF;

-- ============================================
-- INSERT PARTNERS DATA
-- ============================================
INSERT INTO PARTNERS (BRAND_NAME, LOGO_URL) VALUES
('Mercedes-Benz', 'https://upload.wikimedia.org/wikipedia/commons/9/90/Mercedes-Logo.svg'),
('BMW', 'https://upload.wikimedia.org/wikipedia/commons/f/f4/BMW_logo_%28gray%29.svg'),
('Volvo', 'https://www.pngall.com/wp-content/uploads/15/Volvo-Logo-PNG-Photos.png'),
('Toyota', 'https://www.freepnglogos.com/uploads/toyota-logo-png/toyota-logos-brands-10.png'),
('Audi', 'https://th.bing.com/th/id/R.3ff8e4fce59fc30d12fb7707322798cb?rik=kR7PM0FrbZVedg&riu=http%3a%2f%2frgbcarparts.com%2fcdn%2fshop%2fcollections%2faudi-logo_1200x1200.png%3fv%3d1682731815&ehk=7NVPsJA1SN4zyMAQPCnNfwq8buZZ2feaFF21g9JYzPM%3d&risl=&pid=ImgRaw&r=0'),
('Suzuki', 'https://companieslogo.com/img/orig/7269.T-0bd2cd54.png?t=1729487988'),
('Nissan', 'https://companieslogo.com/img/orig/7201.T-001f0258.png?t=1746046233.svg'),
('Hyundai', 'https://upload.wikimedia.org/wikipedia/commons/4/44/Hyundai_Motor_Company_logo.svg'),
('Kia', 'https://companieslogo.com/img/orig/000270.KS-3c80d4c1.png?t=1720244489'),
('Chery', 'https://cdn.pnggallery.com/wp-content/uploads/chery-logo-01.png'),
('Citroen', 'https://www.pngplay.com/wp-content/uploads/13/Citroen-Logo-Transparent-Images.png');

-- ############################################
-- INSERT REST OF DATA
-- ############################################

INSERT INTO ADMINISTRATOR (ADMINISTRATOR_FNAME, ADMINISTRATOR_MNAME, ADMINISTRATOR_LNAME, ADMINISTRATOR_EMAIL, ADMINISTRATOR_PASSWORD, ROLE) VALUES
('John', 'Michael', 'Smith', 'john.smith@carmanagement.com', 'hashed_password_1', 'Super Admin'),
('Sarah', 'Anne', 'Johnson', 'sarah.johnson@carmanagement.com', 'hashed_password_2', 'Admin'),
('Michael', 'James', 'Williams', 'michael.williams@carmanagement.com', 'hashed_password_3', 'Manager');

INSERT INTO CUSTOMER (FNAME, MNAME, LNAME, CUSTOMER_EMAIL, CUSTOMER_PASSWORD, ADDRESS, ADMINISTRATOR_ID) VALUES
('Robert', 'Paul', 'Anderson', 'robert.anderson@email.com', 'pass_hash_1', '123 Main St, Cairo, Egypt', 1),
('Jennifer', 'Marie', 'Taylor', 'jennifer.taylor@email.com', 'pass_hash_2', '456 Oak Ave, Cairo, Egypt', 1),
('Ahmed', 'Mohammed', 'Ali', 'ahmed.ali@email.com', 'pass_hash_3', '789 Nile Corniche, Cairo, Egypt', 1);

INSERT INTO PAYMENT (CUSTOMER_ID, PAYMENT_METHOD, PAYMENT_STATUS, PAYMENT_DATE, AMOUNT) VALUES
(1, 'Credit Card', 'Completed', '2025-01-15', 15386.00),
(2, 'Bank Transfer', 'Completed', '2025-02-20', 18047.00),
(1, 'Cash', 'Completed', '2025-03-05', 12000.00),
(3, 'Credit Card', 'Completed', '2025-04-10', 35000.00),
(1, 'Credit Card', 'Completed', '2025-05-15', 15386.00),
(2, 'Bank Transfer', 'Completed', '2025-06-20', 18047.00),
(3, 'Cash', 'Completed', '2025-07-25', 25000.00),
(1, 'Credit Card', 'Completed', '2025-08-30', 20000.00),
(2, 'Credit Card', 'Completed', '2025-09-10', 50000.00),
(3, 'Bank Transfer', 'Completed', '2025-10-05', 45000.00),
(1, 'Cash', 'Completed', '2025-11-12', 15000.00),
(2, 'Credit Card', 'Completed', '2025-12-01', 60000.00);

-- Insert into BUYING_RENTING table to link cars to customers (for sales data)
INSERT INTO BUYING_RENTING (CAR_ID, CUSTOMER_ID) VALUES 
(1, 1), -- Suzuki
(2, 2), -- Nissan
(3, 3), -- Mercedes
(4, 1), -- BMW
(6, 2), -- MG
(7, 3), -- Toyota
(1, 2), -- Suzuki
(5, 1); -- Proton

-- Insert sample data into RENTALS
INSERT INTO RENTALS (CAR_ID, CUSTOMER_ID, RENTAL_DATE, RETURN_DATE, STATUS) VALUES
(8, 1, DATEADD(day, -2, GETDATE()), DATEADD(day, 5, GETDATE()), 'Active'), -- Hyundai Tucson currently rented
(9, 2, DATEADD(day, -10, GETDATE()), DATEADD(day, -3, GETDATE()), 'Returned'), -- Kia Sportage returned
(10, 3, DATEADD(day, -1, GETDATE()), DATEADD(day, 6, GETDATE()), 'Active'), -- Chery Tiggo currently rented
(11, 1, DATEADD(day, -5, GETDATE()), DATEADD(day, 2, GETDATE()), 'Active'), -- Renault Duster currently rented
(12, 2, DATEADD(day, -20, GETDATE()), DATEADD(day, -15, GETDATE()), 'Returned'); -- Citroen C3 returned

-- Insert sample data into ACTIVITY_LOG
INSERT INTO ACTIVITY_LOG (ACTION, DESCRIPTION, TIMESTAMP, TYPE) VALUES
('New Car Added', 'Car: Suzuki S Presso Automatic 2024 added to inventory', '2025-01-10', 'success'),
('New Car Added', 'Car: Nissan Sunny Manual / Baseline 2026 added to inventory', '2025-01-12', 'success'),
('Sale Completed', 'Car sold to Robert Anderson', '2025-01-15', 'success'),
('Sale Completed', 'Car sold to Jennifer Taylor', '2025-02-20', 'success'),
('Payment Received', 'Payment of EGP 15386.00 received', '2025-01-15', 'success'),
('Payment Received', 'Payment of EGP 18047.00 received', '2025-02-20', 'success');

-- Insert sample data into TRANSACTION_LOG
INSERT INTO TRANSACTION_LOG (PAYMENT_ID, CUSTOMER_NAME, CAR_NAME, AMOUNT, DATE, STATUS) VALUES
(1, 'Robert Anderson', 'Suzuki S Presso Automatic 2024', 15386.00, '2025-01-15', 'Completed'),
(2, 'Jennifer Taylor', 'Nissan Sunny Manual / Baseline 2026', 18047.00, '2025-02-20', 'Completed');
