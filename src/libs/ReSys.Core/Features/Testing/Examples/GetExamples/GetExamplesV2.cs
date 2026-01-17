using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Shared.Models.Query;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Testing.Examples.GetExamples;

public static class GetExamplesV2
{
    public record Request : QueryOptions;

    public record Query(Request Request) : IRequest<PagedList<ExampleListItem>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<ExampleListItem>>
    {
        public async Task<PagedList<ExampleListItem>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<Example>()
                .Include(x => x.Category)
                .AsNoTracking()
                .ApplyFilter(request)
                .ApplySearch(request);

            // Apply dynamic sort
            var sortedQuery = dbQuery.ApplySort(request);
            
            if (string.IsNullOrWhiteSpace(request.Sort))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Name);
            }

            return await sortedQuery.ApplyPagingAsync(request, ExampleListItem.Projection, cancellationToken);
        }
    }
}
