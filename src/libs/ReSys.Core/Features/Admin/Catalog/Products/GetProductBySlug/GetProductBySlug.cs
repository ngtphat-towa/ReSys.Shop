using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Admin.Catalog.Products.Common;

namespace ReSys.Core.Features.Admin.Catalog.Products.GetProductBySlug;

public static class GetProductBySlug
{
    public record Request(string Slug);
    public record Response : ProductDetail;
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var product = await context.Set<Product>()
                .Include(x => x.Variants)
                .Include(x => x.Images)
                .Include(x => x.OptionTypes)
                .Include(x => x.ProductProperties)
                .Include(x => x.Classifications)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Slug == query.Request.Slug, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(Guid.Empty); // Or a specific SlugNotFound if preferred

            return product.Adapt<Response>();
        }
    }
}
