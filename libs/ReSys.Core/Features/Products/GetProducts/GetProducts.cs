using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Extensions;
using ReSys.Core.Common.Models;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.GetProducts;

public static class GetProducts
{
    // Clean PascalCase properties. No attributes. No manual code.
    public record Request
    {
        public string? Search { get; set; }
        public string? Name { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }
        public string? SortBy { get; set; }
        public bool? IsDescending { get; set; }
        public int? Page { get; set; } 
        public int? PageSize { get; set; }
        public Guid[]? ProductId { get; set; }
    }

    public record Query(Request Request) : IRequest<PagedList<ProductListItem>>;

    public class Handler : IRequestHandler<Query, PagedList<ProductListItem>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedList<ProductListItem>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var dbQuery = _context.Set<Product>().AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchTerm = request.Search.ToLower();
                dbQuery = dbQuery.Where(x => x.Name.ToLower().Contains(searchTerm) 
                                          || x.Description.ToLower().Contains(searchTerm));
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

            if (request.CreatedFrom.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
            }

            if (request.CreatedTo.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.CreatedAt <= request.CreatedTo.Value);
            }

            if (request.ProductId != null && request.ProductId.Length > 0)
            {
                dbQuery = dbQuery.Where(x => request.ProductId.Contains(x.Id));
            }

            // Sort
            dbQuery = request.SortBy?.ToLower() switch
            {
                "price" => request.IsDescending == true ? dbQuery.OrderByDescending(x => x.Price) : dbQuery.OrderBy(x => x.Price),
                "created_at" => request.IsDescending == true ? dbQuery.OrderByDescending(x => x.CreatedAt) : dbQuery.OrderBy(x => x.CreatedAt),
                _ => request.IsDescending == true ? dbQuery.OrderByDescending(x => x.Name) : dbQuery.OrderBy(x => x.Name)
            };

            return await dbQuery.ToPagedListAsync(
                ProductListItem.Projection,
                request.Page,
                request.PageSize,
                cancellationToken);
        }
    }
}
