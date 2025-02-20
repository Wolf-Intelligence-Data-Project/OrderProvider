using Microsoft.EntityFrameworkCore;
using OrderProvider.Entities;

namespace OrderProvider.Data
{
    public class OrderDbContext : DbContext
    {
        public DbSet<OrderEntity> Orders { get; set; }

        // Add the constructor that accepts DbContextOptions<OrderDbContext>
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options) // Pass the options to the base DbContext constructor
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderEntity>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18, 2)"); // TotalPrice column

            // Add the same for PricePerProductAtPurchase
            modelBuilder.Entity<OrderEntity>()
                .Property(o => o.PricePerProductAtPurchase)
                .HasColumnType("decimal(18, 2)"); // Define the column type with precision and scale

            base.OnModelCreating(modelBuilder);
        }
    }
}
