using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Infrastructure.Persistence.Converters;

namespace ReSys.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresEnum<Core.Domain.ExampleStatus>();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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
