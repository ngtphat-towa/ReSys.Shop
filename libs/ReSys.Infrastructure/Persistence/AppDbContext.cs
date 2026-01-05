using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Identity;
using ReSys.Infrastructure.Persistence.Converters;
using ReSys.Infrastructure.Persistence.Extensions;

namespace ReSys.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyBaseConfigurations();
        modelBuilder.ApplySoftDeleteQueryFilter();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.UseOpenIddict();
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