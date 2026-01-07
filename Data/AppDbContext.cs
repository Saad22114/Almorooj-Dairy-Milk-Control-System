using Microsoft.EntityFrameworkCore;
using FarmersApp.Data.Entities;

namespace FarmersApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<FarmerEntity> Farmers { get; set; } = null!;
    public DbSet<SettingsEntity> Settings { get; set; } = null!;
    public DbSet<MilkEntryEntity> MilkEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Farmer entity
        modelBuilder.Entity<FarmerEntity>()
            .HasKey(f => f.Code);

        modelBuilder.Entity<FarmerEntity>()
            .Property(f => f.AnimalCount)
            .HasDefaultValue(0);

        modelBuilder.Entity<FarmerEntity>()
            .Property(f => f.ExpectedQty)
            .HasDefaultValue(0);

        modelBuilder.Entity<FarmerEntity>()
            .Property(f => f.Maximum)
            .HasDefaultValue(0);

        modelBuilder.Entity<FarmerEntity>()
            .Property(f => f.Status)
            .HasDefaultValue("active");

        modelBuilder.Entity<FarmerEntity>()
            .Property(f => f.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Create index for common searches
        modelBuilder.Entity<FarmerEntity>()
            .HasIndex(f => f.Name);

        modelBuilder.Entity<FarmerEntity>()
            .HasIndex(f => f.Center);

        modelBuilder.Entity<FarmerEntity>()
            .HasIndex(f => f.Type);

        modelBuilder.Entity<FarmerEntity>()
            .HasIndex(f => f.Status);

        // Configure Settings entity
        modelBuilder.Entity<SettingsEntity>()
            .HasKey(s => s.Id);

        modelBuilder.Entity<SettingsEntity>()
            .Property(s => s.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Seed initial settings
        modelBuilder.Entity<SettingsEntity>()
            .HasData(new SettingsEntity
            {
                Id = 1,
                Port = "COM1",
                BaudRate = 2400,
                SensorMode = "automatic",
                BaudRateQuantity = 9600,
                QuantityMode = "manual",
                MilkPrice = 0.0m,
                MilkPriceCow = 0.25m,
                MilkPriceCamel = 0.4m,
                Currency = "OMR"
            });

        // Configure MilkEntry entity
        modelBuilder.Entity<MilkEntryEntity>()
            .HasKey(m => m.Id);

        modelBuilder.Entity<MilkEntryEntity>()
            .Property(m => m.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        modelBuilder.Entity<MilkEntryEntity>()
            .Property(m => m.Status)
            .HasDefaultValue("pending");

        modelBuilder.Entity<MilkEntryEntity>()
            .Property(m => m.MilkType)
            .HasDefaultValue("cow");

        // Create indexes for milk entries (for performance)
        modelBuilder.Entity<MilkEntryEntity>()
            .HasIndex(m => m.FarmerCode);

        modelBuilder.Entity<MilkEntryEntity>()
            .HasIndex(m => m.EntryDateTime);

        modelBuilder.Entity<MilkEntryEntity>()
            .HasIndex(m => m.Status);

        modelBuilder.Entity<MilkEntryEntity>()
            .HasIndex(m => m.MilkType);

        modelBuilder.Entity<MilkEntryEntity>()
            .HasIndex(m => new { m.EntryDateTime, m.FarmerCode });
    }
}
