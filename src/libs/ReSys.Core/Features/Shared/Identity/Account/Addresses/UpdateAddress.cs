using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Core.Features.Shared.Identity.Account.Common;

using ErrorOr;

using Mapster;

namespace ReSys.Core.Features.Shared.Identity.Account.Addresses;

public static class UpdateAddress
{
    public record Request : AddressInput;
    public record Response : AccountAddressResponse;
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(
        IUserContext userContext,
        UserManager<User> userManager) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            if (!userContext.IsAuthenticated || string.IsNullOrEmpty(userContext.UserId))
                return UserErrors.Unauthorized;

            var user = await userManager.Users
                .Include(u => u.UserAddresses)
                .FirstOrDefaultAsync(u => u.Id == userContext.UserId, ct);

            if (user == null) return UserErrors.NotFound(userContext.UserId);

            var userAddress = user.UserAddresses.FirstOrDefault(a => a.Id == command.Id);
            if (userAddress == null) return UserAddressErrors.NotFound(command.Id);

            var req = command.Request;

            // 1. Create updated Address Value Object
            var addressResult = Address.Create(
                req.Address1, req.City, req.ZipCode, req.CountryCode,
                req.FirstName, req.LastName, null, req.Address2, req.Phone, req.Company, null, req.StateCode);

            if (addressResult.IsError) return addressResult.Errors;

            // 2. Domain Logic: Update Entity
            var updateResult = userAddress.Update(req.Label, addressResult.Value, req.Type);
            if (updateResult.IsError) return updateResult.Errors;

            if (req.IsDefault)
            {
                user.SetDefaultAddress(userAddress.Id);
            }

            // 3. Persistence
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return Error.Failure("Address.UpdateFailed");

            return userAddress.Adapt<Response>();
        }
    }
}
