using System.Data;
using MySqlConnector;

namespace StockTrack.Models;

public class ProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DataTable GetAll()
    {
        const string sql = @"
            SELECT p.ProductID, p.SKU, p.ProductName, p.CategoryID, c.CategoryName,
                   p.SupplierID, s.SupplierName, p.UnitPrice, p.Quantity, p.ReorderLevel,
                   p.CreatedAt,
                   (CASE WHEN p.Quantity <= p.ReorderLevel THEN 'LOW STOCK' ELSE 'OK' END) AS StockStatus
            FROM Products p
            INNER JOIN Categories c ON p.CategoryID = c.CategoryID
            INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
            ORDER BY p.ProductID DESC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public Product? GetById(int productId)
    {
        const string sql = @"
            SELECT p.ProductID, p.SKU, p.ProductName, p.CategoryID, c.CategoryName,
                   p.SupplierID, s.SupplierName, p.UnitPrice, p.Quantity, p.ReorderLevel, p.CreatedAt
            FROM Products p
            INNER JOIN Categories c ON p.CategoryID = c.CategoryID
            INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
            WHERE p.ProductID = @id";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = productId;
        con.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Product
            {
                ProductID = reader.GetInt32("ProductID"),
                SKU = reader.GetString("SKU"),
                ProductName = reader.GetString("ProductName"),
                CategoryID = reader.GetInt32("CategoryID"),
                CategoryName = reader.GetString("CategoryName"),
                SupplierID = reader.GetInt32("SupplierID"),
                SupplierName = reader.GetString("SupplierName"),
                UnitPrice = reader.GetDecimal("UnitPrice"),
                Quantity = reader.GetInt32("Quantity"),
                ReorderLevel = reader.GetInt32("ReorderLevel"),
                CreatedAt = reader.GetDateTime("CreatedAt")
            };
        }
        return null;
    }

    public DataTable Search(string keyword, int? categoryId = null, int? supplierId = null)
    {
        string sql = @"
            SELECT p.ProductID, p.SKU, p.ProductName, p.CategoryID, c.CategoryName,
                   p.SupplierID, s.SupplierName, p.UnitPrice, p.Quantity, p.ReorderLevel,
                   p.CreatedAt,
                   (CASE WHEN p.Quantity <= p.ReorderLevel THEN 'LOW STOCK' ELSE 'OK' END) AS StockStatus
            FROM Products p
            INNER JOIN Categories c ON p.CategoryID = c.CategoryID
            INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
            WHERE 1=1";

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            sql += @" AND (p.ProductName LIKE @kw OR p.SKU LIKE @kw 
                           OR c.CategoryName LIKE @kw OR s.SupplierName LIKE @kw)";
        }
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            sql += " AND p.CategoryID = @catId";
        }
        if (supplierId.HasValue && supplierId.Value > 0)
        {
            sql += " AND p.SupplierID = @supId";
        }

        sql += " ORDER BY p.ProductID DESC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            cmd.Parameters.Add("@kw", MySqlDbType.VarChar, 150).Value = "%" + keyword.Trim() + "%";
        }
        if (categoryId.HasValue && categoryId.Value > 0)
        {
            cmd.Parameters.Add("@catId", MySqlDbType.Int32).Value = categoryId.Value;
        }
        if (supplierId.HasValue && supplierId.Value > 0)
        {
            cmd.Parameters.Add("@supId", MySqlDbType.Int32).Value = supplierId.Value;
        }

        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public DataTable GetLowStock()
    {
        const string sql = @"
            SELECT p.ProductID, p.SKU, p.ProductName, c.CategoryName, s.SupplierName,
                   p.Quantity, p.ReorderLevel, (p.ReorderLevel - p.Quantity) AS Shortage
            FROM Products p
            INNER JOIN Categories c ON p.CategoryID = c.CategoryID
            INNER JOIN Suppliers s ON p.SupplierID = s.SupplierID
            WHERE p.Quantity <= p.ReorderLevel
            ORDER BY p.Quantity ASC";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    public int GetTotalProductCount()
    {
        const string sql = "SELECT COUNT(*) FROM Products";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetLowStockCount()
    {
        const string sql = "SELECT COUNT(*) FROM Products WHERE Quantity <= ReorderLevel";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetTotalStockQuantity()
    {
        const string sql = "SELECT COALESCE(SUM(Quantity), 0) FROM Products";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        con.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public bool SkuExists(string sku, int excludeProductId = 0)
    {
        const string sql = "SELECT COUNT(*) FROM Products WHERE SKU = @sku AND ProductID <> @excludeId";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@sku", MySqlDbType.VarChar, 50).Value = sku.Trim();
        cmd.Parameters.Add("@excludeId", MySqlDbType.Int32).Value = excludeProductId;
        con.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public void Insert(Product product)
    {
        const string sql = @"
            INSERT INTO Products (SKU, ProductName, CategoryID, SupplierID, UnitPrice, Quantity, ReorderLevel)
            VALUES (@sku, @name, @catId, @supId, @price, @qty, @reorder)";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@sku", MySqlDbType.VarChar, 50).Value = product.SKU.Trim();
        cmd.Parameters.Add("@name", MySqlDbType.VarChar, 150).Value = product.ProductName.Trim();
        cmd.Parameters.Add("@catId", MySqlDbType.Int32).Value = product.CategoryID;
        cmd.Parameters.Add("@supId", MySqlDbType.Int32).Value = product.SupplierID;
        cmd.Parameters.Add("@price", MySqlDbType.Decimal).Value = product.UnitPrice;
        cmd.Parameters.Add("@qty", MySqlDbType.Int32).Value = product.Quantity;
        cmd.Parameters.Add("@reorder", MySqlDbType.Int32).Value = product.ReorderLevel;

        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Update(Product product)
    {
        const string sql = @"
            UPDATE Products
            SET SKU = @sku, ProductName = @name, CategoryID = @catId, SupplierID = @supId,
                UnitPrice = @price, Quantity = @qty, ReorderLevel = @reorder
            WHERE ProductID = @id";

        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@sku", MySqlDbType.VarChar, 50).Value = product.SKU.Trim();
        cmd.Parameters.Add("@name", MySqlDbType.VarChar, 150).Value = product.ProductName.Trim();
        cmd.Parameters.Add("@catId", MySqlDbType.Int32).Value = product.CategoryID;
        cmd.Parameters.Add("@supId", MySqlDbType.Int32).Value = product.SupplierID;
        cmd.Parameters.Add("@price", MySqlDbType.Decimal).Value = product.UnitPrice;
        cmd.Parameters.Add("@qty", MySqlDbType.Int32).Value = product.Quantity;
        cmd.Parameters.Add("@reorder", MySqlDbType.Int32).Value = product.ReorderLevel;
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = product.ProductID;

        con.Open();
        cmd.ExecuteNonQuery();
    }

    public void Delete(int productId)
    {
        const string sql = "DELETE FROM Products WHERE ProductID = @id";
        using var con = new MySqlConnection(_connectionString);
        using var cmd = new MySqlCommand(sql, con);
        cmd.Parameters.Add("@id", MySqlDbType.Int32).Value = productId;
        con.Open();
        cmd.ExecuteNonQuery();
    }
}
