using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Catalog.OptionTypes.OptionValues.GetOptionValuesPagedList;

public static class GetOptionValuesPagedList
{
    public record Request : QueryOptions;
    public record Response : OptionValueModel;
    public record Query(Guid OptionTypeId, Request Request) : IRequest<PagedList<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            var request = query.Request;

            var dbQuery = context.Set<OptionValue>()
                .AsNoTracking()
                .Where(x => x.OptionTypeId == query.OptionTypeId)
                .ApplyFilter(request)
                .ApplySearch(request);

            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery
                    .OrderBy(x => x.Position)
                    .ThenBy(x => x.Name);
            }

            return await sortedQuery.ToPagedListAsync<OptionValue, Response>(
                request,
                ct);
        }
    }
}
