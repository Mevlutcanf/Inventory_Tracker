using InventoryChecker.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryChecker.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasIndex(x => x.AssetTag).IsUnique();
            entity.Property(x => x.Status).HasConversion<string>();
        });

        modelBuilder.Entity<AssetAssignment>(entity =>
        {
            entity.HasOne(x => x.Asset)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Employee)
                .WithMany(x => x.Assignments)
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
