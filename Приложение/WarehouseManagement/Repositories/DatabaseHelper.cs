using Microsoft.Data.SqlClient;

namespace WarehouseManagement.Repositories;

public static class DatabaseHelper
{
    private static readonly string MasterConnectionString = @"Server=(localdb)\mssqllocaldb;Integrated Security=true;";
    private static readonly string DatabaseConnectionString = @"Server=(localdb)\mssqllocaldb;Database=WarehouseManagementDB;Integrated Security=true;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(DatabaseConnectionString);
    }

    public static void InitializeDatabase()
    {
        using var conn = new SqlConnection(MasterConnectionString);
        conn.Open();

        // Создаем БД если её нет
        using var createDbCmd = new SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WarehouseManagementDB')
            CREATE DATABASE WarehouseManagementDB", conn);
        createDbCmd.ExecuteNonQuery();

        conn.Close();

        // Подключаемся к новой БД
        using var dbConn = new SqlConnection(DatabaseConnectionString);
        dbConn.Open();

        var commands = new[]
        {
            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
              CREATE TABLE Roles (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  Name NVARCHAR(50) NOT NULL UNIQUE,
                  Description NVARCHAR(500),
                  IsActive BIT DEFAULT 1
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Permissions')
              CREATE TABLE Permissions (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  Name NVARCHAR(100) NOT NULL UNIQUE,
                  Description NVARCHAR(500)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RolePermissions')
              CREATE TABLE RolePermissions (
                  RoleId INT NOT NULL,
                  PermissionId INT NOT NULL,
                  PRIMARY KEY (RoleId, PermissionId),
                  FOREIGN KEY (RoleId) REFERENCES Roles(Id),
                  FOREIGN KEY (PermissionId) REFERENCES Permissions(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
              CREATE TABLE Categories (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  Name NVARCHAR(100) NOT NULL,
                  Description NVARCHAR(500),
                  IsActive BIT DEFAULT 1
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Suppliers')
              CREATE TABLE Suppliers (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  Name NVARCHAR(100) NOT NULL,
                  ContactPerson NVARCHAR(100),
                  Phone NVARCHAR(20),
                  Email NVARCHAR(100),
                  Address NVARCHAR(500),
                  IsActive BIT DEFAULT 1
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
              CREATE TABLE Products (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  Article NVARCHAR(50) NOT NULL UNIQUE,
                  Name NVARCHAR(200) NOT NULL,
                  CategoryId INT NOT NULL,
                  Unit NVARCHAR(20),
                  Price DECIMAL(10,2),
                  Quantity INT DEFAULT 0,
                  MinQuantity INT DEFAULT 0,
                  Description NVARCHAR(500),
                  SupplierId INT,
                  CreatedDate DATETIME DEFAULT GETDATE(),
                  IsActive BIT DEFAULT 1,
                  FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
                  FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IncomeTransactions')
              CREATE TABLE IncomeTransactions (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  ProductId INT NOT NULL,
                  Quantity INT NOT NULL,
                  Price DECIMAL(10,2),
                  SupplierId INT,
                  TransactionDate DATETIME DEFAULT GETDATE(),
                  Notes NVARCHAR(500),
                  CreatedByUserId INT,
                  FOREIGN KEY (ProductId) REFERENCES Products(Id),
                  FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutcomeTransactions')
              CREATE TABLE OutcomeTransactions (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  ProductId INT NOT NULL,
                  Quantity INT NOT NULL,
                  Price DECIMAL(10,2),
                  TransactionDate DATETIME DEFAULT GETDATE(),
                  Reason NVARCHAR(100),
                  Notes NVARCHAR(500),
                  CreatedByUserId INT,
                  FOREIGN KEY (ProductId) REFERENCES Products(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AppUsers')
              CREATE TABLE AppUsers (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  Login NVARCHAR(50) NOT NULL UNIQUE,
                  PasswordHash NVARCHAR(255) NOT NULL,
                  FullName NVARCHAR(100),
                  Email NVARCHAR(100),
                  RoleId INT DEFAULT 2,
                  IsActive BIT DEFAULT 1,
                  CreatedAt DATETIME DEFAULT GETDATE(),
                  LastLoginAt DATETIME,
                  PreferredLanguage NVARCHAR(10) DEFAULT 'ru',
                  FOREIGN KEY (RoleId) REFERENCES Roles(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserActionLogs')
              CREATE TABLE UserActionLogs (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  UserId INT NOT NULL,
                  UserName NVARCHAR(100),
                  Action NVARCHAR(100),
                  Details NVARCHAR(500),
                  ActionDate DATETIME DEFAULT GETDATE(),
                  IpAddress NVARCHAR(50),
                  FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Admin')
              INSERT INTO Roles (Name, Description, IsActive) VALUES ('Admin', 'Administrator with full access', 1)",

            @"IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'User')
              INSERT INTO Roles (Name, Description, IsActive) VALUES ('User', 'Regular user with limited access', 1)",

            @"IF NOT EXISTS (SELECT * FROM Permissions WHERE Name = 'VIEW_PRODUCTS')
              INSERT INTO Permissions (Name, Description) VALUES
              ('VIEW_PRODUCTS', 'View products'),
              ('EDIT_PRODUCTS', 'Edit products'),
              ('DELETE_PRODUCTS', 'Delete products'),
              ('VIEW_CATEGORIES', 'View categories'),
              ('EDIT_CATEGORIES', 'Edit categories'),
              ('DELETE_CATEGORIES', 'Delete categories'),
              ('VIEW_SUPPLIERS', 'View suppliers'),
              ('EDIT_SUPPLIERS', 'Edit suppliers'),
              ('DELETE_SUPPLIERS', 'Delete suppliers'),
              ('VIEW_INCOME', 'View income transactions'),
              ('CREATE_INCOME', 'Create income transactions'),
              ('VIEW_OUTCOME', 'View outcome transactions'),
              ('CREATE_OUTCOME', 'Create outcome transactions'),
              ('VIEW_LOGS', 'View action logs'),
              ('VIEW_USERS', 'View users'),
              ('MANAGE_USERS', 'Manage users')",

            @"IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = 1)
              INSERT INTO RolePermissions (RoleId, PermissionId)
              SELECT 1, Id FROM Permissions",

            @"IF NOT EXISTS (SELECT * FROM RolePermissions WHERE RoleId = 2)
              INSERT INTO RolePermissions (RoleId, PermissionId)
              SELECT 2, Id FROM Permissions WHERE Name IN ('VIEW_PRODUCTS', 'VIEW_CATEGORIES', 'VIEW_SUPPLIERS', 'VIEW_INCOME', 'VIEW_OUTCOME', 'VIEW_LOGS')",

            @"IF NOT EXISTS (SELECT * FROM AppUsers WHERE Login = 'admin')
              INSERT INTO AppUsers (Login, PasswordHash, FullName, Email, RoleId)
              VALUES ('admin', 'ac9689e2272427085e35b9d3e3e8bed88cb3434828b43b86fc0596cad4c6e270', 'Administrator', 'admin@warehouse.com', 1)",

            @"IF NOT EXISTS (SELECT * FROM AppUsers WHERE Login = 'user')
              INSERT INTO AppUsers (Login, PasswordHash, FullName, Email, RoleId)
              VALUES ('user', 'e606e38b0d8c19b24cf0ee3808183162ea7cd63ff7912dbb22b5e803286b4446', 'Regular User', 'user@warehouse.com', 2)",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PriceHistory')
              CREATE TABLE PriceHistory (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  ProductId INT NOT NULL,
                  OldPrice DECIMAL(10,2),
                  NewPrice DECIMAL(10,2) NOT NULL,
                  ChangedByUserId INT,
                  ChangedAt DATETIME DEFAULT GETDATE(),
                  Reason NVARCHAR(200),
                  FOREIGN KEY (ProductId) REFERENCES Products(Id),
                  FOREIGN KEY (ChangedByUserId) REFERENCES AppUsers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TwoFactorSettings')
              CREATE TABLE TwoFactorSettings (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  UserId INT NOT NULL UNIQUE,
                  IsEnabled BIT DEFAULT 0,
                  Method NVARCHAR(20) DEFAULT 'Email',
                  PhoneNumber NVARCHAR(20),
                  SecretKey NVARCHAR(255),
                  BackupCodes NVARCHAR(MAX),
                  CreatedAt DATETIME DEFAULT GETDATE(),
                  UpdatedAt DATETIME DEFAULT GETDATE(),
                  FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TwoFactorAttempts')
              CREATE TABLE TwoFactorAttempts (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  UserId INT NOT NULL,
                  Code NVARCHAR(10) NOT NULL,
                  CreatedAt DATETIME DEFAULT GETDATE(),
                  ExpiresAt DATETIME NOT NULL,
                  IsUsed BIT DEFAULT 0,
                  AttemptCount INT DEFAULT 0,
                  FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
              CREATE TABLE Notifications (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  UserId INT NOT NULL,
                  Title NVARCHAR(200) NOT NULL,
                  Message NVARCHAR(500) NOT NULL,
                  Type NVARCHAR(50),
                  IsRead BIT DEFAULT 0,
                  CreatedAt DATETIME DEFAULT GETDATE(),
                  ActionUrl NVARCHAR(500),
                  FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'NotificationPreferences')
              CREATE TABLE NotificationPreferences (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  UserId INT NOT NULL UNIQUE,
                  StockAlerts BIT DEFAULT 1,
                  PriceAlerts BIT DEFAULT 1,
                  OrderAlerts BIT DEFAULT 1,
                  SystemAlerts BIT DEFAULT 1,
                  DeliveryMethod NVARCHAR(50) DEFAULT 'InApp',
                  FOREIGN KEY (UserId) REFERENCES AppUsers(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserPurchaseHistory')
              CREATE TABLE UserPurchaseHistory (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  UserId INT NOT NULL,
                  ProductId INT NOT NULL,
                  PurchaseDate DATETIME DEFAULT GETDATE(),
                  Quantity INT NOT NULL,
                  Price DECIMAL(10,2),
                  FOREIGN KEY (UserId) REFERENCES AppUsers(Id),
                  FOREIGN KEY (ProductId) REFERENCES Products(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ProductSimilarity')
              CREATE TABLE ProductSimilarity (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  ProductId1 INT NOT NULL,
                  ProductId2 INT NOT NULL,
                  SimilarityScore DECIMAL(5,4),
                  CreatedAt DATETIME DEFAULT GETDATE(),
                  FOREIGN KEY (ProductId1) REFERENCES Products(Id),
                  FOREIGN KEY (ProductId2) REFERENCES Products(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DemandForecasts')
              CREATE TABLE DemandForecasts (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  ProductId INT NOT NULL,
                  ForecastDate DATE NOT NULL,
                  ForecastedQuantity INT,
                  Confidence DECIMAL(5,4),
                  Method NVARCHAR(50),
                  CreatedAt DATETIME DEFAULT GETDATE(),
                  FOREIGN KEY (ProductId) REFERENCES Products(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistoricalDemand')
              CREATE TABLE HistoricalDemand (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  ProductId INT NOT NULL,
                  Date DATE NOT NULL,
                  ActualQuantity INT,
                  ForecastedQuantity INT,
                  Error DECIMAL(10,2),
                  FOREIGN KEY (ProductId) REFERENCES Products(Id)
              )",

            @"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Backups')
              CREATE TABLE Backups (
                  Id INT PRIMARY KEY IDENTITY(1,1),
                  BackupName NVARCHAR(255) NOT NULL,
                  BackupPath NVARCHAR(500) NOT NULL,
                  BackupDate DATETIME DEFAULT GETDATE(),
                  BackupSize BIGINT,
                  BackupType NVARCHAR(50),
                  IsAutomatic BIT DEFAULT 1,
                  IsRestored BIT DEFAULT 0
              )"
        };

        foreach (var cmd in commands)
        {
            using var command = new SqlCommand(cmd, dbConn);
            command.ExecuteNonQuery();
        }
    }
}

