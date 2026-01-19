using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Features.Catalog.Products.Variants.Common;

namespace ReSys.Core.Features.Catalog.Products.Variants.CreateVariant;

public static class CreateVariant
{
    public record Request : VariantInput
    {
        public Guid ProductId { get; set; }
    }

    public record Response : VariantDetail;

    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.ProductId).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new VariantInputValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var product = await context.Set<Product>()
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            if (product == null)
                return ProductErrors.NotFound(request.ProductId);

            var variantResult = product.AddVariant(request.Sku ?? string.Empty, request.Price);
            if (variantResult.IsError)
                return variantResult.Errors;

            var variant = variantResult.Value;
            
            // Update other fields
            variant.Barcode = request.Barcode;
            
            var dimResult = variant.UpdateDimensions(request.Weight, request.Height, request.Width, request.Depth);
            if (dimResult.IsError) return dimResult.Errors;

            variant.CostPrice = request.CostPrice;
            variant.TrackInventory = request.TrackInventory;
            variant.Position = request.Position;
            variant.SetMetadata(request.PublicMetadata, request.PrivateMetadata);

            context.Set<Variant>().Add(variant);
            context.Set<Product>().Update(product);

            await context.SaveChangesAsync(cancellationToken);

            return await context.Set<Variant>()
                .AsNoTracking()
                .Where(x => x.Id == variant.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}