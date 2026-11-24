using System.Data;
using MySqlConnector;

namespace StockTrack.Models;

public class CategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DataTable GetAll()
    {
        const string sql = "SELECT CategoryID, CategoryName, Description FROM Categories ORDER BY CategoryName ASC";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public void Insert(Category category)
    {
        const string sql = "INSERT INTO Categories (CategoryName, Description) VALUES (@name, @desc)";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@name", MySqlDbType.VarChar, 100).Value = category.CategoryName.Trim();
        cmd.Parameters.Add("@desc", MySqlDbType.VarChar, 255).Value = category.Description.Trim();
        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(Category category)
    {
        const string sql = "UPDATE Categories SET CategoryName = @name, Description = @desc WHERE CategoryID = @id";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@name", MySqlDbType.VarChar, 100).Value = category.CategoryName.Trim();
        cmd.Parameters.Add("@desc", MySqlDbType.VarChar, 255).Value = category.Description.Trim();
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = category.CategoryID;
        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int categoryId)
    {
        const string sql = "DELETE FROM Categories WHERE CategoryID = @id";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = categoryId;
        con.Open();
        cmd.ExecuteNonQuery();
    }
}
