using ReSys.Core.Domain.Common.Abstractions;

namespace ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;

public sealed class CustomerProfile : Entity, IHasMetadata
{
    public string UserId { get; set; } = string.Empty;

    // Analytics (Cached for performance)
    public decimal LifetimeValue { get; set; }
    public int OrdersCount { get; set; }
    public DateTimeOffset? LastOrderAt { get; set; }

    // Preferences
    public bool AcceptsMarketing { get; set; }
    public string PreferredLocale { get; set; } = "en-US";
    public string PreferredCurrency { get; set; } = "USD";

    // Identity/Marketing Data
    public string? Gender { get; set; } // "male", "female", "unisex"

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    private CustomerProfile() { }

    internal static CustomerProfile Create(string userId)
    {
        return new CustomerProfile
        {
            UserId = userId,
            AcceptsMarketing = false,
            LifetimeValue = 0,
            OrdersCount = 0
        };
    }

    public void UpdateMetrics(decimal orderTotal)
    {
        LifetimeValue += orderTotal;
        OrdersCount++;
        LastOrderAt = DateTimeOffset.UtcNow;
    }
}