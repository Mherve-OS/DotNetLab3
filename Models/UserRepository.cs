using System.Data;
using MySqlConnector;

namespace StockTrack.Models;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public User? Authenticate(string username, string passwordHash)
    {
        const string sql = @"
            SELECT UserID, Username, PasswordHash, FullName, Role, IsActive, CreatedAt
            FROM Users
            WHERE Username = @username AND PasswordHash = @hash AND IsActive = 1
            LIMIT 1";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@username", MySqlDbType.VarChar, 50).Value = username;
        cmd.Parameters.Add("@hash", MySqlDbType.VarChar, 255).Value = passwordHash;

        con.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                UserID = reader.GetInt32("UserID"),
                Username = reader.GetString("Username"),
                PasswordHash = reader.GetString("PasswordHash"),
                FullName = reader.GetString("FullName"),
                Role = reader.GetString("Role"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
        return null;
    }

    public DataTable GetAll()
    {
        const string sql = @"
            SELECT UserID, Username, FullName, Role, IsActive, CreatedAt
            FROM Users
            ORDER BY UserID ASC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public void Insert(User user)
    {
        const string sql = @"
            INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive)
            VALUES (@username, @hash, @fullname, @role, @active)";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@username", MySqlDbType.VarChar, 50).Value = user.Username;
        cmd.Parameters.Add("@hash", MySqlDbType.VarChar, 255).Value = user.PasswordHash;
        cmd.Parameters.Add("@fullname", MySqlDbType.VarChar, 100).Value = user.FullName;
        cmd.Parameters.Add("@role", MySqlDbType.VarChar, 30).Value = user.Role;
        cmd.Parameters.Add("@active", MySqlDbType.Int16).Value = user.IsActive ? 1 : 0;

        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(User user)
    {
        string sql = @"
            UPDATE Users
            SET FullName = @fullname, Role = @role, IsActive = @active";

        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            sql += ", PasswordHash = @hash";
        }
        sql += " WHERE UserID = @id";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@fullname", MySqlDbType.VarChar, 100).Value = user.FullName;
        cmd.Parameters.Add("@role", MySqlDbType.VarChar, 30).Value = user.Role;
        cmd.Parameters.Add("@active", MySqlDbType.Int16).Value = user.IsActive ? 1 : 0;
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = user.UserID;

        if (!string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            cmd.Parameters.Add("@hash", MySqlDbType.VarChar, 255).Value = user.PasswordHash;
        }

        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Deactivate(int userId, bool isActive)
    {
        const string sql = "UPDATE Users SET IsActive = @active WHERE UserID = @id";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@active", MySqlDbType.Int16).Value = isActive ? 1 : 0;
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = userId;

        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int userId)
    {
        const string sql = "DELETE FROM Users WHERE UserID = @id";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = userId;

        con.Open();
        cmd.ExecuteNonQuery();
    }
}
