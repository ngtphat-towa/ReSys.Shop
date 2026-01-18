using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Mapster;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Catalog.Products.Common;

namespace ReSys.Core.Features.Catalog.Products.GetProductDetail;

public static class GetProductDetail
{
    public record Request(Guid Id);
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
                .FirstOrDefaultAsync(x => x.Id == query.Request.Id, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(query.Request.Id);

            return product.Adapt<Response>();
        }
    }
}
