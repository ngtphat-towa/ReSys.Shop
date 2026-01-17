using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Identity.Roles;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity.Roles;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable(DatabaseTables.Roles, DatabaseSchemas.Identity);

        builder.Property(r => r.DisplayName)
            .HasMaxLength(RoleConstraints.DisplayNameMaxLength);

        builder.Property(r => r.Description)
            .HasMaxLength(RoleConstraints.DescriptionMaxLength);
    }
}
