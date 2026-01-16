using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


using ReSys.Core.Domain.Identity.UserGroups;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity.UserGroups;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable(DatabaseTables.UserGroups, DatabaseSchemas.Identity);

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(UserGroupConstraints.NameMaxLength);

        builder.Property(g => g.Code)
            .IsRequired()
            .HasMaxLength(UserGroupConstraints.CodeMaxLength);

        builder.HasIndex(g => g.Code).IsUnique();

        builder.Property(g => g.Description)
            .HasMaxLength(UserGroupConstraints.DescriptionMaxLength);

        builder.Property(g => g.IsDefault);
        builder.Property(g => g.IsActive);

        // Relationships: Many-to-Many via Membership is configured in UserGroupMembershipConfiguration
    }
}
