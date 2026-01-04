using ReSys.Core.Features.Examples.Common;

namespace ReSys.Core.Features.Examples.DeleteExample;

public static class DeleteExample
{
    public class Command : IRequest<ErrorOr<Deleted>>
    {
        public Guid Id { get; set; }

        public Command(Guid id) => Id = id;
    }

    public class Handler : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Deleted>> Handle(Command request, CancellationToken cancellationToken)
        {
            var example = await _context.Set<Example>().FindAsync(new object[] { request.Id }, cancellationToken);

            if (example is null)
            {
                return ExampleErrors.NotFound(request.Id);
            }

            _context.Set<Example>().Remove(example);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}