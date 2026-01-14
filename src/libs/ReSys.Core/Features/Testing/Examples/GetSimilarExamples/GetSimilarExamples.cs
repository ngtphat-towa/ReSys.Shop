using ErrorOr;


using MediatR;


using Microsoft.EntityFrameworkCore;


using Pgvector.EntityFrameworkCore;


using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Testing.Examples;
using ReSys.Core.Features.Testing.Examples.Common;

namespace ReSys.Core.Features.Testing.Examples.GetSimilarExamples;

public static class GetSimilarExamples
{
    public class Request(Guid id)
    {
        public Guid Id { get; set; } = id;
    }

    public record Query(Request Request) : IRequest<ErrorOr<List<ExampleListItem>>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<List<ExampleListItem>>>
    {
        public async Task<ErrorOr<List<ExampleListItem>>> Handle(Query query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var exampleEmbedding = await context.Set<ExampleEmbedding>()
                .AsNoTracking()
                .FirstOrDefaultAsync(pe => pe.ExampleId == request.Id, cancellationToken);

            if (exampleEmbedding == null)
            {
                return Error.NotFound("Example.NotFound", "Example not found or has no embedding.");
            }

            var similarExamples = await context.Set<ExampleEmbedding>()
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
