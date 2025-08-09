using ECommerce.Core.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.WebUI.Areas.Admin.Models;

/// <summary>
/// ViewModel for User listing with advanced filtering
/// </summary>
public class UserListViewModel
{
    public IEnumerable<UserItemViewModel> Users { get; set; } = new List<UserItemViewModel>();
    
    // Pagination properties
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public int PageSize { get; set; } = 20;
    
    // Search and filter properties
    [Display(Name = "Search")]
    public string? SearchTerm { get; set; }
    
    [Display(Name = "Role")]
    public UserRole? Role { get; set; }
    
    [Display(Name = "Status")]
    public bool? IsActive { get; set; }
    
    [Display(Name = "Registered From")]
    [DataType(DataType.Date)]
    public DateTime? FromDate { get; set; }
    
    [Display(Name = "Registered To")]
    [DataType(DataType.Date)]
    public DateTime? ToDate { get; set; }
    
    [Display(Name = "Sort By")]
    public string SortBy { get; set; } = "createdDate";
    
    [Display(Name = "Sort Order")]
    public string SortOrder { get; set; } = "desc";
    
    // Available options for dropdowns
    public List<SelectListItem> RoleOptions { get; set; } = new();
    public List<SelectListItem> StatusOptions { get; set; } = new();
    public List<SelectListItem> SortOptions { get; set; } = new()
    {
        new SelectListItem { Value = "createdDate", Text = "Registration Date" },
        new SelectListItem { Value = "username", Text = "Username" },
        new SelectListItem { Value = "email", Text = "Email" },
        new SelectListItem { Value = "fullname", Text = "Full Name" },
        new SelectListItem { Value = "role", Text = "Role" },
        new SelectListItem { Value = "lastlogin", Text = "Last Login" }
    };
    
    // Statistics
    public UserStatisticsViewModel Statistics { get; set; } = new();
    
    // Computed properties
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int StartItem => (CurrentPage - 1) * PageSize + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);
    public bool HasFilters => !string.IsNullOrEmpty(SearchTerm) || Role.HasValue || 
                              IsActive.HasValue || FromDate.HasValue || ToDate.HasValue;
}

/// <summary>
/// ViewModel for individual user items in list
/// </summary>
public class UserItemViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int LoginCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    
    // Computed properties
    public string FullName => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) 
        ? $"{FirstName} {LastName}" : UserName;
    public string RoleDisplayName => Role.ToString();
    public string RoleCssClass => GetRoleCssClass(Role);
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public string StatusCssClass => IsActive ? "success" : "secondary";
    public int DaysSinceRegistration => (DateTime.Now - CreatedDate).Days;
    public int DaysSinceLastLogin => LastLoginDate.HasValue ? (DateTime.Now - LastLoginDate.Value).Days : int.MaxValue;
    public bool IsRecentlyActive => LastLoginDate.HasValue && DaysSinceLastLogin <= 30;
    public bool IsNewUser => DaysSinceRegistration <= 7;
    
    private static string GetRoleCssClass(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => "danger",
            UserRole.Customer => "primary",
            _ => "secondary"
        };
    }
}

/// <summary>
/// ViewModel for detailed user information
/// </summary>
public class UserDetailViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int LoginCount { get; set; }
    
    // Activity and statistics
    public UserActivityViewModel Activity { get; set; } = new();
    
    // Orders information
    public List<UserOrderViewModel> RecentOrders { get; set; } = new();
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDate { get; set; }
    
    // Addresses
    public List<UserAddressViewModel> Addresses { get; set; } = new();
    
    // Security information
    public List<string> SecurityActivities { get; set; } = new();
    
    // Computed properties
    public string FullName => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) 
        ? $"{FirstName} {LastName}" : UserName;
    public string RoleDisplayName => Role.ToString();
    public string RoleCssClass => GetRoleCssClass(Role);
    public string StatusDisplayName => IsActive ? "Active" : "Inactive";
    public string StatusCssClass => IsActive ? "success" : "secondary";
    public int DaysSinceRegistration => (DateTime.Now - CreatedDate).Days;
    public int DaysSinceLastLogin => LastLoginDate.HasValue ? (DateTime.Now - LastLoginDate.Value).Days : int.MaxValue;
    public bool IsRecentlyActive => LastLoginDate.HasValue && DaysSinceLastLogin <= 30;
    public bool HasOrders => TotalOrders > 0;
    public bool IsHighValueCustomer => TotalSpent > 1000; // Configurable threshold
    
    private static string GetRoleCssClass(UserRole role)
    {
        return role switch
        {
            UserRole.Admin => "danger",
            UserRole.Customer => "primary",
            _ => "secondary"
        };
    }
}

/// <summary>
/// ViewModel for user creation
/// </summary>
public class UserCreateViewModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }
    
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please confirm password")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required")]
    [Display(Name = "User Role")]
    public UserRole Role { get; set; }
    
    [Display(Name = "Active User")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Send Welcome Email")]
    public bool SendWelcomeEmail { get; set; } = true;
    
    // Available options for dropdowns
    public List<SelectListItem> RoleOptions { get; set; } = new();
    
    // Validation helper properties
    public bool IsValidPassword => !string.IsNullOrEmpty(Password) && Password.Length >= 8;
    public bool PasswordsMatch => Password == ConfirmPassword;
}

/// <summary>
/// ViewModel for user editing
/// </summary>
public class UserEditViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    [Display(Name = "Username")]
    public string UserName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
    
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }
    
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
    
    [Required(ErrorMessage = "Role is required")]
    [Display(Name = "User Role")]
    public UserRole Role { get; set; }
    
    [Display(Name = "Active User")]
    public bool IsActive { get; set; }
    
    // Password reset section
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password (optional)")]
    public string? NewPassword { get; set; }
    
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string? ConfirmNewPassword { get; set; }
    
    [Display(Name = "Force Password Change")]
    public bool ForcePasswordChange { get; set; }
    
    [Display(Name = "Send Notification Email")]
    public bool SendNotificationEmail { get; set; } = true;
    
    // Available options for dropdowns
    public List<SelectListItem> RoleOptions { get; set; } = new();
    
    // Display information
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int LoginCount { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    
    // Computed properties
    public string FullName => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName) 
        ? $"{FirstName} {LastName}" : UserName;
    public bool IsPasswordBeingChanged => !string.IsNullOrEmpty(NewPassword);
    public bool HasActivity => OrderCount > 0 || LoginCount > 0;
}

/// <summary>
/// ViewModel for user deletion
/// </summary>
public class UserDeleteViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastLoginDate { get; set; }
    
    // Impact information
    public int OrderCount { get; set; }
    public int CommentCount { get; set; }
    public decimal TotalSpent { get; set; }
    public List<string> Dependencies { get; set; } = new();
    
    // Deletion options
    public bool CanBeDeleted { get; set; }
    public bool CanBeHardDeleted { get; set; }
    public string DeletionReason { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Deletion reason is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    [Display(Name = "Reason for Deletion")]
    public string Reason { get; set; } = string.Empty;
    
    [Display(Name = "Deletion Type")]
    public bool HardDelete { get; set; } = false;
    
    [Display(Name = "Send Notification")]
    public bool SendNotification { get; set; } = true;
    
    // Computed properties
    public string RoleDisplayName => Role.ToString();
    public string RoleCssClass => Role == UserRole.Admin ? "danger" : "primary";
    public bool HasActivity => OrderCount > 0 || CommentCount > 0;
    public bool IsHighImpactDeletion => Role == UserRole.Admin || TotalSpent > 500;
}

/// <summary>
/// Supporting ViewModels
/// </summary>
public class UserActivityViewModel
{
    public DateTime? LastLoginDate { get; set; }
    public int LoginCount { get; set; }
    public List<string> RecentActivities { get; set; } = new();
    public bool IsActiveUser { get; set; }
    public int DaysSinceRegistration { get; set; }
    public int DaysSinceLastLogin { get; set; }
}

public class UserOrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    
    public string StatusDisplayName => Status.ToString();
    public string StatusCssClass => Status switch
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

public class UserAddressViewModel
{
    public int Id { get; set; }
    public string AddressLine { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    
    public string FullAddress => $"{AddressLine}, {City}, {Country} {ZipCode}";
}

public class UserStatisticsViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int AdminUsers { get; set; }
    public int CustomerUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int NewUsersThisMonth { get; set; }
    public decimal AverageOrdersPerCustomer { get; set; }
    public decimal AverageSpendingPerCustomer { get; set; }
}
