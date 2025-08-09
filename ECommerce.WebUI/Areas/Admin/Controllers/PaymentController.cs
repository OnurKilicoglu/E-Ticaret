using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for payment management in Admin area
/// </summary>
[Area("Admin")]
public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        IUserService userService,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Display list of payments with advanced filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, int? userId, int? orderId,
        PaymentMethod? paymentMethod, PaymentStatus? paymentStatus, DateTime? startDate, DateTime? endDate,
        string sortBy = "paymentDate", string sortOrder = "desc", int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Payment Management";

        try
        {
            var (payments, totalCount) = await _paymentService.GetAllAsync(
                searchTerm, userId, orderId, paymentMethod, paymentStatus, 
                startDate, endDate, sortBy, sortOrder, page, pageSize);

            var statistics = await _paymentService.GetPaymentStatisticsAsync();
            var summary = await _paymentService.GetPaymentSummaryAsync(
                userId, orderId, paymentMethod, paymentStatus, startDate, endDate);

            // Get filter options
            var (users, _) = await _userService.GetUsersAsync(pageSize: 1000);
            var (orders, _) = await _orderService.GetOrdersAsync(pageSize: 1000);

            var viewModel = new PaymentListViewModel
            {
                Payments = payments.Select(p => new PaymentItemViewModel
                {
                    Id = p.Id,
                    OrderId = p.OrderId,
                    OrderNumber = p.Order.OrderNumber ?? $"ORD-{p.OrderId:D6}",
                    UserName = $"{p.Order.AppUser.FirstName} {p.Order.AppUser.LastName}",
                    UserEmail = p.Order.AppUser.Email,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentStatus = p.PaymentStatus,
                    TransactionId = p.TransactionId,
                    PaymentGateway = p.PaymentGateway,
                    PaymentDate = p.PaymentDate,
                    ProcessedDate = p.ProcessedDate,
                    FailureReason = p.FailureReason
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                UserId = userId,
                OrderId = orderId,
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentStatus,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = sortBy,
                SortOrder = sortOrder,
                UserOptions = users.Select(u => new SelectListItem 
                { 
                    Value = u.Id.ToString(), 
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})" 
                }).ToList(),
                OrderOptions = orders.Take(100).Select(o => new SelectListItem 
                { 
                    Value = o.Id.ToString(), 
                    Text = $"Order #{o.Id} - {o.OrderNumber ?? $"ORD-{o.Id:D6}"}" 
                }).ToList(),
                PaymentMethodOptions = Enum.GetValues<PaymentMethod>().Select(pm => new SelectListItem
                {
                    Value = ((int)pm).ToString(),
                    Text = pm.ToString()
                }).ToList(),
                PaymentStatusOptions = Enum.GetValues<PaymentStatus>().Select(ps => new SelectListItem
                {
                    Value = ((int)ps).ToString(),
                    Text = ps.ToString()
                }).ToList(),
                Statistics = new PaymentStatisticsViewModel
                {
                    TotalPayments = statistics.TotalPayments,
                    TotalAmount = statistics.TotalAmount,
                    PendingPayments = statistics.PendingPayments,
                    PendingAmount = statistics.PendingAmount,
                    CompletedPayments = statistics.CompletedPayments,
                    CompletedAmount = statistics.CompletedAmount,
                    FailedPayments = statistics.FailedPayments,
                    FailedAmount = statistics.FailedAmount,
                    RefundedPayments = statistics.RefundedPayments,
                    RefundedAmount = statistics.RefundedAmount,
                    TodayPayments = statistics.TodayPayments,
                    TodayAmount = statistics.TodayAmount,
                    ThisWeekPayments = statistics.ThisWeekPayments,
                    ThisWeekAmount = statistics.ThisWeekAmount,
                    ThisMonthPayments = statistics.ThisMonthPayments,
                    ThisMonthAmount = statistics.ThisMonthAmount,
                    LastPaymentDate = statistics.LastPaymentDate,
                    LatestPaymentUser = statistics.LatestPayment != null ? 
                        $"{statistics.LatestPayment.Order.AppUser.FirstName} {statistics.LatestPayment.Order.AppUser.LastName}" : null,
                    LargestPaymentAmount = statistics.LargestPayment?.Amount,
                    AveragePaymentAmount = statistics.AveragePaymentAmount,
                    MostUsedPaymentMethod = statistics.MostUsedPaymentMethod.ToString()
                },
                Summary = new PaymentSummaryViewModel
                {
                    Count = summary.Count,
                    TotalAmount = summary.TotalAmount,
                    AverageAmount = summary.AverageAmount,
                    MaxAmount = summary.MaxAmount,
                    MinAmount = summary.MinAmount,
                    StatusCounts = summary.StatusCounts,
                    MethodCounts = summary.MethodCounts,
                    StatusAmounts = summary.StatusAmounts,
                    MethodAmounts = summary.MethodAmounts
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payments index");
            TempData["ErrorMessage"] = $"Error loading payments: {ex.Message}";
            return View(new PaymentListViewModel());
        }
    }

    /// <summary>
    /// Display detailed payment information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Payment Details";

        try
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PaymentDetailViewModel
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                OrderNumber = payment.Order.OrderNumber ?? $"ORD-{payment.OrderId:D6}",
                UserId = payment.Order.AppUserId,
                UserName = $"{payment.Order.AppUser.FirstName} {payment.Order.AppUser.LastName}",
                UserEmail = payment.Order.AppUser.Email,
                UserPhone = payment.Order.AppUser.PhoneNumber,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                PaymentGateway = payment.PaymentGateway,
                PaymentDetails = payment.PaymentDetails,
                FailureReason = payment.FailureReason,
                PaymentDate = payment.PaymentDate,
                ProcessedDate = payment.ProcessedDate,
                CreatedDate = payment.CreatedDate,
                UpdatedDate = payment.UpdatedDate,
                OrderTotal = payment.Order.TotalAmount,
                OrderStatus = payment.Order.OrderStatus,
                OrderDate = payment.Order.OrderDate,
                ShippingAddress = payment.Order.ShippingAddress != null ? 
                    $"{payment.Order.ShippingAddress.AddressLine}, {payment.Order.ShippingAddress.City}" : null,
                OrderItems = payment.Order.OrderItems.Select(oi => new PaymentOrderItemViewModel
                {
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.UnitPrice * oi.Quantity - (oi.DiscountAmount ?? 0)
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment details for ID: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading payment details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display payment creation form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Create New Payment";

        try
        {
            var (orders, _) = await _orderService.GetOrdersAsync(pageSize: 1000);
            var viewModel = new PaymentCreateViewModel();
            
            await LoadCreateEditOptions(viewModel, orders);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment create form");
            TempData["ErrorMessage"] = $"Error loading create form: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process payment creation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentCreateViewModel model)
    {
        ViewData["Title"] = "Create New Payment";

        if (ModelState.IsValid)
        {
            try
            {
                var payment = new Payment
                {
                    OrderId = model.OrderId,
                    PaymentMethod = model.PaymentMethod,
                    Amount = model.Amount,
                    PaymentStatus = model.PaymentStatus,
                    TransactionId = model.TransactionId,
                    PaymentGateway = model.PaymentGateway,
                    PaymentDetails = model.PaymentDetails,
                    PaymentDate = model.PaymentDate
                };

                var validationResult = await _paymentService.ValidatePaymentAsync(payment);
                if (!validationResult.IsValid)
                {
                    ModelState.AddModelError("", validationResult.ErrorMessage);
                }
                else
                {
                    var createdPayment = await _paymentService.CreateAsync(payment);
                    if (createdPayment != null)
                    {
                        TempData["SuccessMessage"] = $"Payment for Order #{model.OrderId} has been created successfully.";
                        return RedirectToAction(nameof(Details), new { id = createdPayment.Id });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create payment.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                ModelState.AddModelError("", $"Error creating payment: {ex.Message}");
            }
        }

        // Reload dropdown options if model is invalid
        var (orders, _) = await _orderService.GetOrdersAsync(pageSize: 1000);
        await LoadCreateEditOptions(model, orders);
        return View(model);
    }

    /// <summary>
    /// Display payment edit form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Payment";

        try
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }

            var (orders, _) = await _orderService.GetOrdersAsync(pageSize: 1000);
            var viewModel = new PaymentEditViewModel
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                PaymentGateway = payment.PaymentGateway,
                PaymentDetails = payment.PaymentDetails,
                FailureReason = payment.FailureReason,
                PaymentDate = payment.PaymentDate,
                ProcessedDate = payment.ProcessedDate,
                CreatedDate = payment.CreatedDate,
                UpdatedDate = payment.UpdatedDate,
                UserName = $"{payment.Order.AppUser.FirstName} {payment.Order.AppUser.LastName}",
                UserEmail = payment.Order.AppUser.Email,
                OrderNumber = payment.Order.OrderNumber ?? $"ORD-{payment.OrderId:D6}",
                OrderTotal = payment.Order.TotalAmount
            };

            await LoadEditOptions(viewModel, orders);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment for edit: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading payment: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process payment update
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PaymentEditViewModel model)
    {
        ViewData["Title"] = "Edit Payment";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid payment ID.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                var payment = await _paymentService.GetByIdAsync(id);
                if (payment == null)
                {
                    TempData["ErrorMessage"] = "Payment not found.";
                    return RedirectToAction(nameof(Index));
                }

                payment.OrderId = model.OrderId;
                payment.PaymentMethod = model.PaymentMethod;
                payment.Amount = model.Amount;
                payment.PaymentStatus = model.PaymentStatus;
                payment.TransactionId = model.TransactionId;
                payment.PaymentGateway = model.PaymentGateway;
                payment.PaymentDetails = model.PaymentDetails;
                payment.FailureReason = model.FailureReason;
                payment.PaymentDate = model.PaymentDate;
                payment.ProcessedDate = model.ProcessedDate;

                var success = await _paymentService.UpdateAsync(payment);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Payment #{model.Id} has been updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update payment.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment: {Id}", id);
                ModelState.AddModelError("", $"Error updating payment: {ex.Message}");
            }
        }

        // Reload dropdown options if model is invalid
        var (orders, _) = await _orderService.GetOrdersAsync(pageSize: 1000);
        await LoadEditOptions(model, orders);
        return View(model);
    }

    /// <summary>
    /// Display payment deletion confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete Payment";

        try
        {
            var payment = await _paymentService.GetByIdAsync(id);
            if (payment == null)
            {
                TempData["ErrorMessage"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PaymentDeleteViewModel
            {
                Id = payment.Id,
                OrderId = payment.OrderId,
                OrderNumber = payment.Order.OrderNumber ?? $"ORD-{payment.OrderId:D6}",
                UserName = $"{payment.Order.AppUser.FirstName} {payment.Order.AppUser.LastName}",
                UserEmail = payment.Order.AppUser.Email,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod,
                PaymentStatus = payment.PaymentStatus,
                TransactionId = payment.TransactionId,
                PaymentDate = payment.PaymentDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading payment for delete: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading payment: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process payment deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, PaymentDeleteViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                bool success;
                
                if (model.HardDelete)
                {
                    success = await _paymentService.HardDeleteAsync(id);
                    TempData["SuccessMessage"] = $"Payment #{model.Id} has been permanently deleted.";
                }
                else
                {
                    success = await _paymentService.DeleteAsync(id);
                    TempData["SuccessMessage"] = $"Payment #{model.Id} has been deleted.";
                }

                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete payment.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting payment: {Id}", id);
            TempData["ErrorMessage"] = $"Error deleting payment: {ex.Message}";
            return View("Delete", model);
        }
    }

    #region AJAX Actions

    /// <summary>
    /// AJAX endpoint for changing payment status
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int id, PaymentStatus newStatus, string? reason = null)
    {
        try
        {
            var success = await _paymentService.ChangeStatusAsync(id, newStatus, reason);
            
            if (success)
            {
                return Json(new { success = true, message = "Payment status updated successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update payment status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing payment status: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for processing refund
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessRefund(int id, decimal refundAmount, string reason)
    {
        try
        {
            var success = await _paymentService.ProcessRefundAsync(id, refundAmount, reason);
            
            if (success)
            {
                return Json(new { success = true, message = "Refund processed successfully" });
            }
            
            return Json(new { success = false, message = "Failed to process refund" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Export payments to CSV
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Export(string? searchTerm, int? userId, int? orderId,
        PaymentMethod? paymentMethod, PaymentStatus? paymentStatus, DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var csvData = await _paymentService.ExportToCsvAsync(
                searchTerm, userId, orderId, paymentMethod, paymentStatus, startDate, endDate);
            
            return File(csvData, "text/csv", $"Payments_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting payments");
            TempData["ErrorMessage"] = "Error exporting payments.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Get order details for payment creation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrderDetails(int orderId)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found" });
            }

            return Json(new 
            { 
                success = true, 
                data = new
                {
                    orderId = order.Id,
                    orderNumber = order.OrderNumber ?? $"ORD-{order.Id:D6}",
                    totalAmount = order.TotalAmount,
                    userName = $"{order.AppUser.FirstName} {order.AppUser.LastName}",
                    userEmail = order.AppUser.Email,
                    orderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                    hasExistingPayment = order.Payment != null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order details: {OrderId}", orderId);
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Private Helper Methods

    private Task LoadCreateEditOptions(PaymentCreateViewModel model, IEnumerable<Order> orders)
    {
        model.OrderOptions = orders.Select(o => new SelectListItem 
        { 
            Value = o.Id.ToString(), 
            Text = $"Order #{o.Id} - {o.OrderNumber ?? $"ORD-{o.Id:D6}"} - ${o.TotalAmount:F2}" 
        }).ToList();

        model.PaymentMethodOptions = Enum.GetValues<PaymentMethod>().Select(pm => new SelectListItem
        {
            Value = ((int)pm).ToString(),
            Text = pm.ToString()
        }).ToList();

        model.PaymentStatusOptions = Enum.GetValues<PaymentStatus>().Select(ps => new SelectListItem
        {
            Value = ((int)ps).ToString(),
            Text = ps.ToString()
        }).ToList();
        
        return Task.CompletedTask;
    }

    private Task LoadEditOptions(PaymentEditViewModel model, IEnumerable<Order> orders)
    {
        model.OrderOptions = orders.Select(o => new SelectListItem 
        { 
            Value = o.Id.ToString(), 
            Text = $"Order #{o.Id} - {o.OrderNumber ?? $"ORD-{o.Id:D6}"} - ${o.TotalAmount:F2}",
            Selected = o.Id == model.OrderId
        }).ToList();

        model.PaymentMethodOptions = Enum.GetValues<PaymentMethod>().Select(pm => new SelectListItem
        {
            Value = ((int)pm).ToString(),
            Text = pm.ToString(),
            Selected = pm == model.PaymentMethod
        }).ToList();

        model.PaymentStatusOptions = Enum.GetValues<PaymentStatus>().Select(ps => new SelectListItem
        {
            Value = ((int)ps).ToString(),
            Text = ps.ToString(),
            Selected = ps == model.PaymentStatus
        }).ToList();
        
        return Task.CompletedTask;
    }

    #endregion
}
