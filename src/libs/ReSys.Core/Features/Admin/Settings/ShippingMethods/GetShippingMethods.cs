using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.ShippingMethods;

namespace ReSys.Core.Features.Admin.Settings.ShippingMethods;

public static class GetShippingMethods
{
    public record Query : IRequest<ErrorOr<List<Result>>>;

    public record Result(Guid Id, string Name, string Presentation, decimal BaseCost, bool Active);

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Query, ErrorOr<List<Result>>>
    {
        public async Task<ErrorOr<List<Result>>> Handle(Query query, CancellationToken ct)
        {
            var methods = await dbContext.Set<ShippingMethod>()
                .AsNoTracking()
                .OrderBy(m => m.Position)
                .ToListAsync(ct);

            return methods.Select(m => new Result(
                m.Id,
                m.Name,
                m.Presentation,
                m.BaseCost,
                m.Status == ShippingMethod.ShippingStatus.Active
            )).ToList();
        }
    }
}
