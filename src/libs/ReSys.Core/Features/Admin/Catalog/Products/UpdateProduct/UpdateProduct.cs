using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Admin.Catalog.Products.Common;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Features.Admin.Catalog.Products.UpdateProduct;

public static class UpdateProduct
{
    // Request:
    public record Request : ProductInput;

    // Response:
    public record Response : ProductDetail;

    // Command:
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    private class RequestValidator : ProductInputValidator { }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // 1. Get: domain entity
            var product = await context.Set<Product>()
                .Include(x => x.Variants)
                .Include(x => x.Images)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (product is null)
                return ProductErrors.NotFound(command.Id);

            // 2. Check: name conflict
            if (product.Name != request.Name && await context.Set<Product>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return ProductErrors.DuplicateName;
            }

            // 3. Update: domain entity details
            var updateResult = product.UpdateDetails(request.Name, request.Description);
            if (updateResult.IsError) return updateResult.Errors;

            if (!string.IsNullOrEmpty(request.Slug) && product.Slug != request.Slug)
            {
                var slugResult = product.SetSlug(request.Slug);
                if (slugResult.IsError) return slugResult.Errors;
            }

            var seoResult = product.UpdateSeo(request.MetaTitle, request.MetaDescription, request.MetaKeywords);
            if (seoResult.IsError) return seoResult.Errors;

            product.AvailableOn = request.AvailableOn;
            product.DiscontinuedOn = request.DiscontinuedOn;
            product.MakeActiveAt = request.MakeActiveAt;

            // Update Master Variant
            if (product.MasterVariant != null)
            {
                product.MasterVariant.Sku = request.Sku;
                var priceResult = product.MasterVariant.UpdatePricing(request.Price);
                if (priceResult.IsError) return priceResult.Errors;
                
                context.Set<Variant>().Update(product.MasterVariant);
            }

            // 4. Set: metadata
            product.SetMetadata(request.PublicMetadata, request.PrivateMetadata);

            // 5. Save
            context.Set<Product>().Update(product);
            await context.SaveChangesAsync(cancellationToken);

            // 6. Return: projected response
            return await context.Set<Product>()
                .AsNoTracking()
                .Where(x => x.Id == product.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}
