using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Identity.Domain;
using ReSys.Identity.Persistence;

namespace ReSys.Identity.Features.Management.Permissions;

public static class List
{
    public record Query() : IRequest<ErrorOr<List<ClaimDefinition>>>;

    public class Handler(AppIdentityDbContext dbContext) : IRequestHandler<Query, ErrorOr<List<ClaimDefinition>>>
    {
        public async Task<ErrorOr<List<ClaimDefinition>>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await dbContext.ClaimDefinitions.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}
