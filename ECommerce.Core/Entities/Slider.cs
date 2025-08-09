using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Slider entity for homepage banner management
/// </summary>
public class Slider : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ImageUrl { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Link { get; set; }
    
    [MaxLength(50)]
    public string? ButtonText { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int DisplayOrder { get; set; } = 0;
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}
