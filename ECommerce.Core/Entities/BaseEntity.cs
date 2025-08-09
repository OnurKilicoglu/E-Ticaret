using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Base entity class containing common properties for all entities
/// </summary>
public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedDate { get; set; }
}
