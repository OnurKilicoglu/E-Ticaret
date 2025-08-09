using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for Product entity operations
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Get all products with optional search, filtering, and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for product name or description</param>
    /// <param name="categoryId">Filter by category ID</param>
    /// <param name="sortBy">Sort field (name, price, stock, created)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing products and total count</returns>
    Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAsync(
        string? searchTerm = null, 
        int? categoryId = null, 
        string sortBy = "name", 
        string sortOrder = "asc", 
        int page = 1, 
        int pageSize = 10);

    /// <summary>
    /// Get all active products for public display
    /// </summary>
    /// <returns>List of active products</returns>
    Task<IEnumerable<Product>> GetActiveProductsAsync();

    /// <summary>
    /// Get product by ID with category and images
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product entity or null if not found</returns>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Get product by SKU
    /// </summary>
    /// <param name="sku">Product SKU</param>
    /// <returns>Product entity or null if not found</returns>
    Task<Product?> GetProductBySkuAsync(string sku);

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="product">Product entity to create</param>
    /// <returns>Created product</returns>
    Task<Product> CreateProductAsync(Product product);

    /// <summary>
    /// Update an existing product
    /// </summary>
    /// <param name="product">Product entity to update</param>
    /// <returns>Updated product</returns>
    Task<Product> UpdateProductAsync(Product product);

    /// <summary>
    /// Soft delete a product (set IsActive to false)
    /// </summary>
    /// <param name="id">Product ID to delete</param>
    /// <returns>True if successful, false if product not found</returns>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Hard delete a product (permanent removal)
    /// </summary>
    /// <param name="id">Product ID to delete permanently</param>
    /// <returns>True if successful, false if product not found</returns>
    Task<bool> HardDeleteProductAsync(int id);

    /// <summary>
    /// Check if product name exists (for validation)
    /// </summary>
    /// <param name="name">Product name to check</param>
    /// <param name="excludeId">ID to exclude from check (for edit scenarios)</param>
    /// <returns>True if name exists, false otherwise</returns>
    Task<bool> ProductNameExistsAsync(string name, int? excludeId = null);

    /// <summary>
    /// Check if SKU exists (for validation)
    /// </summary>
    /// <param name="sku">SKU to check</param>
    /// <param name="excludeId">ID to exclude from check (for edit scenarios)</param>
    /// <returns>True if SKU exists, false otherwise</returns>
    Task<bool> SkuExistsAsync(string sku, int? excludeId = null);

    /// <summary>
    /// Get products by category ID
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of products in category</returns>
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

    /// <summary>
    /// Get low stock products (below threshold)
    /// </summary>
    /// <param name="threshold">Stock threshold (default: 10)</param>
    /// <returns>List of products with low stock</returns>
    Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);

    /// <summary>
    /// Get product count for dashboard statistics
    /// </summary>
    /// <returns>Total count of active products</returns>
    Task<int> GetProductCountAsync();

    /// <summary>
    /// Get featured products
    /// </summary>
    /// <param name="count">Number of featured products to return</param>
    /// <returns>List of featured products</returns>
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);

    /// <summary>
    /// Update product stock quantity
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">New quantity</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateStockAsync(int productId, int quantity);

    /// <summary>
    /// Increment product view count
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if successful</returns>
    Task<bool> IncrementViewCountAsync(int productId);
}
