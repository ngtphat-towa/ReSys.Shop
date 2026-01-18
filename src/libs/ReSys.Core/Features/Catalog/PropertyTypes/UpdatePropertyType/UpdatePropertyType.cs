using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Catalog.PropertyTypes.Common;

namespace ReSys.Core.Features.Catalog.PropertyTypes.UpdatePropertyType;

public static class UpdatePropertyType
{
    // Request:
    public record Request : PropertyTypeInput;

    // Response:
    public record Response : PropertyTypeDetail;

    // Command:
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

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

            // 1. Get: domain entity
            var propertyType = await context.Set<PropertyType>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (propertyType is null)
                return PropertyTypeErrors.NotFound(command.Id);

            // 2. Check: name conflict
            if (propertyType.Name != request.Name && await context.Set<PropertyType>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return PropertyTypeErrors.DuplicateName;
            }

            // 3. Update: domain entity
            var updateResult = propertyType.Update(
                name: request.Name,
                presentation: request.Presentation,
                kind: request.Kind,
                position: request.Position,
                filterable: request.Filterable);

            if (updateResult.IsError)
                return updateResult.Errors;

            // 4. Set: metadata
            propertyType.PublicMetadata = request.PublicMetadata;
            propertyType.PrivateMetadata = request.PrivateMetadata;

            // 5. Save
            context.Set<PropertyType>().Update(propertyType);
            await context.SaveChangesAsync(cancellationToken);

            // 6. Return: projected response
            return await context.Set<PropertyType>()
                .AsNoTracking()
                .Where(x => x.Id == propertyType.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}