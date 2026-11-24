namespace StockTrack.Models;

public class StockTransaction
{
    public int TransactionID { get; set; }
    public int ProductID { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = "IN"; // IN, OUT, ADJUSTMENT
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int UserID { get; set; }
    public string PerformedBy { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.Now;
}
