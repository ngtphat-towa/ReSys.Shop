using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Identity;

namespace ReSys.Core.Features.Identity.Roles.GetRoles;

public static class GetRoles
{
    public record Query : IRequest<ErrorOr<List<RoleDto>>>;
    
    public record RoleDto(string Id, string Name);

    public class Handler : IRequestHandler<Query, ErrorOr<List<RoleDto>>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public Handler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ErrorOr<List<RoleDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
            return roles.Select(r => new RoleDto(r.Id, r.Name ?? "")).ToList();
        }
    }
}
