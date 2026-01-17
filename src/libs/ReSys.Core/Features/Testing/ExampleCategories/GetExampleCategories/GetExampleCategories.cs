using MediatR;

using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Mvc;

using ReSys.Core.Common.Data;
using ReSys.Shared.Models;
using ReSys.Core.Features.Testing.ExampleCategories.Common;
using ReSys.Core.Domain.Testing.ExampleCategories;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Shared.Models.Search;
using ReSys.Shared.Models.Filters;
using ReSys.Shared.Models.Pages;

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

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<ExampleCategoryListItem>>
    {
        public async Task<PagedList<ExampleCategoryListItem>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;

            var dbQuery = context.Set<ExampleCategory>()
                .AsNoTracking()
                .ApplyFilter(request)
                .ApplySearch(request);

            // Apply dynamic sort
            var sortedQuery = dbQuery.ApplySort(request);
            
            // If sort string was empty, ApplySort returns original query.
            // We only apply default sort if no sort was provided.
            if (string.IsNullOrWhiteSpace(request.Sort))
            {
                sortedQuery = dbQuery.OrderBy(x => x.Name);
            }

            return await sortedQuery.ApplyPagingAsync(request, ExampleCategoryListItem.Projection, cancellationToken);
        }
    }
}
