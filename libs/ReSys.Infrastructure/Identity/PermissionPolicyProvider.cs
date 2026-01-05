using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ReSys.Infrastructure.Identity;

public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Check if the policy name looks like a permission (e.g. "Permissions.")
        if (policyName.StartsWith("Permissions", StringComparison.OrdinalIgnoreCase))
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return policy.Build();
        }

        // Fallback to default behavior (e.g. [Authorize(Roles="Admin")])
        return await base.GetPolicyAsync(policyName);
    }
}
