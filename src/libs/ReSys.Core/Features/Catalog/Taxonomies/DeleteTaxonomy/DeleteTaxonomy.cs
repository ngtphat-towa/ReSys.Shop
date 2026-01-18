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
            // Check: taxonomy exists
            var taxonomy = await context.Set<Taxonomy>()
                .Include(ot => ot.Taxons)
                .FirstOrDefaultAsync(ot => ot.Id == command.Id, cancellationToken);

            if (taxonomy is null)
                return TaxonomyErrors.NotFound(command.Id);

            // Business Rule: cannot delete if has associated (handled by domain)
            var deleteResult = taxonomy.Delete();
            if (deleteResult.IsError)
                return deleteResult.Errors;

            // Delete from database
            context.Set<Taxonomy>().Remove(taxonomy);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}