using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// FAQ entity for frequently asked questions
/// </summary>
public class FAQ : BaseEntity
{
    [Required]
    [MaxLength(500)]
    public string Question { get; set; } = string.Empty;
    
    [Required]
    public string Answer { get; set; } = string.Empty;
    
    public int? CategoryId { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public int ViewCount { get; set; } = 0;
    
    public bool IsHelpful { get; set; } = false;
    
    public int HelpfulCount { get; set; } = 0;
    
    public int NotHelpfulCount { get; set; } = 0;
    
    [MaxLength(200)]
    public string? Tags { get; set; }
    
    [MaxLength(100)]
    public string? Author { get; set; }
    
    // Navigation Properties
    public virtual FAQCategory? Category { get; set; }
}