using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Features.Catalog.Products.Variants.DeleteVariant;

public static class DeleteVariant
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var variant = await context.Set<Variant>()
                .Include(v => v.Product)
                .ThenInclude(p => p.Variants)
                .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken);

            if (variant == null)
                return VariantErrors.NotFound(command.Id);

            var product = variant.Product;
            var result = product.RemoveVariant(variant.Id);
            if (result.IsError)
                return result.Errors;

            context.Set<Variant>().Update(variant);
            context.Set<Product>().Update(product);
            
            await context.SaveChangesAsync(cancellationToken);

            return result.Value;
        }
    }
}
