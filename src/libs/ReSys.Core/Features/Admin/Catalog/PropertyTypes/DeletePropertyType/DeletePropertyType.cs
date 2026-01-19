using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.PropertyTypes;

namespace ReSys.Core.Features.Admin.Catalog.PropertyTypes.DeletePropertyType;

public static class DeletePropertyType
{
    // Command:
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            // 1. Get: domain entity
            var propertyType = await context.Set<PropertyType>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (propertyType is null)
                return PropertyTypeErrors.NotFound(command.Id);

            // 2. Business Rule: entity-level checks (raises events)
            var deleteResult = propertyType.Delete();
            if (deleteResult.IsError)
                return deleteResult.Errors;

            // 3. Delete from database
            context.Set<PropertyType>().Remove(propertyType);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
