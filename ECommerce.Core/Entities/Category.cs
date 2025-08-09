using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Product category entity for organizing products
/// </summary>
public class Category : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(255)]
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation Properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
