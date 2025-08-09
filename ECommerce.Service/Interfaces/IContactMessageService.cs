using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for contact message operations
/// </summary>
public interface IContactMessageService
{
    /// <summary>
    /// Get paginated and filtered contact messages
    /// </summary>
    Task<(IEnumerable<ContactMessage> Messages, int TotalCount)> GetContactMessagesAsync(
        string? searchTerm = null,
        string? email = null,
        bool? isRead = null,
        bool? isReplied = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "createdDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get contact message by ID
    /// </summary>
    Task<ContactMessage?> GetContactMessageByIdAsync(int id);

    /// <summary>
    /// Create a new contact message
    /// </summary>
    Task<ContactMessage?> CreateContactMessageAsync(ContactMessage contactMessage);

    /// <summary>
    /// Update contact message
    /// </summary>
    Task<bool> UpdateContactMessageAsync(ContactMessage contactMessage);

    /// <summary>
    /// Mark message as read
    /// </summary>
    Task<bool> MarkAsReadAsync(int id);

    /// <summary>
    /// Mark message as unread
    /// </summary>
    Task<bool> MarkAsUnreadAsync(int id);

    /// <summary>
    /// Toggle read status
    /// </summary>
    Task<bool> ToggleReadStatusAsync(int id);

    /// <summary>
    /// Soft delete contact message
    /// </summary>
    Task<bool> DeleteContactMessageAsync(int id);

    /// <summary>
    /// Hard delete contact message (permanent)
    /// </summary>
    Task<bool> HardDeleteContactMessageAsync(int id);

    /// <summary>
    /// Bulk mark messages as read
    /// </summary>
    Task<bool> BulkMarkAsReadAsync(IEnumerable<int> ids);

    /// <summary>
    /// Bulk mark messages as unread
    /// </summary>
    Task<bool> BulkMarkAsUnreadAsync(IEnumerable<int> ids);

    /// <summary>
    /// Bulk delete messages (soft)
    /// </summary>
    Task<bool> BulkDeleteAsync(IEnumerable<int> ids);

    /// <summary>
    /// Bulk hard delete messages
    /// </summary>
    Task<bool> BulkHardDeleteAsync(IEnumerable<int> ids);

    /// <summary>
    /// Get contact message statistics
    /// </summary>
    Task<ContactMessageStatistics> GetContactMessageStatisticsAsync();

    /// <summary>
    /// Reply to contact message
    /// </summary>
    Task<bool> ReplyToMessageAsync(int id, string reply, int repliedByUserId);
}

/// <summary>
/// Contact message statistics model
/// </summary>
public class ContactMessageStatistics
{
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public int ReadMessages { get; set; }
    public int RepliedMessages { get; set; }
    public int UnrepliedMessages { get; set; }
    public int TodayMessages { get; set; }
    public int ThisWeekMessages { get; set; }
    public int ThisMonthMessages { get; set; }
    public DateTime? LastMessageDate { get; set; }
    public ContactMessage? LatestMessage { get; set; }
    public ContactMessage? OldestUnreadMessage { get; set; }
    public double AverageMessagesPerDay { get; set; }
}

