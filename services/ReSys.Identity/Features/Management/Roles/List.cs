using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ReSys.Identity.Features.Management.Roles;

public static class List
{
    public record RoleDto(string Id, string Name);
    public record Query() : IRequest<ErrorOr<List<RoleDto>>>;

    public class Handler(RoleManager<IdentityRole> roleManager) : IRequestHandler<Query, ErrorOr<List<RoleDto>>>
    {
        public async Task<ErrorOr<List<RoleDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var roles = await roleManager.Roles
                .Select(r => new RoleDto(r.Id, r.Name!))
                .ToListAsync(cancellationToken);
            
            return roles;
        }
    }
}
