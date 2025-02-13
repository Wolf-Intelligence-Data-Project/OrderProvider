// OrderDbContext.cs
using Microsoft.EntityFrameworkCore;
using OrderProvider.Models;

namespace OrderProvider.Data;

public class OrderDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Custom configurations for your models (optional)
        base.OnModelCreating(modelBuilder);
    }
}