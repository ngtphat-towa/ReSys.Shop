using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence.Constants;

namespace ReSys.Identity.Persistence;

public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) 
    : IdentityDbContext<
        ApplicationUser, 
        IdentityRole, 
        string, 
        IdentityUserClaim<string>, 
        IdentityUserRole<string>, 
        IdentityUserLogin<string>, 
        IdentityRoleClaim<string>, 
        IdentityUserToken<string>>(options)
{
    public DbSet<ClaimDefinition> ClaimDefinitions => Set<ClaimDefinition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // 1. Set default schema
        builder.HasDefaultSchema(Schemas.Identity);

        base.OnModelCreating(builder);
        
        // 2. Configure OpenIddict entities
        builder.UseOpenIddict();

        // 3. Explicitly override Identity table names to be 100% sure
        builder.Entity<ApplicationUser>().ToTable(TableNames.Users, Schemas.Identity);
        builder.Entity<IdentityRole>().ToTable(TableNames.Roles, Schemas.Identity);
        builder.Entity<IdentityUserRole<string>>().ToTable(TableNames.UserRoles, Schemas.Identity);
        builder.Entity<IdentityUserClaim<string>>().ToTable(TableNames.UserClaims, Schemas.Identity);
        builder.Entity<IdentityUserLogin<string>>().ToTable(TableNames.UserLogins, Schemas.Identity);
        builder.Entity<IdentityRoleClaim<string>>().ToTable(TableNames.RoleClaims, Schemas.Identity);
        builder.Entity<IdentityUserToken<string>>().ToTable(TableNames.UserTokens, Schemas.Identity);

        // 4. Apply all custom configurations (Identity + ClaimDefinition) from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppIdentityDbContext).Assembly);
    }
}