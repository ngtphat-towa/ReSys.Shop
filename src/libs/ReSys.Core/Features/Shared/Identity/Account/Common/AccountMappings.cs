using Mapster;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.UserAddresses;

namespace ReSys.Core.Features.Shared.Identity.Account.Common;

public class AccountMappings : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 1. UserAddress -> AccountAddressResponse (Flattening)
        config.NewConfig<UserAddress, AccountAddressResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Label, src => src.Label)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.IsDefault, src => src.IsDefault)
            .Map(dest => dest.IsVerified, src => src.IsVerified)
            .Map(dest => dest.Address1, src => src.Address.Address1)
            .Map(dest => dest.Address2, src => src.Address.Address2)
            .Map(dest => dest.City, src => src.Address.City)
            .Map(dest => dest.ZipCode, src => src.Address.ZipCode)
            .Map(dest => dest.CountryCode, src => src.Address.CountryCode)
            .Map(dest => dest.FirstName, src => src.Address.FirstName)
            .Map(dest => dest.LastName, src => src.Address.LastName)
            .Map(dest => dest.Company, src => src.Address.Company)
            .Map(dest => dest.Phone, src => src.Address.Phone)
            .Map(dest => dest.StateCode, src => src.Address.StateCode);

        // 2. UserAddress -> AddressSelectListItem
        config.NewConfig<UserAddress, AddressSelectListItem>()
            .Map(dest => dest.FullAddress, src => $"{src.Address.Address1}, {src.Address.City}");

        // 3. User -> FullProfileResponse (Merging Profiles)
        config.NewConfig<User, FullProfileResponse>()
            .Map(dest => dest.FullName, src => src.FullName)
            .Map(dest => dest.AcceptsMarketing, src => src.CustomerProfile != null && src.CustomerProfile.AcceptsMarketing)
            .Map(dest => dest.PreferredLocale, src => src.CustomerProfile != null ? src.CustomerProfile.PreferredLocale : null)
            .Map(dest => dest.PreferredCurrency, src => src.CustomerProfile != null ? src.CustomerProfile.PreferredCurrency : null)
            .Map(dest => dest.JobTitle, src => src.StaffProfile != null ? src.StaffProfile.JobTitle : null)
            .Map(dest => dest.Department, src => src.StaffProfile != null ? src.StaffProfile.Department : null);
    }
}
