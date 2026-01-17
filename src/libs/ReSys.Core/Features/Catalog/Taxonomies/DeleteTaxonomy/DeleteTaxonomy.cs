using ErrorOr;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;

namespace ReSys.Core.Features.Catalog.Taxonomies.DeleteTaxonomy;

public static class DeleteTaxonomy
{
    // Command:
    public record Command(Guid Id) : IRequest<ErrorOr<Deleted>>;

    // Handler:
    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Get: domain entity (Include taxons to check business rules)
            var taxonomy = await context.Set<Taxonomy>()
                .Include(x => x.Taxons)
                .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

            if (taxonomy is null)
                return TaxonomyErrors.NotFound(command.Id);

            // Business Rule: entity-level checks (raises events)
            var deleteResult = taxonomy.Delete();
            if (deleteResult.IsError)
                return deleteResult.Errors;

            // Delete: from database
            context.Set<Taxonomy>().Remove(taxonomy);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
