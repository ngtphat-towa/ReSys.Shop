using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Domain.Identity;
using ReSys.Core.Features.Identity.Contracts;

namespace ReSys.Core.Features.Identity.Users.GetUsers;

public static class GetUsers
{
    public record Query : IRequest<ErrorOr<List<UserResponse>>>;

    public class Handler : IRequestHandler<Query, ErrorOr<List<UserResponse>>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public Handler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ErrorOr<List<UserResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users.ToListAsync(cancellationToken);
            
            var response = users.Select(u => new UserResponse(
                u.Id, 
                u.UserName ?? "", 
                u.Email ?? "", 
                u.FirstName ?? "", 
                u.LastName ?? "", 
                u.UserType.ToString(),
                Array.Empty<string>())) // TODO: Include roles if needed (expensive in list)
                .ToList();

            return response;
        }
    }
}
