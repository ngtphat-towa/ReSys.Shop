using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Identity.Permissions;
using ReSys.Shared.Constants;
using ReSys.Infrastructure.Persistence.Extensions;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity;

public sealed class AccessPermissionConfiguration : IEntityTypeConfiguration<AccessPermission>
{
    public void Configure(EntityTypeBuilder<AccessPermission> builder)
    {
        builder.ToTable(DatabaseTables.AccessPermissions, DatabaseSchemas.Identity);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(AccessPermissionConstraints.MaxNameLength);

        builder.Property(p => p.Area)
            .IsRequired()
            .HasMaxLength(AccessPermissionConstraints.MaxSegmentLength);

        builder.Property(p => p.Resource)
            .IsRequired()
            .HasMaxLength(AccessPermissionConstraints.MaxSegmentLength);

        builder.Property(p => p.Action)
            .IsRequired()
            .HasMaxLength(AccessPermissionConstraints.MaxSegmentLength);

        builder.Property(p => p.DisplayName)
            .HasMaxLength(AccessPermissionConstraints.Display.MaxDisplayNameLength);

        builder.Property(p => p.Description)
            .HasMaxLength(AccessPermissionConstraints.Display.MaxDescriptionLength);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.ConfigureAuditable();
        builder.ConfigureMetadata();
    }
}
