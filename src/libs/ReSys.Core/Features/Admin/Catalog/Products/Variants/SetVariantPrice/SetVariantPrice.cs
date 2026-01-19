using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Features.Admin.Catalog.Products.Variants.SetVariantPrice;

public static class SetVariantPrice
{
    public record Request(decimal Price, decimal? CompareAtPrice);
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var variant = await context.Set<Variant>()
                .FirstOrDefaultAsync(v => v.Id == command.Id && !v.IsDeleted, cancellationToken);

            if (variant == null)
                return VariantErrors.NotFound(command.Id);

            var result = variant.UpdatePricing(command.Request.Price, command.Request.CompareAtPrice);
            if (result.IsError)
                return result.Errors;

            context.Set<Variant>().Update(variant);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
