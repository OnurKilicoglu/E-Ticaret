using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Core.Entities;

/// <summary>
/// Payment entity for tracking order payments
/// </summary>
public class Payment : BaseEntity
{
    [Required]
    public int OrderId { get; set; }
    
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    [Required]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(100)]
    public string? TransactionId { get; set; }
    
    [MaxLength(100)]
    public string? PaymentGateway { get; set; }
    
    [MaxLength(500)]
    public string? PaymentDetails { get; set; }
    
    [MaxLength(500)]
    public string? FailureReason { get; set; }
    
    public DateTime? ProcessedDate { get; set; }
    
    // Navigation Properties
    public virtual Order Order { get; set; } = null!;
}
