using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.DeleteTaxon;

public static class DeleteTaxon
{
    public record Command(Guid TaxonomyId, Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonomy = await context.Set<Taxonomy>()
                .Include(t => t.Taxons)
                .FirstOrDefaultAsync(t => t.Id == command.TaxonomyId, cancellationToken);

            if (taxonomy == null)
                return TaxonomyErrors.NotFound(command.TaxonomyId);

            var taxon = taxonomy.Taxons.FirstOrDefault(t => t.Id == command.Id);
            if (taxon == null)
                return TaxonErrors.NotFound(command.Id);

            var result = taxon.Delete();
            if (result.IsError)
                return result.Errors;

            context.Set<Taxon>().Remove(taxon);
            context.Set<Taxonomy>().Update(taxonomy);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}