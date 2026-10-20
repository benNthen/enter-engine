using Microsoft.EntityFrameworkCore;
using EnterEngine.Api.Models;

namespace EnterEngine.Api.Data;

public class VehicleDbContext : DbContext
{
  // TODO: Add DbSet properties after creating your domain entities.

  public VehicleDbContext(DbContextOptions<VehicleDbContext> options) : base(options)
  {
  }

  public DbSet<Vehicle> Vehicles => Set<Vehicle>();
  public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
  public DbSet<Category> Categories => Set<Category>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Manufacturer entity configuration
    modelBuilder.Entity<Manufacturer>(entity =>
    {
      entity.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

      entity.HasIndex(x => x.Name).IsUnique();

      entity.HasData(
              new Manufacturer { Id = 1, Name = "Toyota" },
              new Manufacturer { Id = 2, Name = "Mazda" },
              new Manufacturer { Id = 3, Name = "Mercedes" },
              new Manufacturer { Id = 4, Name = "Honda" },
              new Manufacturer { Id = 5, Name = "Ferrari" }
          );
    });

    // Vehicle entity configuration
    modelBuilder.Entity<Vehicle>(entity =>
    { // Owner Name is required and has a maximum length of 100 characters
      entity.Property(x => x.OwnerName)
              .HasMaxLength(100)
              .IsRequired();
      // WeightKg is required and has a precision of 10,2
      entity.Property(x => x.WeightKg)
              .HasPrecision(10, 2);
      // YearOfManufacture is required and has a range of 1886 to the current year
      entity.Property(x => x.YearOfManufacture)
              .IsRequired()
              .HasDefaultValueSql("YEAR(GETDATE())");
      // ManufacturerId is required and has a foreign key constraint to the Manufacturer table
      entity.Property(x => x.ManufacturerId)
              .IsRequired();
      entity.HasOne(x => x.Manufacturer)
              .WithMany()
              .HasForeignKey(x => x.ManufacturerId)
              .OnDelete(DeleteBehavior.Restrict);

      entity.ToTable("Vehicles", table =>
              table.HasCheckConstraint(
                  "CK_Vehicles_WeightKg_Positive",
                  "[WeightKg] > 0"));
    });

    // Category entity configuration
    modelBuilder.Entity<Category>(entity =>
    {
      entity.Property(x => x.Name)
              .HasMaxLength(50)
              .IsRequired();

      entity.Property(x => x.IconKey)
              .HasMaxLength(50)
              .IsRequired();
      entity.Property(x => x.MinWeightKg)
              .HasPrecision(10, 2);

      entity.Property(x => x.MaxWeightKg)
              .HasPrecision(10, 2);

      entity.ToTable("Categories", table =>
      {
        table.HasCheckConstraint(
            "CK_Categories_ValidRange",
            "[MinWeightKg] >= 0 AND " +
            "([MaxWeightKg] IS NULL OR [MaxWeightKg] > [MinWeightKg])");
      });
    });
  }
}
