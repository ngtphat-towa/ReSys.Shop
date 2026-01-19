using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Inventories.Locations;
using ReSys.Core.Domain.Location.Addresses;
using ReSys.Core.Features.Admin.Inventories.Locations.Common;

namespace ReSys.Core.Features.Admin.Inventories.Locations.UpdateStockLocation;

public static class UpdateStockLocation
{
    public record Request : StockLocationInput;
    public record Response : StockLocationDetail;
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new StockLocationInputValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var request = command.Request;

            var location = await context.Set<StockLocation>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, ct);

            if (location == null)
                return StockLocationErrors.NotFound(command.Id);

            // Check: Code uniqueness
            if (location.Code != request.Code.ToUpper() && 
                await context.Set<StockLocation>().AnyAsync(x => x.Code == request.Code.ToUpper(), ct))
            {
                return StockLocationErrors.DuplicateCode(request.Code);
            }

            // Create/Update Address owned entity
            var addressResult = Address.Create(
                request.Address.Address1,
                request.Address.City,
                request.Address.ZipCode,
                request.Address.CountryCode,
                request.Address.FirstName,
                request.Address.LastName,
                null,
                request.Address.Address2,
                request.Address.Phone,
                request.Address.Company,
                null,
                request.Address.StateCode);

            if (addressResult.IsError) return addressResult.Errors;

            // Update Domain Aggregate
            var result = location.Update(
                request.Name,
                request.Presentation ?? request.Name,
                request.Code,
                request.IsDefault,
                request.Type,
                addressResult.Value);

            if (result.IsError) return result.Errors;

            // Set Metadata
            location.SetMetadata(request.PublicMetadata, request.PrivateMetadata);

            // Enforce Singleton Default
            if (location.IsDefault)
            {
                var otherDefaults = await context.Set<StockLocation>()
                    .Where(x => x.IsDefault && x.Id != location.Id && !x.IsDeleted)
                    .ToListAsync(ct);
                
                foreach (var other in otherDefaults)
                {
                    other.UnmarkAsDefault();
                    context.Set<StockLocation>().Update(other);
                }
            }

            context.Set<StockLocation>().Update(location);
            await context.SaveChangesAsync(ct);

            return location.Adapt<Response>();
        }
    }
}
