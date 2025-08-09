using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Core.Entities;

/// <summary>
/// Product entity representing items available for purchase
/// </summary>
public class Product : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountPrice { get; set; }
    
    [Required]
    public int StockQuantity { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    [MaxLength(255)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(100)]
    public string? SKU { get; set; }
    
    [MaxLength(100)]
    public string? Brand { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool IsFeatured { get; set; } = false;
    
    public int ViewCount { get; set; } = 0;
    
    // Navigation Properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
