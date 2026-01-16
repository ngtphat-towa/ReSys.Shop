using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Identity.Permissions;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity.Permissions;

public class AccessPermissionConfiguration : IEntityTypeConfiguration<AccessPermission>
{
    public void Configure(EntityTypeBuilder<AccessPermission> builder)
    {
        builder.ToTable(DatabaseTables.AccessPermissions, DatabaseSchemas.Identity);

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(AccessPermissionConstraints.NameMaxLength);

        builder.HasIndex(p => p.Name).IsUnique();

        builder.Property(p => p.DisplayName)
            .HasMaxLength(AccessPermissionConstraints.DisplayNameMaxLength);

        builder.Property(p => p.Description);
    }
}
