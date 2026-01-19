using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Shared.Identity.Admin.Users.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Admin.Roles.GetUsersInRole;

public static class GetUsersInRole
{
    public record Request : QueryOptions;
    public record Response : AdminUserListItem;
    public record Query(string RoleName, Request Options) : IRequest<ErrorOr<PagedList<Response>>>;

    public class Handler(UserManager<User> userManager, RoleManager<Domain.Identity.Roles.Role> roleManager) 
        : IRequestHandler<Query, ErrorOr<PagedList<Response>>>
    {
        public async Task<ErrorOr<PagedList<Response>>> Handle(Query query, CancellationToken ct)
        {
            if (!await roleManager.RoleExistsAsync(query.RoleName))
                return ReSys.Core.Domain.Identity.Roles.RoleErrors.NotFound;

            // Get users in role (Identity Framework abstraction)
            var usersInRole = await userManager.GetUsersInRoleAsync(query.RoleName);
            
            // Apply pagination to the in-memory list (since Identity doesn't provide a paged IQueryable for this)
            var paged = await usersInRole
                .AsQueryable()
                .OrderBy(u => u.Email)
                .ToPagedListAsync<User, Response>(query.Options, ct);

            return paged;
        }
    }
}
