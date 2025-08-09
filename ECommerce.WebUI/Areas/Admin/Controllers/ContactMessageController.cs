using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for contact message management in Admin area
/// </summary>
[Area("Admin")]
public class ContactMessageController : Controller
{
    private readonly IContactMessageService _contactMessageService;
    private readonly ILogger<ContactMessageController> _logger;

    public ContactMessageController(IContactMessageService contactMessageService, ILogger<ContactMessageController> logger)
    {
        _contactMessageService = contactMessageService;
        _logger = logger;
    }

    /// <summary>
    /// Display list of contact messages with advanced filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, string? email, bool? isRead, bool? isReplied,
        DateTime? startDate, DateTime? endDate, string sortBy = "createdDate", string sortOrder = "desc", 
        int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "Contact Messages";

        try
        {
            var (messages, totalCount) = await _contactMessageService.GetContactMessagesAsync(
                searchTerm, email, isRead, isReplied, startDate, endDate, sortBy, sortOrder, page, pageSize);

            var statistics = await _contactMessageService.GetContactMessageStatisticsAsync();

            var viewModel = new ContactMessageListViewModel
            {
                Messages = messages.Select(m => new ContactMessageItemViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    PhoneNumber = m.PhoneNumber,
                    Subject = m.Subject,
                    Message = m.Message,
                    IsRead = m.IsRead,
                    IsReplied = m.IsReplied,
                    CreatedDate = m.CreatedDate,
                    RepliedDate = m.RepliedDate
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                Email = email,
                IsRead = isRead,
                IsReplied = isReplied,
                StartDate = startDate,
                EndDate = endDate,
                SortBy = sortBy,
                SortOrder = sortOrder,
                Statistics = new ContactMessageStatisticsViewModel
                {
                    TotalMessages = statistics.TotalMessages,
                    UnreadMessages = statistics.UnreadMessages,
                    ReadMessages = statistics.ReadMessages,
                    RepliedMessages = statistics.RepliedMessages,
                    UnrepliedMessages = statistics.UnrepliedMessages,
                    TodayMessages = statistics.TodayMessages,
                    ThisWeekMessages = statistics.ThisWeekMessages,
                    ThisMonthMessages = statistics.ThisMonthMessages,
                    LastMessageDate = statistics.LastMessageDate,
                    LatestMessageSender = statistics.LatestMessage?.Name,
                    LatestMessageSubject = statistics.LatestMessage?.Subject,
                    OldestUnreadSender = statistics.OldestUnreadMessage?.Name,
                    OldestUnreadDate = statistics.OldestUnreadMessage?.CreatedDate,
                    AverageMessagesPerDay = statistics.AverageMessagesPerDay
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contact messages index");
            TempData["ErrorMessage"] = $"Error loading contact messages: {ex.Message}";
            return View(new ContactMessageListViewModel());
        }
    }

    /// <summary>
    /// Display detailed contact message information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Contact Message Details";

        try
        {
            var message = await _contactMessageService.GetContactMessageByIdAsync(id);
            if (message == null)
            {
                TempData["ErrorMessage"] = "Contact message not found.";
                return RedirectToAction(nameof(Index));
            }

            // Mark as read when viewing details
            if (!message.IsRead)
            {
                await _contactMessageService.MarkAsReadAsync(id);
                message.IsRead = true;
            }

            var viewModel = new ContactMessageDetailViewModel
            {
                Id = message.Id,
                Name = message.Name,
                Email = message.Email,
                PhoneNumber = message.PhoneNumber,
                Subject = message.Subject,
                Message = message.Message,
                IsRead = message.IsRead,
                IsReplied = message.IsReplied,
                AdminReply = message.AdminReply,
                RepliedDate = message.RepliedDate,
                RepliedByUserId = message.RepliedByUserId,
                CreatedDate = message.CreatedDate,
                UpdatedDate = message.UpdatedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contact message details for ID: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading contact message details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display contact message deletion confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete Contact Message";

        try
        {
            var message = await _contactMessageService.GetContactMessageByIdAsync(id);
            if (message == null)
            {
                TempData["ErrorMessage"] = "Contact message not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ContactMessageDeleteViewModel
            {
                Id = message.Id,
                Name = message.Name,
                Email = message.Email,
                Subject = message.Subject,
                Message = message.Message,
                IsRead = message.IsRead,
                IsReplied = message.IsReplied,
                CreatedDate = message.CreatedDate
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading contact message for delete: {Id}", id);
            TempData["ErrorMessage"] = $"Error loading contact message: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process contact message deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, ContactMessageDeleteViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                bool success;
                
                if (model.HardDelete)
                {
                    success = await _contactMessageService.HardDeleteContactMessageAsync(id);
                    TempData["SuccessMessage"] = $"Contact message from '{model.Name}' has been permanently deleted.";
                }
                else
                {
                    success = await _contactMessageService.DeleteContactMessageAsync(id);
                    TempData["SuccessMessage"] = $"Contact message from '{model.Name}' has been moved to trash.";
                }

                if (success)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete contact message.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact message: {Id}", id);
            TempData["ErrorMessage"] = $"Error deleting contact message: {ex.Message}";
            return View("Delete", model);
        }
    }

    #region AJAX Actions

    /// <summary>
    /// AJAX endpoint for marking message as read
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            var success = await _contactMessageService.MarkAsReadAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "Message marked as read" });
            }
            
            return Json(new { success = false, message = "Failed to mark message as read" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for marking message as unread
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> MarkAsUnread(int id)
    {
        try
        {
            var success = await _contactMessageService.MarkAsUnreadAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "Message marked as unread" });
            }
            
            return Json(new { success = false, message = "Failed to mark message as unread" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as unread: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for toggling read status
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleReadStatus(int id)
    {
        try
        {
            var success = await _contactMessageService.ToggleReadStatusAsync(id);
            
            if (success)
            {
                return Json(new { success = true, message = "Message status updated" });
            }
            
            return Json(new { success = false, message = "Failed to update message status" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling message read status: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for bulk operations
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> BulkOperation([FromBody] BulkContactMessageOperationViewModel model)
    {
        try
        {
            if (!model.SelectedIds.Any())
            {
                return Json(new { success = false, message = "No messages selected" });
            }

            bool success = false;
            string message = "";

            switch (model.Operation.ToLower())
            {
                case "markread":
                    success = await _contactMessageService.BulkMarkAsReadAsync(model.SelectedIds);
                    message = success ? $"Marked {model.SelectedIds.Count} messages as read" : "Failed to mark messages as read";
                    break;
                    
                case "markunread":
                    success = await _contactMessageService.BulkMarkAsUnreadAsync(model.SelectedIds);
                    message = success ? $"Marked {model.SelectedIds.Count} messages as unread" : "Failed to mark messages as unread";
                    break;
                    
                case "delete":
                    success = await _contactMessageService.BulkDeleteAsync(model.SelectedIds);
                    message = success ? $"Moved {model.SelectedIds.Count} messages to trash" : "Failed to delete messages";
                    break;
                    
                case "harddelete":
                    success = await _contactMessageService.BulkHardDeleteAsync(model.SelectedIds);
                    message = success ? $"Permanently deleted {model.SelectedIds.Count} messages" : "Failed to permanently delete messages";
                    break;
                    
                default:
                    return Json(new { success = false, message = "Invalid operation" });
            }

            return Json(new { success, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing bulk operation: {Operation}", model.Operation);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Export contact messages to CSV
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Export(string? searchTerm, string? email, bool? isRead, bool? isReplied,
        DateTime? startDate, DateTime? endDate)
    {
        try
        {
            var (messages, _) = await _contactMessageService.GetContactMessagesAsync(
                searchTerm, email, isRead, isReplied, startDate, endDate, pageSize: int.MaxValue);
            
            var csv = "Name,Email,Phone,Subject,Message,Status,Reply Status,Created Date,Replied Date\n";
            foreach (var message in messages)
            {
                var phone = message.PhoneNumber ?? "";
                var status = message.IsRead ? "Read" : "Unread";
                var replyStatus = message.IsReplied ? "Replied" : "Not Replied";
                var repliedDate = message.RepliedDate?.ToString("yyyy-MM-dd HH:mm") ?? "";
                
                csv += $"\"{message.Name}\",\"{message.Email}\",\"{phone}\",\"{message.Subject}\",\"{message.Message.Replace("\"", "\"\"")}\",\"{status}\",\"{replyStatus}\",\"{message.CreatedDate:yyyy-MM-dd HH:mm}\",\"{repliedDate}\"\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"ContactMessages_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting contact messages");
            TempData["ErrorMessage"] = "Error exporting contact messages.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Reply to contact message
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, string reply)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reply))
            {
                return Json(new { success = false, message = "Reply message is required" });
            }

            // For now, using a default user ID. In a real application, this would come from the authenticated user
            var userId = 1; // This should be replaced with actual user ID from authentication
            
            var success = await _contactMessageService.ReplyToMessageAsync(id, reply, userId);
            
            if (success)
            {
                return Json(new { success = true, message = "Reply sent successfully" });
            }
            
            return Json(new { success = false, message = "Failed to send reply" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to contact message: {Id}", id);
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get contact message statistics for dashboard
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var statistics = await _contactMessageService.GetContactMessageStatisticsAsync();
            
            return Json(new
            {
                success = true,
                data = new
                {
                    totalMessages = statistics.TotalMessages,
                    unreadMessages = statistics.UnreadMessages,
                    readMessages = statistics.ReadMessages,
                    repliedMessages = statistics.RepliedMessages,
                    todayMessages = statistics.TodayMessages,
                    thisWeekMessages = statistics.ThisWeekMessages,
                    averagePerDay = statistics.AverageMessagesPerDay
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact message statistics");
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion
}

