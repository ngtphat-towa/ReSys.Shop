using ErrorOr;
using MediatR;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.ValidateTaxonomy;

public static class ValidateTaxonomy
{
    public record Query(Guid TaxonomyId) : IRequest<ErrorOr<Success>>;

    public class Handler(ITaxonHierarchyService hierarchyService) : IRequestHandler<Query, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Query query, CancellationToken ct)
        {
            return await hierarchyService.ValidateHierarchyAsync(query.TaxonomyId, ct);
        }
    }
}
