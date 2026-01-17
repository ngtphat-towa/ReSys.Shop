using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Services;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.DeleteTaxon;

public static class DeleteTaxon
{
    // Command:
    public record Command(Guid TaxonomyId, Guid Id) : IRequest<ErrorOr<Deleted>>;

    // Handler:
    public class Handler(IApplicationDbContext context, ITaxonHierarchyService hierarchyService) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxon = await context.Set<Taxon>()
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.TaxonomyId == command.TaxonomyId, cancellationToken);

            if (taxon is null)
                return TaxonErrors.NotFound(command.Id);

            // Prevent deleting root taxon
            if (taxon.ParentId == null)
            {
                return TaxonErrors.RootLock;
            }

            // Business Rule: Check for children? Or just delete?
            if (await context.Set<Taxon>().AnyAsync(x => x.ParentId == command.Id, cancellationToken))
            {
                return TaxonErrors.HasChildren;
            }

            // Publish Deleted event
            var deleteResult = taxon.Delete();
            if (deleteResult.IsError)
                return deleteResult.Errors;

            context.Set<Taxon>().Remove(taxon);
            await context.SaveChangesAsync(cancellationToken);

            // Rebuild hierarchy
            await hierarchyService.RebuildAsync(command.TaxonomyId, cancellationToken);

            return Result.Deleted;
        }
    }
}