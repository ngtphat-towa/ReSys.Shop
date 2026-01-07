using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;

namespace ReSys.Core.Features.Testing.Examples.CreateExample;

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
                Status = request.Status,
                HexColor = request.HexColor,
                CategoryId = request.CategoryId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Set<Example>().Add(example);
            await _context.SaveChangesAsync(cancellationToken);

            // Fetch with Category for the response
            return await _context.Set<Example>()
                .AsNoTracking()
                .Select(ExampleDetail.Projection)
                .FirstAsync(x => x.Id == example.Id, cancellationToken);
        }
    }
}