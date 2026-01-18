using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Core.Features.Inventories.Locations.Common;

namespace ReSys.Core.Features.Inventories.Locations.CreateStockLocation;

public static class CreateStockLocation
{
    public record Request : StockLocationInput;
    public record Response : StockLocationDetail;
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new StockLocationInputValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var request = command.Request;

            // Check: Code uniqueness
            if (await context.Set<StockLocation>().AnyAsync(x => x.Code == request.Code.ToUpper(), ct))
                return StockLocationErrors.DuplicateCode(request.Code);

            // Create Address owned entity
            var addressResult = Address.Create(
                request.Address.Address1,
                request.Address.City,
                request.Address.ZipCode,
                request.Address.CountryCode,
                request.Address.FirstName,
                request.Address.LastName,
                null, // countryId
                request.Address.Address2,
                request.Address.Phone,
                request.Address.Company,
                null, // stateId
                request.Address.StateCode);

            if (addressResult.IsError) return addressResult.Errors;

            // Create Domain Aggregate
            var locationResult = StockLocation.Create(
                request.Name,
                request.Code,
                addressResult.Value,
                request.Presentation,
                request.IsDefault,
                request.Type);

            if (locationResult.IsError) return locationResult.Errors;
            var location = locationResult.Value;

            // Enforce Singleton Default
            if (location.IsDefault)
            {
                var currentDefault = await context.Set<StockLocation>()
                    .FirstOrDefaultAsync(x => x.IsDefault && !x.IsDeleted, ct);
                
                if (currentDefault != null)
                {
                    currentDefault.UnmarkAsDefault();
                    context.Set<StockLocation>().Update(currentDefault);
                }
            }

            // Save
            context.Set<StockLocation>().Add(location);
            await context.SaveChangesAsync(ct);

            return location.Adapt<Response>();
        }
    }
}
