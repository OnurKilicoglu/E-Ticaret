using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Core.Entities;

/// <summary>
/// Order item entity representing individual products within an order
/// </summary>
public class OrderItem : BaseEntity
{
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountAmount { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? ProductSKU { get; set; }
    
    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
