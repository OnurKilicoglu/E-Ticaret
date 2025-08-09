using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for Order entity operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly ECommerceDbContext _context;

    public OrderService(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersAsync(
        string? searchTerm = null,
        OrderStatus? status = null,
        PaymentStatus? paymentStatus = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string sortBy = "orderDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.AppUser)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(o => o.OrderNumber.Contains(searchTerm) ||
                                   o.AppUser.UserName.Contains(searchTerm) ||
                                   o.AppUser.Email.Contains(searchTerm) ||
                                   (o.ShippingAddress != null && 
                                    (o.ShippingAddress.AddressLine.Contains(searchTerm) ||
                                     o.ShippingAddress.City.Contains(searchTerm))));
        }

        // Apply status filter
        if (status.HasValue)
        {
            query = query.Where(o => o.OrderStatus == status.Value);
        }

        // Apply payment status filter
        if (paymentStatus.HasValue && paymentStatus.Value != PaymentStatus.None)
        {
            query = query.Where(o => o.Payment != null && o.Payment.PaymentStatus == paymentStatus.Value);
        }

        // Apply date range filter
        if (fromDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= toDate.Value.AddDays(1));
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "totalamount" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            "status" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(o => o.OrderStatus)
                : query.OrderBy(o => o.OrderStatus),
            "customer" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(o => o.AppUser.UserName)
                : query.OrderBy(o => o.AppUser.UserName),
            "ordernumber" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(o => o.OrderNumber)
                : query.OrderBy(o => o.OrderNumber),
            _ => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate)
        };

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.AppUser)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetOrderByOrderNumberAsync(string orderNumber)
    {
        return await _context.Orders
            .Include(o => o.AppUser)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
    }

    public async Task<Order> CreateOrderAsync(int userId, IEnumerable<CartItem> cartItems, ShippingAddress shippingAddress, string paymentMethod, string? notes = null)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Generate unique order number
            string orderNumber;
            do
            {
                orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            } while (await _context.Orders.AnyAsync(o => o.OrderNumber == orderNumber));

            // Save shipping address to database
            _context.ShippingAddresses.Add(shippingAddress);
            await _context.SaveChangesAsync();

            // Calculate totals
            var subtotal = cartItems.Sum(item => item.Product.Price * item.Quantity);
            var shippingCost = subtotal >= 50 ? 0 : 9.99m;
            var tax = subtotal * 0.08m; // 8% tax
            var total = subtotal + shippingCost + tax;

            // Create order
            var order = new Order
            {
                OrderNumber = orderNumber,
                AppUserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = OrderStatus.Pending,
                SubTotal = subtotal,
                ShippingCost = shippingCost,
                TaxAmount = tax,
                TotalAmount = total,
                ShippingAddressId = shippingAddress.Id,
                Notes = notes,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items
            foreach (var cartItem in cartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product.Price,
                    ProductName = cartItem.Product.Name,
                    ProductSKU = cartItem.Product.SKU,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.OrderItems.Add(orderItem);
            }

            // Create payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = ConvertToPaymentMethodEnum(paymentMethod),
                PaymentStatus = PaymentStatus.Pending,
                Amount = total,
                PaymentDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // Return order with all related data
            return await GetOrderByIdAsync(order.Id) ?? order;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Helper method to convert payment method string to enum
    /// </summary>
    private PaymentMethod ConvertToPaymentMethodEnum(string paymentMethod)
    {
        return paymentMethod?.ToLower() switch
        {
            "visa" or "mastercard" or "american express" or "credit card" => PaymentMethod.CreditCard,
            "paypal" => PaymentMethod.PayPal,
            "bank transfer" => PaymentMethod.BankTransfer,
            "cash on delivery" => PaymentMethod.CashOnDelivery,
            _ => PaymentMethod.CreditCard // Default to credit card
        };
    }

    public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByCustomerAsync(int customerId, int page = 1, int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Payment)
            .Where(o => o.AppUserId == customerId)
            .OrderByDescending(o => o.OrderDate);

        var totalCount = await query.CountAsync();
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalCount);
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, string? notes = null, int? adminUserId = null)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            return false;

        // Validate status transition
        if (!IsValidStatusTransition(order.OrderStatus, newStatus))
            return false;

        var oldStatus = order.OrderStatus;
        order.OrderStatus = newStatus;
        order.UpdatedDate = DateTime.UtcNow;

        // Add status history record
        var statusHistory = new OrderStatusHistory
        {
            OrderId = orderId,
            FromStatus = oldStatus,
            ToStatus = newStatus,
            Notes = notes,
            AdminUserId = adminUserId,
            ChangedDate = DateTime.UtcNow
        };

        // Handle business logic based on status change
        await HandleStatusChangeBusinessLogic(order, oldStatus, newStatus);

        _context.Orders.Update(order);
        // Note: In a real implementation, you'd add the status history to the context
        // _context.OrderStatusHistories.Add(statusHistory);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelOrderAsync(int orderId, string reason, int? adminUserId = null)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            return false;

        // Can only cancel if order is pending or processing
        if (order.OrderStatus != OrderStatus.Pending && order.OrderStatus != OrderStatus.Processing)
            return false;

        var oldStatus = order.OrderStatus;
        order.OrderStatus = OrderStatus.Cancelled;
        order.UpdatedDate = DateTime.UtcNow;

        // Restore stock for cancelled orders
        await RestoreStockForOrder(order);

        // Handle payment refund if payment was successful
        if (order.Payment?.PaymentStatus == PaymentStatus.Completed)
        {
            await ProcessRefundAsync(orderId, order.TotalAmount, $"Order cancellation: {reason}", adminUserId);
        }

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProcessRefundAsync(int orderId, decimal refundAmount, string reason, int? adminUserId = null)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order?.Payment == null)
            return false;

        // Validate refund amount
        if (refundAmount <= 0 || refundAmount > order.TotalAmount)
            return false;

        // Update payment status
        order.Payment.PaymentStatus = PaymentStatus.Refunded;
        order.Payment.UpdatedDate = DateTime.UtcNow;

        // In a real implementation, you would integrate with payment gateway here
        // await _paymentGateway.ProcessRefund(order.Payment.TransactionId, refundAmount);

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<OrderStatistics> GetOrderStatisticsAsync()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var today = DateTime.Today;

        var statusCounts = await _context.Orders
            .GroupBy(o => o.OrderStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var totalRevenue = await _context.Orders
            .Where(o => o.OrderStatus == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var todayRevenue = await _context.Orders
            .Where(o => o.OrderDate >= today && o.OrderStatus == OrderStatus.Delivered)
            .SumAsync(o => o.TotalAmount);

        var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

        return new OrderStatistics
        {
            TotalOrders = totalOrders,
            PendingOrders = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Pending)?.Count ?? 0,
            ProcessingOrders = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Processing)?.Count ?? 0,
            ShippedOrders = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Shipped)?.Count ?? 0,
            DeliveredOrders = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Delivered)?.Count ?? 0,
            CancelledOrders = statusCounts.FirstOrDefault(s => s.Status == OrderStatus.Cancelled)?.Count ?? 0,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue,
            AverageOrderValue = averageOrderValue
        };
    }

    public async Task<IEnumerable<Order>> GetRecentOrdersAsync(int count = 10)
    {
        return await _context.Orders
            .Include(o => o.AppUser)
            .Include(o => o.Payment)
            .OrderByDescending(o => o.OrderDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.AppUser)
            .Include(o => o.OrderItems)
            .Include(o => o.Payment)
            .Where(o => o.OrderStatus == status)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public bool IsValidStatusTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => newStatus is OrderStatus.Processing or OrderStatus.Cancelled,
            OrderStatus.Processing => newStatus is OrderStatus.Shipped or OrderStatus.Cancelled,
            OrderStatus.Shipped => newStatus is OrderStatus.Delivered or OrderStatus.Returned,
            OrderStatus.Delivered => newStatus is OrderStatus.Returned,
            OrderStatus.Cancelled => false, // Cannot change from cancelled
            OrderStatus.Returned => false, // Cannot change from returned
            _ => false
        };
    }

    public List<OrderStatus> GetValidNextStatuses(OrderStatus currentStatus)
    {
        return currentStatus switch
        {
            OrderStatus.Pending => new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled },
            OrderStatus.Processing => new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled },
            OrderStatus.Shipped => new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.Returned },
            OrderStatus.Delivered => new List<OrderStatus> { OrderStatus.Returned },
            _ => new List<OrderStatus>()
        };
    }

    public async Task<int> GetOrderCountAsync()
    {
        return await _context.Orders.CountAsync();
    }

    public async Task<IEnumerable<Order>> GetOrdersNeedingAttentionAsync()
    {
        return await _context.Orders
            .Include(o => o.AppUser)
            .Include(o => o.Payment)
            .Where(o => o.OrderStatus == OrderStatus.Pending || o.OrderStatus == OrderStatus.Processing)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<OrderMetrics> GetOrderMetricsAsync(DateTime fromDate, DateTime toDate)
    {
        var orders = await _context.Orders
            .Include(o => o.Payment)
            .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
            .ToListAsync();

        var orderCount = orders.Count;
        var totalRevenue = orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount);
        var averageOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0;

        var ordersByStatus = orders
            .GroupBy(o => o.OrderStatus)
            .ToDictionary(g => g.Key, g => g.Count());

        var ordersByPaymentStatus = orders
            .Where(o => o.Payment != null)
            .GroupBy(o => o.Payment!.PaymentStatus)
            .ToDictionary(g => g.Key, g => g.Count());

        return new OrderMetrics
        {
            OrderCount = orderCount,
            TotalRevenue = totalRevenue,
            AverageOrderValue = averageOrderValue,
            OrdersByStatus = ordersByStatus,
            OrdersByPaymentStatus = ordersByPaymentStatus
        };
    }

    public async Task<bool> AddOrderNoteAsync(int orderId, string note, int? adminUserId = null)
    {
        var order = await GetOrderByIdAsync(orderId);
        if (order == null)
            return false;

        // In a real implementation, you would have an OrderNotes table
        // For now, we'll just update the order's updated date
        order.UpdatedDate = DateTime.UtcNow;
        
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<OrderStatusHistory>> GetOrderStatusHistoryAsync(int orderId)
    {
        // In a real implementation, you would query the OrderStatusHistory table
        // For now, returning empty list as the table doesn't exist in current schema
        return new List<OrderStatusHistory>();
    }

    private async Task HandleStatusChangeBusinessLogic(Order order, OrderStatus oldStatus, OrderStatus newStatus)
    {
        switch (newStatus)
        {
            case OrderStatus.Cancelled:
                await RestoreStockForOrder(order);
                break;
            case OrderStatus.Shipped:
                // Send shipping notification email
                // Update tracking information
                break;
            case OrderStatus.Delivered:
                // Send delivery confirmation
                // Update customer points/rewards
                break;
        }
    }

    private async Task RestoreStockForOrder(Order order)
    {
        foreach (var orderItem in order.OrderItems)
        {
            var product = await _context.Products.FindAsync(orderItem.ProductId);
            if (product != null)
            {
                product.StockQuantity += orderItem.Quantity;
                _context.Products.Update(product);
            }
        }
    }
}
