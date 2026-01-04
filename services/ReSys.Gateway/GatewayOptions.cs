using System.ComponentModel.DataAnnotations;

namespace ReSys.Gateway;

public sealed record GatewayOptions
{
    public const string SectionName = "Gateway";

    public bool EnableRateLimiting { get; init; }

    [Range(1, 10000)]
    public int MaxRequestsPerMinute { get; init; } = 100;

    public bool EnableRequestLogging { get; init; }

    public Dictionary<string, string> CustomHeaders { get; init; } = new();
}

public sealed record ServiceEndpoints
{
    public const string SectionName = "ServiceEndpoints";

    [Required, Url]
    public string ApiUrl { get; init; } = string.Empty;

    [Required, Url]
    public string MlUrl { get; init; } = string.Empty;

    [Required, Url]
    public string ShopUrl { get; init; } = string.Empty;

    [Required, Url]
    public string AdminUrl { get; init; } = string.Empty;
}
