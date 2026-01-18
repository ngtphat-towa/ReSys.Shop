using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Features.Catalog.Products.Variants.DiscontinueVariant;

public static class DiscontinueVariant
{
    public record Command(Guid Id) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var variant = await context.Set<Variant>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);

            if (variant == null)
                return VariantErrors.NotFound(command.Id);

            var result = variant.Discontinue();
            if (result.IsError) return result.Errors;

            context.Set<Variant>().Update(variant);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
