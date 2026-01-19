using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products;
using ReSys.Core.Features.Catalog.Products.Common;

namespace ReSys.Core.Features.Catalog.Products.CreateProduct;

public static class CreateProduct
{
    // Request:
    public record Request : ProductInput;

    // Response:
    public record Response : ProductDetail;

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new ProductInputValidator());
        }
    }

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: duplicate name
            if (await context.Set<Product>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return ProductErrors.DuplicateName;
            }

            // Create: domain entity
            var productResult = Product.Create(
                name: request.Name,
                sku: request.Sku,
                price: request.Price,
                slug: request.Slug,
                description: request.Description);

            if (productResult.IsError)
                return productResult.Errors;

            var product = productResult.Value;

            // Set: SEO
            var seoResult = product.UpdateSeo(request.MetaTitle, request.MetaDescription, request.MetaKeywords);
            if (seoResult.IsError) return seoResult.Errors;

            // Set: Dates
            product.AvailableOn = request.AvailableOn;
            product.DiscontinuedOn = request.DiscontinuedOn;
            product.MakeActiveAt = request.MakeActiveAt;

            // Set: metadata
            product.SetMetadata(request.PublicMetadata, request.PrivateMetadata);

            // Save: domain entity
            context.Set<Product>().Add(product);
            
            foreach(var variant in product.Variants)
            {
                context.Set<ReSys.Core.Domain.Catalog.Products.Variants.Variant>().Add(variant);
            }

            await context.SaveChangesAsync(cancellationToken);

            // Return: projected response
            return await context.Set<Product>()
                .AsNoTracking()
                .Where(x => x.Id == product.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}
