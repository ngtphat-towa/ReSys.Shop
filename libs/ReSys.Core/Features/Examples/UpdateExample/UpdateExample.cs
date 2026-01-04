using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Examples.Common;

namespace ReSys.Core.Features.Examples.UpdateExample;

public static class UpdateExample
{
    public record Request : ExampleInput;

    public record Command(Guid Id, Request Request) : IRequest<ErrorOr<ExampleDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }

        private class RequestValidator : ExampleValidator<Request> { }
    }

    public class Handler : IRequestHandler<Command, ErrorOr<ExampleDetail>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<ExampleDetail>> Handle(Command command, CancellationToken cancellationToken)
        {
            var example = await _context.Set<Example>().FindAsync(new object[] { command.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(command.Id);
            }

            var request = command.Request;

            if (await _context.Set<Example>().AnyAsync(x => x.Name == request.Name && x.Id != command.Id, cancellationToken))
            {
                return ExampleErrors.DuplicateName;
            }

            example.Name = request.Name;
            example.Description = request.Description;
            example.Price = request.Price;
            
            if (!string.IsNullOrEmpty(request.ImageUrl))
            {
                example.ImageUrl = request.ImageUrl;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new ExampleDetail
            {
                Id = example.Id,
                Name = example.Name,
                Description = example.Description,
                Price = example.Price,
                ImageUrl = example.ImageUrl,
                CreatedAt = example.CreatedAt
            };
        }
    }
}
