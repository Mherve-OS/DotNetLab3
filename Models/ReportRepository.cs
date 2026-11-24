using System.Data;
using MySqlConnector;

namespace StockTrack.Models;

public class ReportRepository
{
    private readonly string _connectionString;

    public ReportRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DataTable GetCurrentStockReport()
    {
        const string sql = @"
            SELECT p.SKU, p.ProductName, c.CategoryName, s.SupplierName,
                   p.UnitPrice, p.Quantity, (p.UnitPrice * p.Quantity) AS TotalValue,
                   p.ReorderLevel,
                   (CASE WHEN p.Quantity <= p.ReorderLevel THEN 'LOW STOCK' ELSE 'OK' END) AS Status
            FROM Products p
            INNER JOIN Categories c ON p.CategoryID = c.CategoryID
            INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
            ORDER BY c.CategoryName, p.ProductName";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public DataTable GetStockMovementReport(DateTime? fromDate = null, DateTime? toDate = null)
    {
        string sql = @"
            SELECT t.TransactionID, t.TransactionDate, t.TransactionType, t.Quantity,
                   p.SKU, p.ProductName, u.FullName AS PerformedBy, t.Reason
            FROM StockTransactions t
            INNER JOIN Products p ON t.ProductID = p.ProductID
            INNER JOIN Users u ON t.UserID = u.UserID
            WHERE 1=1";

        if (fromDate.HasValue) sql += " AND t.TransactionDate >= @from";
        if (toDate.HasValue) sql += " AND t.TransactionDate <= @to";

        sql += " ORDER BY t.TransactionDate DESC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        if (fromDate.HasValue) cmd.Parameters.Add("@from", MySqlDbType.DateTime).Value = fromDate.Value;
        if (toDate.HasValue) cmd.Parameters.Add("@to", MySqlDbType.DateTime).Value = toDate.Value;

        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public DataTable GetLowStockReport()
    {
        const string sql = @"
            SELECT p.SKU, p.ProductName, c.CategoryName, s.SupplierName,
                   p.Quantity AS CurrentStock, p.ReorderLevel,
                   (p.ReorderLevel - p.Quantity) AS Shortage,
                   s.ContactPerson, s.Phone, s.Email
            FROM Products p
            INNER JOIN Categories c ON p.CategoryID = c.CategoryID
            INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
            WHERE p.Quantity <= p.ReorderLevel
            ORDER BY CurrentStock ASC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public DataTable GetSupplierReport()
    {
        const string sql = @"
            SELECT s.SupplierID, s.SupplierName, s.ContactPerson, s.Email, s.Phone,
                   COUNT(p.ProductID) AS TotalProductsSupplied,
                   COALESCE(SUM(p.Quantity), 0) AS TotalUnitsInStock,
                   COALESCE(SUM(p.Quantity * p.UnitPrice), 0) AS TotalInventoryValue
            FROM Suppliers s
            LEFT JOIN Products p ON s.SupplierID = p.SupplierID
            GROUP BY s.SupplierID, s.SupplierName, s.ContactPerson, s.Email, s.Phone
            ORDER BY s.SupplierName ASC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }
}
