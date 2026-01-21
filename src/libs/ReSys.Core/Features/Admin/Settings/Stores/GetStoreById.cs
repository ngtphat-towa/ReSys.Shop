using ErrorOr;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Core.Features.Admin.Settings.Stores.Common;

namespace ReSys.Core.Features.Admin.Settings.Stores;

public static class GetStoreById
{
    public record Query(Guid Id) : IRequest<ErrorOr<StoreDetail>>;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Query, ErrorOr<StoreDetail>>
    {
        public async Task<ErrorOr<StoreDetail>> Handle(Query query, CancellationToken ct)
        {
            var store = await dbContext.Set<Store>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == query.Id, ct);

            if (store == null) return StoreErrors.NotFound(query.Id);

            return store.Adapt<StoreDetail>();
        }
    }
}
