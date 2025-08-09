using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Shipping address entity for storing user delivery addresses
/// </summary>
public class ShippingAddress : BaseEntity
{
    [Required]
    public int AppUserId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string AddressLine { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? AddressLine2 { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string ZipCode { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public bool IsDefault { get; set; } = false;
    
    // Navigation Properties
    public virtual AppUser AppUser { get; set; } = null!;
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
