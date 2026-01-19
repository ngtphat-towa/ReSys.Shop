using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Images;

namespace ReSys.Core.Features.Admin.Catalog.Products.Images.RemoveProductImage;

public static class RemoveProductImage
{
    public record Command(Guid ProductId, Guid ImageId) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Set<Product>()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(command.ProductId);

            var existingImage = product.Images.FirstOrDefault(i => i.Id == command.ImageId);
            if (existingImage == null)
                return Error.NotFound("Product.ImageNotFound", "Product image not found.");

            var result = product.RemoveImage(command.ImageId);
            if (result.IsError) return result.Errors;

            context.Set<ProductImage>().Remove(existingImage);
            context.Set<Product>().Update(product);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
