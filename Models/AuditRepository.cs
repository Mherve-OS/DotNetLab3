using System.Data;
using MySqlConnector;

namespace StockTrack.Models;

public class AuditRepository
{
    private readonly string _connectionString;

    public AuditRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void LogAction(int userId, string actionType, string details)
    {
        const string sql = @"
            INSERT INTO AuditLogs (UserID, ActionType, Details)
            VALUES (@uid, @type, @details)";

        try
        {
            using var con = new MySqlConnection(_connectionString);
            using var cmd = new MySqlCommand(sql, con);
            cmd.Parameters.Add("@uid", MySqlDbType.Int32).Value = userId;
            cmd.Parameters.Add("@type", MySqlDbType.VarChar, 50).Value = actionType;
            cmd.Parameters.Add("@details", MySqlDbType.Text).Value = details;
            con.Open();
            cmd.ExecuteNonQuery();
        }
        catch
        {
            // Failure to audit log should not crash main operation
        }
    }

    public DataTable GetAllLogs()
    {
        const string sql = @"
            SELECT a.LogID, a.LogTimestamp, u.Username, u.FullName, a.ActionType, a.Details
            FROM AuditLogs a
            INNER JOIN Users u ON a.UserID = u.UserID
            ORDER BY a.LogID DESC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }
}
