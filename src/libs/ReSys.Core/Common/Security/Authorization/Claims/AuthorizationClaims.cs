using System.Text.Json.Serialization;

namespace ReSys.Core.Common.Security.Authorization.Claims;

public record AuthorizeClaimData(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("user_name")] string UserName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("permissions")] IReadOnlyList<string> Permissions,
    [property: JsonPropertyName("roles")] IReadOnlyList<string> Roles,
    [property: JsonPropertyName("policies")] IReadOnlyList<string> Policies
);

public interface IAuthorizeClaimDataProvider
{
    Task<AuthorizeClaimData?> GetUserAuthorizationAsync(string userId);
    Task InvalidateUserAuthorizationAsync(string userId);
}

public static class CustomClaim
{
    public const string Role = "role";
    public const string Permission = "permission";
    public const string Policy = "policy";
}