using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products.Variants;
using ReSys.Core.Features.Catalog.Products.Variants.Common;

namespace ReSys.Core.Features.Catalog.Products.Variants.UpdateVariant;

public static class UpdateVariant
{
    public record Request : VariantInput;

    public record Response : VariantDetail;

    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new VariantInputValidator());
        }
    }

        public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>

        {

            public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)

            {

                var request = command.Request;

    

                var variant = await context.Set<Variant>()

                    .Include(v => v.Product)

                    .Include(v => v.OptionValues).ThenInclude(ov => ov.OptionType)

                    .FirstOrDefaultAsync(v => v.Id == command.Id, cancellationToken);

    

                if (variant == null)

                    return VariantErrors.NotFound(command.Id);

    

                // Update fields

                variant.Sku = request.Sku;

                variant.Barcode = request.Barcode;

                

                var priceResult = variant.UpdatePricing(request.Price, request.CompareAtPrice);

                if (priceResult.IsError) return priceResult.Errors;

    

                var dimResult = variant.UpdateDimensions(request.Weight, request.Height, request.Width, request.Depth);

                if (dimResult.IsError) return dimResult.Errors;

    

                            variant.CostPrice = request.CostPrice;

    

                            variant.TrackInventory = request.TrackInventory;

    

                            variant.Position = request.Position;

    

                            variant.SetMetadata(request.PublicMetadata, request.PrivateMetadata);

    

                

    

                            context.Set<Variant>().Update(variant);

                await context.SaveChangesAsync(cancellationToken);

    

                return await context.Set<Variant>()

                    .AsNoTracking()

                    .Where(x => x.Id == variant.Id)

                    .ProjectToType<Response>()

                    .FirstAsync(cancellationToken);

            }

        }
}
