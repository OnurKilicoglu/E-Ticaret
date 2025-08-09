using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Application user entity representing registered users
/// </summary>
public class AppUser : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; } = UserRole.Customer;
    
    [MaxLength(50)]
    public string? FirstName { get; set; }
    
    [MaxLength(50)]
    public string? LastName { get; set; }
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastLoginDate { get; set; }
    
    public int LoginCount { get; set; } = 0;
    
    // Navigation Properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public virtual ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();
}
