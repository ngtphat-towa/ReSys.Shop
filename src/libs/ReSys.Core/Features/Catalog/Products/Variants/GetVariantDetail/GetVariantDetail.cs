using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Features.Catalog.Products.Variants.Common;

namespace ReSys.Core.Features.Catalog.Products.Variants.GetVariantDetail;

public static class GetVariantDetail
{
    public record Request(Guid Id);
    public record Response : VariantDetail;
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            var variant = await context.Set<Variant>()
                .Include(v => v.Product)
                .Include(v => v.OptionValues).ThenInclude(ov => ov.OptionType)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == query.Request.Id && !v.IsDeleted, cancellationToken);

            if (variant == null)
                return VariantErrors.NotFound(query.Request.Id);

            return variant.Adapt<Response>();
        }
    }
}
