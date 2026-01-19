using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;
using ReSys.Core.Domain.Catalog.Products.Variants;

namespace ReSys.Core.Features.Admin.Catalog.Products.Variants.ManageVariantOptionValues;

public static class ManageVariantOptionValues
{
    public record Request(List<Guid> OptionValueIds);
    public record Command(Guid VariantId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var variant = await context.Set<Variant>()
                .Include(v => v.OptionValues)
                .FirstOrDefaultAsync(v => v.Id == command.VariantId && !v.IsDeleted, cancellationToken);

            if (variant == null)
                return VariantErrors.NotFound(command.VariantId);

            if (variant.IsMaster)
                return VariantErrors.MasterCannotHaveOptions;

            var requestedIds = command.Request.OptionValueIds.ToHashSet();
            var existingIds = variant.OptionValues.Select(ov => ov.Id).ToList();
            var changesMade = false;

            // Remove
            foreach (var id in existingIds)
            {
                if (!requestedIds.Contains(id))
                {
                    var removeResult = variant.RemoveOptionValue(id);
                    if (removeResult.IsError) return removeResult.Errors;
                    changesMade = true;
                }
            }

            // Add
            var idsToAdd = requestedIds.Where(id => !existingIds.Contains(id)).ToList();
            if (idsToAdd.Any())
            {
                var valuesToAdd = await context.Set<OptionValue>()
                    .Where(ov => idsToAdd.Contains(ov.Id))
                    .ToListAsync(cancellationToken);

                foreach (var val in valuesToAdd)
                {
                    var addResult = variant.AddOptionValue(val);
                    if (addResult.IsError) return addResult.Errors;
                    changesMade = true;
                }
            }

            if (changesMade)
            {
                context.Set<Variant>().Update(variant);
                await context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success;
        }
    }
}
