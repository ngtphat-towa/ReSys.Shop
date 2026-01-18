using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes;

namespace ReSys.Core.Features.Catalog.OptionTypes.DeleteOptionType;

public static class DeleteOptionType
{
    // Command:
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: option type exists (Include values to check business rules)
            var optionType = await context.Set<OptionType>()
                .Include(ot => ot.OptionValues)
                .FirstOrDefaultAsync(ot => ot.Id == command.Id, cancellationToken);

            if (optionType is null)
                return OptionTypeErrors.NotFound(command.Id);

            // Business Rule: cannot delete if has associated 
            var deleteResult = optionType.Delete();
            if (deleteResult.IsError)
                return deleteResult.Errors;

            // Delete: from database
            context.Set<OptionType>().Remove(optionType);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}