using ReSys.Core.Features.Examples.Common;

namespace ReSys.Core.Features.Examples.GetSimilarExamples;

public static class GetSimilarExamples
{
    public class Request
    {
        public Guid Id { get; set; }
        public Request(Guid id) => Id = id;
    }

    public record Query(Request Request) : IRequest<ErrorOr<List<ExampleListItem>>>;

    public class Handler : IRequestHandler<Query, ErrorOr<List<ExampleListItem>>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<List<ExampleListItem>>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var exampleEmbedding = await _context.Set<ExampleEmbedding>()
                .AsNoTracking()
                .FirstOrDefaultAsync(pe => pe.ExampleId == request.Id, cancellationToken);

            if (exampleEmbedding == null)
            {
                return Error.NotFound("Example.NotFound", "Example not found or has no embedding.");
            }

            var similarExamples = await _context.Set<ExampleEmbedding>()
                .AsNoTracking()
                .OrderBy(pe => pe.Embedding.L2Distance(exampleEmbedding.Embedding))
                .Where(pe => pe.ExampleId != request.Id)
                .Take(5)
                .Select(pe => pe.Example)
                .Select(ExampleListItem.Projection)
                .ToListAsync(cancellationToken);

            return similarExamples;
        }
    }
}
