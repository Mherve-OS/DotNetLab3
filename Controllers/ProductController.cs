using System.Data;
using StockTrack.Models;

namespace StockTrack.Controllers;

public class ProductController
{
    private readonly ProductRepository _repository;
    private readonly AuditRepository _auditRepository;

    public ProductController(ProductRepository repository, AuditRepository auditRepository)
    {
        _repository = repository;
        _auditRepository = auditRepository;
    }

    public DataTable GetProducts() => _repository.GetAll();

    public Product? GetProductById(int productId) => _repository.GetById(productId);

    public DataTable SearchProducts(string keyword, int? categoryId = null, int? supplierId = null)
    {
        return _repository.Search(keyword, categoryId, supplierId);
    }

    public DataTable GetLowStockProducts() => _repository.GetLowStock();

    public (int TotalProducts, int LowStockItems, int TotalStockUnits) GetDashboardStats()
    {
        return (_repository.GetTotalProductCount(), _repository.GetLowStockCount(), _repository.GetTotalStockQuantity());
    }

    public void AddProduct(Product product, int currentUserId)
    {
        Validate(product);

        if (_repository.SkuExists(product.SKU))
        {
            throw new InvalidOperationException($"A product with SKU '{product.SKU}' already exists.");
        }

        _repository.Insert(product);
        _auditRepository.LogAction(currentUserId, "PRODUCT_CREATE",
            $"Created product '{product.ProductName}' (SKU: {product.SKU}, Qty: {product.Quantity}).");
    }

    public void EditProduct(Product product, int currentUserId)
    {
        if (product.ProductID <= 0)
            throw new ArgumentException("Select a product to update.");

        Validate(product);

        if (_repository.SkuExists(product.SKU, product.ProductID))
        {
            throw new InvalidOperationException($"Another product already uses SKU '{product.SKU}'.");
        }

        _repository.Update(product);
        _auditRepository.LogAction(currentUserId, "PRODUCT_UPDATE",
            $"Updated product '{product.ProductName}' (ID: {product.ProductID}, SKU: {product.SKU}).");
    }

    public void RemoveProduct(int productId, int currentUserId)
    {
        if (productId <= 0)
            throw new ArgumentException("Select a product to delete.");

        var existing = _repository.GetById(productId);
        try
        {
            _repository.Delete(productId);
            _auditRepository.LogAction(currentUserId, "PRODUCT_DELETE",
                $"Deleted product '{existing?.ProductName ?? productId.ToString()}' (ID: {productId}).");
        }
        catch (MySqlConnector.MySqlException ex) when (ex.Number == 1451) // Foreign key constraint violation
        {
            throw new InvalidOperationException(
                $"Cannot delete '{existing?.ProductName ?? "this product"}' because it has recorded stock transactions in history. Delete transactions first or adjust stock to 0.", ex);
        }
    }

    private static void Validate(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.SKU))
            throw new ArgumentException("Product SKU is required.");
        if (string.IsNullOrWhiteSpace(product.ProductName))
            throw new ArgumentException("Product name is required.");
        if (product.CategoryID <= 0)
            throw new ArgumentException("Please select a category.");
        if (product.SupplierID <= 0)
            throw new ArgumentException("Please select a supplier.");
        if (product.UnitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.");
        if (product.Quantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative.");
        if (product.ReorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative.");
    }
}
