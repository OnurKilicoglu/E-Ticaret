using Microsoft.AspNetCore.Mvc;
using ECommerce.WebUI.Filters;

namespace ECommerce.WebUI.Areas.Admin.Controllers;

/// <summary>
/// Dashboard controller for admin panel overview and statistics
/// </summary>
[Area("Admin")]
[AdminAuthorize]
public class DashboardController : Controller
{
    /// <summary>
    /// Admin dashboard index page with overview statistics
    /// </summary>
    /// <returns>Dashboard view with statistics</returns>
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        
        // TODO: In future, load actual statistics from services
        // For now, we'll use mock data for demonstration
        var dashboardData = new
        {
            TotalOrders = 125,
            TotalProducts = 48,
            TotalUsers = 342,
            TotalRevenue = 15750.50m,
            PendingOrders = 12,
            LowStockProducts = 5,
            NewUsers = 23,
            MonthlyRevenue = 8450.75m
        };
        
        return View(dashboardData);
    }
}
