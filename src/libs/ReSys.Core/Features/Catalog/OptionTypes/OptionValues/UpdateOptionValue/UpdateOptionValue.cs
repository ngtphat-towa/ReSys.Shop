using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Mapster;

using ReSys.Core.Common.Data;
using ReSys.Core.Features.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.Features.Catalog.OptionTypes.OptionValues.UpdateOptionValue;

public static class UpdateOptionValue
{
    // Request:
    public record Request : OptionValueInput;

    // Response:
    public record Response : OptionValueModel;

    // Command:
    public record Command(Guid OptionTypeId, Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    // Validator:
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new OptionValueInputValidator());
        }
    }

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: name uniqueness
            if (await context.Set<OptionValue>().AnyAsync(x =>
                x.OptionTypeId == command.OptionTypeId &&
                x.Name == request.Name &&
                x.Id != command.Id, cancellationToken))
            {
                return OptionValueErrors.NameAlreadyExists(request.Name);
            }

            // Load: option value
            var optionValue = await context.Set<OptionValue>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.OptionTypeId == command.OptionTypeId, cancellationToken);

            // Check: found
            if (optionValue is null)
                return OptionValueErrors.NotFound(command.Id);

            // Update: option value
            var updateResult = optionValue.Update(
                request.Name,
                request.Presentation,
                request.Position);

            if (updateResult.IsError)
                return updateResult.Errors;

            // Save: changes
            context.Set<OptionValue>().Update(optionValue);
            await context.SaveChangesAsync(cancellationToken);

            return optionValue.Adapt<Response>();
        }
    }
}