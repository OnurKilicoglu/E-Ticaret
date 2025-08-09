using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Shopping cart entity for storing user's selected products
/// </summary>
public class Cart : BaseEntity
{
    [Required]
    public int AppUserId { get; set; }
    
    public string? SessionId { get; set; }
    
    // Navigation Properties
    public virtual AppUser AppUser { get; set; } = null!;
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
