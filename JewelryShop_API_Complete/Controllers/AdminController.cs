using Jewelryshop.Api.Data;
using Jewelryshop.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AdminController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardResponse>> GetDashboard()
    {
        var totalUsers = await _dbContext.Users.CountAsync();
        var totalCustomers = await _dbContext.Users.CountAsync(user => user.Role == UserRole.Customer);
        var totalAdmins = await _dbContext.Users.CountAsync(user => user.Role == UserRole.Admin);
        var totalCategories = await _dbContext.Categories.CountAsync();
        var totalProducts = await _dbContext.Products.CountAsync();
        var totalOrders = await _dbContext.Orders.CountAsync();
        var pendingOrders = await _dbContext.Orders.CountAsync(order => order.Status == OrderStatus.Pending);
        var completedOrders = await _dbContext.Orders.CountAsync(order => order.Status == OrderStatus.Completed);
        var totalRevenue = await _dbContext.Orders
            .Where(order => order.Status == OrderStatus.Completed)
            .SumAsync(order => order.TotalAmount);
        var lowStockProducts = await _dbContext.Products.CountAsync(product => product.StockQuantity <= 5);

        var recentOrders = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.User)
            .OrderByDescending(order => order.CreatedAt)
            .Take(5)
            .Select(order => new RecentOrderResponse(
                order.Id,
                order.User != null ? order.User.FullName : "Unknown Customer",
                order.TotalAmount,
                order.Status.ToString(),
                order.CreatedAt))
            .ToListAsync();

        return Ok(new AdminDashboardResponse(
            totalUsers,
            totalCustomers,
            totalAdmins,
            totalCategories,
            totalProducts,
            totalOrders,
            pendingOrders,
            completedOrders,
            totalRevenue,
            lowStockProducts,
            recentOrders));
    }
}

public record AdminDashboardResponse(
    int TotalUsers,
    int TotalCustomers,
    int TotalAdmins,
    int TotalCategories,
    int TotalProducts,
    int TotalOrders,
    int PendingOrders,
    int CompletedOrders,
    decimal TotalRevenue,
    int LowStockProducts,
    IReadOnlyCollection<RecentOrderResponse> RecentOrders);

public record RecentOrderResponse(
    int Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);

