using ReSys.Core.Domain.Settings.ShippingMethods;

namespace ReSys.Core.Features.Admin.Settings.ShippingMethods.Common;

public record ShippingMethodParameters
{
    public string Name { get; set; } = null!;
    public string? Presentation { get; set; }
    public string? Description { get; set; }
    public ShippingMethod.ShippingType Type { get; set; }
    public decimal BaseCost { get; set; }
    public int Position { get; set; }

    // Metadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

public record ShippingMethodInput : ShippingMethodParameters
{
}

public record ShippingMethodListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Presentation { get; set; }
    public decimal BaseCost { get; set; }
    public string Type { get; set; } = null!;
    public bool Active { get; set; }
    public int Position { get; set; }
}

public record ShippingMethodDetail : ShippingMethodInput
{
    public Guid Id { get; set; }
    public bool Active { get; set; }
}
