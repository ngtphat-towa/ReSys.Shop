using ErrorOr;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.PropertyTypes;
using ReSys.Core.Features.Admin.Catalog.PropertyTypes.Common;

namespace ReSys.Core.Features.Admin.Catalog.PropertyTypes.GetPropertyTypeDetail;

public static class GetPropertyTypeDetail
{
    // Request:
    public record Request(Guid Id);

    // Response alias
    public record Response : PropertyTypeDetail;

    // Query:
    public record Query(Request Request) : IRequest<ErrorOr<PropertyTypeDetail>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Query, ErrorOr<PropertyTypeDetail>>
    {
        public async Task<ErrorOr<PropertyTypeDetail>> Handle(Query query, CancellationToken cancellationToken)
        {
            var response = await context.Set<PropertyType>()
                .AsNoTracking()
                .Where(x => x.Id == query.Request.Id)
                .ProjectToType<PropertyTypeDetail>()
                .FirstOrDefaultAsync(cancellationToken);

            if (response is null)
                return PropertyTypeErrors.NotFound(query.Request.Id);

            return response;
        }
    }
}
