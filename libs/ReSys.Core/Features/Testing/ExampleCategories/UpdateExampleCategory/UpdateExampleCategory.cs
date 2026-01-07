using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.ExampleCategories.Common;

namespace ReSys.Core.Features.Testing.ExampleCategories.UpdateExampleCategory;

public static class UpdateExampleCategory
{
    public record Request : ExampleCategoryBase;

    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<ExampleCategoryDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }

        private class RequestValidator : ExampleCategoryValidator<Request> { }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<ExampleCategoryDetail>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<ExampleCategoryDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var category = await _context.Set<ExampleCategory>()
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (category == null)
            {
                return ExampleCategoryErrors.NotFound;
            }

            var request = command.Request;

            if (await _context.Set<ExampleCategory>().AnyAsync(x => x.Name == request.Name && x.Id != command.Id, cancellationToken))
            {
                return ExampleCategoryErrors.DuplicateName;
            }

            category.Name = request.Name;
            category.Description = request.Description;

            await _context.SaveChangesAsync(cancellationToken);

            return new ExampleCategoryDetail
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
