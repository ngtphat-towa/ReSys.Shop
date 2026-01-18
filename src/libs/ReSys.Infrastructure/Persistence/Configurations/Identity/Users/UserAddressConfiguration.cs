using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Infrastructure.Persistence.Configurations.Location;
using ReSys.Shared.Constants;

namespace ReSys.Infrastructure.Persistence.Configurations.Identity.Users;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable(DatabaseTables.UserAddresses, DatabaseSchemas.Identity);

        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.Label)
            .IsRequired()
            .HasMaxLength(UserAddressConstraints.LabelMaxLength);

        builder.Property(ua => ua.Type)
            .HasConversion<string>();

        builder.Property(ua => ua.IsDefault);
        builder.Property(ua => ua.IsVerified);

        // Address Value Object (Owned Entity)
        builder.OwnsOne(ua => ua.Address, AddressConfigurationHelper.ConfigureAddress);

        // Relationships
        builder.HasOne<Core.Domain.Identity.Users.User>()
            .WithMany()
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}