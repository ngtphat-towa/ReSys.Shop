namespace ReSys.Core.Common.Security.Authentication.Externals.Models;

public record ExternalUserTransfer
{
    public string ProviderId { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public bool EmailVerified { get; init; }
    public string ProviderName { get; init; } = null!;
    public IReadOnlyDictionary<string, string> AdditionalClaims { get; init; } = new Dictionary<string, string>();
}
