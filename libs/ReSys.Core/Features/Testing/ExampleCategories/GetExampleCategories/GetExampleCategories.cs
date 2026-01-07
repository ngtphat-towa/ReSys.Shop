using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions;
using ReSys.Core.Common.Models;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.ExampleCategories.Common;

namespace ReSys.Core.Features.Testing.ExampleCategories.GetExampleCategories;

public static class GetExampleCategories
{
    public record Request : IFilterOptions, ISortOptions, ISearchOptions, IPageOptions
    {
        public string? Filter { get; set; }
        public string? Sort { get; set; }
        public string? Search { get; set; }
        public string[]? SearchField { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }

    public record Query(Request Request) : IRequest<PagedList<ExampleCategoryListItem>>;

    public class Handler : IRequestHandler<Query, PagedList<ExampleCategoryListItem>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<ExampleCategoryListItem>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = _context.Set<ExampleCategory>()
                .AsNoTracking()
                .ApplyFilter(request)
                .ApplySearch(request);

            // Apply dynamic sort; if it returns the same query (meaning no valid sort was applied), fallback to default Name sort
            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Name);
            }

            return await sortedQuery.ApplyPagingAsync(request, ExampleCategoryListItem.Projection, cancellationToken);
        }
    }
}
