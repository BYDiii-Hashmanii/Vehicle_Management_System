# Vehicle Service Management System — C# Windows Forms

## Requirements
- Visual Studio 2022 (Community or higher)
- .NET 6.0 SDK (Windows)
- SQL Server (any edition — Express is fine)
- NuGet package: `Microsoft.Data.SqlClient` (auto-restored)

---

## Setup Steps

### 1. Run the SQL Script
Open **SQL Server Management Studio (SSMS)** and run:
```
VehicleServiceDB_Student.sql
```
This creates the database, all tables, stored procedures, functions, and sample data.

### 2. Configure Connection String
Open `Database/DB.cs` and update the connection string at the top:

```csharp
private static readonly string _connStr =
    "Server=.;Database=VehicleServiceDB;Integrated Security=True;TrustServerCertificate=True;";
```

| Scenario | Server value |
|---|---|
| Local SQL Server (default instance) | `Server=.` or `Server=localhost` |
| Named instance | `Server=.\SQLEXPRESS` |
| SQL Auth instead of Windows Auth | Add `User Id=sa;Password=yourpw;` and remove `Integrated Security=True` |

### 3. Open in Visual Studio
- Open `VehicleServiceApp.csproj`
- Press **F5** to build and run

### 4. Login
```
Username: admin
Password: admin123
```
> Passwords are stored as SHA-256 hashes.  
> The sample data script calls `sp_AddUser` with `'hashed_password_here'` — you must update it.

**To generate the correct hash for "admin123" run this in SQL:**
```sql
-- Replace the admin password with proper hash
UPDATE Users 
SET PasswordHash = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2))
WHERE Username = 'admin';

UPDATE Users 
SET PasswordHash = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2))
WHERE Username = 'ali_tech';
```

---

## Project Structure

```
VehicleServiceApp/
├── Program.cs                  Entry point
├── UIHelper.cs                 Colors, fonts, shared UI factory methods
├── Database/
│   └── DB.cs                   All SQL calls (stored procedures + queries)
└── Forms/
    ├── LoginForm.cs            Login screen
    ├── MainForm.cs             Main window with sidebar navigation
    ├── DashboardPanel.cs       Home dashboard with stat cards
    ├── CrudPanel.cs            Base class for all CRUD pages
    ├── CustomersPanel.cs       Customer management
    ├── VehiclesPanel.cs        Vehicle management
    ├── SchedulesPanel.cs       Service scheduling
    ├── ServiceDetailsPanel.cs  Service detail records
    ├── ComplaintsPanel.cs      Complaint management
    ├── ReportsPanel.cs         Service / customer / vehicle reports
    ├── UsersPanel.cs           User management (Admin only)
    └── StatusDialog.cs         Shared status-update popup
```

## Features

| Screen | Functions |
|---|---|
| Dashboard | Live stat cards, recent schedules |
| Customers | Add / Edit / Delete / Search |
| Vehicles | Add / Edit / Delete / Customer-linked |
| Schedules | Add / Edit Status / Cancel / Search |
| Service Details | Add new service / Update status + cost + parts |
| Complaints | Add / Assign to staff / Resolve |
| Reports | Service Report, Customer History, Vehicle Maintenance |
| Users | Add / Edit / Deactivate (Admin only) |

All database operations use **Stored Procedures** from the SQL script.
