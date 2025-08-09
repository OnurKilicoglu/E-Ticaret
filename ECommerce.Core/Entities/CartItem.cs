using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Cart item entity representing individual products in a shopping cart
/// </summary>
public class CartItem : BaseEntity
{
    [Required]
    public int CartId { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
    
    // Navigation Properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
