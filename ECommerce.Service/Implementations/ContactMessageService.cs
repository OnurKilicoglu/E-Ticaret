using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Implementation of contact message service
/// </summary>
public class ContactMessageService : IContactMessageService
{
    private readonly ECommerceDbContext _context;
    private readonly ILogger<ContactMessageService> _logger;

    public ContactMessageService(ECommerceDbContext context, ILogger<ContactMessageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated and filtered contact messages
    /// </summary>
    public async Task<(IEnumerable<ContactMessage> Messages, int TotalCount)> GetContactMessagesAsync(
        string? searchTerm = null,
        string? email = null,
        bool? isRead = null,
        bool? isReplied = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "createdDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.ContactMessages.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(cm => 
                    cm.Name.ToLower().Contains(searchTerm) ||
                    cm.Email.ToLower().Contains(searchTerm) ||
                    cm.Subject.ToLower().Contains(searchTerm) ||
                    cm.Message.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(cm => cm.Email.ToLower().Contains(email.ToLower()));
            }

            if (isRead.HasValue)
            {
                query = query.Where(cm => cm.IsRead == isRead.Value);
            }

            if (isReplied.HasValue)
            {
                query = query.Where(cm => cm.IsReplied == isReplied.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(cm => cm.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(cm => cm.CreatedDate <= endDate.Value.AddDays(1));
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(cm => cm.Name) 
                    : query.OrderByDescending(cm => cm.Name),
                "email" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(cm => cm.Email) 
                    : query.OrderByDescending(cm => cm.Email),
                "subject" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(cm => cm.Subject) 
                    : query.OrderByDescending(cm => cm.Subject),
                "isread" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(cm => cm.IsRead) 
                    : query.OrderByDescending(cm => cm.IsRead),
                "isreplied" => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(cm => cm.IsReplied) 
                    : query.OrderByDescending(cm => cm.IsReplied),
                _ => sortOrder.ToLower() == "asc" 
                    ? query.OrderBy(cm => cm.CreatedDate) 
                    : query.OrderByDescending(cm => cm.CreatedDate)
            };

            // Apply pagination
            var messages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (messages, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact messages");
            return (Enumerable.Empty<ContactMessage>(), 0);
        }
    }

    /// <summary>
    /// Get contact message by ID
    /// </summary>
    public async Task<ContactMessage?> GetContactMessageByIdAsync(int id)
    {
        try
        {
            return await _context.ContactMessages
                .FirstOrDefaultAsync(cm => cm.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact message by ID: {Id}", id);
            return null;
        }
    }

    /// <summary>
    /// Create a new contact message
    /// </summary>
    public async Task<ContactMessage?> CreateContactMessageAsync(ContactMessage contactMessage)
    {
        try
        {
            contactMessage.CreatedDate = DateTime.UtcNow;

            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contact message created with ID: {Id}", contactMessage.Id);
            return contactMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact message");
            return null;
        }
    }

    /// <summary>
    /// Update contact message
    /// </summary>
    public async Task<bool> UpdateContactMessageAsync(ContactMessage contactMessage)
    {
        try
        {
            contactMessage.UpdatedDate = DateTime.UtcNow;
            _context.ContactMessages.Update(contactMessage);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contact message updated with ID: {Id}", contactMessage.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contact message: {Id}", contactMessage.Id);
            return false;
        }
    }

    /// <summary>
    /// Mark message as read
    /// </summary>
    public async Task<bool> MarkAsReadAsync(int id)
    {
        try
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null) return false;

            message.IsRead = true;
            message.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Contact message marked as read: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as read: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Mark message as unread
    /// </summary>
    public async Task<bool> MarkAsUnreadAsync(int id)
    {
        try
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null) return false;

            message.IsRead = false;
            message.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Contact message marked as unread: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message as unread: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Toggle read status
    /// </summary>
    public async Task<bool> ToggleReadStatusAsync(int id)
    {
        try
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null) return false;

            message.IsRead = !message.IsRead;
            message.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Contact message read status toggled: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling message read status: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Delete contact message (hard delete since ContactMessage doesn't support soft delete)
    /// </summary>
    public async Task<bool> DeleteContactMessageAsync(int id)
    {
        try
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null) return false;

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contact message deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contact message: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Hard delete contact message (permanent)
    /// </summary>
    public async Task<bool> HardDeleteContactMessageAsync(int id)
    {
        try
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null) return false;

            _context.ContactMessages.Remove(message);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Contact message hard deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hard deleting contact message: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// Bulk mark messages as read
    /// </summary>
    public async Task<bool> BulkMarkAsReadAsync(IEnumerable<int> ids)
    {
        try
        {
            var messages = await _context.ContactMessages
                .Where(cm => ids.Contains(cm.Id))
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Bulk marked {Count} messages as read", messages.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk marking messages as read");
            return false;
        }
    }

    /// <summary>
    /// Bulk mark messages as unread
    /// </summary>
    public async Task<bool> BulkMarkAsUnreadAsync(IEnumerable<int> ids)
    {
        try
        {
            var messages = await _context.ContactMessages
                .Where(cm => ids.Contains(cm.Id))
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = false;
                message.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Bulk marked {Count} messages as unread", messages.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk marking messages as unread");
            return false;
        }
    }

    /// <summary>
    /// Bulk delete messages (hard delete since ContactMessage doesn't support soft delete)
    /// </summary>
    public async Task<bool> BulkDeleteAsync(IEnumerable<int> ids)
    {
        try
        {
            var messages = await _context.ContactMessages
                .Where(cm => ids.Contains(cm.Id))
                .ToListAsync();

            _context.ContactMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk deleted {Count} messages", messages.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting messages");
            return false;
        }
    }

    /// <summary>
    /// Bulk hard delete messages
    /// </summary>
    public async Task<bool> BulkHardDeleteAsync(IEnumerable<int> ids)
    {
        try
        {
            var messages = await _context.ContactMessages
                .Where(cm => ids.Contains(cm.Id))
                .ToListAsync();

            _context.ContactMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk hard deleted {Count} messages", messages.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk hard deleting messages");
            return false;
        }
    }

    /// <summary>
    /// Get contact message statistics
    /// </summary>
    public async Task<ContactMessageStatistics> GetContactMessageStatisticsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);

            var activeMessages = _context.ContactMessages.AsQueryable();

            var statistics = new ContactMessageStatistics
            {
                TotalMessages = await activeMessages.CountAsync(),
                UnreadMessages = await activeMessages.Where(cm => !cm.IsRead).CountAsync(),
                ReadMessages = await activeMessages.Where(cm => cm.IsRead).CountAsync(),
                RepliedMessages = await activeMessages.Where(cm => cm.IsReplied).CountAsync(),
                UnrepliedMessages = await activeMessages.Where(cm => !cm.IsReplied).CountAsync(),
                TodayMessages = await activeMessages.Where(cm => cm.CreatedDate >= todayStart).CountAsync(),
                ThisWeekMessages = await activeMessages.Where(cm => cm.CreatedDate >= weekStart).CountAsync(),
                ThisMonthMessages = await activeMessages.Where(cm => cm.CreatedDate >= monthStart).CountAsync(),
                LastMessageDate = await activeMessages.MaxAsync(cm => (DateTime?)cm.CreatedDate),
                LatestMessage = await activeMessages.OrderByDescending(cm => cm.CreatedDate).FirstOrDefaultAsync(),
                OldestUnreadMessage = await activeMessages.Where(cm => !cm.IsRead).OrderBy(cm => cm.CreatedDate).FirstOrDefaultAsync()
            };

            // Calculate average messages per day
            if (statistics.TotalMessages > 0 && statistics.LastMessageDate.HasValue)
            {
                var firstMessage = await activeMessages.MinAsync(cm => (DateTime?)cm.CreatedDate);
                if (firstMessage.HasValue)
                {
                    var daysDiff = (statistics.LastMessageDate.Value - firstMessage.Value).TotalDays;
                    statistics.AverageMessagesPerDay = daysDiff > 0 ? statistics.TotalMessages / daysDiff : statistics.TotalMessages;
                }
            }

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact message statistics");
            return new ContactMessageStatistics();
        }
    }

    /// <summary>
    /// Reply to contact message
    /// </summary>
    public async Task<bool> ReplyToMessageAsync(int id, string reply, int repliedByUserId)
    {
        try
        {
            var message = await _context.ContactMessages
                .FirstOrDefaultAsync(cm => cm.Id == id);

            if (message == null) return false;

            message.AdminReply = reply;
            message.IsReplied = true;
            message.RepliedDate = DateTime.UtcNow;
            message.RepliedByUserId = repliedByUserId;
            message.IsRead = true; // Mark as read when replying
            message.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Reply added to contact message: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to contact message: {Id}", id);
            return false;
        }
    }
}
