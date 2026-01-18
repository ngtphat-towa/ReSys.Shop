using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Features.Inventories.Locations.Common;

public record AddressInput
{
    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string City { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string CountryCode { get; set; } = null!; // ISO 2-letter
    public string? StateCode { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
}

public record StockLocationParameters
{
    public string Name { get; set; } = null!;
    public string? Presentation { get; set; }
    public string Code { get; set; } = null!;
    public bool IsDefault { get; set; }
    public StockLocationType Type { get; set; }
}

public record StockLocationInput : StockLocationParameters
{
    public AddressInput Address { get; set; } = null!;
}

public record StockLocationListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public bool Active { get; set; }
    public bool IsDefault { get; set; }
    public string Type { get; set; } = null!;
    public string City { get; set; } = null!;
    public string CountryCode { get; set; } = null!;
}

public record StockLocationDetail : StockLocationInput
{
    public Guid Id { get; set; }
    public bool Active { get; set; }
}