using System.Security.Claims;

namespace ReSys.Core.Common.Security;

public interface ICurrentUser
{
    string? Id { get; }
    string? Name { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    
    bool IsInRole(string role);
    string? GetClaim(string claimType);
    IEnumerable<Claim> Claims { get; }
}