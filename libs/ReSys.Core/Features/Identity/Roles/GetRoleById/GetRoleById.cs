using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Roles.GetRoleById;

public static class GetRoleById
{
    public record Query(string Id) : IRequest<ErrorOr<RoleResponse>>;

    public class Handler : IRequestHandler<Query, ErrorOr<RoleResponse>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public Handler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ErrorOr<RoleResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var role = await _roleManager.FindByIdAsync(request.Id);
            if (role == null) return IdentityErrors.RoleNotFound;

            var claims = await _roleManager.GetClaimsAsync(role);

            return new RoleResponse(
                role.Id,
                role.Name ?? "",
                claims.Where(c => c.Type == "permission").Select(c => c.Value)
            );
        }
    }
}
