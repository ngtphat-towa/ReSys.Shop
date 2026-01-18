using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.DeleteTaxonRule;

public static class DeleteTaxonRule
{
    public record Command(Guid TaxonomyId, Guid TaxonId, Guid Id) : IRequest<ErrorOr<Deleted>>;

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Deleted>>
    {
        public async Task<ErrorOr<Deleted>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxon = await context.Set<ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Taxon>()
                .Include(x => x.TaxonRules)
                .FirstOrDefaultAsync(x => x.Id == command.TaxonId && x.TaxonomyId == command.TaxonomyId, cancellationToken);

            if (taxon is null)
                return ReSys.Core.Domain.Catalog.Taxonomies.Taxa.TaxonErrors.NotFound(command.TaxonId);

            var rule = taxon.TaxonRules.FirstOrDefault(r => r.Id == command.Id);
            if (rule is null)
                return TaxonRuleErrors.NotFound(command.Id);

            var result = taxon.RemoveRule(command.Id);
            if (result.IsError)
                return result.Errors;

            context.Set<TaxonRule>().Remove(rule);
            context.Set<ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Taxon>().Update(taxon);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
