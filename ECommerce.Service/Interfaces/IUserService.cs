using ECommerce.Core.Entities;

namespace ECommerce.Service.Interfaces;

/// <summary>
/// Service interface for User (AppUser) entity operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get all users with advanced filtering, search, and pagination
    /// </summary>
    /// <param name="searchTerm">Search term for username or email</param>
    /// <param name="role">Filter by user role</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="fromDate">Filter users registered from this date</param>
    /// <param name="toDate">Filter users registered to this date</param>
    /// <param name="sortBy">Sort field (username, email, createdDate, role)</param>
    /// <param name="sortOrder">Sort order (asc, desc)</param>
    /// <param name="page">Page number for pagination</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Tuple containing users and total count</returns>
    Task<(IEnumerable<AppUser> Users, int TotalCount)> GetUsersAsync(
        string? searchTerm = null,
        UserRole? role = null,
        bool? isActive = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string sortBy = "createdDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// Get user by ID with all related data
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User entity with related data or null if not found</returns>
    Task<AppUser?> GetUserByIdAsync(int id);

    /// <summary>
    /// Get user by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User entity or null if not found</returns>
    Task<AppUser?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User entity or null if not found</returns>
    Task<AppUser?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="password">Plain text password</param>
    /// <returns>User entity if authentication successful, null otherwise</returns>
    Task<AppUser?> AuthenticateAsync(string email, string password);

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="user">User entity to create</param>
    /// <param name="password">Plain text password</param>
    /// <param name="adminUserId">ID of admin creating the user</param>
    /// <returns>Created user entity</returns>
    Task<AppUser> CreateUserAsync(AppUser user, string password, int? adminUserId = null);

    /// <summary>
    /// Update user information
    /// </summary>
    /// <param name="user">User entity to update</param>
    /// <param name="adminUserId">ID of admin updating the user</param>
    /// <returns>True if successful</returns>
    Task<bool> UpdateUserAsync(AppUser user, int? adminUserId = null);

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <param name="adminUserId">ID of admin deleting the user</param>
    /// <returns>True if successful</returns>
    Task<bool> DeleteUserAsync(int userId, int? adminUserId = null);

    /// <summary>
    /// Hard delete user (permanent removal)
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <param name="adminUserId">ID of admin deleting the user</param>
    /// <returns>True if successful</returns>
    Task<bool> HardDeleteUserAsync(int userId, int? adminUserId = null);

    /// <summary>
    /// Change user role
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newRole">New role to assign</param>
    /// <param name="adminUserId">ID of admin changing the role</param>
    /// <returns>True if successful</returns>
    Task<bool> ChangeUserRoleAsync(int userId, UserRole newRole, int? adminUserId = null);

    /// <summary>
    /// Reset user password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newPassword">New plain text password</param>
    /// <param name="adminUserId">ID of admin resetting password</param>
    /// <returns>True if successful</returns>
    Task<bool> ResetPasswordAsync(int userId, string newPassword, int? adminUserId = null);

    /// <summary>
    /// Toggle user active status
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="isActive">New active status</param>
    /// <param name="adminUserId">ID of admin changing status</param>
    /// <returns>True if successful</returns>
    Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int? adminUserId = null);

    /// <summary>
    /// Check if username is unique
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <returns>True if username is unique</returns>
    Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null);

    /// <summary>
    /// Check if email is unique
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <returns>True if email is unique</returns>
    Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null);

    /// <summary>
    /// Get user statistics for dashboard
    /// </summary>
    /// <returns>User statistics</returns>
    Task<UserStatistics> GetUserStatisticsAsync();

    /// <summary>
    /// Get recent user registrations
    /// </summary>
    /// <param name="count">Number of users to return</param>
    /// <returns>Recent users</returns>
    Task<IEnumerable<AppUser>> GetRecentUsersAsync(int count = 10);

    /// <summary>
    /// Get users by role
    /// </summary>
    /// <param name="role">User role</param>
    /// <returns>Users with specified role</returns>
    Task<IEnumerable<AppUser>> GetUsersByRoleAsync(UserRole role);

    /// <summary>
    /// Get total user count
    /// </summary>
    /// <returns>Total number of users</returns>
    Task<int> GetUserCountAsync();

    /// <summary>
    /// Get user activity summary
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User activity summary</returns>
    Task<UserActivitySummary> GetUserActivityAsync(int userId);

    /// <summary>
    /// Validate password strength
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <returns>Password validation result</returns>
    PasswordValidationResult ValidatePassword(string password);

    /// <summary>
    /// Hash password for storage
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify password against hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Stored password hash</param>
    /// <returns>True if password matches</returns>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Log user activity
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="action">Action performed</param>
    /// <param name="details">Action details</param>
    /// <param name="adminUserId">ID of admin performing action</param>
    /// <returns>True if logged successfully</returns>
    Task<bool> LogUserActivityAsync(int userId, string action, string? details = null, int? adminUserId = null);

    /// <summary>
    /// Check if user can be deleted
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Deletion eligibility result</returns>
    Task<UserDeletionEligibility> CheckDeletionEligibilityAsync(int userId);
}

/// <summary>
/// User statistics for dashboard
/// </summary>
public class UserStatistics
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

/// <summary>
/// User activity summary
/// </summary>
public class UserActivitySummary
{
    public int UserId { get; set; }
    public DateTime LastLoginDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime LastOrderDate { get; set; }
    public int LoginCount { get; set; }
    public List<string> RecentActivities { get; set; } = new();
    public bool IsActiveUser { get; set; }
    public int DaysSinceRegistration { get; set; }
    public int DaysSinceLastLogin { get; set; }
}

/// <summary>
/// Password validation result
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public int Score { get; set; } // 0-100 strength score
}

/// <summary>
/// User deletion eligibility
/// </summary>
public class UserDeletionEligibility
{
    public bool CanBeDeleted { get; set; }
    public bool CanBeHardDeleted { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public int CommentCount { get; set; }
    public decimal TotalSpent { get; set; }
    public List<string> Dependencies { get; set; } = new();
}
