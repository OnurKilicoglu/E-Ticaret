using ECommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.Areas.Admin.Models;

#region Payment ViewModels

/// <summary>
/// ViewModel for payment list page
/// </summary>
public class PaymentListViewModel
{
    public IEnumerable<PaymentItemViewModel> Payments { get; set; } = new List<PaymentItemViewModel>();
    public PaymentStatisticsViewModel Statistics { get; set; } = new();
    public PaymentSummaryViewModel Summary { get; set; } = new();
    
    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalPages { get; set; }
    
    // Filtering
    public string? SearchTerm { get; set; }
    public int? UserId { get; set; }
    public int? OrderId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Sorting
    public string SortBy { get; set; } = "paymentDate";
    public string SortOrder { get; set; } = "desc";
    
    // Filter options
    public List<SelectListItem> UserOptions { get; set; } = new();
    public List<SelectListItem> OrderOptions { get; set; } = new();
    public List<SelectListItem> PaymentMethodOptions { get; set; } = new();
    public List<SelectListItem> PaymentStatusOptions { get; set; } = new();
    
    // Helper properties
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || 
                             UserId.HasValue || 
                             OrderId.HasValue || 
                             PaymentMethod.HasValue || 
                             PaymentStatus.HasValue || 
                             StartDate.HasValue || 
                             EndDate.HasValue;
                             
    public string GetSortIcon(string column)
    {
        if (SortBy.Equals(column, StringComparison.OrdinalIgnoreCase))
        {
            return SortOrder.ToLower() == "asc" ? "bi-sort-up" : "bi-sort-down";
        }
        return "bi-sort";
    }
    
    public string GetNextSortOrder(string column)
    {
        if (SortBy.Equals(column, StringComparison.OrdinalIgnoreCase))
        {
            return SortOrder.ToLower() == "asc" ? "desc" : "asc";
        }
        return "asc";
    }
}

/// <summary>
/// ViewModel for individual payment item in list
/// </summary>
public class PaymentItemViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? FailureReason { get; set; }
    
    // Helper properties
    public string PaymentMethodDisplayName => PaymentMethod.ToString();
    public string PaymentStatusDisplayName => PaymentStatus.ToString();
    
    public string PaymentStatusCssClass => PaymentStatus switch
    {
        Core.Entities.PaymentStatus.Pending => "warning",
        Core.Entities.PaymentStatus.Completed => "success",
        Core.Entities.PaymentStatus.Failed => "danger",
        Core.Entities.PaymentStatus.Refunded => "info",
        _ => "secondary"
    };
    
    public string PaymentStatusIcon => PaymentStatus switch
    {
        Core.Entities.PaymentStatus.Pending => "bi-clock",
        Core.Entities.PaymentStatus.Completed => "bi-check-circle",
        Core.Entities.PaymentStatus.Failed => "bi-x-circle",
        Core.Entities.PaymentStatus.Refunded => "bi-arrow-counterclockwise",
        _ => "bi-question-circle"
    };
    
    public string PaymentMethodIcon => PaymentMethod switch
    {
        Core.Entities.PaymentMethod.CreditCard => "bi-credit-card",
        Core.Entities.PaymentMethod.PayPal => "bi-paypal",
        Core.Entities.PaymentMethod.BankTransfer => "bi-bank",
        Core.Entities.PaymentMethod.CashOnDelivery => "bi-cash",
        _ => "bi-wallet"
    };
    
    public string FormattedAmount => $"${Amount:F2}";
    
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.UtcNow - PaymentDate;
            return timeSpan.TotalDays >= 1 ? $"{(int)timeSpan.TotalDays} days ago" :
                   timeSpan.TotalHours >= 1 ? $"{(int)timeSpan.TotalHours} hours ago" :
                   timeSpan.TotalMinutes >= 1 ? $"{(int)timeSpan.TotalMinutes} minutes ago" :
                   "Just now";
        }
    }
}

/// <summary>
/// ViewModel for payment details page
/// </summary>
public class PaymentDetailViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentGateway { get; set; }
    public string? PaymentDetails { get; set; }
    public string? FailureReason { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    
    // Order details
    public decimal OrderTotal { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime OrderDate { get; set; }
    public string? ShippingAddress { get; set; }
    public List<PaymentOrderItemViewModel> OrderItems { get; set; } = new();
    
    // Helper properties
    public string PaymentMethodDisplayName => PaymentMethod.ToString();
    public string PaymentStatusDisplayName => PaymentStatus.ToString();
    public string OrderStatusDisplayName => OrderStatus.ToString();
    
    public string PaymentStatusCssClass => PaymentStatus switch
    {
        Core.Entities.PaymentStatus.Pending => "warning",
        Core.Entities.PaymentStatus.Completed => "success",
        Core.Entities.PaymentStatus.Failed => "danger",
        Core.Entities.PaymentStatus.Refunded => "info",
        _ => "secondary"
    };
    
    public string PaymentStatusIcon => PaymentStatus switch
    {
        Core.Entities.PaymentStatus.Pending => "bi-clock-fill",
        Core.Entities.PaymentStatus.Completed => "bi-check-circle-fill",
        Core.Entities.PaymentStatus.Failed => "bi-x-circle-fill",
        Core.Entities.PaymentStatus.Refunded => "bi-arrow-counterclockwise",
        _ => "bi-question-circle-fill"
    };
    
    public string PaymentMethodIcon => PaymentMethod switch
    {
        Core.Entities.PaymentMethod.CreditCard => "bi-credit-card-fill",
        Core.Entities.PaymentMethod.PayPal => "bi-paypal",
        Core.Entities.PaymentMethod.BankTransfer => "bi-bank2",
        Core.Entities.PaymentMethod.CashOnDelivery => "bi-cash-stack",
        _ => "bi-wallet-fill"
    };
    
    public string FormattedAmount => $"${Amount:F2}";
    public string FormattedOrderTotal => $"${OrderTotal:F2}";
    
    public bool CanRefund => PaymentStatus == Core.Entities.PaymentStatus.Completed;
    public bool CanRetry => PaymentStatus == Core.Entities.PaymentStatus.Failed;
    public bool CanCancel => PaymentStatus == Core.Entities.PaymentStatus.Pending;
}

/// <summary>
/// ViewModel for order item display in payment details
/// </summary>
public class PaymentOrderItemViewModel
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    public string FormattedUnitPrice => $"${UnitPrice:F2}";
    public string FormattedTotalPrice => $"${TotalPrice:F2}";
}

/// <summary>
/// ViewModel for payment creation
/// </summary>
public class PaymentCreateViewModel
{
    [Required(ErrorMessage = "Order is required")]
    [Display(Name = "Order")]
    public int OrderId { get; set; }
    
    [Required(ErrorMessage = "Payment method is required")]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
    
    [Display(Name = "Payment Status")]
    public PaymentStatus PaymentStatus { get; set; } = Core.Entities.PaymentStatus.Pending;
    
    [StringLength(100, ErrorMessage = "Transaction ID cannot exceed 100 characters")]
    [Display(Name = "Transaction ID")]
    public string? TransactionId { get; set; }
    
    [StringLength(100, ErrorMessage = "Payment gateway cannot exceed 100 characters")]
    [Display(Name = "Payment Gateway")]
    public string? PaymentGateway { get; set; }
    
    [StringLength(500, ErrorMessage = "Payment details cannot exceed 500 characters")]
    [Display(Name = "Payment Details")]
    public string? PaymentDetails { get; set; }
    
    [Display(Name = "Payment Date")]
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    
    public List<SelectListItem> OrderOptions { get; set; } = new();
    public List<SelectListItem> PaymentMethodOptions { get; set; } = new();
    public List<SelectListItem> PaymentStatusOptions { get; set; } = new();
    
    // Helper for pre-filling order info
    public string? SelectedOrderInfo { get; set; }
    public decimal? SelectedOrderAmount { get; set; }
}

/// <summary>
/// ViewModel for payment editing
/// </summary>
public class PaymentEditViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Order is required")]
    [Display(Name = "Order")]
    public int OrderId { get; set; }
    
    [Required(ErrorMessage = "Payment method is required")]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }
    
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }
    
    [Display(Name = "Payment Status")]
    public PaymentStatus PaymentStatus { get; set; }
    
    [StringLength(100, ErrorMessage = "Transaction ID cannot exceed 100 characters")]
    [Display(Name = "Transaction ID")]
    public string? TransactionId { get; set; }
    
    [StringLength(100, ErrorMessage = "Payment gateway cannot exceed 100 characters")]
    [Display(Name = "Payment Gateway")]
    public string? PaymentGateway { get; set; }
    
    [StringLength(500, ErrorMessage = "Payment details cannot exceed 500 characters")]
    [Display(Name = "Payment Details")]
    public string? PaymentDetails { get; set; }
    
    [StringLength(500, ErrorMessage = "Failure reason cannot exceed 500 characters")]
    [Display(Name = "Failure Reason")]
    public string? FailureReason { get; set; }
    
    [Display(Name = "Payment Date")]
    public DateTime PaymentDate { get; set; }
    
    [Display(Name = "Processed Date")]
    public DateTime? ProcessedDate { get; set; }
    
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    
    public List<SelectListItem> OrderOptions { get; set; } = new();
    public List<SelectListItem> PaymentMethodOptions { get; set; } = new();
    public List<SelectListItem> PaymentStatusOptions { get; set; } = new();
    
    // Read-only display info
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal OrderTotal { get; set; }
}

/// <summary>
/// ViewModel for payment deletion
/// </summary>
public class PaymentDeleteViewModel
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public string? TransactionId { get; set; }
    public DateTime PaymentDate { get; set; }
    
    [Required(ErrorMessage = "Please provide a reason for deletion")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
    
    [Display(Name = "Permanent Delete")]
    public bool HardDelete { get; set; }
    
    // Helper properties
    public string PaymentMethodDisplayName => PaymentMethod.ToString();
    public string PaymentStatusDisplayName => PaymentStatus.ToString();
    public string FormattedAmount => $"${Amount:F2}";
    
    public string PaymentStatusCssClass => PaymentStatus switch
    {
        Core.Entities.PaymentStatus.Pending => "warning",
        Core.Entities.PaymentStatus.Completed => "success",
        Core.Entities.PaymentStatus.Failed => "danger",
        Core.Entities.PaymentStatus.Refunded => "info",
        _ => "secondary"
    };
    
    public string DeleteTypeDisplayName => HardDelete ? "Permanent Deletion" : "Standard Deletion";
    public string DeleteTypeDescription => HardDelete 
        ? "This payment will be permanently removed from the database and cannot be recovered." 
        : "This payment will be removed from the system.";
}

/// <summary>
/// ViewModel for payment statistics
/// </summary>
public class PaymentStatisticsViewModel
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
    public string? LatestPaymentUser { get; set; }
    public decimal? LargestPaymentAmount { get; set; }
    public double AveragePaymentAmount { get; set; }
    public string? MostUsedPaymentMethod { get; set; }
    
    // Helper properties
    public string FormattedTotalAmount => $"${TotalAmount:F2}";
    public string FormattedPendingAmount => $"${PendingAmount:F2}";
    public string FormattedCompletedAmount => $"${CompletedAmount:F2}";
    public string FormattedFailedAmount => $"${FailedAmount:F2}";
    public string FormattedRefundedAmount => $"${RefundedAmount:F2}";
    public string FormattedTodayAmount => $"${TodayAmount:F2}";
    public string FormattedThisWeekAmount => $"${ThisWeekAmount:F2}";
    public string FormattedThisMonthAmount => $"${ThisMonthAmount:F2}";
    public string FormattedAverageAmount => $"${AveragePaymentAmount:F2}";
    public string FormattedLargestAmount => LargestPaymentAmount.HasValue ? $"${LargestPaymentAmount.Value:F2}" : "$0.00";
    
    public double CompletedPercentage => TotalPayments > 0 ? (double)CompletedPayments / TotalPayments * 100 : 0;
    public double FailedPercentage => TotalPayments > 0 ? (double)FailedPayments / TotalPayments * 100 : 0;
    public double PendingPercentage => TotalPayments > 0 ? (double)PendingPayments / TotalPayments * 100 : 0;
    
    public string CompletedPercentageFormatted => $"{CompletedPercentage:F1}%";
    public string FailedPercentageFormatted => $"{FailedPercentage:F1}%";
    public string PendingPercentageFormatted => $"{PendingPercentage:F1}%";
}

/// <summary>
/// ViewModel for payment summary
/// </summary>
public class PaymentSummaryViewModel
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
    
    // Helper properties
    public string FormattedTotalAmount => $"${TotalAmount:F2}";
    public string FormattedAverageAmount => $"${AverageAmount:F2}";
    public string FormattedMaxAmount => $"${MaxAmount:F2}";
    public string FormattedMinAmount => $"${MinAmount:F2}";
}

/// <summary>
/// ViewModel for status change
/// </summary>
public class PaymentStatusChangeViewModel
{
    public int Id { get; set; }
    public PaymentStatus CurrentStatus { get; set; }
    public PaymentStatus NewStatus { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// ViewModel for refund processing
/// </summary>
public class PaymentRefundViewModel
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    
    public string FormattedAmount => $"${Amount:F2}";
    public string FormattedRefundAmount => $"${RefundAmount:F2}";
}

#endregion
