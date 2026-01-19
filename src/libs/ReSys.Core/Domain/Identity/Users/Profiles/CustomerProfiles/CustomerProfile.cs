using ReSys.Core.Domain.Common.Abstractions;
using ErrorOr;

namespace ReSys.Core.Domain.Identity.Users.Profiles.CustomerProfiles;

/// <summary>
/// Represents extended metadata and preferences for a customer user.
/// </summary>
public sealed class CustomerProfile : Entity, IHasMetadata
{
    public string UserId { get; set; } = string.Empty;

    // Analytics (Read-only from outside the aggregate flow)
    public decimal LifetimeValue { get; set; }
    public int OrdersCount { get; set; }
    public DateTimeOffset? LastOrderAt { get; set; }

    // Preferences
    public bool AcceptsMarketing { get; set; }
    public string PreferredLocale { get; set; } = "en-US";
    public string PreferredCurrency { get; set; } = "USD";

    // Demographics
    public string? Gender { get; set; }

    // IHasMetadata
    public IDictionary<string, object?> PublicMetadata { get; set; } = new Dictionary<string, object?>();
    public IDictionary<string, object?> PrivateMetadata { get; set; } = new Dictionary<string, object?>();

    public CustomerProfile() { }

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

    /// <summary>
    /// Synchronizes financial metrics from the ordering system.
    /// </summary>
    public void UpdateMetrics(decimal orderTotal)
    {
        LifetimeValue += orderTotal;
        OrdersCount++;
        LastOrderAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the customer's communication and locale preferences.
    /// </summary>
    public void SetPreferences(bool acceptsMarketing, string? locale, string? currency)
    {
        AcceptsMarketing = acceptsMarketing;
        if (!string.IsNullOrWhiteSpace(locale)) PreferredLocale = locale;
        if (!string.IsNullOrWhiteSpace(currency)) PreferredCurrency = currency;
    }

    public void SetDemographics(string? gender)
    {
        Gender = gender;
    }

    public void SetMetadata(IDictionary<string, object?> publicMetadata, IDictionary<string, object?> privateMetadata)
    {
        PublicMetadata = publicMetadata;
        PrivateMetadata = privateMetadata;
    }
}