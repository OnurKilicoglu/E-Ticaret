using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Core.Entities;

/// <summary>
/// Order entity representing customer purchases
/// </summary>
public class Order : BaseEntity
{
    [Required]
    public int AppUserId { get; set; }
    
    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingCost { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountAmount { get; set; }
    
    [Required]
    public int ShippingAddressId { get; set; }
    
    [Required]
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    
    [MaxLength(50)]
    public string? OrderNumber { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime? ShippedDate { get; set; }
    
    public DateTime? DeliveredDate { get; set; }
    
    [MaxLength(100)]
    public string? TrackingNumber { get; set; }
    
    // Navigation Properties
    public virtual AppUser AppUser { get; set; } = null!;
    public virtual ShippingAddress ShippingAddress { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual Payment? Payment { get; set; }
}
