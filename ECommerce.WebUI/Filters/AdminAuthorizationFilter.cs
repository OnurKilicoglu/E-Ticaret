using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ECommerce.Core.Entities;

namespace ECommerce.WebUI.Filters;

/// <summary>
/// Authorization filter to restrict access to admin area for admin users only
/// </summary>
public class AdminAuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        
        // Check if user is logged in
        var userId = session.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            // Redirect to login page
            context.Result = new RedirectToActionResult("Login", "Account", new { area = "", returnUrl = context.HttpContext.Request.Path });
            return;
        }

        // Check if user has admin role
        var userRole = session.GetString("UserRole");
        if (userRole != UserRole.Admin.ToString())
        {
            // Redirect to access denied page or home page
            context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
            return;
        }
    }
}

/// <summary>
/// Attribute to apply admin authorization filter
/// </summary>
public class AdminAuthorizeAttribute : TypeFilterAttribute
{
    public AdminAuthorizeAttribute() : base(typeof(AdminAuthorizationFilter))
    {
    }
}
