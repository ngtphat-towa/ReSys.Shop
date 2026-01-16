using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReSys.Core.Domain.Location.Addresses;

namespace ReSys.Infrastructure.Persistence.Configurations.Location;

public static class AddressConfigurationHelper
{
    public static void ConfigureAddress<T>(OwnedNavigationBuilder<T, Address> builder) where T : class
    {
        builder.Property(p => p.Address1)
            .IsRequired()
            .HasMaxLength(AddressConstraints.Address1MaxLength);

        builder.Property(p => p.Address2)
            .HasMaxLength(AddressConstraints.Address2MaxLength);

        builder.Property(p => p.City)
            .IsRequired()
            .HasMaxLength(AddressConstraints.CityMaxLength);

        builder.Property(p => p.ZipCode)
            .IsRequired()
            .HasMaxLength(AddressConstraints.ZipCodeMaxLength);

        builder.Property(p => p.CountryCode)
            .IsRequired()
            .IsFixedLength()
            .HasMaxLength(AddressConstraints.CountryCodeLength);

        builder.Property(p => p.StateCode)
            .HasMaxLength(AddressConstraints.StateCodeMaxLength);

        builder.Property(p => p.FirstName)
            .HasMaxLength(AddressConstraints.NameMaxLength);

        builder.Property(p => p.LastName)
            .HasMaxLength(AddressConstraints.NameMaxLength);

        builder.Property(p => p.Company)
            .HasMaxLength(AddressConstraints.NameMaxLength);

        builder.Property(p => p.Phone)
            .HasMaxLength(AddressConstraints.PhoneMaxLength);

        builder.Ignore(p => p.FullName);
    }
}
