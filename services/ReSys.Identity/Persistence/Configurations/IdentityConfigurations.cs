using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenIddict.EntityFrameworkCore.Models;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence.Constants;

namespace ReSys.Identity.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable(TableNames.Users, Schemas.Identity);
    }
}

public class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.ToTable(TableNames.Roles, Schemas.Identity);
    }
}

public class IdentityUserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
    {
        builder.ToTable(TableNames.UserRoles, Schemas.Identity);
    }
}

public class IdentityUserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
    {
        builder.ToTable(TableNames.UserClaims, Schemas.Identity);
    }
}

public class IdentityUserLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
    {
        builder.ToTable(TableNames.UserLogins, Schemas.Identity);
    }
}

public class IdentityRoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
    {
        builder.ToTable(TableNames.RoleClaims, Schemas.Identity);
    }
}

public class IdentityUserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<string>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
    {
        builder.ToTable(TableNames.UserTokens, Schemas.Identity);
    }
}

// OpenIddict Configurations
public class OpenIddictApplicationConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreApplication>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreApplication> builder)
    {
        builder.ToTable(TableNames.OpenIddict.Applications, Schemas.Identity);
    }
}

public class OpenIddictAuthorizationConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreAuthorization>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreAuthorization> builder)
    {
        builder.ToTable(TableNames.OpenIddict.Authorizations, Schemas.Identity);
    }
}

public class OpenIddictScopeConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreScope>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreScope> builder)
    {
        builder.ToTable(TableNames.OpenIddict.Scopes, Schemas.Identity);
    }
}

public class OpenIddictTokenConfiguration : IEntityTypeConfiguration<OpenIddictEntityFrameworkCoreToken>
{
    public void Configure(EntityTypeBuilder<OpenIddictEntityFrameworkCoreToken> builder)
    {
        builder.ToTable(TableNames.OpenIddict.Tokens, Schemas.Identity);
    }
}
