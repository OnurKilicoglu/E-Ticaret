using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Implementation of payment service
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly ECommerceDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ECommerceDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all payments with pagination and filtering
    /// </summary>
    public async Task<(IEnumerable<Payment> Payments, int TotalCount)> GetAllAsync(
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
        int pageSize = 20)
    {
        try
        {
            var query = _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.AppUser)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p => 
                    p.TransactionId!.ToLower().Contains(searchTerm) ||
                    p.PaymentGateway!.ToLower().Contains(searchTerm) ||
                    p.Order.AppUser.Email.ToLower().Contains(searchTerm) ||
                    p.Order.AppUser.FirstName.ToLower().Contains(searchTerm) ||
                    p.Order.AppUser.LastName.ToLower().Contains(searchTerm) ||
                    p.Order.OrderNumber!.ToLower().Contains(searchTerm));
            }

            if (userId.HasValue)
            {
                query = query.Where(p => p.Order.AppUserId == userId.Value);
            }

            if (orderId.HasValue)
            {
                query = query.Where(p => p.OrderId == orderId.Value);
            }

            if (paymentMethod.HasValue)
            {
                query = query.Where(p => p.PaymentMethod == paymentMethod.Value);
            }

            if (paymentStatus.HasValue)
            {
                query = query.Where(p => p.PaymentStatus == paymentStatus.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.PaymentDate <= endDate.Value.AddDays(1));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "amount" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(p => p.Amount) 
                    : query.OrderByDescending(p => p.Amount),
                "user" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(p => p.Order.AppUser.FirstName).ThenBy(p => p.Order.AppUser.LastName)
                    : query.OrderByDescending(p => p.Order.AppUser.FirstName).ThenByDescending(p => p.Order.AppUser.LastName),
                "order" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(p => p.OrderId) 
                    : query.OrderByDescending(p => p.OrderId),
                "method" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(p => p.PaymentMethod) 
                    : query.OrderByDescending(p => p.PaymentMethod),
                "status" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(p => p.PaymentStatus) 
                    : query.OrderByDescending(p => p.PaymentStatus),
                "processeddate" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(p => p.ProcessedDate) 
                    : query.OrderByDescending(p => p.ProcessedDate),
                _ => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(p => p.PaymentDate) 
                    : query.OrderByDescending(p => p.PaymentDate)
            };

            // Apply pagination
            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (payments, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments");
            return (Enumerable.Empty<Payment>(), 0);
        }
    }

    /// <summary>
    /// Get payment by ID with related data
    /// </summary>
    public async Task<Payment?> GetByIdAsync(int id)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.AppUser)
                .Include(p => p.Order)
                    .ThenInclude(o => o.ShippingAddress)
                .Include(p => p.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment by ID: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Create a new payment
    /// </summary>
    public async Task<Payment?> CreateAsync(Payment payment)
    {
        try
        {
            payment.CreatedDate = DateTime.UtcNow;
            payment.PaymentDate = DateTime.UtcNow;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment created with ID: {Id}", payment.Id);
            return payment;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return null;
        }
    }

    /// <summary>
    /// Update existing payment
    /// </summary>
    public async Task<bool> UpdateAsync(Payment payment)
    {
        try
        {
            payment.UpdatedDate = DateTime.UtcNow;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment updated with ID: {Id}", payment.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating payment: {Id}", payment.Id);
            return false;
        }
    }

    /// <summary>
    /// Delete payment (since Payment doesn't have IsActive, this will be hard delete)
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Hard delete payment (permanent)
    /// </summary>
    public async Task<bool> HardDeleteAsync(int id)
    {
        return await DeleteAsync(id); // Same as soft delete since Payment doesn't have IsActive
    }

    /// <summary>
    /// Change payment status
    /// </summary>
    public async Task<bool> ChangeStatusAsync(int id, PaymentStatus newStatus, string? reason = null)
    {
        try
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return false;

            var oldStatus = payment.PaymentStatus;
            payment.PaymentStatus = newStatus;
            payment.UpdatedDate = DateTime.UtcNow;

            if (newStatus == PaymentStatus.Completed)
            {
                payment.ProcessedDate = DateTime.UtcNow;
            }
            else if (newStatus == PaymentStatus.Failed && !string.IsNullOrEmpty(reason))
            {
                payment.FailureReason = reason;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment status changed from {OldStatus} to {NewStatus} for ID: {Id}", 
                oldStatus, newStatus, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing payment status: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Get payments by order ID
    /// </summary>
    public async Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.AppUser)
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments by order ID: {OrderId}", orderId);
            return Enumerable.Empty<Payment>();
        }
    }

    /// <summary>
    /// Get payments by user ID
    /// </summary>
    public async Task<IEnumerable<Payment>> GetByUserIdAsync(int userId)
    {
        try
        {
            return await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.AppUser)
                .Where(p => p.Order.AppUserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payments by user ID: {UserId}", userId);
            return Enumerable.Empty<Payment>();
        }
    }

    /// <summary>
    /// Get payment statistics
    /// </summary>
    public async Task<PaymentStatistics> GetPaymentStatisticsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var allPayments = _context.Payments.Include(p => p.Order).ThenInclude(o => o.AppUser);

            var statistics = new PaymentStatistics
            {
                TotalPayments = await allPayments.CountAsync(),
                TotalAmount = await allPayments.SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                PendingPayments = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Pending).CountAsync(),
                PendingAmount = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Pending).SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                CompletedPayments = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Completed).CountAsync(),
                CompletedAmount = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Completed).SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                FailedPayments = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Failed).CountAsync(),
                FailedAmount = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Failed).SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                RefundedPayments = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Refunded).CountAsync(),
                RefundedAmount = await allPayments.Where(p => p.PaymentStatus == PaymentStatus.Refunded).SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                TodayPayments = await allPayments.Where(p => p.PaymentDate >= todayStart).CountAsync(),
                TodayAmount = await allPayments.Where(p => p.PaymentDate >= todayStart).SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                ThisWeekPayments = await allPayments.Where(p => p.PaymentDate >= weekStart).CountAsync(),
                ThisWeekAmount = await allPayments.Where(p => p.PaymentDate >= weekStart).SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                ThisMonthPayments = await allPayments.Where(p => p.PaymentDate >= monthStart).CountAsync(),
                ThisMonthAmount = await allPayments.Where(p => p.PaymentDate >= monthStart).SumAsync(p => (decimal?)p.Amount) ?? 0,
                
                LastPaymentDate = await allPayments.MaxAsync(p => (DateTime?)p.PaymentDate),
                LatestPayment = await allPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefaultAsync(),
                LargestPayment = await allPayments.OrderByDescending(p => p.Amount).FirstOrDefaultAsync()
            };

            // Calculate average
            if (statistics.TotalPayments > 0)
            {
                statistics.AveragePaymentAmount = (double)(statistics.TotalAmount / statistics.TotalPayments);
            }

            // Find most used payment method
            var methodGroups = await allPayments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            if (methodGroups != null)
            {
                statistics.MostUsedPaymentMethod = methodGroups.Method;
            }

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment statistics");
            return new PaymentStatistics();
        }
    }

    /// <summary>
    /// Get payment summary for specific filters
    /// </summary>
    public async Task<PaymentSummary> GetPaymentSummaryAsync(
        int? userId = null,
        int? orderId = null,
        PaymentMethod? paymentMethod = null,
        PaymentStatus? paymentStatus = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var query = _context.Payments.Include(p => p.Order).AsQueryable();

            // Apply same filters as GetAllAsync
            if (userId.HasValue)
                query = query.Where(p => p.Order.AppUserId == userId.Value);

            if (orderId.HasValue)
                query = query.Where(p => p.OrderId == orderId.Value);

            if (paymentMethod.HasValue)
                query = query.Where(p => p.PaymentMethod == paymentMethod.Value);

            if (paymentStatus.HasValue)
                query = query.Where(p => p.PaymentStatus == paymentStatus.Value);

            if (startDate.HasValue)
                query = query.Where(p => p.PaymentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.PaymentDate <= endDate.Value.AddDays(1));

            var payments = await query.ToListAsync();

            var summary = new PaymentSummary
            {
                Count = payments.Count,
                TotalAmount = payments.Sum(p => p.Amount),
                AverageAmount = payments.Any() ? payments.Average(p => p.Amount) : 0,
                MaxAmount = payments.Any() ? payments.Max(p => p.Amount) : 0,
                MinAmount = payments.Any() ? payments.Min(p => p.Amount) : 0
            };

            // Group by status
            summary.StatusCounts = payments.GroupBy(p => p.PaymentStatus)
                .ToDictionary(g => g.Key, g => g.Count());

            summary.StatusAmounts = payments.GroupBy(p => p.PaymentStatus)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            // Group by method
            summary.MethodCounts = payments.GroupBy(p => p.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Count());

            summary.MethodAmounts = payments.GroupBy(p => p.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting payment summary");
            return new PaymentSummary();
        }
    }

    /// <summary>
    /// Export payments to CSV
    /// </summary>
    public async Task<byte[]> ExportToCsvAsync(
        string? searchTerm = null,
        int? userId = null,
        int? orderId = null,
        PaymentMethod? paymentMethod = null,
        PaymentStatus? paymentStatus = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            var (payments, _) = await GetAllAsync(searchTerm, userId, orderId, paymentMethod, 
                paymentStatus, startDate, endDate, pageSize: int.MaxValue);

            var csv = new StringBuilder();
            csv.AppendLine("Payment ID,User,Email,Order ID,Amount,Currency,Payment Method,Status,Transaction ID,Payment Gateway,Payment Date,Processed Date,Failure Reason");

            foreach (var payment in payments)
            {
                var user = $"{payment.Order.AppUser.FirstName} {payment.Order.AppUser.LastName}";
                var processedDate = payment.ProcessedDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
                var failureReason = payment.FailureReason ?? "";

                csv.AppendLine($"{payment.Id}," +
                              $"\"{user}\"," +
                              $"\"{payment.Order.AppUser.Email}\"," +
                              $"{payment.OrderId}," +
                              $"{payment.Amount:F2}," +
                              $"USD," +
                              $"\"{payment.PaymentMethod}\"," +
                              $"\"{payment.PaymentStatus}\"," +
                              $"\"{payment.TransactionId ?? ""}\"," +
                              $"\"{payment.PaymentGateway ?? ""}\"," +
                              $"\"{payment.PaymentDate:yyyy-MM-dd HH:mm}\"," +
                              $"\"{processedDate}\"," +
                              $"\"{failureReason.Replace("\"", "\"\"")}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting payments to CSV");
            return Encoding.UTF8.GetBytes("Error exporting data");
        }
    }

    /// <summary>
    /// Process refund for a payment
    /// </summary>
    public async Task<bool> ProcessRefundAsync(int id, decimal refundAmount, string reason)
    {
        try
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return false;

            if (payment.PaymentStatus != PaymentStatus.Completed)
            {
                _logger.LogWarning("Cannot refund payment {Id} with status {Status}", id, payment.PaymentStatus);
                return false;
            }

            if (refundAmount > payment.Amount)
            {
                _logger.LogWarning("Refund amount {RefundAmount} exceeds payment amount {PaymentAmount} for payment {Id}", 
                    refundAmount, payment.Amount, id);
                return false;
            }

            payment.PaymentStatus = PaymentStatus.Refunded;
            payment.FailureReason = $"Refunded: {reason}";
            payment.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment {Id} refunded successfully. Amount: {RefundAmount}, Reason: {Reason}", 
                id, refundAmount, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Validate payment data
    /// </summary>
    public async Task<(bool IsValid, string ErrorMessage)> ValidatePaymentAsync(Payment payment)
    {
        try
        {
            // Check if order exists
            var orderExists = await _context.Orders.AnyAsync(o => o.Id == payment.OrderId);
            if (!orderExists)
            {
                return (false, "Order not found");
            }

            // Check if amount is positive
            if (payment.Amount <= 0)
            {
                return (false, "Payment amount must be greater than zero");
            }

            // Check if there's already a completed payment for this order
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == payment.OrderId && 
                                         p.PaymentStatus == PaymentStatus.Completed &&
                                         p.Id != payment.Id);

            if (existingPayment != null)
            {
                return (false, "Order already has a completed payment");
            }

            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment");
            return (false, "Validation error occurred");
        }
    }

    /// <summary>
    /// Get payments pending processing
    /// </summary>
    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
    {
        try
        {
            return await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.AppUser)
                .Where(p => p.PaymentStatus == PaymentStatus.Pending)
                .OrderBy(p => p.PaymentDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending payments");
            return Enumerable.Empty<Payment>();
        }
    }

    /// <summary>
    /// Get failed payments for retry
    /// </summary>
    public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync()
    {
        try
        {
            return await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.AppUser)
                .Where(p => p.PaymentStatus == PaymentStatus.Failed)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting failed payments");
            return Enumerable.Empty<Payment>();
        }
    }
}

