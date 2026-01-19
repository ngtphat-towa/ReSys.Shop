using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Domain.Identity.UserGroups;
using ReSys.Core.Features.Identity.Admin.UserGroups.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Identity.Admin.UserGroups.GetGroupsPagedList;

public static class GetGroupsPagedList
{
    public record Request : QueryOptions;
    public record Response : GroupResponse;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            return await context.Set<UserGroup>()
                .AsNoTracking()
                .Include(x => x.Memberships)
                .ApplyFilter(query.Request)
                .ApplySearch(query.Request)
                .ApplySort(query.Request)
                .ToPagedListAsync<UserGroup, Response>(query.Request, ct);
        }
    }
}