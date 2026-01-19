using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;

namespace ReSys.Core.Features.Admin.Inventories.Locations.DeleteStockLocation;

public static class DeleteStockLocation
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            var location = await context.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (location == null)
                return StockLocationErrors.NotFound(command.Id);

            var result = location.Delete();
            if (result.IsError) return result.Errors;

            context.Set<StockLocation>().Update(location);
            await context.SaveChangesAsync(ct);

            return Result.Deleted;
        }
    }
}
