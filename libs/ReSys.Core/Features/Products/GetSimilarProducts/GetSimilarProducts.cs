using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Features.Products.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.GetSimilarProducts;

public static class GetSimilarProducts
{
    public class Request
    {
        public Guid Id { get; set; }
        public Request(Guid id) => Id = id;
    }

    public record Query(Request Request) : IRequest<ErrorOr<List<ProductListItem>>>;

    public class Handler : IRequestHandler<Query, ErrorOr<List<ProductListItem>>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<List<ProductListItem>>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var productEmbedding = await _context.Set<ProductEmbedding>()
                .AsNoTracking()
                .FirstOrDefaultAsync(pe => pe.ProductId == request.Id, cancellationToken);

            if (productEmbedding == null)
            {
                return Error.NotFound("Product.NotFound", "Product not found or has no embedding.");
            }

            var similarProducts = await _context.Set<ProductEmbedding>()
                .AsNoTracking()
                .OrderBy(pe => pe.Embedding.L2Distance(productEmbedding.Embedding))
                .Where(pe => pe.ProductId != request.Id)
                .Take(5)
                .Select(pe => pe.Product)
                .Select(ProductListItem.Projection)
                .ToListAsync(cancellationToken);

            return similarProducts;
        }
    }
}
