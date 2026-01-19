using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes;

namespace ReSys.Core.Features.Admin.Catalog.OptionTypes.OptionValues.UpdateOptionValuePositions;

public static class UpdateOptionValuePositions
{
    public record ValuePosition(Guid Id, int Position);
    public record Request(IEnumerable<ValuePosition> Values);
    public record Command(Guid OptionTypeId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Values).NotEmpty();
            RuleForEach(x => x.Request.Values).ChildRules(v =>
            {
                v.RuleFor(x => x.Id).NotEmpty();
                v.RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
            });
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: the Aggregate Root
            var optionType = await context.Set<OptionType>()
                .Include(x => x.OptionValues)
                .FirstOrDefaultAsync(x => x.Id == command.OptionTypeId, cancellationToken);

            // Check: found
            if (optionType is null)
                return OptionTypeErrors.NotFound(command.OptionTypeId);

            // Reorder: via the Aggregate Root
            var reorderResult = optionType.ReorderValues(
                command.Request.Values.Select(v => (v.Id, v.Position)));

            if (reorderResult.IsError)
                return reorderResult.Errors;

            // Save: changes
            context.Set<OptionType>().Update(optionType);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
