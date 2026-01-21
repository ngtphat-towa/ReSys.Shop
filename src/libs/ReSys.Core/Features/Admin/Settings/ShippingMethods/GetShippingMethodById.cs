using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.ShippingMethods;

namespace ReSys.Core.Features.Admin.Settings.ShippingMethods;

public static class GetShippingMethodById
{
    public record Query(Guid Id) : IRequest<ErrorOr<Response>>;

    public record Response(Guid Id, string Name, string Presentation, decimal BaseCost, string? Description, bool Active);

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken ct)
        {
            var method = await dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == query.Id, ct);

            if (method == null) return ShippingMethodErrors.NotFound(query.Id);

            return new Response(
                method.Id,
                method.Name,
                method.Presentation,
                method.BaseCost,
                method.Description,
                method.Status == ShippingMethod.ShippingStatus.Active
            );
        }
    }
}
