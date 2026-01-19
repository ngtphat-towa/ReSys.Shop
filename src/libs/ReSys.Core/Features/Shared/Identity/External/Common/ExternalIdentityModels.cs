namespace ReSys.Core.Features.Shared.Identity.External.Common;

public record ExternalProviderModel
{
    public string Provider { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string? IconUrl { get; init; }
    public bool IsEnabled { get; init; }
}

public record OAuthConfigResponse
{
    public string Provider { get; init; } = null!;
    public string ClientId { get; init; } = null!;
    public string AuthorizationUrl { get; init; } = null!;
    public string TokenUrl { get; init; } = null!;
    public string[] Scopes { get; init; } = [];
}

public record ExternalExchangeResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public bool IsNewUser { get; init; }
    public bool IsNewLogin { get; init; }
}
