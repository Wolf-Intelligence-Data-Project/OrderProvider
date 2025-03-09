using Microsoft.EntityFrameworkCore;
using OrderProvider.Entities; // Ensure this is correct based on your project structure

namespace OrderProvider.Data;// Consider renaming to ProductProvider.Data if it's only for products

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

            // Set Revenue with specified precision and scale
            entity.Property(e => e.Revenue)
                .IsRequired()
                .HasColumnType("decimal(18,2)");  // 18 total digits with 2 decimal places

            entity.Property(e => e.NumberOfEmployees).IsRequired();
            entity.Property(e => e.CEO).IsRequired();

            // Nullable fields
            entity.Property(e => e.CustomerId).IsRequired(false);
            entity.Property(e => e.SoldUntil).IsRequired(false).HasColumnType("datetime");
            entity.Property(e => e.ReservedUntil).IsRequired(false).HasColumnType("datetime");

            // Exclude this table from migration updates in this provider (this one is handled in productprovider)
            modelBuilder.Entity<ProductEntity>().Metadata.SetIsTableExcludedFromMigrations(true);
        });

        // Configure ReservationEntity
        modelBuilder.Entity<ReservationEntity>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(e => e.ReservationId);

            entity.Property(e => e.CustomerId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.ReservedFrom).IsRequired().HasColumnType("datetime");
            entity.Property(e => e.SoldFrom).IsRequired(false).HasColumnType("datetime");

            // Nullable Fields
            entity.Property(e => e.BusinessTypes).IsRequired(false);
            entity.Property(e => e.Regions).IsRequired(false);
            entity.Property(e => e.CitiesByRegion).IsRequired(false);
            entity.Property(e => e.Cities).IsRequired(false);
            entity.Property(e => e.PostalCodes).IsRequired(false);
            entity.Property(e => e.MinRevenue).IsRequired(false);
            entity.Property(e => e.MaxRevenue).IsRequired(false);
            entity.Property(e => e.MinNumberOfEmployees).IsRequired(false);
            entity.Property(e => e.MaxNumberOfEmployees).IsRequired(false);
        });
    }

}
