using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ErrorOr;

namespace ReSys.Core.Features.Shared.Identity.Account.Addresses;

public static class DeleteAddress
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(
        IUserContext userContext,
        UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return UserErrors.Unauthorized;

            var user = await userManager.Users
                .Include(u => u.UserAddresses)
                .FirstOrDefaultAsync(u => u.Id == userContext.UserId, ct);

            if (user == null) return UserErrors.NotFound(userContext.UserId);

            var userAddress = user.UserAddresses.FirstOrDefault(a => a.Id == command.Id);
            if (userAddress == null) return UserAddressErrors.NotFound(command.Id);

            user.UserAddresses.Remove(userAddress);

            var result = await userManager.UpdateAsync(user);
            return result.Succeeded ? Result.Deleted : Error.Failure("Address.DeleteFailed");
        }
    }
}
