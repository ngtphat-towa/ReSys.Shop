using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;
using ReSys.Core.Domain.Identity.Users.Profiles.StaffProfiles;
using ReSys.Shared.Constants;
using ReSys.Infrastructure.Persistence.Extensions;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(DatabaseTables.Users, DatabaseSchemas.Identity);

        // Standard Identity property constraints
        builder.Property(u => u.Email).HasMaxLength(UserConstraints.EmailMaxLength);
        builder.Property(u => u.UserName).HasMaxLength(UserConstraints.UserNameMaxLength);
        builder.Property(u => u.PhoneNumber).HasMaxLength(UserConstraints.PhoneMaxLength);

        // Domain-specific property constraints
        builder.Property(u => u.FirstName).HasMaxLength(UserConstraints.FirstNameMaxLength);
        builder.Property(u => u.LastName).HasMaxLength(UserConstraints.LastNameMaxLength);
        builder.Property(u => u.ProfileImagePath).HasMaxLength(UserConstraints.ProfileImageMaxLength);
        builder.Property(u => u.LastIpAddress).HasMaxLength(UserConstraints.IpAddressMaxLength);
        builder.Property(u => u.CurrentSignInIp).HasMaxLength(UserConstraints.IpAddressMaxLength);

        // Optimistic Concurrency
        builder.Property(u => u.Version).IsRowVersion();

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

        // Refresh Tokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditable();
        builder.ConfigureMetadata();
    }
}
