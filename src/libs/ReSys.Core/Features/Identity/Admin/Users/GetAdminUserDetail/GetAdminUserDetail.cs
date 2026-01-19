using MediatR;
using Microsoft.AspNetCore.Identity;
using Mapster;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Features.Identity.Admin.Users.Common;
using ErrorOr;

namespace ReSys.Core.Features.Identity.Admin.Users.GetAdminUserDetail;

public static class GetAdminUserDetail
{
    public record Query(string Id) : IRequest<ErrorOr<AdminUserDetailResponse>>;

    public class Handler(UserManager<User> userManager) : IRequestHandler<Query, ErrorOr<AdminUserDetailResponse>>
    {
        public async Task<ErrorOr<AdminUserDetailResponse>> Handle(Query request, CancellationToken ct)
        {
            var user = await userManager.FindByIdAsync(request.Id);
            
            if (user == null) return UserErrors.NotFound(request.Id);

            return user.Adapt<AdminUserDetailResponse>();
        }
    }
}