using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Features.Testing.Examples.Common;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Shared.Models.Pages;

namespace ReSys.Core.Features.Testing.Examples.GetExamples;

public static class GetExamples
{
    // Clean PascalCase properties. No attributes. No manual code.
    public record Request
    {
        public string? Search { get; set; }
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public ExampleStatus[]? Status { get; set; }
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }
        public string? SortBy { get; set; }
        public bool? IsDescending { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public Guid[]? ExampleId { get; set; }
    }

    public record Query(Request Request) : IRequest<PagedList<ExampleListItem>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, PagedList<ExampleListItem>>
    {
        public async Task<PagedList<ExampleListItem>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var dbQuery = context.Set<Example>().AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.ToLower();
                dbQuery = dbQuery.Where(x => x.Name.ToLower().Contains(searchTerm)
                                          || (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(searchTerm)));
            }

            // Filters
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var nameFilter = request.Name.ToLower();
                dbQuery = dbQuery.Where(x => x.Name.ToLower().Contains(nameFilter));
            }

            if (request.MinPrice.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Price >= request.MinPrice.Value);
            }

            if (request.MaxPrice.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Price <= request.MaxPrice.Value);
            }

            if (request.Status != null && request.Status.Length > 0)
            {
                dbQuery = dbQuery.Where(x => request.Status.Contains(x.Status));
            }

            if (request.CreatedFrom.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedAt <= request.CreatedTo.Value);
            }

            if (request.ExampleId != null && request.ExampleId.Length > 0)
            {
                dbQuery = dbQuery.Where(x => request.ExampleId.Contains(x.Id));
            }

            // Sort
            dbQuery = request.SortBy?.ToLower() switch
            {
                "price" => request.IsDescending == true ? dbQuery.OrderByDescending(x => x.Price) : dbQuery.OrderBy(x => x.Price),
                "created_at" => request.IsDescending == true ? dbQuery.OrderByDescending(x => x.CreatedAt) : dbQuery.OrderBy(x => x.CreatedAt),
                _ => request.IsDescending == true ? dbQuery.OrderByDescending(x => x.Name) : dbQuery.OrderBy(x => x.Name)
            };

            return await dbQuery.ToPagedListAsync(
                ExampleListItem.Projection,
                request.Page,
                request.PageSize,
                cancellationToken);
        }
    }
}
