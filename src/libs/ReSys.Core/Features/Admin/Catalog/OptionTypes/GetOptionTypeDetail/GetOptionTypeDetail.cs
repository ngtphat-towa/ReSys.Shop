using ErrorOr;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.OptionTypes;
using ReSys.Core.Features.Admin.Catalog.OptionTypes.Common;

namespace ReSys.Core.Features.Admin.Catalog.OptionTypes.GetOptionTypeDetail;

public static class GetOptionTypeDetail
{
    // Request: Get by ID
    public record Request(Guid Id);

    // Response: Slice-specific alias for the full detail
    public record Response : OptionTypeDetail;

    // Query: Returns the specific Response type or an error
    public record Query(Request Request) : IRequest<ErrorOr<Response>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Query query, CancellationToken cancellationToken)
        {
            // Fetch: option type by ID
            var response = await context.Set<OptionType>()
                .AsNoTracking()
                .Where(x => x.Id == query.Request.Id)
                .ProjectToType<Response>()
                .FirstOrDefaultAsync(cancellationToken);

            // Check: found
            if (response is null)
            {
                return OptionTypeErrors.NotFound(query.Request.Id);
            }

            return response;
        }
    }
}
