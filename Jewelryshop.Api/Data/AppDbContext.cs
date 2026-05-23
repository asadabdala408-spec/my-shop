using Jewelryshop.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Jewelryshop.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<Category>().HasIndex(category => category.Name).IsUnique();

        modelBuilder.Entity<Product>().Property(product => product.Price).HasColumnType("numeric(18,2)");
        modelBuilder.Entity<Order>().Property(order => order.TotalAmount).HasColumnType("numeric(18,2)");
        modelBuilder.Entity<OrderItem>().Property(item => item.UnitPrice).HasColumnType("numeric(18,2)");

        modelBuilder.Entity<CartItem>()
            .HasIndex(item => new { item.UserId, item.ProductId })
            .IsUnique();
    }
}
