using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Domain.Identity.Permissions;
using ReSys.Core.Features.Identity.Admin.Permissions.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Identity.Admin.Permissions.GetPermissionsPagedList;

public static class GetPermissionsPagedList
{
    public record Request : QueryOptions;
    public record Response : PermissionResponse;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            return await context.Set<AccessPermission>()
                .AsNoTracking()
                .ApplyFilter(query.Request)
                .ApplySearch(query.Request)
                .ApplySort(query.Request)
                .ToPagedListAsync<AccessPermission, Response>(query.Request, ct);
        }
    }
}