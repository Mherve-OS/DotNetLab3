using System.Data;
using MySqlConnector;

namespace StockTrack.Models;

public class SupplierRepository
{
    private readonly string _connectionString;

    public SupplierRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DataTable GetAll()
    {
        const string sql = "SELECT SupplierID, SupplierName, ContactPerson, Email, Phone, Address FROM Suppliers ORDER BY SupplierName ASC";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public void Insert(Supplier supplier)
    {
        const string sql = @"
            INSERT INTO Suppliers (SupplierName, ContactPerson, Email, Phone, Address)
            VALUES (@name, @contact, @email, @phone, @address)";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@name", MySqlDbType.VarChar, 150).Value = supplier.SupplierName.Trim();
        cmd.Parameters.Add("@contact", MySqlDbType.VarChar, 100).Value = supplier.ContactPerson.Trim();
        cmd.Parameters.Add("@email", MySqlDbType.VarChar, 100).Value = supplier.Email.Trim();
        cmd.Parameters.Add("@phone", MySqlDbType.VarChar, 50).Value = supplier.Phone.Trim();
        cmd.Parameters.Add("@address", MySqlDbType.VarChar, 255).Value = supplier.Address.Trim();
        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(Supplier supplier)
    {
        const string sql = @"
            UPDATE Suppliers
            SET SupplierName = @name, ContactPerson = @contact, Email = @email, Phone = @phone, Address = @address
            WHERE SupplierID = @id";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@name", MySqlDbType.VarChar, 150).Value = supplier.SupplierName.Trim();
        cmd.Parameters.Add("@contact", MySqlDbType.VarChar, 100).Value = supplier.ContactPerson.Trim();
        cmd.Parameters.Add("@email", MySqlDbType.VarChar, 100).Value = supplier.Email.Trim();
        cmd.Parameters.Add("@phone", MySqlDbType.VarChar, 50).Value = supplier.Phone.Trim();
        cmd.Parameters.Add("@address", MySqlDbType.VarChar, 255).Value = supplier.Address.Trim();
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = supplier.SupplierID;
        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int supplierId)
    {
        const string sql = "DELETE FROM Suppliers WHERE SupplierID = @id";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = supplierId;
        con.Open();
        cmd.ExecuteNonQuery();
    }
}
