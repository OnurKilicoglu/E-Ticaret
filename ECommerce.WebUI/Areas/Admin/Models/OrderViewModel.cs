using ECommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.Areas.Admin.Models;

/// <summary>
/// ViewModel for Order listing with advanced filtering
/// </summary>
public class OrderListViewModel
{
    public IEnumerable<OrderItemViewModel> Orders { get; set; } = new List<OrderItemViewModel>();
    
    // Pagination properties
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 20;
    
    // Search and filter properties
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }
    
    [Display(Name = "Order Status")]
    public OrderStatus? Status { get; set; }
    
    [Display(Name = "Payment Status")]
    public PaymentStatus? PaymentStatus { get; set; }
    
    [Display(Name = "From Date")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }
    
    [Display(Name = "To Date")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }
    
    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "orderDate";
    
    [Display(Name = "Sort Order")]
    public string SortOrder { get; set; } = "desc";
    
    // Available options for dropdowns
    public List<SelectListItem> StatusOptions { get; set; } = new();
    public List<SelectListItem> PaymentStatusOptions { get; set; } = new();
    public List<SelectListItem> SortOptions { get; set; } = new()
    {
        new SelectListItem { Value = "orderDate", Text = "Order Date" },
        new SelectListItem { Value = "totalAmount", Text = "Total Amount" },
        new SelectListItem { Value = "status", Text = "Status" },
        new SelectListItem { Value = "customer", Text = "Customer" },
        new SelectListItem { Value = "orderNumber", Text = "Order Number" }
    };
    
    // Statistics
    public OrderStatisticsViewModel Statistics { get; set; } = new();
    
    // Computed properties
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || Status.HasValue || 
                              PaymentStatus.HasValue || FromDate.HasValue || ToDate.HasValue;
}

/// <summary>
/// ViewModel for individual order items in list
/// </summary>
public class OrderItemViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus OrderStatus { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string ShippingCity { get; set; } = string.Empty;
    public DateTime? LastUpdated { get; set; }
    
    // Computed properties
    public string StatusDisplayName => OrderStatus.ToString();
    public string StatusCssClass => GetStatusCssClass(OrderStatus);
    public string PaymentStatusDisplayName => PaymentStatus?.ToString() ?? "N/A";
    public string PaymentStatusCssClass => GetPaymentStatusCssClass(PaymentStatus);
    public bool NeedsAttention => OrderStatus == OrderStatus.Pending || OrderStatus == OrderStatus.Processing;
    public int DaysOld => (DateTime.Now - OrderDate).Days;
    
    private static string GetStatusCssClass(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "warning",
            OrderStatus.Processing => "info",
            OrderStatus.Shipped => "primary",
            OrderStatus.Delivered => "success",
            OrderStatus.Cancelled => "danger",
            OrderStatus.Returned => "secondary",
            _ => "light"
        };
    }
    
    private static string GetPaymentStatusCssClass(PaymentStatus? status)
    {
        return status switch
        {
            ECommerce.Core.Entities.PaymentStatus.Completed => "success",
            ECommerce.Core.Entities.PaymentStatus.Pending => "warning",
            ECommerce.Core.Entities.PaymentStatus.Failed => "danger",
            ECommerce.Core.Entities.PaymentStatus.Refunded => "info",
            _ => "secondary"
        };
    }
}

/// <summary>
/// ViewModel for detailed order information
/// </summary>
public class OrderDetailViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime? LastUpdated { get; set; }
    
    // Customer Information
    public CustomerInfoViewModel Customer { get; set; } = new();
    
    // Order Items
    public List<OrderItemDetailViewModel> OrderItems { get; set; } = new();
    
    // Shipping Information
    public ShippingAddressViewModel? ShippingAddress { get; set; }
    
    // Payment Information
    public PaymentInfoViewModel? Payment { get; set; }
    
    // Order Summary
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Status Management
    public List<SelectListItem> AvailableStatuses { get; set; } = new();
    public List<OrderStatusHistoryViewModel> StatusHistory { get; set; } = new();
    
    // Admin Notes
    public List<OrderNoteViewModel> AdminNotes { get; set; } = new();
    
    // Computed properties
    public string StatusDisplayName => OrderStatus.ToString();
    public string StatusCssClass => GetStatusCssClass(OrderStatus);
    public bool CanBeCancelled => OrderStatus == OrderStatus.Pending || OrderStatus == OrderStatus.Processing;
    public bool CanBeRefunded => Payment?.PaymentStatus == ECommerce.Core.Entities.PaymentStatus.Completed;
    public bool IsCompleted => OrderStatus == OrderStatus.Delivered;
    public int ItemCount => OrderItems.Count;
    public int TotalQuantity => OrderItems.Sum(i => i.Quantity);
    
    private static string GetStatusCssClass(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "warning",
            OrderStatus.Processing => "info",
            OrderStatus.Shipped => "primary",
            OrderStatus.Delivered => "success",
            OrderStatus.Cancelled => "danger",
            OrderStatus.Returned => "secondary",
            _ => "light"
        };
    }
}

/// <summary>
/// ViewModel for order editing (status changes)
/// </summary>
public class OrderEditViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus CurrentStatus { get; set; }
    
    [Required(ErrorMessage = "Please select a new status")]
    [Display(Name = "New Status")]
    public OrderStatus NewStatus { get; set; }
    
    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    [Display(Name = "Admin Notes")]
    public string? Notes { get; set; }
    
    [Display(Name = "Send Notification")]
    public bool SendNotification { get; set; } = true;
    
    // Available statuses for dropdown
    public List<SelectListItem> AvailableStatuses { get; set; } = new();
    
    // Order summary for display
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Validation properties
    public bool IsValidTransition => GetValidStatuses().Any(s => s.Value == NewStatus.ToString());
    
    private List<SelectListItem> GetValidStatuses()
    {
        return CurrentStatus switch
        {
            OrderStatus.Pending => new List<SelectListItem>
            {
                new() { Value = OrderStatus.Processing.ToString(), Text = "Processing" },
                new() { Value = OrderStatus.Cancelled.ToString(), Text = "Cancelled" }
            },
            OrderStatus.Processing => new List<SelectListItem>
            {
                new() { Value = OrderStatus.Shipped.ToString(), Text = "Shipped" },
                new() { Value = OrderStatus.Cancelled.ToString(), Text = "Cancelled" }
            },
            OrderStatus.Shipped => new List<SelectListItem>
            {
                new() { Value = OrderStatus.Delivered.ToString(), Text = "Delivered" },
                new() { Value = OrderStatus.Returned.ToString(), Text = "Returned" }
            },
            OrderStatus.Delivered => new List<SelectListItem>
            {
                new() { Value = OrderStatus.Returned.ToString(), Text = "Returned" }
            },
            _ => new List<SelectListItem>()
        };
    }
}

/// <summary>
/// ViewModel for order cancellation
/// </summary>
public class OrderCancelViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus CurrentStatus { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    
    [Required(ErrorMessage = "Cancellation reason is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    [Display(Name = "Cancellation Reason")]
    public string Reason { get; set; } = string.Empty;
    
    [Display(Name = "Process Refund")]
    public bool ProcessRefund { get; set; } = true;
    
    [Display(Name = "Send Notification")]
    public bool SendNotification { get; set; } = true;
    
    // Computed properties
    public bool CanBeCancelled => CurrentStatus == OrderStatus.Pending || CurrentStatus == OrderStatus.Processing;
    public bool HasPayment => PaymentStatus == ECommerce.Core.Entities.PaymentStatus.Completed;
    public string RefundAmount => HasPayment ? TotalAmount.ToString("C") : "N/A";
}

/// <summary>
/// Supporting ViewModels
/// </summary>
public class CustomerInfoViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public class OrderItemDetailViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public string? ProductImageUrl { get; set; }
}

public class ShippingAddressViewModel
{
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string FullAddress => $"{AddressLine}, {City}, {Country} {ZipCode}";
}

public class PaymentInfoViewModel
{
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? TransactionId { get; set; }
    
    public string PaymentMethodDisplayName => PaymentMethod.ToString();
    public string PaymentStatusDisplayName => PaymentStatus.ToString();
    public string PaymentStatusCssClass => PaymentStatus switch
    {
        ECommerce.Core.Entities.PaymentStatus.Completed => "success",
        ECommerce.Core.Entities.PaymentStatus.Pending => "warning",
        ECommerce.Core.Entities.PaymentStatus.Failed => "danger",
        ECommerce.Core.Entities.PaymentStatus.Refunded => "info",
        _ => "secondary"
    };
}

public class OrderStatusHistoryViewModel
{
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Notes { get; set; }
    public string? AdminUserName { get; set; }
    public DateTime ChangedDate { get; set; }
}

public class OrderNoteViewModel
{
    public string Note { get; set; } = string.Empty;
    public string? AdminUserName { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class OrderStatisticsViewModel
{
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int ShippedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
}
