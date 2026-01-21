using ReSys.Core.Domain.Identity.Users.UserAddresses;

namespace ReSys.Core.Features.Shared.Identity.Account.Common;

// 1. Address Models
public record AddressInput
{
    public string Label { get; init; } = "Default";
    public AddressType Type { get; init; } = AddressType.Both;
    public bool IsDefault { get; init; }

    public string Address1 { get; init; } = null!;
    public string? Address2 { get; init; }
    public string City { get; init; } = null!;
    public string ZipCode { get; init; } = null!;
    public string CountryCode { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Company { get; init; }
    public string? Phone { get; init; }
    public string? StateCode { get; init; }
}

public record AccountAddressResponse
{
    public Guid Id { get; init; }
    public string Label { get; init; } = null!;
    public AddressType Type { get; init; }
    public bool IsDefault { get; init; }
    public bool IsVerified { get; init; }

    public string Address1 { get; init; } = null!;
    public string? Address2 { get; init; }
    public string City { get; init; } = null!;
    public string ZipCode { get; init; } = null!;
    public string CountryCode { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Company { get; init; }
    public string? Phone { get; init; }
    public string? StateCode { get; init; }
}

public record AddressSelectListItem
{
    public Guid Id { get; init; }
    public string Label { get; init; } = null!;
    public string FullAddress { get; init; } = null!;
    public bool IsDefault { get; init; }
}

// 2. Profile Models
public record FullProfileResponse
{
    public string Id { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? UserName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? FullName { get; init; }
    public DateTimeOffset? DateOfBirth { get; init; }
    public string? ProfileImagePath { get; init; }

    // Customer Info
    public bool AcceptsMarketing { get; init; }
    public string? PreferredLocale { get; init; }
    public string? PreferredCurrency { get; init; }

    // Staff Info
    public string? JobTitle { get; init; }
    public string? Department { get; init; }
}
