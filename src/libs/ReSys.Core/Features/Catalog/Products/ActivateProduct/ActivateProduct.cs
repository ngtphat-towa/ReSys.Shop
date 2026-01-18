using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;

namespace ReSys.Core.Features.Catalog.Products.ActivateProduct;

public static class ActivateProduct
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Set<Product>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(command.Id);

            var result = product.Activate();
            if (result.IsError) return result.Errors;

            context.Set<Product>().Update(product);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
