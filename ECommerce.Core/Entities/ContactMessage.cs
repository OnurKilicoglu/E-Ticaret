using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Contact message entity for storing customer inquiries
/// </summary>
public class ContactMessage : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;
    
    public bool IsRead { get; set; } = false;
    
    public bool IsReplied { get; set; } = false;
    
    [MaxLength(1000)]
    public string? AdminReply { get; set; }
    
    public DateTime? RepliedDate { get; set; }
    
    public int? RepliedByUserId { get; set; }
}
