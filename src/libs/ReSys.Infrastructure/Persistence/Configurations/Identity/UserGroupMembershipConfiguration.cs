using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Identity.UserGroups.UserGroupMemberships;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity.UserGroups;

public class UserGroupMembershipConfiguration : IEntityTypeConfiguration<UserGroupMembership>
{
    public void Configure(EntityTypeBuilder<UserGroupMembership> builder)
    {
        builder.ToTable(DatabaseTables.UserGroupMemberships, DatabaseSchemas.Identity);

        // Composite Key
        builder.HasKey(m => new { m.UserId, m.UserGroupId });

        builder.Property(m => m.JoinedAt).IsRequired();

        builder.Property(m => m.AssignedBy)
            .HasMaxLength(UserGroupMembershipConstraints.AssignedByMaxLength);

        builder.Property(m => m.IsPrimary);

        // Relationships
        builder.HasOne(m => m.User)
            .WithMany(u => u.GroupMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Group)
            .WithMany(g => g.Memberships)
            .HasForeignKey(m => m.UserGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
