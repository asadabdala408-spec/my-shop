using Jewelryshop.Api.Data;
using Jewelryshop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = nameof(UserRole.Admin))]
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

        var lowStockItems = await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .Where(product => product.StockQuantity <= 5)
            .OrderBy(product => product.StockQuantity)
            .ThenBy(product => product.Name)
            .Take(8)
            .Select(product => new LowStockProductResponse(
                product.Id,
                product.Name,
                product.Category != null ? product.Category.Name : "No category",
                product.StockQuantity))
            .ToListAsync();

        var bestSellers = await _dbContext.OrderItems
            .AsNoTracking()
            .Include(item => item.Product)
            .GroupBy(item => new { item.ProductId, ProductName = item.Product != null ? item.Product.Name : "Unknown Product" })
            .Select(group => new BestSellingProductResponse(
                group.Key.ProductId,
                group.Key.ProductName,
                group.Sum(item => item.Quantity),
                group.Sum(item => item.UnitPrice * item.Quantity)))
            .OrderByDescending(product => product.QuantitySold)
            .Take(5)
            .ToListAsync();

        var statusBreakdown = await _dbContext.Orders
            .AsNoTracking()
            .GroupBy(order => order.Status)
            .Select(group => new OrderStatusBreakdownResponse(
                group.Key.ToString(),
                group.Count(),
                group.Sum(order => order.TotalAmount)))
            .OrderBy(item => item.Status)
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
            recentOrders,
            lowStockItems,
            bestSellers,
            statusBreakdown));
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
    IReadOnlyCollection<RecentOrderResponse> RecentOrders,
    IReadOnlyCollection<LowStockProductResponse> LowStockItems,
    IReadOnlyCollection<BestSellingProductResponse> BestSellers,
    IReadOnlyCollection<OrderStatusBreakdownResponse> StatusBreakdown);

public record RecentOrderResponse(
    int Id,
    string CustomerName,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt);

public record LowStockProductResponse(
    int Id,
    string Name,
    string CategoryName,
    int StockQuantity);

public record BestSellingProductResponse(
    int ProductId,
    string ProductName,
    int QuantitySold,
    decimal Revenue);

public record OrderStatusBreakdownResponse(
    string Status,
    int Count,
    decimal TotalAmount);

