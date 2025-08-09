using System.ComponentModel.DataAnnotations;

namespace ECommerce.Core.Entities;

/// <summary>
/// Blog post entity for content management and SEO
/// </summary>
public class BlogPost : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? ImageUrl { get; set; }
    
    [MaxLength(500)]
    public string? Summary { get; set; }
    
    [MaxLength(200)]
    public string? Slug { get; set; }
    
    [MaxLength(160)]
    public string? MetaDescription { get; set; }
    
    [MaxLength(200)]
    public string? MetaKeywords { get; set; }
    
    public bool IsPublished { get; set; } = true;
    
    public bool IsFeatured { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? PublishedDate { get; set; }
    
    public int ViewCount { get; set; } = 0;
    
    [MaxLength(100)]
    public string? Category { get; set; }
    
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
