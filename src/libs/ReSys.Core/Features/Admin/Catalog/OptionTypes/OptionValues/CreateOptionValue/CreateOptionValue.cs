using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.Common;
using ReSys.Core.Domain.Catalog.OptionTypes.OptionValues;

namespace ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.CreateOptionValue;

public static class CreateOptionValue
{
    public record Request : OptionValueInput;
    public record Response : OptionValueModel;
    public record Command(Guid OptionTypeId, Request Request) : IRequest<ErrorOr<Response>>;

    private class RequestValidator : OptionValueValidator<Request> { }
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Load the Aggregate Root
            var optionType = await context.Set<OptionType>()
                .Include(x => x.OptionValues)
                .FirstOrDefaultAsync(x => x.Id == command.OptionTypeId, cancellationToken);

            if (optionType is null)
                return OptionTypeErrors.NotFound(command.OptionTypeId);

            // Add via the Aggregate Root
            var addResult = optionType.AddValue(request.Name, request.Presentation);

            if (addResult.IsError)
                return addResult.Errors;
            
            var optionValue = addResult.Value;

            context.Set<OptionValue>().Add(optionValue);
            context.Set<OptionType>().Update(optionType);
            
            await context.SaveChangesAsync(cancellationToken);

            return optionValue.Adapt<Response>();
        }
    }
}
