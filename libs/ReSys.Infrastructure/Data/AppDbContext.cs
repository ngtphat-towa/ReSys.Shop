using Microsoft.EntityFrameworkCore;
using ReSys.Core.Interfaces;

namespace ReSys.Infrastructure.Data;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Ensure all DateTimeOffset properties are UTC when reading/writing
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, DateTimeOffset>(
                        v => v.ToUniversalTime(),
                        v => v.ToUniversalTime()));
                }
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Use 'timestamp with time zone' for DateTimeOffset to natively support UTC in PostgreSQL
        configurationBuilder.Properties<DateTimeOffset>().HaveColumnType("timestamp with time zone");
        configurationBuilder.Properties<DateTimeOffset?>().HaveColumnType("timestamp with time zone");
    }
}