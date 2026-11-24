using System.Data;
using StockTrack.Models;

namespace StockTrack.Controllers;

public class StockController
{
    private readonly StockRepository _stockRepo;
    private readonly ProductRepository _productRepo;
    private readonly AuditRepository _auditRepo;

    public StockController(StockRepository stockRepo, ProductRepository productRepo, AuditRepository auditRepo)
    {
        _stockRepo = stockRepo;
        _productRepo = productRepo;
        _auditRepo = auditRepo;
    }

    public DataTable GetRecentTransactions(int limit = 10) => _stockRepo.GetRecentTransactions(limit);

    public DataTable GetAllTransactions() => _stockRepo.GetAllTransactions();

    public void RecordStockIn(int productId, int quantity, string reason, int userId)
    {
        if (productId <= 0) throw new ArgumentException("Select a valid product.");
        if (quantity <= 0) throw new ArgumentException("Stock-in quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(reason)) reason = "Stock In / Restock";

        var trans = new StockTransaction
        {
            ProductID = productId,
            TransactionType = "IN",
            Quantity = quantity,
            Reason = reason.Trim(),
            UserID = userId
        };

        _stockRepo.ProcessTransaction(trans, allowNegative: false);
        _auditRepo.LogAction(userId, "STOCK_IN", $"Added {quantity} units to product ID {productId}. Reason: {reason}");
    }

    public void RecordStockOut(int productId, int quantity, string reason, int userId, bool isAdminOverride = false)
    {
        if (productId <= 0) throw new ArgumentException("Select a valid product.");
        if (quantity <= 0) throw new ArgumentException("Stock-out quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(reason)) reason = "Sales / Issuance";

        var trans = new StockTransaction
        {
            ProductID = productId,
            TransactionType = "OUT",
            Quantity = quantity,
            Reason = reason.Trim(),
            UserID = userId
        };

        _stockRepo.ProcessTransaction(trans, allowNegative: isAdminOverride);
        _auditRepo.LogAction(userId, "STOCK_OUT", $"Removed {quantity} units from product ID {productId}. Reason: {reason}");
    }

    public void RecordStockAdjustment(int productId, int deltaQuantity, string reason, int userId, bool isAdminOverride = false)
    {
        if (productId <= 0) throw new ArgumentException("Select a valid product.");
        if (deltaQuantity == 0) throw new ArgumentException("Adjustment quantity cannot be zero.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Adjustment reason is required (e.g. Damage, Return, Physical Count Correction).");

        var trans = new StockTransaction
        {
            ProductID = productId,
            TransactionType = "ADJUSTMENT",
            Quantity = deltaQuantity,
            Reason = reason.Trim(),
            UserID = userId
        };

        _stockRepo.ProcessTransaction(trans, allowNegative: isAdminOverride);
        _auditRepo.LogAction(userId, "STOCK_ADJUSTMENT", $"Adjusted stock of product ID {productId} by {deltaQuantity}. Reason: {reason}");
    }
}
