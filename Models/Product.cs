namespace StockTrack.Models;

public class Product
{
    public int ProductID { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int SupplierID { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsLowStock => Quantity <= ReorderLevel;
}
