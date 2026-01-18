using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;

namespace ReSys.Core.Features.Catalog.OptionTypes.CreateOptionType;

public static class CreateOptionType
{
    // Request:
    public record Request : OptionTypeInput;

    // Response:
    public record Response : OptionTypeDetail;

    // Command:
    public record Command(Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new OptionTypeInputValidator());
        }
    }

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: duplicate name
            if (await context.Set<OptionType>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return OptionTypeErrors.DuplicateName;
            }

            // Create: domain model
            var optionTypeResult = OptionType.Create(
                name: request.Name,
                presentation: request.Presentation,
                position: request.Position,
                filterable: request.Filterable);

            if (optionTypeResult.IsError)
                return optionTypeResult.Errors;
            
            var optionType = optionTypeResult.Value;

            // Set: metadata
            optionType.PublicMetadata = request.PublicMetadata;
            optionType.PrivateMetadata = request.PrivateMetadata;

            // Save: domain model
            context.Set<OptionType>().Add(optionType);
            await context.SaveChangesAsync(cancellationToken);

            // Return: detail via Mapster projection
            return await context.Set<OptionType>()
                .AsNoTracking()
                .Where(x => x.Id == optionType.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}
