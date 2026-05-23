using Jewelryshop.Api.Data;
using Jewelryshop.Api.DTOs;
using Jewelryshop.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public OrdersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        var userId = GetCurrentUserId();

        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .AsQueryable();

        query = query.Where(order => order.UserId == userId);

        var orders = await query.OrderByDescending(order => order.CreatedAt).ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var userId = GetCurrentUserId();

        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(order => order.Id == id);

        if (order is null)
        {
            return NotFound();
        }

        if (order.UserId != userId)
        {
            return Forbid();
        }

        return Ok(order);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<Order>> Checkout()
    {
        var userId = GetCurrentUserId();
        var cartItems = await _dbContext.CartItems
            .Include(item => item.Product)
            .Where(item => item.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return BadRequest("Cart is empty.");
        }

        foreach (var item in cartItems)
        {
            if (item.Product is null)
            {
                return BadRequest("A product in your cart no longer exists.");
            }

            if (item.Product.StockQuantity < item.Quantity)
            {
                return BadRequest($"Not enough stock for {item.Product.Name}.");
            }
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            TotalAmount = cartItems.Sum(item => item.Product!.Price * item.Quantity),
            Items = cartItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product!.Price
            }).ToList()
        };

        foreach (var item in cartItems)
        {
            item.Product!.StockQuantity -= item.Quantity;
        }

        _dbContext.Orders.Add(order);
        _dbContext.CartItems.RemoveRange(cartItems);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        var order = await _dbContext.Orders.FindAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
        {
            return BadRequest("Invalid order status.");
        }

        order.Status = status;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        return 1;
    }
}



