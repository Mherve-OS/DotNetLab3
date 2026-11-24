using System.Data;
using StockTrack.Models;

namespace StockTrack.Controllers;

public class SupplierController
{
    private readonly SupplierRepository _repository;
    private readonly AuditRepository _auditRepository;

    public SupplierController(SupplierRepository repository, AuditRepository auditRepository)
    {
        _repository = repository;
        _auditRepository = auditRepository;
    }

    public DataTable GetSuppliers() => _repository.GetAll();

    public void AddSupplier(Supplier supplier, int currentUserId)
    {
        Validate(supplier);
        _repository.Insert(supplier);
        _auditRepository.LogAction(currentUserId, "SUPPLIER_CREATE", $"Created supplier '{supplier.SupplierName}'.");
    }

    public void EditSupplier(Supplier supplier, int currentUserId)
    {
        if (supplier.SupplierID <= 0)
            throw new ArgumentException("Select a supplier to update.");
        Validate(supplier);
        _repository.Update(supplier);
        _auditRepository.LogAction(currentUserId, "SUPPLIER_UPDATE", $"Updated supplier '{supplier.SupplierName}' (ID: {supplier.SupplierID}).");
    }

    public void RemoveSupplier(int supplierId, int currentUserId)
    {
        if (supplierId <= 0)
            throw new ArgumentException("Select a supplier to delete.");
        _repository.Delete(supplierId);
        _auditRepository.LogAction(currentUserId, "SUPPLIER_DELETE", $"Deleted supplier ID {supplierId}.");
    }

    private static void Validate(Supplier supplier)
    {
        if (string.IsNullOrWhiteSpace(supplier.SupplierName))
            throw new ArgumentException("Supplier name cannot be empty.");
    }
}
