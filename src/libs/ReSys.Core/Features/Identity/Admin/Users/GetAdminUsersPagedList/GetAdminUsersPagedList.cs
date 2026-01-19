using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Admin.Users.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Identity.Admin.Users.GetAdminUsersPagedList;

public static class GetAdminUsersPagedList
{
    public record Request : QueryOptions;
    public record Response : AdminUserListItem;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            return await userManager.Users
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToPagedListAsync<User, Response>(query.Request, ct);
        }
    }
}