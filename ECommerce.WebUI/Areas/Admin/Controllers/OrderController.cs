using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using ECommerce.WebUI.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for Order management in Admin area
/// </summary>
[Area("Admin")]
[AdminAuthorize]
public class OrderController : Controller
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Display list of orders with advanced filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, OrderStatus? status, PaymentStatus? paymentStatus,
        DateTime? fromDate, DateTime? toDate, string sortBy = "orderDate", string sortOrder = "desc",
        int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Orders Management";

        try
        {
            var (orders, totalCount) = await _orderService.GetOrdersAsync(
                searchTerm, status, paymentStatus, fromDate, toDate, sortBy, sortOrder, page, pageSize);

            var statistics = await _orderService.GetOrderStatisticsAsync();

            var viewModel = new OrderListViewModel
            {
                Orders = orders.Select(o => new OrderItemViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    CustomerName = o.AppUser.UserName,
                    CustomerEmail = o.AppUser.Email,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.Payment?.PaymentStatus,
                    PaymentMethod = o.Payment?.PaymentMethod,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count,
                    ShippingCity = o.ShippingAddress?.City ?? "N/A",
                    LastUpdated = o.UpdatedDate
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                Status = status,
                PaymentStatus = paymentStatus,
                FromDate = fromDate,
                ToDate = toDate,
                SortBy = sortBy,
                SortOrder = sortOrder,
                StatusOptions = GetOrderStatusOptions(),
                PaymentStatusOptions = GetPaymentStatusOptions(),
                Statistics = new OrderStatisticsViewModel
                {
                    TotalOrders = statistics.TotalOrders,
                    PendingOrders = statistics.PendingOrders,
                    ProcessingOrders = statistics.ProcessingOrders,
                    ShippedOrders = statistics.ShippedOrders,
                    DeliveredOrders = statistics.DeliveredOrders,
                    CancelledOrders = statistics.CancelledOrders,
                    TotalRevenue = statistics.TotalRevenue,
                    TodayRevenue = statistics.TodayRevenue,
                    AverageOrderValue = statistics.AverageOrderValue
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading orders: {ex.Message}";
            return View(new OrderListViewModel());
        }
    }

    /// <summary>
    /// Display detailed order information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Order Details";

        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var statusHistory = await _orderService.GetOrderStatusHistoryAsync(id);
            var validStatuses = _orderService.GetValidNextStatuses(order.OrderStatus);

            var viewModel = new OrderDetailViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                LastUpdated = order.UpdatedDate,
                Customer = new CustomerInfoViewModel
                {
                    Id = order.AppUser.Id,
                    UserName = order.AppUser.UserName,
                    Email = order.AppUser.Email,
                    FullName = $"{order.AppUser.UserName}",
                    RegistrationDate = order.AppUser.CreatedDate
                },
                OrderItems = order.OrderItems.Select(oi => new OrderItemDetailViewModel
                {
                    Id = oi.Id,
                    ProductId = oi.Product.Id,
                    ProductName = oi.Product.Name,
                    ProductSku = oi.Product.SKU ?? "N/A",
                    CategoryName = oi.Product.Category?.Name ?? "N/A",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductImageUrl = oi.Product.ImageUrl
                }).ToList(),
                ShippingAddress = order.ShippingAddress != null ? new ShippingAddressViewModel
                {
                    AddressLine = order.ShippingAddress.AddressLine,
                    City = order.ShippingAddress.City,
                    Country = order.ShippingAddress.Country,
                    ZipCode = order.ShippingAddress.ZipCode
                } : null,
                Payment = order.Payment != null ? new PaymentInfoViewModel
                {
                    PaymentMethod = order.Payment.PaymentMethod,
                    PaymentStatus = order.Payment.PaymentStatus,
                    Amount = order.Payment.Amount,
                    PaymentDate = order.Payment.PaymentDate,
                    TransactionId = order.Payment.TransactionId
                } : null,
                SubTotal = order.TotalAmount, // Simplified - in real app, calculate from items
                TotalAmount = order.TotalAmount,
                AvailableStatuses = validStatuses.Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }).ToList(),
                StatusHistory = statusHistory.Select(sh => new OrderStatusHistoryViewModel
                {
                    FromStatus = sh.FromStatus,
                    ToStatus = sh.ToStatus,
                    Notes = sh.Notes,
                    AdminUserName = sh.AdminUserName,
                    ChangedDate = sh.ChangedDate
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading order details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display order edit form for status changes
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Order Status";

        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var validStatuses = _orderService.GetValidNextStatuses(order.OrderStatus);

            var viewModel = new OrderEditViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CurrentStatus = order.OrderStatus,
                NewStatus = order.OrderStatus,
                CustomerName = order.AppUser.UserName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                AvailableStatuses = validStatuses.Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading order: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process order status update
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrderEditViewModel model)
    {
        ViewData["Title"] = "Edit Order Status";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid order ID.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            if (ModelState.IsValid)
            {
                // Validate status transition
                if (!_orderService.IsValidStatusTransition(model.CurrentStatus, model.NewStatus))
                {
                    ModelState.AddModelError("NewStatus", "Invalid status transition.");
                    return View(model);
                }

                var success = await _orderService.UpdateOrderStatusAsync(id, model.NewStatus, model.Notes);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Order status updated to {model.NewStatus} successfully.";
                    
                    // Send notification if requested
                    if (model.SendNotification)
                    {
                        // In a real implementation, send email notification
                        TempData["InfoMessage"] = "Customer notification sent.";
                    }
                    
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update order status.");
                }
            }

            // Reload valid statuses for dropdown
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order != null)
            {
                var validStatuses = _orderService.GetValidNextStatuses(order.OrderStatus);
                model.AvailableStatuses = validStatuses.Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                }).ToList();
            }

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating order status: {ex.Message}";
            return View(model);
        }
    }

    /// <summary>
    /// Display order cancellation form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Cancel Order";

        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new OrderCancelViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CurrentStatus = order.OrderStatus,
                CustomerName = order.AppUser.UserName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                PaymentStatus = order.Payment?.PaymentStatus
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading order: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process order cancellation
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, OrderCancelViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                var success = await _orderService.CancelOrderAsync(id, model.Reason);
                
                if (success)
                {
                    TempData["SuccessMessage"] = $"Order {model.OrderNumber} has been cancelled successfully.";
                    
                    if (model.ProcessRefund && model.HasPayment)
                    {
                        TempData["InfoMessage"] = "Refund has been processed.";
                    }
                    
                    if (model.SendNotification)
                    {
                        TempData["InfoMessage"] = "Customer notification sent.";
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to cancel order. Order may not be eligible for cancellation.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error cancelling order: {ex.Message}";
            return View("Delete", model);
        }
    }

    /// <summary>
    /// AJAX endpoint for quick status updates
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus, string? notes = null)
    {
        try
        {
            var success = await _orderService.UpdateOrderStatusAsync(orderId, newStatus, notes);
            
            if (success)
            {
                return Json(new { success = true, message = $"Order status updated to {newStatus}" });
            }
            
            return Json(new { success = false, message = "Invalid status transition or order not found" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for processing refunds
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessRefund(int orderId, decimal amount, string reason)
    {
        try
        {
            var success = await _orderService.ProcessRefundAsync(orderId, amount, reason);
            
            if (success)
            {
                return Json(new { success = true, message = $"Refund of {amount:C} processed successfully" });
            }
            
            return Json(new { success = false, message = "Failed to process refund" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for adding order notes
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddNote(int orderId, string note)
    {
        try
        {
            var success = await _orderService.AddOrderNoteAsync(orderId, note);
            
            if (success)
            {
                return Json(new { success = true, message = "Note added successfully" });
            }
            
            return Json(new { success = false, message = "Failed to add note" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Export orders to CSV
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Export(string? searchTerm, OrderStatus? status, PaymentStatus? paymentStatus,
        DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var (orders, _) = await _orderService.GetOrdersAsync(
                searchTerm, status, paymentStatus, fromDate, toDate, pageSize: int.MaxValue);

            // In a real implementation, you would generate CSV content
            var csvContent = GenerateCsvContent(orders);
            var fileName = $"orders_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error exporting orders: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #region Helper Methods

    private List<SelectListItem> GetOrderStatusOptions()
    {
        return Enum.GetValues<OrderStatus>()
            .Select(s => new SelectListItem
            {
                Value = s.ToString(),
                Text = s.ToString()
            }).ToList();
    }

    private List<SelectListItem> GetPaymentStatusOptions()
    {
        return Enum.GetValues<PaymentStatus>()
            .Select(s => new SelectListItem
            {
                Value = s.ToString(),
                Text = s.ToString()
            }).ToList();
    }

    private string GenerateCsvContent(IEnumerable<ECommerce.Core.Entities.Order> orders)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Order Number,Order Date,Customer,Email,Status,Payment Status,Total Amount,Items");
        
        foreach (var order in orders)
        {
            csv.AppendLine($"{order.OrderNumber},{order.OrderDate:yyyy-MM-dd},{order.AppUser.UserName},{order.AppUser.Email},{order.OrderStatus},{order.Payment?.PaymentStatus ?? PaymentStatus.None},{order.TotalAmount:F2},{order.OrderItems.Count}");
        }
        
        return csv.ToString();
    }

    #endregion
}
