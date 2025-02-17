using Microsoft.EntityFrameworkCore;
using OrderProvider.Core.Entities;

namespace OrderProvider.Persistence.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        // Entity to represent orders placed by companies
        public DbSet<Order> Orders { get; set; }

        // Entity to represent cart items (products in the order)
        public DbSet<CartItemEntity> CartItems { get; set; }

        // Entity to represent carts (this is likely associated with the company placing the order)
        public DbSet<CartEntity> Carts { get; set; }

        // You don't need the Products DbSet here, as it's managed by a separate service

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define Cart - CartItems relationship
            modelBuilder.Entity<CartEntity>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Define Order - Cart relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cart)
                .WithMany()
                .HasForeignKey(o => o.CartId)
                .OnDelete(DeleteBehavior.Restrict); // Orders should not be deleted if the cart is deleted

            base.OnModelCreating(modelBuilder);
        }

    }
}
