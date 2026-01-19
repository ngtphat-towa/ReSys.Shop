using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Identity.Roles;
using ReSys.Core.Features.Identity.Admin.Roles.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Identity.Admin.Roles.GetRolesPagedList;

public static class GetRolesPagedList
{
    public record Request : QueryOptions;
    public record Response : RoleResponse;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(RoleManager<Role> roleManager) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            return await roleManager.Roles
                .AsNoTracking()
                .OrderBy(r => r.Priority)
                .ToPagedListAsync<Role, Response>(query.Request, ct);
        }
    }
}