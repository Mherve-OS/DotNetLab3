using System.Data;
using StockTrack.Models;

namespace StockTrack.Controllers;

public class CategoryController
{
    private readonly CategoryRepository _repository;
    private readonly AuditRepository _auditRepository;

    public CategoryController(CategoryRepository repository, AuditRepository auditRepository)
    {
        _repository = repository;
        _auditRepository = auditRepository;
    }

    public DataTable GetCategories() => _repository.GetAll();

    public void AddCategory(Category category, int currentUserId)
    {
        Validate(category);
        _repository.Insert(category);
        _auditRepository.LogAction(currentUserId, "CATEGORY_CREATE", $"Created category '{category.CategoryName}'.");
    }

    public void EditCategory(Category category, int currentUserId)
    {
        if (category.CategoryID <= 0)
            throw new ArgumentException("Select a category to update.");
        Validate(category);
        _repository.Update(category);
        _auditRepository.LogAction(currentUserId, "CATEGORY_UPDATE", $"Updated category '{category.CategoryName}' (ID: {category.CategoryID}).");
    }

    public void RemoveCategory(int categoryId, int currentUserId)
    {
        if (categoryId <= 0)
            throw new ArgumentException("Select a category to delete.");
        _repository.Delete(categoryId);
        _auditRepository.LogAction(currentUserId, "CATEGORY_DELETE", $"Deleted category ID {categoryId}.");
    }

    private static void Validate(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.CategoryName))
            throw new ArgumentException("Category name cannot be empty.");
    }
}
