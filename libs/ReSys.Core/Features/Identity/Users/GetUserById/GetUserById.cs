using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

using ReSys.Core.Features.Identity.Common;

namespace ReSys.Core.Features.Identity.Users.GetUserById;

public static class GetUserById
{
    public record Query(string Id) : IRequest<ErrorOr<UserResponse>>;

    public class Handler : IRequestHandler<Query, ErrorOr<UserResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<UserResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null) return IdentityErrors.UserNotFound;
            
            var roles = await _userManager.GetRolesAsync(user);
            
            return new UserResponse(
                user.Id, 
                user.UserName ?? "", 
                user.Email ?? "", 
                user.FirstName ?? "", 
                user.LastName ?? "", 
                user.UserType.ToString(),
                roles);
        }
    }
}
