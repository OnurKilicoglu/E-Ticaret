using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for Category entity operations
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Get all categories with optional search and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for category name</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing categories and total count</returns>
    Task<(IEnumerable<Category> Categories, int TotalCount)> GetCategoriesAsync(string? searchTerm = null, int page = 1, int pageSize = 10);

    /// <summary>
    /// Get all active categories for dropdown lists
    /// </summary>
    /// <returns>List of active categories</returns>
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();

    /// <summary>
    /// Get category by ID
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category entity or null if not found</returns>
    Task<Category?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <param name="category">Category entity to create</param>
    /// <returns>Created category</returns>
    Task<Category> CreateCategoryAsync(Category category);

    /// <summary>
    /// Update an existing category
    /// </summary>
    /// <param name="category">Category entity to update</param>
    /// <returns>Updated category</returns>
    Task<Category> UpdateCategoryAsync(Category category);

    /// <summary>
    /// Soft delete a category (set IsActive to false)
    /// </summary>
    /// <param name="id">Category ID to delete</param>
    /// <returns>True if successful, false if category not found</returns>
    Task<bool> DeleteCategoryAsync(int id);

    /// <summary>
    /// Check if category name exists (for validation)
    /// </summary>
    /// <param name="name">Category name to check</param>
    /// <param name="excludeId">ID to exclude from check (for edit scenarios)</param>
    /// <returns>True if name exists, false otherwise</returns>
    Task<bool> CategoryNameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Get category count for dashboard statistics
    /// </summary>
    /// <returns>Total count of active categories</returns>
    Task<int> GetCategoryCountAsync();
}
