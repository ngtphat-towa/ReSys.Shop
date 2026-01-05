using System.Security.Claims;
using ReSys.Core.Common.Security;

namespace ReSys.Infrastructure.Identity;

public class SystemUser : ICurrentUser
{
    public string? Id => "system";
    public string? Name => "System";
    public string? Email => "system@resys.shop";
    public bool IsAuthenticated => true;

    public bool IsInRole(string role) => true;
    public string? GetClaim(string claimType) => null;
    public IEnumerable<Claim> Claims => Enumerable.Empty<Claim>();
}