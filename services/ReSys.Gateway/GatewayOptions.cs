using System.ComponentModel.DataAnnotations;

namespace ReSys.Gateway;

public class GatewayOptions
{
    public const string SectionName = "Gateway";

    public bool EnableRateLimiting { get; set; }

    [Range(1, 10000)]
    public int MaxRequestsPerMinute { get; set; } = 100;

    public bool EnableRequestLogging { get; set; }

    public Dictionary<string, string> CustomHeaders { get; set; } = new();
}

public class ServiceEndpoints
{
    public const string SectionName = "ServiceEndpoints";

    [Required, Url]
    public string ApiUrl { get; set; } = string.Empty;

    [Required, Url]
    public string MlUrl { get; set; } = string.Empty;

    [Required, Url]
    public string ShopUrl { get; set; } = string.Empty;

    [Required, Url]
    public string AdminUrl { get; set; } = string.Empty;
}