using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain;
using ReSys.Core.Features.Testing.Examples.Common;

namespace ReSys.Core.Features.Testing.Examples.GetExampleById;

public static class GetExampleById
{
    public record Request(Guid id)
    {
        public Guid Id { get; set; } = id;
    }

    public record Query(Request Request) : IRequest<ErrorOr<ExampleDetail>>;

    public class Handler : IRequestHandler<Query, ErrorOr<ExampleDetail>>
    {
        private readonly IApplicationDbContext _context;

        public Handler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<ExampleDetail>> Handle(Query query, CancellationToken cancellationToken)
        {
            var response = await _context.Set<Example>()
                .AsNoTracking()
                .Where(x => x.Id == query.Request.Id)
                .Select(ExampleDetail.Projection)
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
            {
                return ExampleErrors.NotFound(query.Request.Id);
            }

            return response;
        }
    }
}