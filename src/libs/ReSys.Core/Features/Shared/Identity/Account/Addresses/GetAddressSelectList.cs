using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Features.Shared.Identity.Account.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Shared.Identity.Account.Addresses;

public static class GetAddressSelectList
{
    public record Query(QueryOptions Request) : IRequest<PagedList<AddressSelectListItem>>;

    public class Handler(
        IApplicationDbContext context,
        IUserContext userContext) : IRequestHandler<Query, PagedList<AddressSelectListItem>>
    {
        public async Task<PagedList<AddressSelectListItem>> Handle(Query query, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return new PagedList<AddressSelectListItem>([], 0, query.Request.Page ?? 1, query.Request.PageSize ?? 10);

            return await context.Set<UserAddress>()
                .AsNoTracking()
                .Where(x => x.UserId == userContext.UserId)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Label)
                .ToPagedOrAllAsync<UserAddress, AddressSelectListItem>(query.Request, ct);
        }
    }
}
