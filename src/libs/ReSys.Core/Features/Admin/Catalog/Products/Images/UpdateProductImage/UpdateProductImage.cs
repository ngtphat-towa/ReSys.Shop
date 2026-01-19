using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Images;
using ReSys.Core.Features.Admin.Catalog.Products.Images.Common;

namespace ReSys.Core.Features.Admin.Catalog.Products.Images.UpdateProductImage;

public static class UpdateProductImage
{
    public record Request : ProductImageParameters
    {
        public Guid ProductId { get; set; }
    }

    public record Response : ProductImageListItem;

    public record Command(Guid ImageId, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.ProductId).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new ProductImageValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var product = await context.Set<Product>()
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(request.ProductId);

            var existingImage = product.Images.FirstOrDefault(i => i.Id == command.ImageId);
            if (existingImage == null)
                return Error.NotFound("Product.ImageNotFound", "Product image not found.");

            var updateResult = product.UpdateImage(
                command.ImageId, 
                request.Alt, 
                request.Position, 
                request.Role);

            if (updateResult.IsError) return updateResult.Errors;

            context.Set<ProductImage>().Update(existingImage);
            context.Set<Product>().Update(product);

            await context.SaveChangesAsync(cancellationToken);

            return existingImage.Adapt<Response>();
        }
    }
}
