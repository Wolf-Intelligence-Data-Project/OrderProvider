using Microsoft.EntityFrameworkCore;
using OrderProvider.Entities;

namespace OrderProvider.Data;

public class ProductDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<ReservationEntity> Reservations { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ProductEntity
        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.CompanyName).IsRequired();
            entity.Property(e => e.OrganizationNumber).IsRequired();
            entity.Property(e => e.Address).IsRequired();
            entity.Property(e => e.PostalCode).IsRequired();
            entity.Property(e => e.City).IsRequired();
            entity.Property(e => e.PhoneNumber).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.BusinessType).IsRequired();
            entity.Property(e => e.Revenue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");  

            entity.Property(e => e.NumberOfEmployees).IsRequired();
            entity.Property(e => e.CEO).IsRequired();

            entity.Property(e => e.CustomerId).IsRequired(false);
            entity.Property(e => e.SoldUntil).IsRequired(false).HasColumnType("datetime");
            entity.Property(e => e.ReservedUntil).IsRequired(false).HasColumnType("datetime");

            modelBuilder.Entity<ProductEntity>().Metadata.SetIsTableExcludedFromMigrations(true);
        });
    }

}
