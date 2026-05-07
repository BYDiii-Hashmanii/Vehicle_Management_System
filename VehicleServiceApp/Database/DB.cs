using Microsoft.Data.SqlClient;
using System.Data;

namespace VehicleServiceApp.Database;

public static class DB
{
    // ── Change this connection string to match your SQL Server ──
    private static readonly string _connStr =
        "Server=DESKTOP-HC334EG;Database=VehicleServiceDB;Integrated Security=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection() => new(_connStr);

    // ─────────────────────────────────────────────────────────────
    //  Generic helpers
    // ─────────────────────────────────────────────────────────────
    public static DataTable ExecuteQuery(string sql, params SqlParameter[] prms)
    {
        using var con = GetConnection();
        using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddRange(prms);
        con.Open();
        var dt = new DataTable();
        new SqlDataAdapter(cmd).Fill(dt);
        return dt;
    }

    public static DataTable ExecuteProc(string proc, params SqlParameter[] prms)
    {
        using var con = GetConnection();
        using var cmd = new SqlCommand(proc, con) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddRange(prms);
        con.Open();
        var dt = new DataTable();
        new SqlDataAdapter(cmd).Fill(dt);
        return dt;
    }

    public static object? ExecuteScalarProc(string proc, params SqlParameter[] prms)
    {
        using var con = GetConnection();
        using var cmd = new SqlCommand(proc, con) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddRange(prms);
        con.Open();
        return cmd.ExecuteScalar();
    }

    public static void ExecuteNonQueryProc(string proc, params SqlParameter[] prms)
    {
        using var con = GetConnection();
        using var cmd = new SqlCommand(proc, con) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddRange(prms);
        con.Open();
        cmd.ExecuteNonQuery();
    }

    // ─────────────────────────────────────────────────────────────
    //  USERS
    // ─────────────────────────────────────────────────────────────
    public static DataTable GetAllUsers() =>
        ExecuteQuery("SELECT UserID, Username, FullName, Role, Email, Phone, IsActive, CreatedAt FROM Users");

    public static DataTable ValidateLogin(string username, string passwordHash) =>
        ExecuteQuery(
            "SELECT UserID, FullName, Role FROM Users WHERE Username=@u AND PasswordHash=@p AND IsActive=1",
            new SqlParameter("@u", username),
            new SqlParameter("@p", passwordHash));

    public static void AddUser(string username, string pwHash, string fullName,
                               string role, string? email, string? phone) =>
        ExecuteNonQueryProc("usp_AddUser",
            new SqlParameter("@Username", username),
            new SqlParameter("@PasswordHash", pwHash),
            new SqlParameter("@FullName", fullName),
            new SqlParameter("@Role", role),
            new SqlParameter("@Email", (object?)email ?? DBNull.Value),
            new SqlParameter("@Phone", (object?)phone ?? DBNull.Value));

    public static void UpdateUser(int id, string fullName, string role,
                                  string? email, string? phone, bool isActive) =>
        ExecuteNonQueryProc("usp_UpdateUser",
            new SqlParameter("@UserID", id),
            new SqlParameter("@FullName", fullName),
            new SqlParameter("@Role", role),
            new SqlParameter("@Email", (object?)email ?? DBNull.Value),
            new SqlParameter("@Phone", (object?)phone ?? DBNull.Value),
            new SqlParameter("@IsActive", isActive));

    public static void DeleteUser(int id) =>
        ExecuteNonQueryProc("usp_DeleteUser", new SqlParameter("@UserID", id));

    // ─────────────────────────────────────────────────────────────
    //  CUSTOMERS
    // ─────────────────────────────────────────────────────────────
    public static DataTable GetAllCustomers() =>
        ExecuteQuery("SELECT CustomerID, FullName, Email, Phone, Address, CreatedAt FROM Customers");

    public static void AddCustomer(string fullName, string? email, string phone, string? address) =>
        ExecuteNonQueryProc("usp_AddCustomer",
            new SqlParameter("@FullName", fullName),
            new SqlParameter("@Email", (object?)email ?? DBNull.Value),
            new SqlParameter("@Phone", phone),
            new SqlParameter("@Address", (object?)address ?? DBNull.Value));

    public static void UpdateCustomer(int id, string fullName, string? email,
                                      string phone, string? address) =>
        ExecuteNonQueryProc("usp_UpdateCustomer",
            new SqlParameter("@CustomerID", id),
            new SqlParameter("@FullName", fullName),
            new SqlParameter("@Email", (object?)email ?? DBNull.Value),
            new SqlParameter("@Phone", phone),
            new SqlParameter("@Address", (object?)address ?? DBNull.Value));

    public static void DeleteCustomer(int id) =>
        ExecuteNonQueryProc("sp_DeleteCustomer", new SqlParameter("@CustomerID", id));

    // ─────────────────────────────────────────────────────────────
    //  VEHICLES
    // ─────────────────────────────────────────────────────────────
    public static DataTable GetAllVehicles() =>
        ExecuteQuery(@"SELECT v.VehicleID, c.FullName AS Customer, v.Make, v.Model,
                              v.Year, v.LicensePlate, v.Mileage, v.CreatedAt
                       FROM Vehicles v JOIN Customers c ON v.CustomerID=c.CustomerID");

    public static DataTable GetVehiclesByCustomer(int customerId) =>
        ExecuteQuery(
            "SELECT VehicleID, Make+' '+Model+' ('+CAST(Year AS VARCHAR)+') ['+LicensePlate+']' AS Label FROM Vehicles WHERE CustomerID=@cid",
            new SqlParameter("@cid", customerId));

    public static void AddVehicle(int customerId, string make, string model,
                                  int year, string plate, int mileage) =>
        ExecuteNonQueryProc("usp_AddVehicle",
            new SqlParameter("@CustomerID", customerId),
            new SqlParameter("@Make", make),
            new SqlParameter("@Model", model),
            new SqlParameter("@Year", year),
            new SqlParameter("@LicensePlate", plate),
            new SqlParameter("@Mileage", mileage));

    public static void UpdateVehicle(int id, string make, string model,
                                     int year, string plate, int mileage) =>
        ExecuteNonQueryProc("usp_UpdateVehicle",
            new SqlParameter("@VehicleID", id),
            new SqlParameter("@Make", make),
            new SqlParameter("@Model", model),
            new SqlParameter("@Year", year),
            new SqlParameter("@LicensePlate", plate),
            new SqlParameter("@Mileage", mileage));

    public static void DeleteVehicle(int id) =>
        ExecuteNonQueryProc("usp_DeleteVehicle", new SqlParameter("@VehicleID", id));

    // ─────────────────────────────────────────────────────────────
    //  SERVICE SCHEDULE
    // ─────────────────────────────────────────────────────────────
    public static DataTable GetAllSchedules() =>
        ExecuteQuery(@"SELECT ss.ScheduleID, c.FullName AS Customer,
                              v.Make+' '+v.Model AS Vehicle, v.LicensePlate,
                              ss.ScheduledDate, ss.ServiceType, ss.Status, ss.Notes
                       FROM ServiceSchedule ss
                       JOIN Vehicles  v ON ss.VehicleID  = v.VehicleID
                       JOIN Customers c ON ss.CustomerID = c.CustomerID
                       ORDER BY ss.ScheduledDate DESC");

    public static void AddSchedule(int vehicleId, int customerId, DateTime date,
                                   string serviceType, string? notes, int createdBy) =>
        ExecuteNonQueryProc("usp_ScheduleService",
            new SqlParameter("@VehicleID", vehicleId),
            new SqlParameter("@CustomerID", customerId),
            new SqlParameter("@ScheduledDate", date.Date),
            new SqlParameter("@ServiceType", serviceType),
            new SqlParameter("@Notes", (object?)notes ?? DBNull.Value),
            new SqlParameter("@CreatedBy", createdBy));

    public static void UpdateScheduleStatus(int scheduleId, string status) =>
        ExecuteNonQueryProc("usp_UpdateScheduleStatus",
            new SqlParameter("@ScheduleID", scheduleId),
            new SqlParameter("@Status", status));

    // ─────────────────────────────────────────────────────────────
    //  SERVICE DETAILS
    // ─────────────────────────────────────────────────────────────
    public static DataTable GetAllServiceDetails() =>
        ExecuteQuery(@"SELECT sd.ServiceID, c.FullName AS Customer,
                              v.Make+' '+v.Model AS Vehicle, v.LicensePlate,
                              sd.ServiceDate, sd.Description, sd.Status,
                              sd.ServiceCost, sd.PartsUsed,
                              u.FullName AS Technician
                       FROM ServiceDetails sd
                       JOIN Vehicles   v ON sd.VehicleID    = v.VehicleID
                       JOIN Customers  c ON v.CustomerID    = c.CustomerID
                       LEFT JOIN Users u ON sd.TechnicianID = u.UserID
                       ORDER BY sd.ServiceDate DESC");

    public static void AddServiceDetail(int scheduleId, int vehicleId, int? techId,
                                        string description, decimal cost, string? parts) =>
        ExecuteNonQueryProc("usp_AddServiceDetails",
            new SqlParameter("@ScheduleID", scheduleId),
            new SqlParameter("@VehicleID", vehicleId),
            new SqlParameter("@TechnicianID", (object?)techId ?? DBNull.Value),
            new SqlParameter("@Description", description),
            new SqlParameter("@ServiceCost", cost),
            new SqlParameter("@PartsUsed", (object?)parts ?? DBNull.Value));

    public static void UpdateServiceStatus(int serviceId, string status, decimal? cost, string? parts) =>
        ExecuteNonQueryProc("usp_UpdateServiceStatus",
            new SqlParameter("@ServiceID", serviceId),
            new SqlParameter("@Status", status),
            new SqlParameter("@ServiceCost", (object?)cost ?? DBNull.Value),
            new SqlParameter("@PartsUsed", (object?)parts ?? DBNull.Value));

    // ─────────────────────────────────────────────────────────────
    //  COMPLAINTS
    // ─────────────────────────────────────────────────────────────
    public static DataTable GetAllComplaints() =>
        ExecuteQuery(@"SELECT comp.ComplaintID, c.FullName AS Customer,
                              v.Make+' '+v.Model AS Vehicle,
                              comp.Subject, comp.Description,
                              comp.Status, comp.ResolutionNotes,
                              u.FullName AS AssignedTo,
                              comp.CreatedAt, comp.ResolvedAt
                       FROM Complaints comp
                       JOIN Customers c ON comp.CustomerID = c.CustomerID
                       LEFT JOIN Vehicles v ON comp.VehicleID = v.VehicleID
                       LEFT JOIN Users    u ON comp.AssignedTo = u.UserID
                       ORDER BY comp.CreatedAt DESC");

    public static void AddComplaint(int customerId, int? vehicleId,
                                    string subject, string description) =>
        ExecuteNonQueryProc("usp_AddComplaint",
            new SqlParameter("@CustomerID", customerId),
            new SqlParameter("@VehicleID", (object?)vehicleId ?? DBNull.Value),
            new SqlParameter("@Subject", subject),
            new SqlParameter("@Description", description));

    public static void AssignComplaint(int complaintId, int userId) =>
        ExecuteNonQueryProc("usp_AssignComplaint",
            new SqlParameter("@ComplaintID", complaintId),
            new SqlParameter("@AssignedTo", userId));

    public static void ResolveComplaint(int complaintId, string notes) =>
        ExecuteNonQueryProc("usp_ResolveComplaint",
            new SqlParameter("@ComplaintID", complaintId),
            new SqlParameter("@ResolutionNotes", notes));

    // ─────────────────────────────────────────────────────────────
    //  REPORTS
    // ─────────────────────────────────────────────────────────────
    public static DataTable GetServiceReport(DateTime? start, DateTime? end) =>
        ExecuteProc("usp_GetServiceReport",
            new SqlParameter("@StartDate", (object?)start?.Date ?? DBNull.Value),
            new SqlParameter("@EndDate", (object?)end?.Date ?? DBNull.Value));

    public static DataTable GetCustomerHistory(int customerId) =>
        ExecuteProc("usp_GetCustomerHistory",
            new SqlParameter("@CustomerID", customerId));

    public static DataTable GetVehicleMaintenanceRecords(int vehicleId) =>
        ExecuteProc("usp_GetVehicleMaintenanceRecords",
            new SqlParameter("@VehicleID", vehicleId));

    // ─────────────────────────────────────────────────────────────
    //  FUNCTIONS (wrapped as SQL scalar calls)
    // ─────────────────────────────────────────────────────────────
    public static decimal GetVehicleTotalCost(int vehicleId)
    {
        var result = ExecuteQuery(
            "SELECT dbo.fn_GetVehicleTotalCost(@id) AS v",
            new SqlParameter("@id", vehicleId));
        return result.Rows.Count > 0 ? Convert.ToDecimal(result.Rows[0]["v"]) : 0;
    }

    public static DataTable GetAllUsers_Staff() =>
        ExecuteQuery("SELECT UserID, FullName FROM Users WHERE IsActive=1 AND Role='Staff'");

    public static DataTable GetAllUsersActive() =>
        ExecuteQuery("SELECT UserID, FullName FROM Users WHERE IsActive=1");
}

//using Microsoft.Data.SqlClient;
//using System.Data;

//namespace VehicleServiceApp.Database;

//public static class DB
//{
//    // Connection string set to your server as seen in WhatsApp Image 2026-05-01 at 10.27.36 AM.jpeg
//    private static readonly string _connStr =
//        "Server=DESKTOP-HC334EG;Database=VehicleServiceDB;Integrated Security=True;TrustServerCertificate=True;";

//    public static SqlConnection GetConnection() => new(_connStr);

//    // ─────────────────────────────────────────────────────────────
//    //  GENERIC HELPERS (Updated to avoid Stored Procedures)
//    // ─────────────────────────────────────────────────────────────
//    public static DataTable ExecuteQuery(string sql, params SqlParameter[] prms)
//    {
//        using var con = GetConnection();
//        using var cmd = new SqlCommand(sql, con);
//        if (prms != null) cmd.Parameters.AddRange(prms);
//        con.Open();
//        var dt = new DataTable();
//        new SqlDataAdapter(cmd).Fill(dt);
//        return dt;
//    }

//    public static void ExecuteNonQuery(string sql, params SqlParameter[] prms)
//    {
//        using var con = GetConnection();
//        using var cmd = new SqlCommand(sql, con);
//        if (prms != null) cmd.Parameters.AddRange(prms);
//        con.Open();
//        cmd.ExecuteNonQuery();
//    }

//    // ─────────────────────────────────────────────────────────────
//    //  USERS
//    // ─────────────────────────────────────────────────────────────
//    public static DataTable GetAllUsers() =>
//        ExecuteQuery("SELECT UserID, Username, FullName, Role, Email, Phone, IsActive, CreatedAt FROM Users");

//    public static DataTable ValidateLogin(string username, string passwordHash) =>
//        ExecuteQuery(
//            "SELECT UserID, FullName, Role FROM Users WHERE Username=@u AND PasswordHash=@p AND IsActive=1",
//            new SqlParameter("@u", username),
//            new SqlParameter("@p", passwordHash));

//    public static void AddUser(string username, string pwHash, string fullName, string role, string? email, string? phone) =>
//        ExecuteNonQuery("INSERT INTO Users (Username, PasswordHash, FullName, Role, Email, Phone) VALUES (@u, @p, @f, @r, @e, @ph)",
//            new SqlParameter("@u", username),
//            new SqlParameter("@p", pwHash),
//            new SqlParameter("@f", fullName),
//            new SqlParameter("@r", role),
//            new SqlParameter("@e", (object?)email ?? DBNull.Value),
//            new SqlParameter("@ph", (object?)phone ?? DBNull.Value));

//    public static void UpdateUser(int id, string fullName, string role, string? email, string? phone, bool isActive) =>
//        ExecuteNonQuery("UPDATE Users SET FullName=@f, Role=@r, Email=@e, Phone=@ph, IsActive=@a WHERE UserID=@id",
//            new SqlParameter("@id", id),
//            new SqlParameter("@f", fullName),
//            new SqlParameter("@r", role),
//            new SqlParameter("@e", (object?)email ?? DBNull.Value),
//            new SqlParameter("@ph", (object?)phone ?? DBNull.Value),
//            new SqlParameter("@a", isActive));

//    public static void DeleteUser(int id) =>
//        ExecuteNonQuery("DELETE FROM Users WHERE UserID=@id", new SqlParameter("@id", id));

//    // ─────────────────────────────────────────────────────────────
//    //  CUSTOMERS
//    // ─────────────────────────────────────────────────────────────
//    public static DataTable GetAllCustomers() =>
//        ExecuteQuery("SELECT CustomerID, FullName, Email, Phone, Address, CreatedAt FROM Customers");

//    public static void AddCustomer(string fullName, string? email, string phone, string? address) =>
//        ExecuteNonQuery("INSERT INTO Customers (FullName, Email, Phone, Address) VALUES (@f, @e, @p, @a)",
//            new SqlParameter("@f", fullName),
//            new SqlParameter("@e", (object?)email ?? DBNull.Value),
//            new SqlParameter("@p", phone),
//            new SqlParameter("@a", (object?)address ?? DBNull.Value));

//    public static void UpdateCustomer(int id, string fullName, string? email, string phone, string? address) =>
//        ExecuteNonQuery("UPDATE Customers SET FullName=@f, Email=@e, Phone=@p, Address=@a WHERE CustomerID=@id",
//            new SqlParameter("@id", id),
//            new SqlParameter("@f", fullName),
//            new SqlParameter("@e", (object?)email ?? DBNull.Value),
//            new SqlParameter("@p", phone),
//            new SqlParameter("@a", (object?)address ?? DBNull.Value));

//    public static void DeleteCustomer(int id) =>
//        ExecuteNonQuery("DELETE FROM Customers WHERE CustomerID=@id", new SqlParameter("@id", id));

//    // ─────────────────────────────────────────────────────────────
//    //  VEHICLES
//    // ─────────────────────────────────────────────────────────────
//    public static DataTable GetAllVehicles() =>
//        ExecuteQuery(@"SELECT v.VehicleID, c.FullName AS Customer, v.Make, v.Model, v.Year, v.LicensePlate, v.Mileage, v.CreatedAt
//                       FROM Vehicles v JOIN Customers c ON v.CustomerID=c.CustomerID");

//    public static DataTable GetVehiclesByCustomer(int customerId) =>
//        ExecuteQuery("SELECT VehicleID, Make+' '+Model+' ('+CAST(Year AS VARCHAR)+') ['+LicensePlate+']' AS Label FROM Vehicles WHERE CustomerID=@cid",
//            new SqlParameter("@cid", customerId));

//    public static void AddVehicle(int customerId, string make, string model, int year, string plate, int mileage) =>
//        ExecuteNonQuery("INSERT INTO Vehicles (CustomerID, Make, Model, Year, LicensePlate, Mileage) VALUES (@cid, @m, @mo, @y, @p, @mi)",
//            new SqlParameter("@cid", customerId),
//            new SqlParameter("@m", make),
//            new SqlParameter("@mo", model),
//            new SqlParameter("@y", year),
//            new SqlParameter("@p", plate),
//            new SqlParameter("@mi", mileage));

//    public static void UpdateVehicle(int id, string make, string model, int year, string plate, int mileage) =>
//        ExecuteNonQuery("UPDATE Vehicles SET Make=@m, Model=@mo, Year=@y, LicensePlate=@p, Mileage=@mi WHERE VehicleID=@id",
//            new SqlParameter("@id", id),
//            new SqlParameter("@m", make),
//            new SqlParameter("@mo", model),
//            new SqlParameter("@y", year),
//            new SqlParameter("@p", plate),
//            new SqlParameter("@mi", mileage));

//    public static void DeleteVehicle(int id) =>
//        ExecuteNonQuery("DELETE FROM Vehicles WHERE VehicleID=@id", new SqlParameter("@id", id));

//    // ─────────────────────────────────────────────────────────────
//    //  SERVICE SCHEDULE
//    // ─────────────────────────────────────────────────────────────
//    public static DataTable GetAllSchedules() =>
//        ExecuteQuery(@"SELECT ss.ScheduleID, c.FullName AS Customer, v.Make+' '+v.Model AS Vehicle, v.LicensePlate,
//                              ss.ScheduledDate, ss.ServiceType, ss.Status, ss.Notes
//                       FROM ServiceSchedule ss
//                       JOIN Vehicles v ON ss.VehicleID = v.VehicleID
//                       JOIN Customers c ON ss.CustomerID = c.CustomerID
//                       ORDER BY ss.ScheduledDate DESC");

//    public static void AddSchedule(int vehicleId, int customerId, DateTime date, string serviceType, string? notes, int createdBy) =>
//        ExecuteNonQuery("INSERT INTO ServiceSchedule (VehicleID, CustomerID, ScheduledDate, ServiceType, Notes, CreatedBy) VALUES (@vid, @cid, @d, @st, @n, @cb)",
//            new SqlParameter("@vid", vehicleId),
//            new SqlParameter("@cid", customerId),
//            new SqlParameter("@d", date),
//            new SqlParameter("@st", serviceType),
//            new SqlParameter("@n", (object?)notes ?? DBNull.Value),
//            new SqlParameter("@cb", createdBy));

//    public static void UpdateScheduleStatus(int scheduleId, string status) =>
//        ExecuteNonQuery("UPDATE ServiceSchedule SET Status=@s WHERE ScheduleID=@id",
//            new SqlParameter("@id", scheduleId),
//            new SqlParameter("@s", status));

//    // ─────────────────────────────────────────────────────────────
//    //  SERVICE DETAILS
//    // ─────────────────────────────────────────────────────────────
//    public static DataTable GetAllServiceDetails() =>
//        ExecuteQuery(@"SELECT sd.ServiceID, c.FullName AS Customer, v.Make+' '+v.Model AS Vehicle, v.LicensePlate,
//                              sd.ServiceDate, sd.Description, sd.Status, sd.ServiceCost, sd.PartsUsed, u.FullName AS Technician
//                       FROM ServiceDetails sd
//                       JOIN Vehicles v ON sd.VehicleID = v.VehicleID
//                       JOIN Customers c ON v.CustomerID = c.CustomerID
//                       LEFT JOIN Users u ON sd.TechnicianID = u.UserID
//                       ORDER BY sd.ServiceDate DESC");

//    public static void AddServiceDetail(int scheduleId, int vehicleId, int? techId, string description, decimal cost, string? parts) =>
//        ExecuteNonQuery("INSERT INTO ServiceDetails (ScheduleID, VehicleID, TechnicianID, Description, ServiceCost, PartsUsed, Status) VALUES (@sid, @vid, @tid, @d, @c, @p, 'Completed')",
//            new SqlParameter("@sid", scheduleId),
//            new SqlParameter("@vid", vehicleId),
//            new SqlParameter("@tid", (object?)techId ?? DBNull.Value),
//            new SqlParameter("@d", description),
//            new SqlParameter("@c", cost),
//            new SqlParameter("@p", (object?)parts ?? DBNull.Value));

//    // ─────────────────────────────────────────────────────────────
//    //  COMPLAINTS (Updated to Raw SQL)
//    // ─────────────────────────────────────────────────────────────
//    public static DataTable GetAllComplaints() =>
//        ExecuteQuery(@"SELECT comp.ComplaintID, c.FullName AS Customer, v.Make+' '+v.Model AS Vehicle,
//                              comp.Subject, comp.Description, comp.Status, comp.ResolutionNotes,
//                              u.FullName AS AssignedTo, comp.CreatedAt, comp.ResolvedAt
//                       FROM Complaints comp
//                       JOIN Customers c ON comp.CustomerID = c.CustomerID
//                       LEFT JOIN Vehicles v ON comp.VehicleID = v.VehicleID
//                       LEFT JOIN Users u ON comp.AssignedTo = u.UserID
//                       ORDER BY comp.CreatedAt DESC");

//    public static void AddComplaint(int customerId, int? vehicleId, string subject, string description) =>
//        ExecuteNonQuery("INSERT INTO Complaints (CustomerID, VehicleID, Subject, Description, Status) VALUES (@cid, @vid, @s, @d, 'Open')",
//            new SqlParameter("@cid", customerId),
//            new SqlParameter("@vid", (object?)vehicleId ?? DBNull.Value),
//            new SqlParameter("@s", subject),
//            new SqlParameter("@d", description));

//    public static void AssignComplaint(int complaintId, int userId) =>
//        ExecuteNonQuery("UPDATE Complaints SET AssignedTo=@uid, Status='Assigned' WHERE ComplaintID=@id",
//            new SqlParameter("@id", complaintId),
//            new SqlParameter("@uid", userId));

//    public static void ResolveComplaint(int complaintId, string notes) =>
//        ExecuteNonQuery("UPDATE Complaints SET ResolutionNotes=@n, Status='Resolved', ResolvedAt=GETDATE() WHERE ComplaintID=@id",
//            new SqlParameter("@id", complaintId),
//            new SqlParameter("@n", notes));

//    // ─────────────────────────────────────────────────────────────
//    //  REPORTS / UTIL
//    // ─────────────────────────────────────────────────────────────
//    public static decimal GetVehicleTotalCost(int vehicleId)
//    {
//        var result = ExecuteQuery("SELECT SUM(ServiceCost) as v FROM ServiceDetails WHERE VehicleID=@id", new SqlParameter("@id", vehicleId));
//        return (result.Rows.Count > 0 && result.Rows[0]["v"] != DBNull.Value) ? Convert.ToDecimal(result.Rows[0]["v"]) : 0;
//    }

//    public static DataTable GetAllUsersActive() =>
//        ExecuteQuery("SELECT UserID, FullName FROM Users WHERE IsActive=1");
//}