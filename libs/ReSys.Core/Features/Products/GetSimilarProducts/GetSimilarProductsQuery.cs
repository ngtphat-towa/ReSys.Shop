using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Products.GetSimilarProducts;

public record GetSimilarProductsQuery(Guid Id) : IRequest<ErrorOr<List<Product>>>;

public class GetSimilarProductsQueryHandler : IRequestHandler<GetSimilarProductsQuery, ErrorOr<List<Product>>>
{
    private readonly IApplicationDbContext _context;

    public GetSimilarProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<List<Product>>> Handle(GetSimilarProductsQuery request, CancellationToken cancellationToken)
    {
        var productEmbedding = await _context.ProductEmbeddings
            .FirstOrDefaultAsync(pe => pe.ProductId == request.Id, cancellationToken);

        if (productEmbedding == null)
        {
            return Error.NotFound("Product.NotFound", "Product not found or has no embedding.");
        }

        var similarProducts = await _context.ProductEmbeddings
            .OrderBy(pe => pe.Embedding.L2Distance(productEmbedding.Embedding))
            .Where(pe => pe.ProductId != request.Id)
            .Take(5)
            .Select(pe => pe.Product)
            .ToListAsync(cancellationToken);

        return similarProducts;
    }
}
