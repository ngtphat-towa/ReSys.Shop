using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.DeleteOptionValue;

public static class DeleteOptionValue
{
    public record Command(Guid OptionTypeId, Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Fetch: option value by ID and OptionTypeId
            var optionValue = await context.Set<OptionValue>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.OptionTypeId == command.OptionTypeId, cancellationToken);

            // Check: found
            if (optionValue is null)
                return OptionValueErrors.NotFound(command.Id);

            // Business Rule: can delete
            var deleteResult = optionValue.Delete();
            if (deleteResult.IsError)
                return deleteResult.Errors;

            // Delete: from database
            context.Set<OptionValue>().Remove(optionValue);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
