using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Testing.ExampleCategories;

namespace ReSys.Core.Features.Testing.ExampleCategories.DeleteExampleCategory;

public static class DeleteExampleCategory
{
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var category = await context.Set<ExampleCategory>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (category == null)
            {
                return ExampleCategoryErrors.NotFound;
            }

            context.Set<ExampleCategory>().Remove(category);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
