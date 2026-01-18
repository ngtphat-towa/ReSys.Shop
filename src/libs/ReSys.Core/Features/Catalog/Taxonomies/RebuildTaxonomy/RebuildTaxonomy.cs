using ErrorOr;

using MediatR;

using ReSys.Core.Features.Catalog.Taxonomies.Services;

namespace ReSys.Core.Features.Catalog.Taxonomies.RebuildTaxonomy;

public static class RebuildTaxonomy
{
    public record Command(Guid TaxonomyId) : IRequest<ErrorOr<Success>>;

    public class Handler(ITaxonHierarchyService hierarchyService) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken ct)
        {
            return await hierarchyService.RebuildHierarchyAsync(command.TaxonomyId, ct);
        }
    }
}