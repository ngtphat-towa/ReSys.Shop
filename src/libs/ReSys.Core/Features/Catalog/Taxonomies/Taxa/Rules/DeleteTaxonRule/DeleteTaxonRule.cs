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
            var rule = await context.Set<TaxonRule>()
                .Include(x => x.Taxon)
                .FirstOrDefaultAsync(x => x.Id == command.Id && x.TaxonId == command.TaxonId, cancellationToken);

            if (rule is null)
                return TaxonRuleErrors.NotFound(command.Id);

            var deleteResult = rule.Delete();
            if (deleteResult.IsError)
                return deleteResult.Errors;

            rule.Taxon.MarkedForRegenerateProducts = true;

            context.Set<TaxonRule>().Remove(rule);
            await context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}