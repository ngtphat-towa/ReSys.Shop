using ReSys.Core.Domain.Settings.Stores;

namespace ReSys.Core.Features.Admin.Settings.Stores.Common;

public record StoreParameters
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string DefaultCurrency { get; set; } = "USD";
    public bool PricesIncludeTax { get; set; }
    public string DefaultWeightUnit { get; set; } = "kg";
    public Guid? DefaultStockLocationId { get; set; }

    // Metadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

public record StoreInput : StoreParameters { }

public record StoreListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string DefaultCurrency { get; set; } = null!;
}

public record StoreDetail : StoreInput
{
    public Guid Id { get; set; }
}
