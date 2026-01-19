using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;

namespace ReSys.Core.Features.Admin.Catalog.Products.DeleteProduct;

public static class DeleteProduct
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Set<Product>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(command.Id);

            var result = product.Delete();
            if (result.IsError) return result.Errors;
            
            context.Set<Product>().Update(product);
            await context.SaveChangesAsync(cancellationToken);

            return result.Value;
        }
    }
}
