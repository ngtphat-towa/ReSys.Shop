using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.Common;

namespace ReSys.Core.Features.Admin.Catalog.PropertyTypes.CreatePropertyType;

public static class CreatePropertyType
{
    // Request:
    public record Request : PropertyTypeInput;

    // Response:
    public record Response : PropertyTypeDetail;

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    private class RequestValidator : PropertyTypeValidator<Request> { }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // 1. Check: duplicate name
            if (await context.Set<PropertyType>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return PropertyTypeErrors.DuplicateName;
            }

            // 2. Create: domain entity
            var propertyTypeResult = PropertyType.Create(
                name: request.Name,
                presentation: request.Presentation,
                kind: request.Kind,
                position: request.Position,
                filterable: request.Filterable);

            if (propertyTypeResult.IsError)
                return propertyTypeResult.Errors;

            var propertyType = propertyTypeResult.Value;

            // 3. Set: metadata
            propertyType.PublicMetadata = request.PublicMetadata;
            propertyType.PrivateMetadata = request.PrivateMetadata;

            // 4. Save
            context.Set<PropertyType>().Add(propertyType);
            await context.SaveChangesAsync(cancellationToken);

            // 5. Return: projected response
            return await context.Set<PropertyType>()
                .AsNoTracking()
                .Where(x => x.Id == propertyType.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}
