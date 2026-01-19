using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Shared.Constants;
using ReSys.Infrastructure.Persistence.Extensions;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(DatabaseTables.Roles, DatabaseSchemas.Identity);

        builder.Property(r => r.DisplayName)
            .HasMaxLength(RoleConstraints.DisplayNameMaxLength);

        builder.Property(r => r.Description)
            .HasMaxLength(RoleConstraints.DescriptionMaxLength);

        builder.Property(r => r.Priority)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasMany(r => r.RoleClaims)
            .WithOne()
            .HasForeignKey(rc => rc.RoleId)
            .IsRequired();

        builder.HasMany(r => r.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        builder.ConfigureAuditable();
        builder.ConfigureMetadata();
    }
}