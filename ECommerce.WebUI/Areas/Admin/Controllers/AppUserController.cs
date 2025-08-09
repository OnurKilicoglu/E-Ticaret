using ECommerce.Core.Entities;
using ECommerce.Service.Interfaces;
using ECommerce.WebUI.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Controller for AppUser management in Admin area
/// </summary>
[Area("Admin")]
public class AppUserController : Controller
{
    private readonly IUserService _userService;

    public AppUserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Display list of users with advanced filtering and search
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, UserRole? role, bool? isActive,
        DateTime? fromDate, DateTime? toDate, string sortBy = "createdDate", string sortOrder = "desc",
        int page = 1, int pageSize = 20)
    {
        ViewData["Title"] = "AppUser Management";

        try
        {
            var (users, totalCount) = await _userService.GetUsersAsync(
                searchTerm, role, isActive, fromDate, toDate, sortBy, sortOrder, page, pageSize);

            var statistics = await _userService.GetUserStatisticsAsync();

            var viewModel = new UserListViewModel
            {
                Users = users.Select(u => new UserItemViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    LastLoginDate = u.LastLoginDate,
                    LoginCount = u.LoginCount,
                    OrderCount = u.Orders.Count,
                    TotalSpent = u.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount)
                }),
                CurrentPage = page,
                TotalItems = totalCount,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchTerm = searchTerm,
                Role = role,
                IsActive = isActive,
                FromDate = fromDate,
                ToDate = toDate,
                SortBy = sortBy,
                SortOrder = sortOrder,
                RoleOptions = GetRoleOptions(),
                StatusOptions = GetStatusOptions(),
                Statistics = new UserStatisticsViewModel
                {
                    TotalUsers = statistics.TotalUsers,
                    ActiveUsers = statistics.ActiveUsers,
                    InactiveUsers = statistics.InactiveUsers,
                    AdminUsers = statistics.AdminUsers,
                    CustomerUsers = statistics.CustomerUsers,
                    NewUsersToday = statistics.NewUsersToday,
                    NewUsersThisWeek = statistics.NewUsersThisWeek,
                    NewUsersThisMonth = statistics.NewUsersThisMonth,
                    AverageOrdersPerCustomer = statistics.AverageOrdersPerCustomer,
                    AverageSpendingPerCustomer = statistics.AverageSpendingPerCustomer
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading users: {ex.Message}";
            return View(new UserListViewModel());
        }
    }

    /// <summary>
    /// Display detailed user information
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "AppUser Details";

        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var activity = await _userService.GetUserActivityAsync(id);

            var viewModel = new UserDetailViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate,
                LastLoginDate = user.LastLoginDate,
                LoginCount = user.LoginCount,
                Activity = new UserActivityViewModel
                {
                    LastLoginDate = activity.LastLoginDate == DateTime.MinValue ? null : activity.LastLoginDate,
                    LoginCount = activity.LoginCount,
                    RecentActivities = activity.RecentActivities,
                    IsActiveUser = activity.IsActiveUser,
                    DaysSinceRegistration = activity.DaysSinceRegistration,
                    DaysSinceLastLogin = activity.DaysSinceLastLogin
                },
                RecentOrders = user.Orders.OrderByDescending(o => o.OrderDate).Take(10).Select(o => new UserOrderViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.OrderStatus,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count
                }).ToList(),
                TotalOrders = user.Orders.Count,
                TotalSpent = user.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                LastOrderDate = user.Orders.Any() ? user.Orders.Max(o => o.OrderDate) : null,
                Addresses = user.ShippingAddresses.Select(a => new UserAddressViewModel
                {
                    Id = a.Id,
                    AddressLine = a.AddressLine,
                    City = a.City,
                    Country = a.Country,
                    ZipCode = a.ZipCode
                }).ToList(),
                SecurityActivities = new List<string>
                {
                    $"Account created on {user.CreatedDate:MMM dd, yyyy}",
                    user.LastLoginDate.HasValue ? $"Last login on {user.LastLoginDate:MMM dd, yyyy}" : "Never logged in",
                    $"Total logins: {user.LoginCount}",
                    $"Account status: {(user.IsActive ? "Active" : "Inactive")}"
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading user details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Display user creation form
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create New AppUser";

        var viewModel = new UserCreateViewModel
        {
            RoleOptions = GetRoleOptions()
        };

        return View(viewModel);
    }

    /// <summary>
    /// Process user creation
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        ViewData["Title"] = "Create New AppUser";

        // Reload role options for view re-rendering
        model.RoleOptions = GetRoleOptions();

        if (ModelState.IsValid)
        {
            try
            {
                // Check for unique username
                if (!await _userService.IsUsernameUniqueAsync(model.UserName))
                {
                    ModelState.AddModelError("UserName", "This username is already taken.");
                    return View(model);
                }

                // Check for unique email
                if (!await _userService.IsEmailUniqueAsync(model.Email))
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }

                // Validate password
                var passwordValidation = _userService.ValidatePassword(model.Password);
                if (!passwordValidation.IsValid)
                {
                    foreach (var error in passwordValidation.Errors)
                    {
                        ModelState.AddModelError("Password", error);
                    }
                    return View(model);
                }

                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role,
                    IsActive = model.IsActive
                };

                var createdUser = await _userService.CreateUserAsync(user, model.Password);

                TempData["SuccessMessage"] = $"User '{model.UserName}' has been created successfully.";

                if (model.SendWelcomeEmail)
                {
                    TempData["InfoMessage"] = "Welcome email will be sent to the user.";
                }

                return RedirectToAction(nameof(Details), new { id = createdUser.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating user: {ex.Message}";
            }
        }

        return View(model);
    }

    /// <summary>
    /// Display user edit form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit AppUser";

        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                LoginCount = user.LoginCount,
                OrderCount = user.Orders.Count,
                TotalSpent = user.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount),
                RoleOptions = GetRoleOptions()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading user: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process user update
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserEditViewModel model)
    {
        ViewData["Title"] = "Edit AppUser";

        if (id != model.Id)
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToAction(nameof(Index));
        }

        // Reload role options for view re-rendering
        model.RoleOptions = GetRoleOptions();

        if (ModelState.IsValid)
        {
            try
            {
                // Check for unique username (excluding current user)
                if (!await _userService.IsUsernameUniqueAsync(model.UserName, model.Id))
                {
                    ModelState.AddModelError("UserName", "This username is already taken.");
                    return View(model);
                }

                // Check for unique email (excluding current user)
                if (!await _userService.IsEmailUniqueAsync(model.Email, model.Id))
                {
                    ModelState.AddModelError("Email", "This email address is already registered.");
                    return View(model);
                }

                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Update user properties
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;
                user.Role = model.Role;
                user.IsActive = model.IsActive;

                var success = await _userService.UpdateUserAsync(user);

                if (success)
                {
                    // Handle password reset if provided
                    if (!string.IsNullOrEmpty(model.NewPassword))
                    {
                        var passwordValidation = _userService.ValidatePassword(model.NewPassword);
                        if (!passwordValidation.IsValid)
                        {
                            foreach (var error in passwordValidation.Errors)
                            {
                                ModelState.AddModelError("NewPassword", error);
                            }
                            return View(model);
                        }

                        var passwordReset = await _userService.ResetPasswordAsync(id, model.NewPassword);
                        if (passwordReset)
                        {
                            TempData["InfoMessage"] = "Password has been reset successfully.";
                        }
                    }

                    TempData["SuccessMessage"] = $"User '{model.UserName}' has been updated successfully.";

                    if (model.SendNotificationEmail)
                    {
                        TempData["InfoMessage"] = "Notification email will be sent to the user.";
                    }

                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update user.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating user: {ex.Message}";
            }
        }

        return View(model);
    }

    /// <summary>
    /// Display user deletion confirmation
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Delete AppUser";

        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var eligibility = await _userService.CheckDeletionEligibilityAsync(id);

            var viewModel = new UserDeleteViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = !string.IsNullOrEmpty(user.FirstName) && !string.IsNullOrEmpty(user.LastName) 
                    ? $"{user.FirstName} {user.LastName}" : user.UserName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                OrderCount = eligibility.OrderCount,
                CommentCount = eligibility.CommentCount,
                TotalSpent = eligibility.TotalSpent,
                Dependencies = eligibility.Dependencies,
                CanBeDeleted = eligibility.CanBeDeleted,
                CanBeHardDeleted = eligibility.CanBeHardDeleted,
                DeletionReason = eligibility.Reason
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading user: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Process user deletion
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, UserDeleteViewModel model)
    {
        try
        {
            if (ModelState.IsValid)
            {
                bool success;
                
                if (model.HardDelete && model.CanBeHardDeleted)
                {
                    success = await _userService.HardDeleteUserAsync(id);
                    TempData["SuccessMessage"] = $"User '{model.UserName}' has been permanently deleted.";
                }
                else
                {
                    success = await _userService.DeleteUserAsync(id);
                    TempData["SuccessMessage"] = $"User '{model.UserName}' has been deactivated.";
                }

                if (success)
                {
                    if (model.SendNotification)
                    {
                        TempData["InfoMessage"] = "User notification email will be sent.";
                    }

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete user.";
                }
            }

            return View("Delete", model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            return View("Delete", model);
        }
    }

    /// <summary>
    /// AJAX endpoint for quick role changes
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ChangeRole(int userId, UserRole newRole)
    {
        try
        {
            var success = await _userService.ChangeUserRoleAsync(userId, newRole);
            
            if (success)
            {
                return Json(new { success = true, message = $"User role changed to {newRole}" });
            }
            
            return Json(new { success = false, message = "Failed to change user role" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for quick status toggle
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int userId, bool isActive)
    {
        try
        {
            var success = await _userService.ToggleUserStatusAsync(userId, isActive);
            
            if (success)
            {
                var status = isActive ? "activated" : "deactivated";
                return Json(new { success = true, message = $"User {status} successfully" });
            }
            
            return Json(new { success = false, message = "Failed to update user status" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for password reset
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ResetPassword(int userId, string newPassword)
    {
        try
        {
            var success = await _userService.ResetPasswordAsync(userId, newPassword);
            
            if (success)
            {
                return Json(new { success = true, message = "Password reset successfully" });
            }
            
            return Json(new { success = false, message = "Failed to reset password" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// AJAX endpoint for username uniqueness check
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckUsername(string username, int? id = null)
    {
        try
        {
            var isUnique = await _userService.IsUsernameUniqueAsync(username, id);
            return Json(new { available = isUnique });
        }
        catch
        {
            return Json(new { available = false });
        }
    }

    /// <summary>
    /// AJAX endpoint for email uniqueness check
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckEmail(string email, int? id = null)
    {
        try
        {
            var isUnique = await _userService.IsEmailUniqueAsync(email, id);
            return Json(new { available = isUnique });
        }
        catch
        {
            return Json(new { available = false });
        }
    }

    /// <summary>
    /// Export users to CSV
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Export(string? searchTerm, UserRole? role, bool? isActive,
        DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            var (users, _) = await _userService.GetUsersAsync(
                searchTerm, role, isActive, fromDate, toDate, pageSize: int.MaxValue);

            var csvContent = GenerateCsvContent(users);
            var fileName = $"appusers_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            
            return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error exporting users: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #region Helper Methods

    private List<SelectListItem> GetRoleOptions()
    {
        return Enum.GetValues<UserRole>()
            .Select(r => new SelectListItem
            {
                Value = r.ToString(),
                Text = r.ToString()
            }).ToList();
    }

    private List<SelectListItem> GetStatusOptions()
    {
        return new List<SelectListItem>
        {
            new() { Value = "true", Text = "Active" },
            new() { Value = "false", Text = "Inactive" }
        };
    }

    private string GenerateCsvContent(IEnumerable<AppUser> users)
    {
        var csv = new System.Text.StringBuilder();
        csv.AppendLine("Username,Email,First Name,Last Name,Phone,Role,Status,Created Date,Last Login,Orders,Total Spent");
        
        foreach (var user in users)
        {
            var totalSpent = user.Orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalAmount);
            csv.AppendLine($"{user.UserName},{user.Email},{user.FirstName},{user.LastName},{user.PhoneNumber},{user.Role},{(user.IsActive ? "Active" : "Inactive")},{user.CreatedDate:yyyy-MM-dd},{user.LastLoginDate?.ToString("yyyy-MM-dd") ?? "Never"},{user.Orders.Count},{totalSpent:F2}");
        }
        
        return csv.ToString();
    }

    #endregion
}
