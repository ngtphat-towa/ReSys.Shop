using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Core.Features.Admin.Settings.PaymentMethods.Common;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods;

public static class GetPaymentMethods
{
    public record Query : IRequest<ErrorOr<List<PaymentMethodListItem>>>;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Query, ErrorOr<List<PaymentMethodListItem>>>
    {
        public async Task<ErrorOr<List<PaymentMethodListItem>>> Handle(Query query, CancellationToken ct)
        {
            var methods = await dbContext.Set<PaymentMethod>()
                .AsNoTracking()
                .OrderBy(m => m.Position)
                .ToListAsync(ct);

            return methods.Select(m => new PaymentMethodListItem
            {
                Id = m.Id,
                Name = m.Name,
                Presentation = m.Presentation,
                Type = m.Type.ToString(),
                Active = m.Status == PaymentMethod.PaymentStatus.Active,
                Position = m.Position
            }).ToList();
        }
    }
}
