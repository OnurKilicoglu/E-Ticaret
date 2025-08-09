using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// FAQ Category entity for organizing frequently asked questions
/// </summary>
public class FAQCategory : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [MaxLength(100)]
    public string? Icon { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual ICollection<FAQ> FAQs { get; set; } = new List<FAQ>();
}

