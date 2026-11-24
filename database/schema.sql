-- ============================================================
-- StockTrack Database Schema
-- Compatible with MariaDB and MySQL (XAMPP / Local)
-- ============================================================

CREATE DATABASE IF NOT EXISTS StockTrackDB
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE StockTrackDB;

-- 1. Users Table (FR-01, FR-02, FR-03)
DROP TABLE IF EXISTS AuditLogs;
DROP TABLE IF EXISTS StockTransactions;
DROP TABLE IF EXISTS Products;
DROP TABLE IF EXISTS Suppliers;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS Users;

CREATE TABLE Users (
    UserID INT NOT NULL AUTO_INCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100) NOT NULL,
    Role ENUM('Admin', 'Inventory Manager', 'Staff') NOT NULL,
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Categories Table (FR-06)
CREATE TABLE Categories (
    CategoryID INT NOT NULL AUTO_INCREMENT,
    CategoryName VARCHAR(100) NOT NULL UNIQUE,
    Description VARCHAR(255),
    PRIMARY KEY (CategoryID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Suppliers Table (FR-07)
CREATE TABLE Suppliers (
    SupplierID INT NOT NULL AUTO_INCREMENT,
    SupplierName VARCHAR(150) NOT NULL,
    ContactPerson VARCHAR(100),
    Email VARCHAR(100),
    Phone VARCHAR(50),
    Address VARCHAR(255),
    PRIMARY KEY (SupplierID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4. Products Table (FR-04, FR-05, FR-11, FR-12)
CREATE TABLE Products (
    ProductID INT NOT NULL AUTO_INCREMENT,
    SKU VARCHAR(50) NOT NULL UNIQUE,
    ProductName VARCHAR(150) NOT NULL,
    CategoryID INT NOT NULL,
    SupplierID INT NOT NULL,
    UnitPrice DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    Quantity INT NOT NULL DEFAULT 0,
    ReorderLevel INT NOT NULL DEFAULT 10,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (ProductID),
    CONSTRAINT fk_products_category FOREIGN KEY (CategoryID) REFERENCES Categories (CategoryID) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_products_supplier FOREIGN KEY (SupplierID) REFERENCES Suppliers (SupplierID) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5. Stock Transactions Table (FR-08, FR-09, FR-10, FR-18)
CREATE TABLE StockTransactions (
    TransactionID INT NOT NULL AUTO_INCREMENT,
    ProductID INT NOT NULL,
    TransactionType ENUM('IN', 'OUT', 'ADJUSTMENT') NOT NULL,
    Quantity INT NOT NULL,
    Reason VARCHAR(255),
    UserID INT NOT NULL,
    TransactionDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (TransactionID),
    CONSTRAINT fk_trans_product FOREIGN KEY (ProductID) REFERENCES Products (ProductID) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_trans_user FOREIGN KEY (UserID) REFERENCES Users (UserID) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 6. Audit Trail Table (FR-16)
CREATE TABLE AuditLogs (
    LogID INT NOT NULL AUTO_INCREMENT,
    UserID INT NOT NULL,
    ActionType VARCHAR(50) NOT NULL,
    Details TEXT,
    LogTimestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (LogID),
    CONSTRAINT fk_audit_user FOREIGN KEY (UserID) REFERENCES Users (UserID) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ============================================================
-- Seed Sample Data
-- Default Users:
--   admin / admin123 (Admin)
--   manager / manager123 (Inventory Manager)
--   staff / staff123 (Staff)
-- Passwords stored with SHA-256 (matches secure storage requirement)
-- ============================================================

INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive) VALUES
('admin', SHA2('admin123', 256), 'System Administrator', 'Admin', 1),
('manager', SHA2('manager123', 256), 'Alex Inventory Manager', 'Inventory Manager', 1),
('staff', SHA2('staff123', 256), 'Sam Store Clerk', 'Staff', 1);

INSERT INTO Categories (CategoryName, Description) VALUES
('Food Staples & Grains', 'Wholesale sugar, rice, wheat flour, and maize flour'),
('Edible Oils & Fats', 'Pure vegetable cooking oil and refined fats in bulk'),
('Cleaning & Hygiene', 'Laundry bar soaps, detergent powders, and cleaners'),
('Beverages & Drinks', 'Mineral water, bottled juices, and packaged soft drinks'),
('Snacks & Confectionery', 'Packaged biscuits, confectionery, and baked goods');

INSERT INTO Suppliers (SupplierName, ContactPerson, Email, Phone, Address) VALUES
('Kakira Sugar Works Ltd', 'James Mukasa', 'sales@kakirasugar.com', '+256 414 123 456', 'Plot 12 Jinja Rd, Kakira'),
('Bakhresa Grain Millers', 'Fatuma Hassan', 'orders@bakhresa.com', '+256 414 789 012', 'Industrial Area, Kampala'),
('Mukwano Industries Uganda', 'Robert Ssemwogerere', 'wholesale@mukwano.com', '+256 414 345 678', 'Plot 30 Mukwano Complex, Kibuli'),
('Rwenzori Bottling Company', 'Grace Nakato', 'dispatch@rwenzoribev.com', '+256 414 901 234', 'Plot 4 Namanve Industrial Estate'),
('Britania Allied Industries', 'David Ochieng', 'orders@britania.com', '+256 414 567 890', 'Plot 8 Ntinda Road, Kampala');

INSERT INTO Products (SKU, ProductName, CategoryID, SupplierID, UnitPrice, Quantity, ReorderLevel) VALUES
('WHL-001', 'Super White Sugar (50kg Bag)', 1, 1, 48.50, 45, 15),
('WHL-002', 'Premium Basmati Rice (25kg Bag)', 1, 2, 32.00, 60, 20),
('WHL-003', 'Pure Refined Cooking Oil (20L Jerrycan)', 2, 3, 28.00, 25, 10),
('WHL-004', 'All-Purpose Wheat Flour (50kg Bag)', 1, 2, 36.50, 8, 12),      -- Low stock!
('WHL-005', 'Multipurpose Laundry Soap (Box of 24 Bars)', 3, 3, 16.50, 80, 25),
('WHL-006', 'Sparkling Mineral Water (Pack of 24 x 500ml)', 4, 4, 7.50, 110, 30),
('WHL-007', 'Sweet Tea Biscuits (Carton of 48 Packs)', 5, 5, 14.00, 4, 10),       -- Low stock!
('WHL-008', 'Powder Detergent (Bucket of 10kg)', 3, 3, 19.00, 35, 15);

-- Initial audit log entry
INSERT INTO AuditLogs (UserID, ActionType, Details) VALUES
(1, 'DATABASE_INIT', 'JamboStock Wholesale & Warehouse database initialized.');
