using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Catalog.OptionTypes.Common;

namespace ReSys.Core.Features.Catalog.OptionTypes.UpdateOptionType;

public static class UpdateOptionType
{
    // Request: 
    public record Request : OptionTypeInput;

    // Response:
    public record Response : OptionTypeDetail;

    // Command:
    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    private class RequestValidator : OptionTypeValidator<Request> { }
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

            // Check: option type exists
            var optionType = await context.Set<OptionType>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (optionType is null)
                return OptionTypeErrors.NotFound(command.Id);

            // Check: name change and duplicate name
            if (optionType.Name != request.Name && await context.Set<OptionType>()
                .AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return OptionTypeErrors.DuplicateName;
            }

            // Update: option type
            var updateResult = optionType.Update(
                name: request.Name,
                presentation: request.Presentation,
                position: request.Position,
                filterable: request.Filterable);

            if (updateResult.IsError)
                return updateResult.Errors;

            // Set: metadata
            optionType.PublicMetadata = request.PublicMetadata;
            optionType.PrivateMetadata = request.PrivateMetadata;

            // Save: option type
            context.Set<OptionType>().Update(optionType);
            await context.SaveChangesAsync(cancellationToken);

            // Return: detail
            return await context.Set<OptionType>()
                .AsNoTracking()
                .Where(x => x.Id == optionType.Id)
                .ProjectToType<Response>()
                .FirstAsync(cancellationToken);
        }
    }
}