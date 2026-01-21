using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Security.Authentication.Context;
using ReSys.Core.Domain.Identity.Users;
using ReSys.Core.Domain.Identity.Users.UserAddresses;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Core.Features.Shared.Identity.Account.Common;
using ReSys.Core.Features.Shared.Identity.Common;

using ErrorOr;

using Mapster;
using ReSys.Core.Common.Data;

namespace ReSys.Core.Features.Shared.Identity.Account.Addresses;

public static class CreateAddress
{
    public record Request : AddressInput;
    public record Response : AccountAddressResponse;
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(
        IUserContext userContext,
        IApplicationDbContext dbContext,
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

            var req = command.Request;

            // 1. Create Address Value Object
            var addressResult = Address.Create(
                req.Address1, req.City, req.ZipCode, req.CountryCode,
                req.FirstName, req.LastName, null, req.Address2, req.Phone, req.Company, null, req.StateCode);

            if (addressResult.IsError) return addressResult.Errors;

            // 2. Create UserAddress Aggregate
            var userAddressResult = UserAddress.Create(
                user.Id, addressResult.Value, req.Label, req.Type, req.IsDefault);

            if (userAddressResult.IsError) return userAddressResult.Errors;
            var userAddress = userAddressResult.Value;

            // 3. Logic: Default management
            user.AddAddress(userAddress);
            if (req.IsDefault)
            {
                user.SetDefaultAddress(userAddress.Id);
            }

            // 4. Persistence 
            dbContext.Set<UserAddress>().Add(userAddress);
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded) return AccountResultExtensions.ToApplicationResult(result.Errors, prefix: "Address");

            return userAddress.Adapt<Response>();
        }
    }
}
