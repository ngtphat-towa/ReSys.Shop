using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions;
using ReSys.Core.Common.Models;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;

namespace ReSys.Core.Features.Testing.Examples.GetExamples;

public static class GetExamplesV2
{
    public record Request : IFilterOptions, ISortOptions, ISearchOptions, IPageOptions
    {
        public string? Filter { get; set; }
        public string? Sort { get; set; }
        public string? Search { get; set; }
        public string[]? SearchField { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }

        // Explicit interface implementation mapping if names differ
        string[]? ISearchOptions.SearchField => SearchField;

        public static implicit operator QueryOptions(Request r) => new()
        {
            Filter = r.Filter,
            Sort = r.Sort,
            Search = r.Search,
            SearchField = r.SearchField,
            Page = r.Page,
            PageSize = r.PageSize
        };
    }

    public record Query(Request Request) : IRequest<PagedList<ExampleListItem>>;

    public class Handler : IRequestHandler<Query, PagedList<ExampleListItem>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<ExampleListItem>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = _context.Set<Example>()
                .Include(x => x.Category)
                .AsNoTracking()
                .ApplyFilter(request)
                .ApplySearch(request);

            // Apply dynamic sort; if it returns the same query (meaning no valid sort was applied), fallback to default Name sort
            var sortedQuery = dbQuery.ApplySort(request);
            if (ReferenceEquals(sortedQuery, dbQuery))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Name);
            }

            return await sortedQuery.ApplyPagingAsync(request, ExampleListItem.Projection, cancellationToken);
        }
    }
}
