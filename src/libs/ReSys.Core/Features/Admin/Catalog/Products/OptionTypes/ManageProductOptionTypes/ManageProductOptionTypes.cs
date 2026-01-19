using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Domain.Catalog.Products;

namespace ReSys.Core.Features.Admin.Catalog.Products.OptionTypes.ManageProductOptionTypes;

public static class ManageProductOptionTypes
{
    public record Request(List<Guid> OptionTypeIds);
    public record Command(Guid ProductId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var product = await context.Set<Product>()
                .Include(p => p.OptionTypes)
                .FirstOrDefaultAsync(p => p.Id == command.ProductId, cancellationToken);

            if (product == null)
                return ProductErrors.NotFound(command.ProductId);

            var requestedIds = command.Request.OptionTypeIds.ToHashSet();
            var existingOptionTypes = product.OptionTypes.ToList();
            var changesMade = false;

            // 1. Process Removals
            foreach (var existing in existingOptionTypes)
            {
                if (!requestedIds.Contains(existing.Id))
                {
                    var removeResult = product.RemoveOptionType(existing.Id);
                    if (removeResult.IsError) return removeResult.Errors;
                    changesMade = true;
                }
            }

            // 2. Process Adds
            var idsToAdd = requestedIds.Where(id => !existingOptionTypes.Any(e => e.Id == id)).ToList();
            if (idsToAdd.Any())
            {
                var optionTypesToAdd = await context.Set<OptionType>()
                    .Where(ot => idsToAdd.Contains(ot.Id))
                    .ToListAsync(cancellationToken);

                foreach (var ot in optionTypesToAdd)
                {
                    var addResult = product.AddOptionType(ot);
                    if (addResult.IsError) return addResult.Errors;
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                context.Set<Product>().Update(product);
                await context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success;
        }
    }
}
