using Jewelryshop.Api.Data;
using Jewelryshop.Api.DTOs;
using Jewelryshop.Api.Models;
using Jewelryshop.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly CloudinaryService _cloudinaryService;

    public ProductsController(AppDbContext dbContext, CloudinaryService cloudinaryService)
    {
        _dbContext = dbContext;
        _cloudinaryService = cloudinaryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .OrderBy(product => product.Name)
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(ProductRequest request)
    {
        var categoryExists = await _dbContext.Categories.AnyAsync(category => category.Id == request.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist.");
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl?.Trim(),
            CategoryId = request.CategoryId
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductRequest request)
    {
        var product = await _dbContext.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        var categoryExists = await _dbContext.Categories.AnyAsync(category => category.Id == request.CategoryId);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist.");
        }

        product.Name = request.Name.Trim();
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.CategoryId = request.CategoryId;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost("{id:int}/image")]
    public async Task<ActionResult<Product>> UploadProductImage(int id, IFormFile file)
    {
        var product = await _dbContext.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        if (file.Length == 0)
        {
            return BadRequest("Image file is required.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Only JPG, JPEG, PNG, and WEBP images are allowed.");
        }

        product.ImageUrl = await _cloudinaryService.UploadProductImageAsync(file);
        await _dbContext.SaveChangesAsync();

        return Ok(product);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _dbContext.Products.FindAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}
