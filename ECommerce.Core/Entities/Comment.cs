using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Comment entity for blog post comments
/// </summary>
public class Comment : BaseEntity
{
    [Required]
    public int BlogPostId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Website { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
    
    public bool IsApproved { get; set; } = false;
    
    public bool IsSpam { get; set; } = false;
    
    public int? ParentCommentId { get; set; }
    
    [MaxLength(45)]
    public string? IpAddress { get; set; }
    
    [MaxLength(500)]
    public string? UserAgent { get; set; }
    
    // Navigation Properties
    public virtual BlogPost BlogPost { get; set; } = null!;
    public virtual Comment? ParentComment { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}
