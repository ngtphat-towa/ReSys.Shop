using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Settings.Stores;
using ReSys.Core.Features.Admin.Settings.Stores.Common;

namespace ReSys.Core.Features.Admin.Settings.Stores;

public static class GetStores
{
    public record Query : IRequest<ErrorOr<List<StoreListItem>>>;

    public class Handler(IApplicationDbContext dbContext) : IRequestHandler<Query, ErrorOr<List<StoreListItem>>>
    {
        public async Task<ErrorOr<List<StoreListItem>>> Handle(Query query, CancellationToken ct)
        {
            var stores = await dbContext.Set<Store>()
                .AsNoTracking()
                .ToListAsync(ct);

            return stores.Select(s => new StoreListItem
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                DefaultCurrency = s.DefaultCurrency
            }).ToList();
        }
    }
}
