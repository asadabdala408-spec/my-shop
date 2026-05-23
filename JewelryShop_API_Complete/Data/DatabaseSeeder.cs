using Jewelryshop.Api.Models;
using Jewelryshop.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

        await dbContext.Database.MigrateAsync();

        if (!await dbContext.Users.AnyAsync(user => user.Role == UserRole.Admin))
        {
            dbContext.Users.Add(new User
            {
                FullName = "System Admin",
                Email = "admin@jewelryshop.com",
                PasswordHash = passwordService.HashPassword("Admin12345!"),
                Role = UserRole.Admin
            });
        }

        if (!await dbContext.Categories.AnyAsync())
        {
            var rings = new Category
            {
                Name = "Rings",
                Description = "Elegant rings for weddings, engagements, and everyday style."
            };

            var necklaces = new Category
            {
                Name = "Necklaces",
                Description = "Beautiful necklaces crafted for modern and classic looks."
            };

            var bracelets = new Category
            {
                Name = "Bracelets",
                Description = "Luxury bracelets for casual and formal occasions."
            };

            var earrings = new Category
            {
                Name = "Earrings",
                Description = "Stylish earrings designed with premium finishes."
            };

            dbContext.Categories.AddRange(rings, necklaces, bracelets, earrings);

            dbContext.Products.AddRange(
                new Product
                {
                    Name = "Diamond Engagement Ring",
                    Description = "Premium diamond ring with a polished gold band.",
                    Price = 1299.99m,
                    StockQuantity = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1605100804763-247f67b3557e",
                    Category = rings
                },
                new Product
                {
                    Name = "Classic Gold Necklace",
                    Description = "Elegant gold necklace suitable for special occasions.",
                    Price = 899.50m,
                    StockQuantity = 15,
                    ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f",
                    Category = necklaces
                },
                new Product
                {
                    Name = "Silver Charm Bracelet",
                    Description = "Modern silver bracelet with a minimal charm design.",
                    Price = 249.99m,
                    StockQuantity = 25,
                    ImageUrl = "https://images.unsplash.com/photo-1611591437281-460bfbe1220a",
                    Category = bracelets
                },
                new Product
                {
                    Name = "Pearl Drop Earrings",
                    Description = "Classic pearl earrings with a luxury finish.",
                    Price = 179.99m,
                    StockQuantity = 30,
                    ImageUrl = "https://images.unsplash.com/photo-1535632066927-ab7c9ab60908",
                    Category = earrings
                });
        }

        await dbContext.SaveChangesAsync();
    }
}
