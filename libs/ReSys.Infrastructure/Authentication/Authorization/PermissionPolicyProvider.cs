using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using ReSys.Core.Common.Attributes;

namespace ReSys.Infrastructure.Authentication.Authorization;

public class PermissionPolicyProvider(
    IOptions<AuthorizationOptions> options,
    IOptions<PermissionAuthorizationOptions> permissionOptions) 
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(RequirePermissionAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissions = policyName[RequirePermissionAttribute.PolicyPrefix.Length..].Split(',');
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(permissions));

            if (permissionOptions.Value.AuthenticationSchemes.Count > 0)
            {
                policy.AddAuthenticationSchemes(permissionOptions.Value.AuthenticationSchemes.ToArray());
            }

            return policy.Build();
        }

        return await base.GetPolicyAsync(policyName);
    }
}
