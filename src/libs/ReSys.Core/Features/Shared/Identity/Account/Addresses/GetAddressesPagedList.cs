using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Common.Extensions.Pagination;
using ReSys.Core.Common.Extensions.Query;
using ReSys.Core.Common.Security.Authentication.Contexts;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Features.Shared.Identity.Account.Common;
using ReSys.Shared.Models.Pages;
using ReSys.Shared.Models.Query;

namespace ReSys.Core.Features.Shared.Identity.Account.Addresses;

public static class GetAddressesPagedList
{
    public record Request : QueryOptions;
    public record Response : AccountAddressResponse;
    public record Query(Request Request) : IRequest<PagedList<Response>>;

    public class Handler(
        IApplicationDbContext context,
        IUserContext userContext) : IRequestHandler<Query, PagedList<Response>>
    {
        public async Task<PagedList<Response>> Handle(Query query, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return new PagedList<Response>([], 0, query.Request.Page ?? 1, query.Request.PageSize ?? 10);

            return await context.Set<UserAddress>()
                .AsNoTracking()
                .Where(x => x.UserId == userContext.UserId)
                .ApplyFilter(query.Request)
                .ApplySearch(query.Request)
                .ApplySort(query.Request)
                .ToPagedListAsync<UserAddress, Response>(query.Request, ct);
        }
    }
}
