namespace ECommerce.WebUI.Areas.Admin.Models;

/// <summary>
/// ViewModel for admin dashboard statistics and data
/// </summary>
public class DashboardViewModel
{
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int LowStockProducts { get; set; }
    public int NewUsers { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public List<RecentOrderViewModel> RecentOrders { get; set; } = new();
}

/// <summary>
/// ViewModel for recent orders display in dashboard
/// </summary>
public class RecentOrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
