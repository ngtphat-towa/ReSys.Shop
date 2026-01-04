using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Entities;
using ReSys.Core.Features.Examples.Common;
using ReSys.Core.Interfaces;

namespace ReSys.Core.Features.Examples.CreateExample;

public static class CreateExample
{
    public record Request : ExampleInput;

    public record Command(Request Request) : IRequest<ErrorOr<ExampleDetail>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
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
            var request = command.Request;

            if (await _context.Set<Example>().AnyAsync(x => x.Name == request.Name, cancellationToken))
            {
                return ExampleErrors.DuplicateName;
            }

            var example = new Example
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Set<Example>().Add(example);
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