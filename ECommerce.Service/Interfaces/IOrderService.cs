using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for Order entity operations
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Get all orders with advanced filtering, search, and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for order number, customer name, or email</param>
    /// <param name="status">Filter by order status</param>
    /// <param name="paymentStatus">Filter by payment status</param>
    /// <param name="fromDate">Filter orders from this date</param>
    /// <param name="toDate">Filter orders to this date</param>
    /// <param name="sortBy">Sort field (orderDate, totalAmount, status, customer)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing orders and total count</returns>
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersAsync(
        string? searchTerm = null,
        OrderStatus? status = null,
        PaymentStatus? paymentStatus = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string sortBy = "orderDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 10);

    /// <summary>
    /// Get order by ID with all related data
    /// </summary>
    /// <param name="id">Order ID</param>
    /// <returns>Order entity with related data or null if not found</returns>
    Task<Order?> GetOrderByIdAsync(int id);

    /// <summary>
    /// Get order by order number
    /// </summary>
    /// <param name="orderNumber">Order number</param>
    /// <returns>Order entity or null if not found</returns>
    Task<Order?> GetOrderByOrderNumberAsync(string orderNumber);

    /// <summary>
    /// Create a new order from checkout data
    /// </summary>
    /// <param name="userId">User ID placing the order</param>
    /// <param name="cartItems">Items in the cart</param>
    /// <param name="shippingAddress">Shipping address</param>
    /// <param name="paymentMethod">Payment method</param>
    /// <param name="notes">Special instructions</param>
    /// <returns>Created order</returns>
    Task<Order> CreateOrderAsync(int userId, IEnumerable<CartItem> cartItems, ShippingAddress shippingAddress, string paymentMethod, string? notes = null);

    /// <summary>
    /// Get orders by customer ID
    /// </summary>
    /// <param name="customerId">Customer ID</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Customer orders with pagination</returns>
    Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByCustomerAsync(int customerId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Update order status with business logic validation
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="newStatus">New order status</param>
    /// <param name="notes">Admin notes for status change</param>
    /// <param name="adminUserId">ID of admin making the change</param>
    /// <returns>True if successful, false if invalid transition</returns>
    Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes = null, int? adminUserId = null);

    /// <summary>
    /// Cancel order with reason
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="reason">Cancellation reason</param>
    /// <param name="adminUserId">ID of admin cancelling the order</param>
    /// <returns>True if successful</returns>
    Task<bool> CancelOrderAsync(int orderId, string reason, int? adminUserId = null);

    /// <summary>
    /// Process refund for an order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="refundAmount">Amount to refund</param>
    /// <param name="reason">Refund reason</param>
    /// <param name="adminUserId">ID of admin processing refund</param>
    /// <returns>True if successful</returns>
    Task<bool> ProcessRefundAsync(int orderId, decimal refundAmount, string reason, int? adminUserId = null);

    /// <summary>
    /// Get order statistics for dashboard
    /// </summary>
    /// <returns>Order statistics</returns>
    Task<OrderStatistics> GetOrderStatisticsAsync();

    /// <summary>
    /// Get recent orders for dashboard
    /// </summary>
    /// <param name="count">Number of orders to return</param>
    /// <returns>Recent orders</returns>
    Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10);

    /// <summary>
    /// Get orders by status
    /// </summary>
    /// <param name="status">Order status</param>
    /// <returns>Orders with specified status</returns>
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);

    /// <summary>
    /// Check if order status transition is valid
    /// </summary>
    /// <param name="currentStatus">Current order status</param>
    /// <param name="newStatus">Proposed new status</param>
    /// <returns>True if transition is valid</returns>
    bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus);

    /// <summary>
    /// Get valid next statuses for an order
    /// </summary>
    /// <param name="currentStatus">Current order status</param>
    /// <returns>List of valid next statuses</returns>
    List<OrderStatus> GetValidNextStatuses(OrderStatus currentStatus);

    /// <summary>
    /// Get order total count
    /// </summary>
    /// <returns>Total number of orders</returns>
    Task<int> GetOrderCountAsync();

    /// <summary>
    /// Get orders needing attention (pending, processing)
    /// </summary>
    /// <returns>Orders that need admin attention</returns>
    Task<IEnumerable<Order>> GetOrdersNeedingAttentionAsync();

    /// <summary>
    /// Calculate order metrics for date range
    /// </summary>
    /// <param name="fromDate">Start date</param>
    /// <param name="toDate">End date</param>
    /// <returns>Order metrics</returns>
    Task<OrderMetrics> GetOrderMetricsAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Add admin note to order
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <param name="note">Admin note</param>
    /// <param name="adminUserId">ID of admin adding note</param>
    /// <returns>True if successful</returns>
    Task<bool> AddOrderNoteAsync(int orderId, string note, int? adminUserId = null);

    /// <summary>
    /// Get order status history
    /// </summary>
    /// <param name="orderId">Order ID</param>
    /// <returns>Status change history</returns>
    Task<IEnumerable<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId);
}

/// <summary>
/// Order statistics for dashboard
/// </summary>
public class OrderStatistics
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

/// <summary>
/// Order metrics for analytics
/// </summary>
public class OrderMetrics
{
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new();
    public Dictionary<PaymentStatus, int> OrdersByPaymentStatus { get; set; } = new();
}

/// <summary>
/// Order status change history
/// </summary>
public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Notes { get; set; }
    public int? AdminUserId { get; set; }
    public string? AdminUserName { get; set; }
    public DateTime ChangedDate { get; set; }
}
