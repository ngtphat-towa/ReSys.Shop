using ReSys.Core.Domain.Settings.PaymentMethods;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods.Common;

public record PaymentMethodParameters
{
    public string Name { get; set; } = null!;
    public string? Presentation { get; set; }
    public string? Description { get; set; }
    public PaymentMethod.PaymentType Type { get; set; }
    public bool AutoCapture { get; set; }
    public int Position { get; set; }

    // Metadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();
}

public record PaymentMethodInput : PaymentMethodParameters
{
}

public record PaymentMethodListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Presentation { get; set; }
    public string Type { get; set; } = null!;
    public bool Active { get; set; }
    public int Position { get; set; }
}

public record PaymentMethodDetail : PaymentMethodInput
{
    public Guid Id { get; set; }
    public bool Active { get; set; }
}
