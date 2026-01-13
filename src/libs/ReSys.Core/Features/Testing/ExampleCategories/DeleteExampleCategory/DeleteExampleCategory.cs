using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.ExampleCategories.Common;

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

    public class Handler : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var category = await _context.Set<ExampleCategory>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (category == null)
            {
                return ExampleCategoryErrors.NotFound;
            }

            _context.Set<ExampleCategory>().Remove(category);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
