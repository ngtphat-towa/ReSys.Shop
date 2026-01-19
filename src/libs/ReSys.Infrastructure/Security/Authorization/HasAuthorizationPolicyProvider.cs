using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Security.Authorization.Claims;
using ReSys.Infrastructure.Security.Authorization.Requirements;

namespace ReSys.Infrastructure.Security.Authorization;

internal sealed class HasAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options = options.Value;

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => Task.FromResult(_options.DefaultPolicy);
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult(_options.FallbackPolicy);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName)) return Task.FromResult<AuthorizationPolicy?>(null);

        var existingPolicy = _options.GetPolicy(policyName);
        if (existingPolicy != null) return Task.FromResult<AuthorizationPolicy?>(existingPolicy);

        // Logic to parse "permission:catalog.view;role:admin" style strings
        var requirement = ParsePolicy(policyName);
        var policy = new AuthorizationPolicyBuilder().AddRequirements(requirement).Build();
        
        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    private static HasAuthorizeClaimRequirement ParsePolicy(string policyName)
    {
        var permissions = new List<string>();
        var roles = new List<string>();
        
        var parts = policyName.Split(';');
        foreach (var part in parts)
        {
            var kv = part.Split(':');
            if (kv.Length != 2) continue;
            
            var type = kv[0].ToLower();
            var values = kv[1].Split(',');

            if (type == CustomClaim.Permission) permissions.AddRange(values);
            else if (type == CustomClaim.Role) roles.AddRange(values);
        }

        return new HasAuthorizeClaimRequirement(permissions.ToArray(), null, roles.ToArray());
    }
}