using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for payment operations
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Get all payments with pagination and filtering
    /// </summary>
    Task<(IEnumerable<Payment> Payments, int TotalCount)> GetAllAsync(
        string? searchTerm = null,
        int? userId = null,
        int? orderId = null,
        PaymentMethod? paymentMethod = null,
        PaymentStatus? paymentStatus = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "paymentDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get payment by ID with related data
    /// </summary>
    Task<Payment?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new payment
    /// </summary>
    Task<Payment?> CreateAsync(Payment payment);

    /// <summary>
    /// Update existing payment
    /// </summary>
    Task<bool> UpdateAsync(Payment payment);

    /// <summary>
    /// Delete payment (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Hard delete payment (permanent)
    /// </summary>
    Task<bool> HardDeleteAsync(int id);

    /// <summary>
    /// Change payment status
    /// </summary>
    Task<bool> ChangeStatusAsync(int id, PaymentStatus newStatus, string? reason = null);

    /// <summary>
    /// Get payments by order ID
    /// </summary>
    Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId);

    /// <summary>
    /// Get payments by user ID
    /// </summary>
    Task<IEnumerable<Payment>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Get payment statistics
    /// </summary>
    Task<PaymentStatistics> GetPaymentStatisticsAsync();

    /// <summary>
    /// Get payment summary for specific filters
    /// </summary>
    Task<PaymentSummary> GetPaymentSummaryAsync(
        int? userId = null,
        int? orderId = null,
        PaymentMethod? paymentMethod = null,
        PaymentStatus? paymentStatus = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Export payments to CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync(
        string? searchTerm = null,
        int? userId = null,
        int? orderId = null,
        PaymentMethod? paymentMethod = null,
        PaymentStatus? paymentStatus = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Process refund for a payment
    /// </summary>
    Task<bool> ProcessRefundAsync(int id, decimal refundAmount, string reason);

    /// <summary>
    /// Validate payment data
    /// </summary>
    Task<(bool IsValid, string ErrorMessage)> ValidatePaymentAsync(Payment payment);

    /// <summary>
    /// Get payments pending processing
    /// </summary>
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync();

    /// <summary>
    /// Get failed payments for retry
    /// </summary>
    Task<IEnumerable<Payment>> GetFailedPaymentsAsync();
}

/// <summary>
/// Payment statistics model
/// </summary>
public class PaymentStatistics
{
    public int TotalPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public int PendingPayments { get; set; }
    public decimal PendingAmount { get; set; }
    public int CompletedPayments { get; set; }
    public decimal CompletedAmount { get; set; }
    public int FailedPayments { get; set; }
    public decimal FailedAmount { get; set; }
    public int RefundedPayments { get; set; }
    public decimal RefundedAmount { get; set; }
    public int TodayPayments { get; set; }
    public decimal TodayAmount { get; set; }
    public int ThisWeekPayments { get; set; }
    public decimal ThisWeekAmount { get; set; }
    public int ThisMonthPayments { get; set; }
    public decimal ThisMonthAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public Payment? LatestPayment { get; set; }
    public Payment? LargestPayment { get; set; }
    public double AveragePaymentAmount { get; set; }
    public PaymentMethod MostUsedPaymentMethod { get; set; }
}

/// <summary>
/// Payment summary model for filtered results
/// </summary>
public class PaymentSummary
{
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal MinAmount { get; set; }
    public Dictionary<PaymentStatus, int> StatusCounts { get; set; } = new();
    public Dictionary<PaymentMethod, int> MethodCounts { get; set; } = new();
    public Dictionary<PaymentStatus, decimal> StatusAmounts { get; set; } = new();
    public Dictionary<PaymentMethod, decimal> MethodAmounts { get; set; } = new();
}

