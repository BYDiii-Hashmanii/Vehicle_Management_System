# Vehicle Service & Maintenance Management System

A comprehensive desktop application for managing vehicle services, maintenance schedules, customer records, and service complaints. Built with C# Windows Forms frontend and SQL Server backend, this system streamlines automotive service operations with role-based access control and complete audit tracking.

---

## Overview

The Vehicle Service & Maintenance Management System is designed to help automotive service centers efficiently manage:

- **Customer Relationships**: Register, track, and maintain customer information
- **Vehicle Inventory**: Manage customer vehicles with detailed specifications and service history
- **Service Scheduling**: Schedule preventive maintenance and service appointments
- **Service Execution**: Track ongoing service work with cost management and parts inventory
- **Complaint Resolution**: Log and track customer complaints with resolution tracking
- **Business Intelligence**: Generate comprehensive reports for service trends and customer history

The application enforces strict role-based access control, maintains complete audit trails for compliance, and uses industry-standard security practices including password hashing with SHA-256.

---

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **Frontend** | C# Windows Forms (.NET 6.0) |
| **Backend** | SQL Server (any edition) |
| **Data Access** | Microsoft.Data.SqlClient |
| **IDE** | Visual Studio 2022+ |
| **Authentication** | SHA-256 Password Hashing |
| **Architecture** | N-Tier (Presentation, Data Access, Database) |

**Language Composition**: C# (82.6%), T-SQL (17.4%)

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│           Presentation Layer (Windows Forms)                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ LoginForm → MainForm (Sidebar Navigation)            │  │
│  │ ├─ DashboardPanel (Statistics & Overview)            │  │
│  │ ├─ CustomersPanel (Customer Management)              │  │
│  │ ├─ VehiclesPanel (Vehicle Registry)                  │  │
│  │ ├─ SchedulesPanel (Service Scheduling)               │  │
│  │ ├─ ServiceDetailsPanel (Service Execution)           │  │
│  │ ├─ ComplaintsPanel (Complaint Management)            │  │
│  │ ├─ ReportsPanel (Business Intelligence)              │  │
│  │ └─ UsersPanel (User Management - Admin Only)         │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                            ↓ UIHelper.cs
┌─────────────────────────────────────────────────────────────┐
│           Data Access Layer (Database/DB.cs)                │
│  ├─ Stored Procedures (usp_* convention)                    │
│  ├─ User-Defined Functions (fn_* convention)                │
│  ├─ Parameterized SQL Queries                               │
│  └─ Connection Management                                   │
└─────────────────────────────────────────────────────────────┘
                            ↓ SqlClient
┌─────────────────────────────────────────────────────────────┐
│           Database Layer (SQL Server)                       │
│  ├─ Users, Customers, Vehicles Tables                       │
│  ├─ ServiceSchedule, ServiceDetails Tables                  │
│  ├─ Complaints, AuditLog Tables                             │
│  ├─ Triggers for Audit & Cascading Updates                  │
│  └─ Business Logic via Stored Procedures & Functions        │
└─────────────────────────────────────────────────────────────┘
```

---

## Role-Based Access Control

### Admin Role

**Responsibilities**: System configuration, user management, and full system access

| Feature | Functionality |
|---------|---------------|
| **User Management** | Create, update, deactivate staff accounts; manage user roles |
| **System Access** | Unrestricted access to all modules and features |
| **Audit Review** | View complete audit logs of all system changes |
| **Configuration** | Modify system settings and database operations |
| **Reports** | Access all reports including financial and operational analytics |

**Accessible Screens**:
- Dashboard (Full overview)
- Customers (Full CRUD - Create, Read, Update, Delete)
- Vehicles (Full CRUD)
- Service Schedules (Full CRUD)
- Service Details (Full CRUD)
- Complaints (Full CRUD with assignment)
- Reports (All report types)
- Users (Full management capabilities)

---

### Staff Role

**Responsibilities**: Day-to-day operations including service execution and customer interaction

| Feature | Functionality |
|---------|---------------|
| **Customer Interaction** | View customer records and contact information |
| **Service Execution** | Record service work, update status, track costs and parts used |
| **Schedule Management** | View assigned schedules and update their status |
| **Complaint Handling** | Receive assigned complaints and provide resolution documentation |
| **Service Records** | Create and maintain detailed service documentation |
| **Reporting** | Access service reports for their own work records |

**Accessible Screens**:
- Dashboard (Statistics relevant to staff)
- Customers (Read-only view)
- Vehicles (Read-only view)
- Service Schedules (View and update status)
- Service Details (Create new services, update status and costs)
- Complaints (View assigned complaints, update resolution)
- Reports (Service history reports)
- Users (Restricted - cannot access)

---

## Core Features

### 1. Customer Management

Maintain comprehensive customer profiles with contact information and communication history.

- **Add Customers**: Register new customers with name, email, phone, and address
- **View Records**: Access complete customer contact details and service history
- **Edit Information**: Update customer information as needed
- **Delete Records**: Remove customers (only if no associated vehicles)
- **Search/Filter**: Quickly locate customers by name or contact details

### 2. Vehicle Management

Track all vehicles associated with customers with detailed specifications.

- **Vehicle Registration**: Link vehicles to customers with make, model, year, and license plate
- **Mileage Tracking**: Record current vehicle mileage for maintenance scheduling
- **Service History**: View all historical services for a specific vehicle
- **Unique License Plate Enforcement**: Prevent duplicate vehicle registrations
- **Edit/Delete**: Maintain vehicle records with protection against deleting vehicles with active services

### 3. Service Scheduling

Plan and organize service appointments to optimize workflow and customer satisfaction.

- **Schedule Services**: Create service appointments with date, service type, and notes
- **Status Management**: Track schedule status (Scheduled → In Progress → Completed)
- **Technician Assignment**: Assign staff members to handle specific services
- **Appointment Notes**: Record special requests or customer requirements
- **Schedule Cancellation**: Cancel appointments when necessary

### 4. Service Details & Execution

Record detailed information about each service performed including costs and materials.

- **Service Logging**: Create detailed service records linked to schedules
- **Status Tracking**: Monitor service progress (Pending → In Progress → Completed)
- **Cost Management**: Record service costs and ensure financial accuracy
- **Parts Documentation**: Log parts used in each service with descriptions
- **Technician Assignment**: Assign specific technicians to service records
- **Completion Timestamp**: Automatically record when service is completed

### 5. Complaint Management

Efficiently handle and resolve customer complaints with full documentation.

- **Complaint Registration**: Log customer complaints with detailed descriptions
- **Status Tracking**: Monitor complaint status (Open → In Progress → Resolved → Closed)
- **Staff Assignment**: Assign complaints to specific staff members
- **Resolution Documentation**: Record how complaints were resolved
- **Timeline Tracking**: Automatically capture creation and resolution dates

### 6. Reporting & Analytics

Generate comprehensive reports for business intelligence and performance monitoring.

**Available Reports**:
- **Service Report**: Detailed service history with dates, costs, technicians, and parts used
- **Customer History**: Complete service records for individual customers
- **Vehicle Maintenance Records**: Full maintenance timeline for specific vehicles

**Report Features**:
- Date range filtering (from date to date)
- Customer-specific filtering
- Vehicle-specific filtering
- Cost analysis and parts tracking
- Technician performance visibility

### 7. Dashboard

Real-time system overview with key performance indicators and recent activity.

- **Statistics Cards**: Display total counts for customers, vehicles, and services
- **Recent Schedules**: Show upcoming and recent service appointments
- **System Status**: Quick access to key operational metrics
- **Navigation Hub**: Easy access to all system modules

### 8. User Management

Control system access and staff account administration (Admin only).

- **User Creation**: Add new staff members with role assignment (Admin/Staff)
- **Account Management**: Edit user details including name, email, and phone
- **Role Assignment**: Assign Admin or Staff roles with corresponding permissions
- **Account Deactivation**: Soft delete users without removing historical data
- **Active Status**: Track which users are currently active in the system

---

## Project Structure

```
Vehicle_Management_System/
│
├── README.md                           (This file)
├── final_project_Updated_vehicle.sql   (Database schema & procedures)
│
└── VehicleServiceApp/                  (Main Application)
    │
    ├── Program.cs                      (Application entry point)
    ├── UIHelper.cs                     (Shared UI utilities, colors, fonts)
    ├── VehicleServiceApp.csproj        (Project configuration)
    ├── VehicleServiceApp.sln           (Visual Studio solution)
    │
    ├── Database/
    │   └── DB.cs                       (Data access layer - all SQL calls)
    │
    └── Forms/
        ├── LoginForm.cs                (Authentication & login screen)
        ├── LoginForm.resx              (Login form resources)
        ├── MainForm.cs                 (Main application window with sidebar)
        ├── DashboardPanel.cs           (Dashboard with statistics)
        ├── CrudPanel.cs                (Base class for CRUD panels)
        ├── CustomersPanel.cs           (Customer CRUD operations)
        ├── VehiclesPanel.cs            (Vehicle CRUD operations)
        ├── SchedulesPanel.cs           (Service scheduling)
        ├── ServiceDetailsPanel.cs      (Service record management)
        ├── ComplaintsPanel.cs          (Complaint handling)
        ├── ReportsPanel.cs             (Reporting & analytics)
        ├── UsersPanel.cs               (User management - Admin only)
        └── StatusDialog.cs             (Shared status update dialog)
```

---

## Database Schema Overview

### Tables

| Table | Purpose | Key Fields |
|-------|---------|-----------|
| **Users** | Staff accounts and authentication | UserID, Username, PasswordHash, Role (Admin/Staff) |
| **Customers** | Customer information | CustomerID, FullName, Email, Phone, Address |
| **Vehicles** | Vehicle registry | VehicleID, CustomerID, Make, Model, Year, LicensePlate, Mileage |
| **ServiceSchedule** | Appointment scheduling | ScheduleID, VehicleID, ScheduledDate, ServiceType, Status |
| **ServiceDetails** | Service execution records | ServiceID, ScheduleID, TechnicianID, ServiceCost, PartsUsed, Status |
| **Complaints** | Customer complaint tracking | ComplaintID, CustomerID, Subject, Status, ResolutionNotes |
| **AuditLog** | System change tracking | LogID, TableName, Action (INSERT/UPDATE/DELETE), OldValues, NewValues |

### Stored Procedures

All database operations use stored procedures with usp_ prefix for security and consistency:

- **User Management**: usp_AddUser, usp_UpdateUser, usp_DeleteUser
- **Customer Management**: usp_AddCustomer, usp_UpdateCustomer, usp_DeleteCustomer
- **Vehicle Management**: usp_AddVehicle, usp_UpdateVehicle, usp_DeleteVehicle
- **Schedule Management**: usp_ScheduleService, usp_UpdateScheduleStatus
- **Service Details**: usp_AddServiceDetails, usp_UpdateServiceStatus
- **Complaint Management**: usp_AddComplaint, usp_AssignComplaint, usp_ResolveComplaint
- **Reporting**: usp_GetServiceReport, usp_GetCustomerHistory, usp_GetVehicleMaintenanceRecords

### User-Defined Functions

- `fn_GetVehicleTotalCost()` - Calculate total service cost for a vehicle
- `fn_GetCustomerServiceCount()` - Count services performed for a customer
- `fn_HasActiveSchedule()` - Check if vehicle has pending/in-progress services
- `fn_GetVehicleLabel()` - Generate readable vehicle description string
- `fn_GetOpenComplaintCount()` - Count unresolved complaints for a customer

### Triggers

- **trg_ServiceDetails_AfterInsert**: Auto-update schedule status and audit logging
- **trg_ServiceDetails_AfterUpdate**: Cascade service completion to schedules
- **trg_Vehicles_Audit**: Track all vehicle record changes
- **trg_Customers_Audit**: Track all customer record changes
- **trg_Complaints_Audit**: Track complaint status changes

---

## Prerequisites

Before you begin, ensure you have the following installed:

- **Visual Studio 2022** (Community Edition or higher)
- **.NET 6.0 SDK** (Windows)
- **SQL Server** (Express, Developer, or higher edition)
- **SQL Server Management Studio (SSMS)** (optional but recommended)

---

## Installation & Setup

### Step 1: Clone the Repository

```bash
git clone https://github.com/BYDiii-Hashmanii/Vehicle_Management_System.git
cd Vehicle_Management_System
```

### Step 2: Create the Database

1. Open **SQL Server Management Studio (SSMS)**
2. Connect to your SQL Server instance
3. Open the file: `final_project_Updated_vehicle.sql`
4. Execute the entire script to create:
   - Database: `VehicleServiceDB`
   - All tables with relationships and constraints
   - All stored procedures and functions
   - All triggers for audit and cascading operations
   - Sample data (admin user, sample customers and vehicles)

### Step 3: Configure Database Connection

1. Open the project in Visual Studio
2. Navigate to: `VehicleServiceApp/Database/DB.cs`
3. Update the connection string at line 5:

```csharp
private static readonly string _connStr =
    "Server=.;Database=VehicleServiceDB;Integrated Security=True;TrustServerCertificate=True;";
```

**Connection String Options**:

| Scenario | Server Value |
|----------|--------------|
| Local SQL Server (Default Instance) | `Server=.` or `Server=localhost` |
| Named Instance (SQLEXPRESS) | `Server=.\SQLEXPRESS` |
| Remote Server | `Server=yourserver.com` |
| SQL Server Authentication | Add `User Id=sa;Password=yourpw;` and remove `Integrated Security=True` |
| Named Instance + SQL Auth | `Server=.\SQLEXPRESS;User Id=sa;Password=pw;` |

### Step 4: Set User Passwords

The database is initialized with two users:
- **Username**: admin
- **Username**: ali_tech

Update their passwords to SHA-256 hashes by running this SQL command:

```sql
USE VehicleServiceDB;

UPDATE Users 
SET PasswordHash = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'admin123'), 2))
WHERE Username IN ('admin', 'ali_tech');
```

### Step 5: Build and Run

1. Open `VehicleServiceApp.sln` in Visual Studio
2. Restore NuGet packages: **Tools** → **NuGet Package Manager** → **Restore Packages**
3. Build the solution: **Ctrl+Shift+B**
4. Run the application: **F5** or **Debug** → **Start Debugging**

### Step 6: Login

Use the following credentials to access the application:

```
Username: admin
Password: admin123
Role: Admin
```

For staff account:
```
Username: ali_tech
Password: admin123
Role: Staff
```

---

## Usage Guide

### For Administrators

1. **First Login**: Use admin credentials provided in setup
2. **Create Staff Users**: Navigate to Users panel and create accounts for service staff
3. **Configure System**: Set up customers and vehicles
4. **Monitor Operations**: Review audit logs and system activity
5. **Generate Reports**: Create comprehensive service and financial reports

### For Service Staff

1. **View Dashboard**: Check today's schedule and pending services
2. **Execute Services**: Navigate to Service Details to record work performed
3. **Update Schedules**: Change schedule status as work progresses
4. **Handle Complaints**: Review assigned complaints and provide resolution
5. **Access Reports**: View service history and customer records

### Common Workflows

**Adding a New Customer and Vehicle**:
1. Go to Customers panel → Click "Add New"
2. Fill in customer information → Save
3. Go to Vehicles panel → Click "Add New"
4. Select the customer → Enter vehicle details → Save

**Scheduling a Service**:
1. Go to Schedules panel → Click "Add New"
2. Select vehicle and customer
3. Set service date and type
4. Add notes if needed → Save

**Recording Service Work**:
1. Go to Service Details panel
2. Link to existing schedule
3. Enter service description
4. Record cost and parts used
5. Save → Mark as completed when done

**Resolving a Complaint**:
1. Go to Complaints panel
2. Select complaint → Click "Assign to Me"
3. Work on resolution
4. Add resolution notes → Mark as resolved

---

## Security Features

- **Password Security**: SHA-256 hashing for all stored passwords
- **Role-Based Access**: Separate Admin and Staff functionalities
- **SQL Injection Prevention**: All database operations use parameterized stored procedures
- **Audit Trail**: Complete logging of all INSERT, UPDATE, DELETE operations
- **Session Management**: User session tracking with role-based features
- **Input Validation**: Form-level validation before database submission

---

## Troubleshooting

### Issue: "Connection timeout" or "Cannot connect to database"

**Solution**:
1. Verify SQL Server is running
2. Check connection string in `DB.cs`
3. Ensure database `VehicleServiceDB` exists
4. Test connection using SSMS first
5. For named instances, use `.\SQLEXPRESS` format

### Issue: "Login failed" with correct credentials

**Solution**:
1. Verify password hash was updated in database
2. Check that Users table has data using SSMS
3. Run password update script again
4. Ensure UserID exists in Users table

### Issue: "Database does not exist"

**Solution**:
1. Ensure `final_project_Updated_vehicle.sql` was fully executed
2. Check SQL Server is running and accessible
3. Verify script execution completed without errors
4. Use SSMS to manually verify database creation

### Issue: NuGet Package Errors

**Solution**:
1. Clean the solution: **Build** → **Clean Solution**
2. Restore packages: **Tools** → **NuGet Package Manager** → **Restore Packages**
3. Rebuild solution: **Build** → **Rebuild Solution**

---

## Performance Considerations

- **Indexing**: The database includes primary key and foreign key indexing for optimal query performance
- **Stored Procedures**: All operations use pre-compiled stored procedures
- **Triggers**: Automated cascading updates reduce redundant API calls
- **DataGridView Caching**: Forms implement efficient data binding patterns

---

## Future Enhancements

Potential improvements for future versions:

- Export reports to PDF/Excel format
- Email notifications for scheduled appointments
- SMS reminder system for customers
- Mobile app for field technicians
- Integration with payment processing
- Advanced analytics and dashboards
- Multi-location support
- Inventory management for parts
- Service recommendation engine
- Customer portal for status tracking

---

## Contributing

For contributions to this project:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## Learnings & Best Practices

This project demonstrates:

- **N-Tier Architecture**: Separation of concerns between presentation, data, and database layers
- **Stored Procedures**: Database-level business logic for security and performance
- **Triggers**: Automated data consistency and audit trail maintenance
- **User-Defined Functions**: Reusable calculations and queries
- **Windows Forms Design**: Professional UI with consistent theming
- **Role-Based Access Control**: Secure permission management
- **SQL Injection Prevention**: Parameterized queries and procedures
- **Audit Logging**: Complete history tracking for compliance

---

## License

This project is provided for educational purposes. All rights reserved.

---

## Support & Feedback

For issues, questions, or feedback:

1. Check the Troubleshooting section above
2. Review the project structure and database schema
3. Examine the SQL Server Management Studio for data verification
4. Test connection strings independently

---

## Author

**BYDiii-Hashmanii**  
Student Vehicle Service Management System Project

---

**Last Updated**: May 2026  
**Version**: 1.0 Final  
**Status**: Complete & Functional
