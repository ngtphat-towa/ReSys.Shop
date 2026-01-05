using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ReSys.Core.Common.Security;

namespace ReSys.Infrastructure.Identity;

public class WebUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WebUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public string? Name => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) 
                           ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("name");

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

    public string? GetClaim(string claimType) => _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);

    public IEnumerable<Claim> Claims => _httpContextAccessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
}
