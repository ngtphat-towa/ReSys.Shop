using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.ExampleCategories.Common;

namespace ReSys.Core.Features.Testing.ExampleCategories.CreateExampleCategory;

public static class CreateExampleCategory
{
    public record Request : ExampleCategoryBase;

    public record Command(Request Request) : IRequest<ErrorOr<ExampleCategoryDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
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
            var request = command.Request;

            if (await _context.Set<ExampleCategory>().AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return ExampleCategoryErrors.DuplicateName;
            }

            var category = new ExampleCategory
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description
            };

            _context.Set<ExampleCategory>().Add(category);
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
