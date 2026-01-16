using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;
using ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity.Users;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(DatabaseTables.Users, DatabaseSchemas.Identity);

        // Properties
        builder.Property(u => u.FirstName)
            .HasMaxLength(UserConstraints.FirstNameMaxLength);

        builder.Property(u => u.LastName)
            .HasMaxLength(UserConstraints.LastNameMaxLength);

        // Relationships: Customer Profile (1:0..1)
        builder.OwnsOne(u => u.CustomerProfile, profile =>
        {
            profile.ToTable(DatabaseTables.CustomerProfiles, DatabaseSchemas.Identity);
            profile.WithOwner().HasForeignKey(p => p.UserId);
            profile.HasKey(p => p.UserId);

            profile.Property(p => p.LifetimeValue).HasColumnType("decimal(18,2)");
            profile.Property(p => p.PreferredLocale)
                .HasMaxLength(CustomerProfileConstraints.PreferredLocaleMaxLength)
                .HasDefaultValue("en-US");
            profile.Property(p => p.PreferredCurrency)
                .HasMaxLength(CustomerProfileConstraints.PreferredCurrencyMaxLength)
                .HasDefaultValue("USD");
        });

        // Relationships: Staff Profile (1:0..1)
        builder.OwnsOne(u => u.StaffProfile, profile =>
        {
            profile.ToTable(DatabaseTables.StaffProfiles, DatabaseSchemas.Identity);
            profile.WithOwner().HasForeignKey(p => p.UserId);
            profile.HasKey(p => p.UserId);

            profile.Property(p => p.JobTitle).HasMaxLength(StaffProfileConstraints.JobTitleMaxLength);
            profile.Property(p => p.Department).HasMaxLength(StaffProfileConstraints.DepartmentMaxLength);
            profile.Property(p => p.EmployeeId).HasMaxLength(StaffProfileConstraints.EmployeeIdMaxLength);
        });
    }
}
