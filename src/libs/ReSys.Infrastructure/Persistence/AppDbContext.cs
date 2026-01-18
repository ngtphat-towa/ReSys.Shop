using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Testing.ExampleCategories;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Inventories.Stocks;
using ReSys.Core.Domain.Inventories.Movements;
using ReSys.Infrastructure.Persistence.Converters;
using ReSys.Infrastructure.Persistence.Extensions;

namespace ReSys.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, Role, string>(options), IApplicationDbContext
{
    public DbSet<Example> Examples { get; set; }
    public DbSet<ExampleCategory> ExampleCategories { get; set; }

    public DbSet<StockLocation> StockLocations { get; set; }
    public DbSet<StockItem> StockItems { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<InventoryUnit> InventoryUnits { get; set; }
    public DbSet<StockTransfer> StockTransfers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Essential for Identity

        if (Database.IsNpgsql())
        {
            modelBuilder.HasPostgresExtension("vector");
            modelBuilder.HasPostgresEnum<ExampleStatus>();
            modelBuilder.HasPostgresEnum<StockTransferStatus>();
            modelBuilder.HasPostgresEnum<StockLocationType>();
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global Configurations via Reflection
        modelBuilder.ApplyPostgresConfiguration(Database.ProviderName);
        modelBuilder.ApplyAuditableConfiguration();
        modelBuilder.ApplyMetadataConfiguration();
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTimeOffset>()
            .HaveColumnType("timestamp with time zone")
            .HaveConversion<UtcDateTimeOffsetConverter>();

        configurationBuilder.Properties<DateTimeOffset?>()
            .HaveColumnType("timestamp with time zone")
            .HaveConversion<UtcDateTimeOffsetConverter>();
    }
}