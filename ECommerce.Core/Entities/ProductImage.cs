using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Product image entity for storing multiple images per product
/// </summary>
public class ProductImage : BaseEntity
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ImageUrl { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? AltText { get; set; }
    
    public bool IsMain { get; set; } = false;
    
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
}
