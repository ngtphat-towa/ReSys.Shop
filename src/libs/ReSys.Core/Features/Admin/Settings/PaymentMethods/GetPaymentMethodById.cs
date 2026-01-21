using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.PaymentMethods;
using ReSys.Core.Features.Admin.Settings.PaymentMethods.Common;

namespace ReSys.Core.Features.Admin.Settings.PaymentMethods;

public static class GetPaymentMethodById
{
    public record Query(Guid Id) : IRequest<ErrorOr<PaymentMethodDetail>>;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Query, ErrorOr<PaymentMethodDetail>>
    {
        public async Task<ErrorOr<PaymentMethodDetail>> Handle(Query query, CancellationToken ct)
        {
            var method = await dbContext.Set<PaymentMethod>()
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == query.Id, ct);

            if (method == null) return PaymentMethodErrors.NotFound(query.Id);

            var detail = method.Adapt<PaymentMethodDetail>();
            detail.Active = method.Status == PaymentMethod.PaymentStatus.Active;
            
            return detail;
        }
    }
}
