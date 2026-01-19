using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Identity.Permissions;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Shared.Identity.Admin.Permissions.GetPermissionSelectList;

public static class GetPermissionSelectList
{
    public record Response
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
    }

    public record Query(QueryOptions Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            return await context.Set<AccessPermission>()
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToPagedOrAllAsync<AccessPermission, Response>(query.Request, ct);
        }
    }
}
