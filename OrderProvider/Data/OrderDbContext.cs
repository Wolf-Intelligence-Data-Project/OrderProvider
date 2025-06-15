using Microsoft.EntityFrameworkCore;
using OrderProvider.Entities;

namespace OrderProvider.Data;

public class OrderDbContext : DbContext
{
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<ReservationEntity> Reservations { get; set; } 

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.HasKey(o => o.OrderId); 

            entity.Property(o => o.TotalPriceWithoutVat)
                .HasColumnType("decimal(18,2)");

            entity.Property(o => o.PricePerProduct)
                .HasColumnType("decimal(18,2)");

            entity.Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<ReservationEntity>(entity =>
        {
            entity.ToTable("Reservations", "dbo"); 
        });

        base.OnModelCreating(modelBuilder);
    }
}
