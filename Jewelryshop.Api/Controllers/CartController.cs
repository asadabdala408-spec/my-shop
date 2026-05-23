using System.Security.Claims;
using Jewelryshop.Api.Data;
using Jewelryshop.Api.DTOs;
using Jewelryshop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CartController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CartItem>>> GetCart()
    {
        var userId = GetCurrentUserId();
        var items = await _dbContext.CartItems
            .AsNoTracking()
            .Include(item => item.Product)
            .ThenInclude(product => product!.Category)
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<CartItem>> AddToCart(CartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be greater than zero.");
        }

        var userId = GetCurrentUserId();
        var product = await _dbContext.Products.FindAsync(request.ProductId);

        if (product is null)
        {
            return NotFound("Product was not found.");
        }

        if (product.StockQuantity < request.Quantity)
        {
            return BadRequest("Not enough stock available.");
        }

        var existingItem = await _dbContext.CartItems.FirstOrDefaultAsync(item => item.UserId == userId && item.ProductId == request.ProductId);

        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + request.Quantity;
            if (product.StockQuantity < newQuantity)
            {
                return BadRequest("Not enough stock available.");
            }

            existingItem.Quantity = newQuantity;
            await _dbContext.SaveChangesAsync();

            return Ok(existingItem);
        }

        var cartItem = new CartItem
        {
            UserId = userId,
            ProductId = request.ProductId,
            Quantity = request.Quantity
        };

        _dbContext.CartItems.Add(cartItem);
        await _dbContext.SaveChangesAsync();

        return Ok(cartItem);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCartItem(int id, CartItemRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest("Quantity must be greater than zero.");
        }

        var userId = GetCurrentUserId();
        var item = await _dbContext.CartItems.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (item is null)
        {
            return NotFound();
        }

        var product = await _dbContext.Products.FindAsync(item.ProductId);

        if (product is null)
        {
            return BadRequest("Product no longer exists.");
        }

        if (product.StockQuantity < request.Quantity)
        {
            return BadRequest("Not enough stock available.");
        }

        item.Quantity = request.Quantity;
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> RemoveCartItem(int id)
    {
        var userId = GetCurrentUserId();
        var item = await _dbContext.CartItems.FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId);

        if (item is null)
        {
            return NotFound();
        }

        _dbContext.CartItems.Remove(item);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var id) ? id : throw new UnauthorizedAccessException("User id claim is missing.");
    }
}


