using Jewelryshop.Api.Data;
using Jewelryshop.Api.DTOs;
using Jewelryshop.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public CategoriesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return Ok(await _dbContext.Categories.AsNoTracking().OrderBy(category => category.Name).ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await _dbContext.Categories.AsNoTracking().FirstOrDefaultAsync(category => category.Id == id);

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPost]
    public async Task<ActionResult<Category>> CreateCategory(CategoryRequest request)
    {
        var name = request.Name.Trim();
        var exists = await _dbContext.Categories.AnyAsync(category => category.Name.ToLower() == name.ToLower());

        if (exists)
        {
            return Conflict("Category already exists.");
        }

        var category = new Category
        {
            Name = name,
            Description = request.Description?.Trim()
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var category = await _dbContext.Categories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = nameof(UserRole.Admin))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);

        if (category is null)
        {
            return NotFound();
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }
}

