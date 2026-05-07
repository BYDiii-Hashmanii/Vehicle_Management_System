-- ============================================================
--   VEHICLE SERVICE & MAINTENANCE SYSTEM
--   SQL Server Database Script (Student Version)
--   Includes: Tables, Stored Procedures, Functions, Triggers
-- ============================================================


USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'VehicleServiceDB')
    CREATE DATABASE VehicleServiceDB;
GO

USE VehicleServiceDB;
GO

-- ============================================================
--  1. TABLES
-- ============================================================

-- -------------------------------------------------------
-- Users (Admin / Staff)
-- -------------------------------------------------------
CREATE TABLE Users (
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    Username      VARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash VARCHAR(256) NOT NULL,
    FullName      VARCHAR(100) NOT NULL,
    Role          VARCHAR(20)  NOT NULL CHECK (Role IN ('Admin', 'Staff')),
    Email         VARCHAR(100),
    Phone         VARCHAR(20),
    IsActive      BIT          NOT NULL DEFAULT 1,
    CreatedAt     DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

-- -------------------------------------------------------
-- Customers
-- -------------------------------------------------------
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    FullName   VARCHAR(100) NOT NULL,
    Email      VARCHAR(100),
    Phone      VARCHAR(20)  NOT NULL,
    Address    VARCHAR(255),
    CreatedAt  DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

-- -------------------------------------------------------
-- Vehicles
-- -------------------------------------------------------
CREATE TABLE Vehicles (
    VehicleID    INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID   INT          NOT NULL REFERENCES Customers(CustomerID),
    Make         VARCHAR(50)  NOT NULL,
    Model        VARCHAR(50)  NOT NULL,
    Year         INT          NOT NULL CHECK (Year BETWEEN 1900 AND 2100),
    LicensePlate VARCHAR(20)  NOT NULL UNIQUE,
    Mileage      INT          CHECK (Mileage >= 0),
    CreatedAt    DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

-- -------------------------------------------------------
-- ServiceSchedule
-- -------------------------------------------------------
CREATE TABLE ServiceSchedule (
    ScheduleID     INT IDENTITY(1,1) PRIMARY KEY,
    VehicleID      INT          NOT NULL REFERENCES Vehicles(VehicleID),
    CustomerID     INT          NOT NULL REFERENCES Customers(CustomerID),
    ScheduledDate  DATE         NOT NULL,
    ServiceType    VARCHAR(100) NOT NULL,
    Status         VARCHAR(20)  NOT NULL DEFAULT 'Scheduled'
                        CHECK (Status IN ('Scheduled', 'In Progress', 'Completed', 'Cancelled')),
    Notes          VARCHAR(500),
    CreatedBy      INT          NOT NULL REFERENCES Users(UserID),
    CreatedAt      DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

-- -------------------------------------------------------
-- ServiceDetails
-- -------------------------------------------------------
CREATE TABLE ServiceDetails (
    ServiceID    INT IDENTITY(1,1) PRIMARY KEY,
    ScheduleID   INT            NOT NULL REFERENCES ServiceSchedule(ScheduleID),
    VehicleID    INT            NOT NULL REFERENCES Vehicles(VehicleID),
    TechnicianID INT            REFERENCES Users(UserID),
    ServiceDate  DATE           NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    Description  VARCHAR(1000)  NOT NULL,
    Status       VARCHAR(20)    NOT NULL DEFAULT 'Pending'
                     CHECK (Status IN ('Pending', 'In Progress', 'Completed')),
    ServiceCost  DECIMAL(10,2)  NOT NULL DEFAULT 0 CHECK (ServiceCost >= 0),
    PartsUsed    VARCHAR(500),
    CompletedAt  DATETIME,
    CreatedAt    DATETIME       NOT NULL DEFAULT GETDATE()
);
GO

-- -------------------------------------------------------
-- Complaints
-- -------------------------------------------------------
CREATE TABLE Complaints (
    ComplaintID      INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID       INT           NOT NULL REFERENCES Customers(CustomerID),
    VehicleID        INT           REFERENCES Vehicles(VehicleID),
    Subject          VARCHAR(200)  NOT NULL,
    Description      VARCHAR(2000) NOT NULL,
    Status           VARCHAR(20)   NOT NULL DEFAULT 'Open'
                         CHECK (Status IN ('Open', 'In Progress', 'Resolved', 'Closed')),
    AssignedTo       INT           REFERENCES Users(UserID),
    ResolutionNotes  VARCHAR(2000),
    CreatedAt        DATETIME      NOT NULL DEFAULT GETDATE(),
    ResolvedAt       DATETIME
);
GO

-- -------------------------------------------------------
-- AuditLog
-- -------------------------------------------------------
CREATE TABLE AuditLog (
    LogID      INT IDENTITY(1,1) PRIMARY KEY,
    TableName  VARCHAR(50)  NOT NULL,
    Action     VARCHAR(10)  NOT NULL CHECK (Action IN ('INSERT', 'UPDATE', 'DELETE')),
    RecordID   INT          NOT NULL,
    ChangedAt  DATETIME     NOT NULL DEFAULT GETDATE(),
    OldValues  VARCHAR(MAX),
    NewValues  VARCHAR(MAX)
);
GO


-- ============================================================
--  2. STORED PROCEDURES (Renamed to usp_ to avoid system conflicts)
-- ============================================================

-- ===== USERS =====

CREATE OR ALTER PROCEDURE usp_AddUser
    @Username     VARCHAR(50),
    @PasswordHash VARCHAR(256),
    @FullName     VARCHAR(100),
    @Role         VARCHAR(20),
    @Email        VARCHAR(100) = NULL,
    @Phone        VARCHAR(20)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        RAISERROR('Username already exists.', 16, 1); RETURN;
    END
    INSERT INTO Users (Username, PasswordHash, FullName, Role, Email, Phone)
    VALUES (@Username, @PasswordHash, @FullName, @Role, @Email, @Phone);
    SELECT SCOPE_IDENTITY() AS NewUserID;
END;
GO

CREATE OR ALTER PROCEDURE usp_UpdateUser
    @UserID   INT,
    @FullName VARCHAR(100),
    @Role     VARCHAR(20),
    @Email    VARCHAR(100) = NULL,
    @Phone    VARCHAR(20)  = NULL,
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users
    SET FullName = @FullName, Role = @Role,
        Email = @Email, Phone = @Phone, IsActive = @IsActive
    WHERE UserID = @UserID;
END;
GO

CREATE OR ALTER PROCEDURE usp_DeleteUser
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;
    -- Soft delete: just mark inactive
    UPDATE Users SET IsActive = 0 WHERE UserID = @UserID;
END;
GO

-- ===== CUSTOMERS =====

CREATE OR ALTER PROCEDURE usp_AddCustomer
    @FullName VARCHAR(100),
    @Email    VARCHAR(100) = NULL,
    @Phone    VARCHAR(20),
    @Address  VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Customers (FullName, Email, Phone, Address)
    VALUES (@FullName, @Email, @Phone, @Address);
    SELECT SCOPE_IDENTITY() AS NewCustomerID;
END;
GO

CREATE OR ALTER PROCEDURE usp_UpdateCustomer
    @CustomerID INT,
    @FullName   VARCHAR(100),
    @Email      VARCHAR(100) = NULL,
    @Phone      VARCHAR(20),
    @Address    VARCHAR(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Customers
    SET FullName = @FullName, Email = @Email,
        Phone = @Phone, Address = @Address
    WHERE CustomerID = @CustomerID;
END;
GO

CREATE OR ALTER PROCEDURE usp_DeleteCustomer
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Vehicles WHERE CustomerID = @CustomerID)
    BEGIN
        RAISERROR('Cannot delete customer with existing vehicles.', 16, 1); RETURN;
    END
    DELETE FROM Customers WHERE CustomerID = @CustomerID;
END;
GO

-- ===== VEHICLES =====

CREATE OR ALTER PROCEDURE usp_AddVehicle
    @CustomerID   INT,
    @Make         VARCHAR(50),
    @Model        VARCHAR(50),
    @Year         INT,
    @LicensePlate VARCHAR(20),
    @Mileage      INT = 0
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Vehicles (CustomerID, Make, Model, Year, LicensePlate, Mileage)
    VALUES (@CustomerID, @Make, @Model, @Year, @LicensePlate, @Mileage);
    SELECT SCOPE_IDENTITY() AS NewVehicleID;
END;
GO

CREATE OR ALTER PROCEDURE usp_UpdateVehicle
    @VehicleID    INT,
    @Make         VARCHAR(50),
    @Model        VARCHAR(50),
    @Year         INT,
    @LicensePlate VARCHAR(20),
    @Mileage      INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Vehicles
    SET Make = @Make, Model = @Model, Year = @Year,
        LicensePlate = @LicensePlate, Mileage = @Mileage
    WHERE VehicleID = @VehicleID;
END;
GO

CREATE OR ALTER PROCEDURE usp_DeleteVehicle
    @VehicleID INT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM ServiceSchedule
        WHERE VehicleID = @VehicleID AND Status IN ('Scheduled', 'In Progress')
    )
    BEGIN
        RAISERROR('Cannot delete vehicle with active service schedules.', 16, 1); RETURN;
    END
    DELETE FROM Vehicles WHERE VehicleID = @VehicleID;
END;
GO

-- ===== SERVICE SCHEDULE =====

CREATE OR ALTER PROCEDURE usp_ScheduleService
    @VehicleID     INT,
    @CustomerID    INT,
    @ScheduledDate DATE,
    @ServiceType   VARCHAR(100),
    @Notes         VARCHAR(500) = NULL,
    @CreatedBy     INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ServiceSchedule
        (VehicleID, CustomerID, ScheduledDate, ServiceType, Notes, CreatedBy)
    VALUES
        (@VehicleID, @CustomerID, @ScheduledDate, @ServiceType, @Notes, @CreatedBy);
    SELECT SCOPE_IDENTITY() AS NewScheduleID;
END;
GO

CREATE OR ALTER PROCEDURE usp_UpdateScheduleStatus
    @ScheduleID INT,
    @Status     VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ServiceSchedule SET Status = @Status WHERE ScheduleID = @ScheduleID;
END;
GO

-- ===== SERVICE DETAILS =====

CREATE OR ALTER PROCEDURE usp_AddServiceDetails
    @ScheduleID   INT,
    @VehicleID    INT,
    @TechnicianID INT            = NULL,
    @Description  VARCHAR(1000),
    @ServiceCost  DECIMAL(10,2) = 0,
    @PartsUsed    VARCHAR(500)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ServiceDetails
        (ScheduleID, VehicleID, TechnicianID, Description, ServiceCost, PartsUsed)
    VALUES
        (@ScheduleID, @VehicleID, @TechnicianID, @Description, @ServiceCost, @PartsUsed);
    SELECT SCOPE_IDENTITY() AS NewServiceID;
END;
GO

CREATE OR ALTER PROCEDURE usp_UpdateServiceStatus
    @ServiceID   INT,
    @Status      VARCHAR(20),
    @ServiceCost DECIMAL(10,2) = NULL,
    @PartsUsed   VARCHAR(500)  = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ServiceDetails
    SET Status      = @Status,
        ServiceCost = ISNULL(@ServiceCost, ServiceCost),
        PartsUsed   = ISNULL(@PartsUsed,   PartsUsed),
        CompletedAt = CASE WHEN @Status = 'Completed' THEN GETDATE() ELSE CompletedAt END
    WHERE ServiceID = @ServiceID;
END;
GO

-- ===== COMPLAINTS =====

CREATE OR ALTER PROCEDURE usp_AddComplaint
    @CustomerID  INT,
    @VehicleID   INT = NULL,
    @Subject     VARCHAR(200),
    @Description VARCHAR(2000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Complaints (CustomerID, VehicleID, Subject, Description)
    VALUES (@CustomerID, @VehicleID, @Subject, @Description);
    SELECT SCOPE_IDENTITY() AS NewComplaintID;
END;
GO

CREATE OR ALTER PROCEDURE usp_AssignComplaint
    @ComplaintID INT,
    @AssignedTo  INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Complaints
    SET AssignedTo = @AssignedTo, Status = 'In Progress'
    WHERE ComplaintID = @ComplaintID;
END;
GO

CREATE OR ALTER PROCEDURE usp_ResolveComplaint
    @ComplaintID      INT,
    @ResolutionNotes VARCHAR(2000)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Complaints
    SET Status = 'Resolved', ResolutionNotes = @ResolutionNotes, ResolvedAt = GETDATE()
    WHERE ComplaintID = @ComplaintID;
END;
GO

-- ===== REPORTS =====

CREATE OR ALTER PROCEDURE usp_GetServiceReport
    @StartDate DATE = NULL,
    @EndDate   DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        sd.ServiceID,
        c.FullName      AS Customer,
        v.Make + ' ' + v.Model + ' (' + CAST(v.Year AS VARCHAR) + ')' AS Vehicle,
        v.LicensePlate,
        sd.ServiceDate,
        sd.Description,
        sd.Status,
        sd.ServiceCost,
        sd.PartsUsed,
        u.FullName      AS Technician
    FROM ServiceDetails sd
    JOIN Vehicles   v ON sd.VehicleID    = v.VehicleID
    JOIN Customers  c ON v.CustomerID    = c.CustomerID
    LEFT JOIN Users u ON sd.TechnicianID = u.UserID
    WHERE (@StartDate IS NULL OR sd.ServiceDate >= @StartDate)
      AND (@EndDate   IS NULL OR sd.ServiceDate <= @EndDate)
    ORDER BY sd.ServiceDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE usp_GetCustomerHistory
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        c.FullName, c.Phone, c.Email,
        v.Make, v.Model, v.Year, v.LicensePlate,
        sd.ServiceDate, sd.Description, sd.Status, sd.ServiceCost
    FROM Customers c
    JOIN Vehicles        v  ON c.CustomerID = v.CustomerID
    JOIN ServiceDetails sd ON v.VehicleID  = sd.VehicleID
    WHERE c.CustomerID = @CustomerID
    ORDER BY sd.ServiceDate DESC;
END;
GO

CREATE OR ALTER PROCEDURE usp_GetVehicleMaintenanceRecords
    @VehicleID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        v.Make, v.Model, v.Year, v.LicensePlate,
        sd.ServiceDate, sd.Description, sd.Status,
        sd.ServiceCost, sd.PartsUsed,
        u.FullName AS Technician
    FROM Vehicles v
    JOIN ServiceDetails sd ON v.VehicleID    = sd.VehicleID
    LEFT JOIN Users u      ON sd.TechnicianID = u.UserID
    WHERE v.VehicleID = @VehicleID
    ORDER BY sd.ServiceDate DESC;
END;
GO


-- ============================================================
--  3. FUNCTIONS
-- ============================================================

-- Total cost of all completed services for a vehicle
CREATE OR ALTER FUNCTION fn_GetVehicleTotalCost (@VehicleID INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Total DECIMAL(10,2);
    SELECT @Total = ISNULL(SUM(ServiceCost), 0)
    FROM ServiceDetails
    WHERE VehicleID = @VehicleID AND Status = 'Completed';
    RETURN @Total;
END;
GO

-- Count of all services done for a customer
CREATE OR ALTER FUNCTION fn_GetCustomerServiceCount (@CustomerID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    SELECT @Count = COUNT(*)
    FROM ServiceDetails sd
    JOIN Vehicles v ON sd.VehicleID = v.VehicleID
    WHERE v.CustomerID = @CustomerID;
    RETURN @Count;
END;
GO

-- Check if a vehicle has any active (open) schedule
CREATE OR ALTER FUNCTION fn_HasActiveSchedule (@VehicleID INT)
RETURNS BIT
AS
BEGIN
    DECLARE @Result BIT = 0;
    IF EXISTS (
        SELECT 1 FROM ServiceSchedule
        WHERE VehicleID = @VehicleID AND Status IN ('Scheduled', 'In Progress')
    )
        SET @Result = 1;
    RETURN @Result;
END;
GO

-- Get a readable vehicle label string
CREATE OR ALTER FUNCTION fn_GetVehicleLabel (@VehicleID INT)
RETURNS VARCHAR(200)
AS
BEGIN
    DECLARE @Label VARCHAR(200);
    SELECT @Label = Make + ' ' + Model + ' ' + CAST(Year AS VARCHAR)
                  + ' [' + LicensePlate + ']'
    FROM Vehicles WHERE VehicleID = @VehicleID;
    RETURN ISNULL(@Label, 'Unknown Vehicle');
END;
GO

-- Count open complaints for a customer
CREATE OR ALTER FUNCTION fn_GetOpenComplaintCount (@CustomerID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    SELECT @Count = COUNT(*)
    FROM Complaints
    WHERE CustomerID = @CustomerID AND Status IN ('Open', 'In Progress');
    RETURN @Count;
END;
GO


-- ============================================================
--  4. TRIGGERS
-- ============================================================

-- When a ServiceDetail row is INSERTED:
--   -> automatically move the linked schedule from 'Scheduled' to 'In Progress'
--   -> log to AuditLog
CREATE OR ALTER TRIGGER trg_ServiceDetails_AfterInsert
ON ServiceDetails
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Move schedule status forward
    UPDATE ServiceSchedule
    SET Status = 'In Progress'
    FROM ServiceSchedule ss
    JOIN inserted i ON ss.ScheduleID = i.ScheduleID
    WHERE ss.Status = 'Scheduled';

    -- Audit log
    INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, NewValues)
    SELECT
        'ServiceDetails',
        'INSERT',
        i.ServiceID,
        GETDATE(),
        'ScheduleID=' + CAST(i.ScheduleID AS VARCHAR)
        + ', VehicleID=' + CAST(i.VehicleID AS VARCHAR)
        + ', Cost=' + CAST(i.ServiceCost AS VARCHAR)
    FROM inserted i;
END;
GO

-- When a ServiceDetail row is UPDATED:
--   -> if marked 'Completed', also mark the linked schedule 'Completed'
--   -> log old vs new status to AuditLog
CREATE OR ALTER TRIGGER trg_ServiceDetails_AfterUpdate
ON ServiceDetails
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- Cascade completion to schedule
    UPDATE ServiceSchedule
    SET Status = 'Completed'
    FROM ServiceSchedule ss
    JOIN inserted i ON ss.ScheduleID = i.ScheduleID
    WHERE i.Status = 'Completed';

    -- Audit log
    INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, OldValues, NewValues)
    SELECT
        'ServiceDetails',
        'UPDATE',
        i.ServiceID,
        GETDATE(),
        'Status=' + d.Status + ', Cost=' + CAST(d.ServiceCost AS VARCHAR),
        'Status=' + i.Status + ', Cost=' + CAST(i.ServiceCost AS VARCHAR)
    FROM inserted i
    JOIN deleted d ON i.ServiceID = d.ServiceID;
END;
GO

-- Audit all INSERT / UPDATE / DELETE on Vehicles
CREATE OR ALTER TRIGGER trg_Vehicles_Audit
ON Vehicles
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- UPDATE: both inserted and deleted rows exist
    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, OldValues, NewValues)
        SELECT
            'Vehicles', 'UPDATE', i.VehicleID, GETDATE(),
            'Plate=' + d.LicensePlate + ', Mileage=' + CAST(ISNULL(d.Mileage, 0) AS VARCHAR),
            'Plate=' + i.LicensePlate + ', Mileage=' + CAST(ISNULL(i.Mileage, 0) AS VARCHAR)
        FROM inserted i
        JOIN deleted d ON i.VehicleID = d.VehicleID;
    END
    -- INSERT: only inserted rows exist
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, NewValues)
        SELECT
            'Vehicles', 'INSERT', VehicleID, GETDATE(),
            'Make=' + Make + ', Model=' + Model + ', Plate=' + LicensePlate
        FROM inserted;
    END
    -- DELETE: only deleted rows exist
    ELSE
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, OldValues)
        SELECT
            'Vehicles', 'DELETE', VehicleID, GETDATE(),
            'Make=' + Make + ', Model=' + Model + ', Plate=' + LicensePlate
        FROM deleted;
    END
END;
GO

-- Audit all INSERT / UPDATE / DELETE on Customers
CREATE OR ALTER TRIGGER trg_Customers_Audit
ON Customers
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, OldValues, NewValues)
        SELECT
            'Customers', 'UPDATE', i.CustomerID, GETDATE(),
            'Name=' + d.FullName + ', Phone=' + d.Phone,
            'Name=' + i.FullName + ', Phone=' + i.Phone
        FROM inserted i
        JOIN deleted d ON i.CustomerID = d.CustomerID;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, NewValues)
        SELECT
            'Customers', 'INSERT', CustomerID, GETDATE(),
            'Name=' + FullName + ', Phone=' + Phone
        FROM inserted;
    END
    ELSE
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, OldValues)
        SELECT
            'Customers', 'DELETE', CustomerID, GETDATE(),
            'Name=' + FullName + ', Phone=' + Phone
        FROM deleted;
    END
END;
GO

-- Audit INSERT / UPDATE on Complaints
CREATE OR ALTER TRIGGER trg_Complaints_Audit
ON Complaints
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted)
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, OldValues, NewValues)
        SELECT
            'Complaints', 'UPDATE', i.ComplaintID, GETDATE(),
            'Status=' + d.Status,
            'Status=' + i.Status
        FROM inserted i
        JOIN deleted d ON i.ComplaintID = d.ComplaintID;
    END
    ELSE IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO AuditLog (TableName, Action, RecordID, ChangedAt, NewValues)
        SELECT
            'Complaints', 'INSERT', ComplaintID, GETDATE(),
            'Subject=' + Subject + ', Status=' + Status
        FROM inserted;
    END
END;
GO


-- ============================================================
--  5. SAMPLE DATA (Using usp_ prefix)
-- ============================================================

-- Users
EXEC usp_AddUser 'admin',    'hashed_pw', 'System Admin',    'Admin', 'admin@vsms.com', '0300-0000001';
EXEC usp_AddUser 'ali_tech', 'hashed_pw', 'Ali Technician',  'Staff', 'ali@vsms.com',   '0300-0000002';
GO

-- Customers
EXEC usp_AddCustomer 'Ahmed Khan', 'ahmed@email.com', '0311-1111111', 'House 5, Rawalpindi';
EXEC usp_AddCustomer 'Sara Malik', 'sara@email.com',  '0322-2222222', 'Flat 12, Islamabad';
GO

-- Vehicles (Make, Model, Year, LicensePlate, Mileage)
EXEC usp_AddVehicle 1, 'Toyota', 'Corolla', 2020, 'RIW-1234', 45000;
EXEC usp_AddVehicle 2, 'Honda',  'Civic',   2019, 'ISB-5678', 62000;
GO

-- Schedule a service
EXEC usp_ScheduleService 1, 1, '2026-04-28', 'Oil Change', 'Synthetic oil requested', 1;
GO

-- Add service details
EXEC usp_AddServiceDetails 1, 1, 2, 'Full oil change with filter replacement', 3500.00, 'Oil Filter, 4L Synthetic Oil';
GO

-- Add and assign a complaint
EXEC usp_AddComplaint 1, 1, 'Noise from engine', 'Strange knocking noise after last service.';
EXEC usp_AssignComplaint 1, 2;
GO

-- ============================================================
--  QUICK REFERENCE (Using usp_ prefix)
-- ============================================================
/*
  -- Functions
  SELECT dbo.fn_GetVehicleTotalCost(1)       AS TotalCost;
  SELECT dbo.fn_GetCustomerServiceCount(1)   AS ServiceCount;
  SELECT dbo.fn_HasActiveSchedule(1)         AS HasActiveSchedule;
  SELECT dbo.fn_GetVehicleLabel(1)           AS VehicleLabel;
  SELECT dbo.fn_GetOpenComplaintCount(1)     AS OpenComplaints;

  -- Reports
  EXEC usp_GetServiceReport '2026-01-01', '2026-12-31';
  EXEC usp_GetCustomerHistory 1;
  EXEC usp_GetVehicleMaintenanceRecords 1;
*/

USE VehicleServiceDB;

UPDATE Users 
SET PasswordHash = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2))
WHERE Username IN ('admin', 'ali_tech');


