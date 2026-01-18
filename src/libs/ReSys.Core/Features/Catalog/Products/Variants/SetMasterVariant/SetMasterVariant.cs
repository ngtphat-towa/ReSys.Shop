using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Features.Catalog.Products.Variants.SetMasterVariant;

public static class SetMasterVariant
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var variant = await context.Set<Variant>()
                .Include(v => v.Product)
                .ThenInclude(p => p.Variants)
                .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken);

            if (variant == null)
                return VariantErrors.NotFound(command.Id);

            var result = variant.Product.SetMasterVariant(variant.Id);
            if (result.IsError)
                return result.Errors;

            context.Set<Product>().Update(variant.Product);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
