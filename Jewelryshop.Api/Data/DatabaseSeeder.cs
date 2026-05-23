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

        var categorySeed = new[]
        {
            new Category { Name = "Rings", Description = "Elegant rings for weddings, engagements, and everyday style." },
            new Category { Name = "Necklaces", Description = "Beautiful necklaces crafted for modern and classic looks." },
            new Category { Name = "Bracelets", Description = "Luxury bracelets for casual and formal occasions." },
            new Category { Name = "Earrings", Description = "Stylish earrings designed with premium finishes." },
            new Category { Name = "Watches", Description = "Premium watches that complete a polished luxury look." }
        };

        foreach (var category in categorySeed)
        {
            if (!await dbContext.Categories.AnyAsync(item => item.Name == category.Name))
            {
                dbContext.Categories.Add(category);
            }
        }

        await dbContext.SaveChangesAsync();

        var categories = await dbContext.Categories.ToDictionaryAsync(category => category.Name);
        var productSeed = new[]
        {
            new Product { Name = "Diamond Engagement Ring", Description = "Premium diamond ring with a polished gold band.", Price = 1299.99m, StockQuantity = 10, ImageUrl = "https://images.unsplash.com/photo-1605100804763-247f67b3557e", CategoryId = categories["Rings"].Id },
            new Product { Name = "Rose Gold Halo Ring", Description = "Rose gold ring with a sparkling halo setting.", Price = 749.99m, StockQuantity = 12, ImageUrl = "https://images.unsplash.com/photo-1589674781759-c21c37956a44", CategoryId = categories["Rings"].Id },
            new Product { Name = "Sapphire Statement Ring", Description = "Bold sapphire ring designed for elegant evening wear.", Price = 980.00m, StockQuantity = 7, ImageUrl = "https://images.unsplash.com/photo-1603561591411-07134e71a2a9", CategoryId = categories["Rings"].Id },
            new Product { Name = "Classic Gold Necklace", Description = "Elegant gold necklace suitable for special occasions.", Price = 899.50m, StockQuantity = 15, ImageUrl = "https://images.unsplash.com/photo-1599643478518-a784e5dc4c8f", CategoryId = categories["Necklaces"].Id },
            new Product { Name = "Pearl Pendant Necklace", Description = "Timeless pearl pendant necklace with a delicate chain.", Price = 320.00m, StockQuantity = 18, ImageUrl = "https://images.unsplash.com/photo-1611085583191-a3b181a88401", CategoryId = categories["Necklaces"].Id },
            new Product { Name = "Emerald Layered Necklace", Description = "Layered necklace with emerald-inspired luxury accents.", Price = 540.75m, StockQuantity = 9, ImageUrl = "https://images.unsplash.com/photo-1515562141207-7a88fb7ce338", CategoryId = categories["Necklaces"].Id },
            new Product { Name = "Silver Charm Bracelet", Description = "Modern silver bracelet with a minimal charm design.", Price = 249.99m, StockQuantity = 25, ImageUrl = "https://images.unsplash.com/photo-1611591437281-460bfbe1220a", CategoryId = categories["Bracelets"].Id },
            new Product { Name = "Gold Tennis Bracelet", Description = "Luxury tennis bracelet with brilliant polished stones.", Price = 670.00m, StockQuantity = 8, ImageUrl = "https://images.unsplash.com/photo-1602751584552-8ba73aad10e1", CategoryId = categories["Bracelets"].Id },
            new Product { Name = "Leather Accent Bracelet", Description = "Modern leather and metal bracelet for everyday style.", Price = 120.00m, StockQuantity = 20, ImageUrl = "https://images.unsplash.com/photo-1611652022419-a9419f74343d", CategoryId = categories["Bracelets"].Id },
            new Product { Name = "Pearl Drop Earrings", Description = "Classic pearl earrings with a luxury finish.", Price = 179.99m, StockQuantity = 30, ImageUrl = "https://images.unsplash.com/photo-1535632066927-ab7c9ab60908", CategoryId = categories["Earrings"].Id },
            new Product { Name = "Diamond Stud Earrings", Description = "Brilliant diamond stud earrings for timeless elegance.", Price = 499.99m, StockQuantity = 16, ImageUrl = "https://images.unsplash.com/photo-1600721391776-b5cd0e0048f9", CategoryId = categories["Earrings"].Id },
            new Product { Name = "Gold Hoop Earrings", Description = "Polished gold hoops with a clean modern silhouette.", Price = 210.00m, StockQuantity = 22, ImageUrl = "https://images.unsplash.com/photo-1630019852942-f89202989a59", CategoryId = categories["Earrings"].Id },
            new Product { Name = "Classic Gold Watch", Description = "Premium gold watch with a refined jewelry-inspired finish.", Price = 1190.00m, StockQuantity = 6, ImageUrl = "https://images.unsplash.com/photo-1523170335258-f5ed11844a49", CategoryId = categories["Watches"].Id },
            new Product { Name = "Silver Minimal Watch", Description = "Minimal silver watch designed for daily luxury.", Price = 430.00m, StockQuantity = 14, ImageUrl = "https://images.unsplash.com/photo-1524592094714-0f0654e20314", CategoryId = categories["Watches"].Id },
            new Product { Name = "Black Luxury Watch", Description = "Bold black luxury watch with premium detailing.", Price = 860.00m, StockQuantity = 5, ImageUrl = "https://images.unsplash.com/photo-1542496658-e33a6d0d50f6", CategoryId = categories["Watches"].Id }
        };

        foreach (var product in productSeed)
        {
            if (!await dbContext.Products.AnyAsync(item => item.Name == product.Name))
            {
                dbContext.Products.Add(product);
            }
        }

        await dbContext.SaveChangesAsync();
    }
}
