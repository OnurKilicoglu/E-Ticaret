using ECommerce.Core.Entities;
using ECommerce.Data.Context;
using ECommerce.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ECommerce.Service.Implementations;

/// <summary>
/// Service implementation for User (AppUser) entity operations
/// </summary>
public class UserService : IUserService
{
    private readonly ECommerceDbContext _context;

    public UserService(ECommerceDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<AppUser> Users, int TotalCount)> GetUsersAsync(
        string? searchTerm = null,
        UserRole? role = null,
        bool? isActive = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string sortBy = "createdDate",
        string sortOrder = "desc",
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.AppUsers
            .Include(u => u.Orders)
            .Include(u => u.ShippingAddresses)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.UserName.Contains(searchTerm) ||
                                   u.Email.Contains(searchTerm) ||
                                   (u.FirstName != null && u.FirstName.Contains(searchTerm)) ||
                                   (u.LastName != null && u.LastName.Contains(searchTerm)));
        }

        // Apply role filter
        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        // Apply active status filter
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        // Apply date range filter
        if (fromDate.HasValue)
        {
            query = query.Where(u => u.CreatedDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(u => u.CreatedDate <= toDate.Value.AddDays(1));
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "username" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(u => u.UserName)
                : query.OrderBy(u => u.UserName),
            "email" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "role" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(u => u.Role)
                : query.OrderBy(u => u.Role),
            "fullname" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName)
                : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName),
            "lastlogin" => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(u => u.LastLoginDate)
                : query.OrderBy(u => u.LastLoginDate),
            _ => sortOrder.ToLower() == "desc"
                ? query.OrderByDescending(u => u.CreatedDate)
                : query.OrderBy(u => u.CreatedDate)
        };

        // Get total count for pagination
        var totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        return await _context.AppUsers
            .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
            .Include(u => u.ShippingAddresses)
            .Include(u => u.Carts)
                .ThenInclude(c => c.CartItems)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.UserName == username);
    }

    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        return await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> AuthenticateAsync(string email, string password)
    {
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user != null && VerifyPassword(password, user.PasswordHash))
        {
            // Update last login date and count
            user.LastLoginDate = DateTime.UtcNow;
            user.LoginCount++;
            await _context.SaveChangesAsync();

            return user;
        }

        return null;
    }

    public async Task<AppUser> CreateUserAsync(AppUser user, string password, int? adminUserId = null)
    {
        // Validate password
        var passwordValidation = ValidatePassword(password);
        if (!passwordValidation.IsValid)
        {
            throw new ArgumentException($"Password validation failed: {string.Join(", ", passwordValidation.Errors)}");
        }

        // Hash password
        user.PasswordHash = HashPassword(password);
        user.CreatedDate = DateTime.UtcNow;
        user.UpdatedDate = DateTime.UtcNow;

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        // Log activity
        await LogUserActivityAsync(user.Id, "User Created", $"User created by admin {adminUserId}", adminUserId);

        return user;
    }

    public async Task<bool> UpdateUserAsync(AppUser user, int? adminUserId = null)
    {
        try
        {
            var existingUser = await GetUserByIdAsync(user.Id);
            if (existingUser == null)
                return false;

            // Update properties
            existingUser.UserName = user.UserName;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;
            existingUser.UpdatedDate = DateTime.UtcNow;

            _context.AppUsers.Update(existingUser);
            await _context.SaveChangesAsync();

            // Log activity
            await LogUserActivityAsync(user.Id, "User Updated", $"User updated by admin {adminUserId}", adminUserId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(int userId, int? adminUserId = null)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            // Soft delete
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();

            // Log activity
            await LogUserActivityAsync(userId, "User Deleted (Soft)", $"User soft deleted by admin {adminUserId}", adminUserId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> HardDeleteUserAsync(int userId, int? adminUserId = null)
    {
        try
        {
            var eligibility = await CheckDeletionEligibilityAsync(userId);
            if (!eligibility.CanBeHardDeleted)
                return false;

            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            // Log activity before deletion
            await LogUserActivityAsync(userId, "User Deleted (Hard)", $"User permanently deleted by admin {adminUserId}", adminUserId);

            _context.AppUsers.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangeUserRoleAsync(int userId, UserRole newRole, int? adminUserId = null)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            var oldRole = user.Role;
            user.Role = newRole;
            user.UpdatedDate = DateTime.UtcNow;

            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();

            // Log activity
            await LogUserActivityAsync(userId, "Role Changed", $"Role changed from {oldRole} to {newRole} by admin {adminUserId}", adminUserId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword, int? adminUserId = null)
    {
        try
        {
            // Validate password
            var passwordValidation = ValidatePassword(newPassword);
            if (!passwordValidation.IsValid)
                return false;

            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            user.PasswordHash = HashPassword(newPassword);
            user.UpdatedDate = DateTime.UtcNow;

            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();

            // Log activity
            await LogUserActivityAsync(userId, "Password Reset", $"Password reset by admin {adminUserId}", adminUserId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive, int? adminUserId = null)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = isActive;
            user.UpdatedDate = DateTime.UtcNow;

            _context.AppUsers.Update(user);
            await _context.SaveChangesAsync();

            // Log activity
            var action = isActive ? "User Activated" : "User Deactivated";
            await LogUserActivityAsync(userId, action, $"Status changed to {(isActive ? "Active" : "Inactive")} by admin {adminUserId}", adminUserId);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsUsernameUniqueAsync(string username, int? excludeUserId = null)
    {
        var query = _context.AppUsers.Where(u => u.UserName == username);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeUserId = null)
    {
        var query = _context.AppUsers.Where(u => u.Email == email);
        
        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        var totalUsers = await _context.AppUsers.CountAsync();
        var activeUsers = await _context.AppUsers.CountAsync(u => u.IsActive);
        var adminUsers = await _context.AppUsers.CountAsync(u => u.Role == UserRole.Admin);
        var customerUsers = await _context.AppUsers.CountAsync(u => u.Role == UserRole.Customer);

        var today = DateTime.Today;
        var thisWeek = today.AddDays(-7);
        var thisMonth = today.AddDays(-30);

        var newUsersToday = await _context.AppUsers.CountAsync(u => u.CreatedDate >= today);
        var newUsersThisWeek = await _context.AppUsers.CountAsync(u => u.CreatedDate >= thisWeek);
        var newUsersThisMonth = await _context.AppUsers.CountAsync(u => u.CreatedDate >= thisMonth);

        // Calculate customer metrics
        var customerStats = await _context.AppUsers
            .Where(u => u.Role == UserRole.Customer)
            .Select(u => new
            {
                OrderCount = u.Orders.Count,
                TotalSpent = u.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount)
            })
            .ToListAsync();

        var avgOrdersPerCustomer = customerStats.Count > 0 ? (decimal)customerStats.Average(c => c.OrderCount) : 0;
        var avgSpendingPerCustomer = customerStats.Count > 0 ? customerStats.Average(c => c.TotalSpent) : 0;

        return new UserStatistics
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = totalUsers - activeUsers,
            AdminUsers = adminUsers,
            CustomerUsers = customerUsers,
            NewUsersToday = newUsersToday,
            NewUsersThisWeek = newUsersThisWeek,
            NewUsersThisMonth = newUsersThisMonth,
            AverageOrdersPerCustomer = avgOrdersPerCustomer,
            AverageSpendingPerCustomer = avgSpendingPerCustomer
        };
    }

    public async Task<IEnumerable<AppUser>> GetRecentUsersAsync(int count = 10)
    {
        return await _context.AppUsers
            .OrderByDescending(u => u.CreatedDate)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<AppUser>> GetUsersByRoleAsync(UserRole role)
    {
        return await _context.AppUsers
            .Where(u => u.Role == role)
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.AppUsers.CountAsync();
    }

    public async Task<UserActivitySummary> GetUserActivityAsync(int userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return new UserActivitySummary { UserId = userId };

        var totalOrders = user.Orders.Count;
        var totalSpent = user.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount);
        var lastOrderDate = user.Orders.Any() ? user.Orders.Max(o => o.OrderDate) : DateTime.MinValue;
        var daysSinceRegistration = (DateTime.Now - user.CreatedDate).Days;
        var daysSinceLastLogin = user.LastLoginDate.HasValue ? (DateTime.Now - user.LastLoginDate.Value).Days : int.MaxValue;

        return new UserActivitySummary
        {
            UserId = userId,
            LastLoginDate = user.LastLoginDate ?? DateTime.MinValue,
            TotalOrders = totalOrders,
            TotalSpent = totalSpent,
            LastOrderDate = lastOrderDate,
            LoginCount = user.LoginCount,
            IsActiveUser = user.IsActive && daysSinceLastLogin <= 30,
            DaysSinceRegistration = daysSinceRegistration,
            DaysSinceLastLogin = daysSinceLastLogin,
            RecentActivities = new List<string>
            {
                $"Registered {daysSinceRegistration} days ago",
                $"Total orders: {totalOrders}",
                $"Total spent: {totalSpent:C}",
                user.LastLoginDate.HasValue ? $"Last login: {daysSinceLastLogin} days ago" : "Never logged in"
            }
        };
    }

    public PasswordValidationResult ValidatePassword(string password)
    {
        var result = new PasswordValidationResult { IsValid = true, Score = 0 };

        if (string.IsNullOrWhiteSpace(password))
        {
            result.IsValid = false;
            result.Errors.Add("Password is required");
            return result;
        }

        // Length check
        if (password.Length < 8)
        {
            result.IsValid = false;
            result.Errors.Add("Password must be at least 8 characters long");
        }
        else
        {
            result.Score += 20;
        }

        // Uppercase letter check
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one uppercase letter");
        }
        else
        {
            result.Score += 20;
        }

        // Lowercase letter check
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one lowercase letter");
        }
        else
        {
            result.Score += 20;
        }

        // Number check
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            result.IsValid = false;
            result.Errors.Add("Password must contain at least one number");
        }
        else
        {
            result.Score += 20;
        }

        // Special character check
        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]"))
        {
            result.Errors.Add("Password should contain at least one special character");
            result.Score += 10; // Not required but recommended
        }
        else
        {
            result.Score += 20;
        }

        // Length bonus
        if (password.Length >= 12)
        {
            result.Score += 10;
        }

        result.Score = Math.Min(result.Score, 100);
        return result;
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = "ECommerce_Salt_2024"; // In production, use a proper salt generation
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToBase64String(hashedBytes);
    }

    public bool VerifyPassword(string password, string hash)
    {
        var newHash = HashPassword(password);
        return newHash == hash;
    }

    public async Task<bool> LogUserActivityAsync(int userId, string action, string? details = null, int? adminUserId = null)
    {
        try
        {
            // In a real implementation, you would have a UserActivityLog table
            // For now, we'll just update the user's UpdatedDate
            var user = await _context.AppUsers.FindAsync(userId);
            if (user != null)
            {
                user.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<UserDeletionEligibility> CheckDeletionEligibilityAsync(int userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return new UserDeletionEligibility
            {
                CanBeDeleted = false,
                CanBeHardDeleted = false,
                Reason = "User not found"
            };
        }

        var orderCount = user.Orders.Count;
        var totalSpent = user.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount);
        var commentCount = 0; // Would be calculated from comments table

        var dependencies = new List<string>();
        if (orderCount > 0)
            dependencies.Add($"{orderCount} order(s)");
        if (commentCount > 0)
            dependencies.Add($"{commentCount} comment(s)");

        var canBeHardDeleted = orderCount == 0 && commentCount == 0;

        return new UserDeletionEligibility
        {
            CanBeDeleted = true, // Soft delete always allowed
            CanBeHardDeleted = canBeHardDeleted,
            Reason = canBeHardDeleted ? "User can be permanently deleted" : "User has associated data",
            OrderCount = orderCount,
            CommentCount = commentCount,
            TotalSpent = totalSpent,
            Dependencies = dependencies
        };
    }
}
