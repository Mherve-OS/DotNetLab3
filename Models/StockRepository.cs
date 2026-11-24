using System.Data;
using MySqlConnector;

namespace StockTrack.Models;

public class StockRepository
{
    private readonly string _connectionString;

    public StockRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void ProcessTransaction(StockTransaction trans, bool allowNegative = false)
    {
        using var con = new MySqlConnection(_connectionString);
        con.Open();
        using var transaction = con.BeginTransaction();

        try
        {
            // 1. Fetch current product quantity
            int currentQty = 0;
            const string getQtySql = "SELECT Quantity FROM Products WHERE ProductID = @id FOR UPDATE";
            using (var getCmd = new MySqlCommand(getQtySql, con, transaction))
            {
                getCmd.Parameters.Add("@id", MySqlDbType.Int32).Value = trans.ProductID;
                var res = getCmd.ExecuteScalar();
                if (res == null)
                {
                    throw new InvalidOperationException($"Product with ID {trans.ProductID} does not exist.");
                }
                currentQty = Convert.ToInt32(res);
            }

            // 2. Compute new quantity
            int newQty = currentQty;
            if (trans.TransactionType.Equals("IN", StringComparison.OrdinalIgnoreCase))
            {
                newQty += trans.Quantity;
            }
            else if (trans.TransactionType.Equals("OUT", StringComparison.OrdinalIgnoreCase))
            {
                newQty -= trans.Quantity;
            }
            else if (trans.TransactionType.Equals("ADJUSTMENT", StringComparison.OrdinalIgnoreCase))
            {
                // In adjustments, the quantity can be positive (found extra) or negative (damaged/lost)
                newQty += trans.Quantity;
            }

            // FR-18: Prevent negative stock quantities unless explicitly allowed by an admin
            if (newQty < 0 && !allowNegative)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock! Current stock is {currentQty}. Resulting quantity cannot be negative ({newQty}) without Admin permission.");
            }

            // 3. Update product quantity
            const string updateSql = "UPDATE Products SET Quantity = @qty WHERE ProductID = @id";
            using (var updateCmd = new MySqlCommand(updateSql, con, transaction))
            {
                updateCmd.Parameters.Add("@qty", MySqlDbType.Int32).Value = newQty;
                updateCmd.Parameters.Add("@id", MySqlDbType.Int32).Value = trans.ProductID;
                updateCmd.ExecuteNonQuery();
            }

            // 4. Insert transaction record
            const string insertTransSql = @"
                INSERT INTO StockTransactions (ProductID, TransactionType, Quantity, Reason, UserID)
                VALUES (@pid, @type, @qty, @reason, @uid)";
            using (var insertCmd = new MySqlCommand(insertTransSql, con, transaction))
            {
                insertCmd.Parameters.Add("@pid", MySqlDbType.Int32).Value = trans.ProductID;
                insertCmd.Parameters.Add("@type", MySqlDbType.VarChar, 20).Value = trans.TransactionType.ToUpper();
                insertCmd.Parameters.Add("@qty", MySqlDbType.Int32).Value = trans.Quantity;
                insertCmd.Parameters.Add("@reason", MySqlDbType.VarChar, 255).Value = trans.Reason.Trim();
                insertCmd.Parameters.Add("@uid", MySqlDbType.Int32).Value = trans.UserID;
                insertCmd.ExecuteNonQuery();
            }

            // 5. Commit transaction
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public DataTable GetAllTransactions()
    {
        const string sql = @"
            SELECT t.TransactionID, t.TransactionDate, t.TransactionType, t.Quantity,
                   p.SKU, p.ProductName, u.FullName AS PerformedBy, t.Reason
            FROM StockTransactions t
            INNER JOIN Products p ON t.ProductID = p.ProductID
            INNER JOIN Users u ON t.UserID = u.UserID
            ORDER BY t.TransactionID DESC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public DataTable GetRecentTransactions(int limit = 10)
    {
        const string sql = @"
            SELECT t.TransactionID, t.TransactionDate, t.TransactionType, t.Quantity,
                   p.SKU, p.ProductName, u.FullName AS PerformedBy, t.Reason
            FROM StockTransactions t
            INNER JOIN Products p ON t.ProductID = p.ProductID
            INNER JOIN Users u ON t.UserID = u.UserID
            ORDER BY t.TransactionID DESC
            LIMIT @lim";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@lim", MySqlDbType.Int32).Value = limit;
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }
}
