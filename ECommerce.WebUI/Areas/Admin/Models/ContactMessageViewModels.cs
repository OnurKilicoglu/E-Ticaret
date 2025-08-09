using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.Areas.Admin.Models;

#region Contact Message ViewModels

/// <summary>
/// ViewModel for contact message list page
/// </summary>
public class ContactMessageListViewModel
{
    public IEnumerable<ContactMessageItemViewModel> Messages { get; set; } = new List<ContactMessageItemViewModel>();
    public ContactMessageStatisticsViewModel Statistics { get; set; } = new();
    
    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 20;
    public int TotalPages { get; set; }
    
    // Filtering
    public string? SearchTerm { get; set; }
    public string? Email { get; set; }
    public bool? IsRead { get; set; }
    public bool? IsReplied { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Sorting
    public string SortBy { get; set; } = "createdDate";
    public string SortOrder { get; set; } = "desc";
    
    // Filter options
    public List<SelectListItem> ReadStatusOptions { get; set; } = new()
    {
        new SelectListItem { Value = "", Text = "All Messages" },
        new SelectListItem { Value = "false", Text = "Unread Only" },
        new SelectListItem { Value = "true", Text = "Read Only" }
    };
    
    public List<SelectListItem> ReplyStatusOptions { get; set; } = new()
    {
        new SelectListItem { Value = "", Text = "All Messages" },
        new SelectListItem { Value = "false", Text = "Not Replied" },
        new SelectListItem { Value = "true", Text = "Replied" }
    };
    
    // Helper properties
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || 
                             !string.IsNullOrEmpty(Email) || 
                             IsRead.HasValue || 
                             IsReplied.HasValue || 
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
/// ViewModel for individual contact message item in list
/// </summary>
public class ContactMessageItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? RepliedDate { get; set; }
    
    // Helper properties
    public string ReadStatusCssClass => IsRead ? "success" : "warning";
    public string ReadStatusIcon => IsRead ? "bi-envelope-open" : "bi-envelope";
    public string ReadStatusDisplayName => IsRead ? "Read" : "Unread";
    
    public string ReplyStatusCssClass => IsReplied ? "info" : "secondary";
    public string ReplyStatusIcon => IsReplied ? "bi-reply-fill" : "bi-reply";
    public string ReplyStatusDisplayName => IsReplied ? "Replied" : "Not Replied";
    
    public string TruncatedMessage => Message.Length > 100 ? Message[..100] + "..." : Message;
    public string TruncatedSubject => Subject.Length > 50 ? Subject[..50] + "..." : Subject;
    
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.UtcNow - CreatedDate;
            return timeSpan.TotalDays >= 1 ? $"{(int)timeSpan.TotalDays} days ago" :
                   timeSpan.TotalHours >= 1 ? $"{(int)timeSpan.TotalHours} hours ago" :
                   timeSpan.TotalMinutes >= 1 ? $"{(int)timeSpan.TotalMinutes} minutes ago" :
                   "Just now";
        }
    }
}

/// <summary>
/// ViewModel for contact message details page
/// </summary>
public class ContactMessageDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public string? AdminReply { get; set; }
    public DateTime? RepliedDate { get; set; }
    public int? RepliedByUserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    
    // Helper properties
    public string ReadStatusCssClass => IsRead ? "success" : "warning";
    public string ReadStatusIcon => IsRead ? "bi-envelope-open-fill" : "bi-envelope-fill";
    public string ReadStatusDisplayName => IsRead ? "Read" : "Unread";
    
    public string ReplyStatusCssClass => IsReplied ? "info" : "secondary";
    public string ReplyStatusIcon => IsReplied ? "bi-reply-fill" : "bi-reply";
    public string ReplyStatusDisplayName => IsReplied ? "Replied" : "Not Replied";
    
    public string MailtoLink => $"mailto:{Email}?subject=Re: {Uri.EscapeDataString(Subject)}";
    
    public string FormattedMessage => Message.Replace("\n", "<br>");
    public string FormattedAdminReply => AdminReply?.Replace("\n", "<br>") ?? string.Empty;
}

/// <summary>
/// ViewModel for contact message deletion
/// </summary>
public class ContactMessageDeleteViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public DateTime CreatedDate { get; set; }
    
    [Required(ErrorMessage = "Please provide a reason for deletion")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string Reason { get; set; } = string.Empty;
    
    [Display(Name = "Permanent Delete")]
    public bool HardDelete { get; set; }
    
    // Helper properties
    public string ReadStatusCssClass => IsRead ? "success" : "warning";
    public string ReadStatusDisplayName => IsRead ? "Read" : "Unread";
    
    public string ReplyStatusCssClass => IsReplied ? "info" : "secondary";
    public string ReplyStatusDisplayName => IsReplied ? "Replied" : "Not Replied";
    
    public string TruncatedMessage => Message.Length > 200 ? Message[..200] + "..." : Message;
    
    public string DeleteTypeDisplayName => HardDelete ? "Permanent Deletion" : "Soft Deletion";
    public string DeleteTypeDescription => HardDelete 
        ? "This message will be permanently removed from the database and cannot be recovered." 
        : "This message will be marked as inactive but can be restored later.";
}

/// <summary>
/// ViewModel for contact message reply
/// </summary>
public class ContactMessageReplyViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Reply message is required")]
    [StringLength(1000, ErrorMessage = "Reply cannot exceed 1000 characters")]
    public string Reply { get; set; } = string.Empty;
    
    public int RepliedByUserId { get; set; }
}

/// <summary>
/// ViewModel for contact message statistics
/// </summary>
public class ContactMessageStatisticsViewModel
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
    public string? LatestMessageSender { get; set; }
    public string? LatestMessageSubject { get; set; }
    public string? OldestUnreadSender { get; set; }
    public DateTime? OldestUnreadDate { get; set; }
    public double AverageMessagesPerDay { get; set; }
    
    // Helper properties
    public double ReadPercentage => TotalMessages > 0 ? (double)ReadMessages / TotalMessages * 100 : 0;
    public double ReplyPercentage => TotalMessages > 0 ? (double)RepliedMessages / TotalMessages * 100 : 0;
    public double UnreadPercentage => TotalMessages > 0 ? (double)UnreadMessages / TotalMessages * 100 : 0;
    
    public string ReadPercentageFormatted => $"{ReadPercentage:F1}%";
    public string ReplyPercentageFormatted => $"{ReplyPercentage:F1}%";
    public string UnreadPercentageFormatted => $"{UnreadPercentage:F1}%";
    
    public string AverageMessagesPerDayFormatted => $"{AverageMessagesPerDay:F1}";
    
    public string LastMessageTimeAgo
    {
        get
        {
            if (!LastMessageDate.HasValue) return "No messages yet";
            
            var timeSpan = DateTime.UtcNow - LastMessageDate.Value;
            return timeSpan.TotalDays >= 1 ? $"{(int)timeSpan.TotalDays} days ago" :
                   timeSpan.TotalHours >= 1 ? $"{(int)timeSpan.TotalHours} hours ago" :
                   timeSpan.TotalMinutes >= 1 ? $"{(int)timeSpan.TotalMinutes} minutes ago" :
                   "Just now";
        }
    }
}

/// <summary>
/// ViewModel for bulk operations
/// </summary>
public class BulkContactMessageOperationViewModel
{
    public List<int> SelectedIds { get; set; } = new();
    public string Operation { get; set; } = string.Empty; // markread, markunread, delete, harddelete
    public string? Reason { get; set; }
}

/// <summary>
/// ViewModel for bulk update operations
/// </summary>
public class BulkContactMessageUpdateViewModel
{
    public List<ContactMessageBulkUpdateItem> Updates { get; set; } = new();
}

/// <summary>
/// Individual bulk update item
/// </summary>
public class ContactMessageBulkUpdateItem
{
    public int Id { get; set; }
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
}

#endregion

