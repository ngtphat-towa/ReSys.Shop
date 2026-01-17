using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.UpdateTaxonPositions;

public static class UpdateTaxonPositions
{
    public record TaxonPosition(Guid Id, int Position);
    public record Request(IEnumerable<TaxonPosition> Taxons);
    public record Command(Guid TaxonomyId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Taxons).NotEmpty();
            RuleForEach(x => x.Request.Taxons).ChildRules(v =>
            {
                v.RuleFor(x => x.Id).NotEmpty();
                v.RuleFor(x => x.Position).GreaterThanOrEqualTo(0);
            });
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonIds = command.Request.Taxons.Select(v => v.Id).ToList();
            
            var taxons = await context.Set<Taxon>()
                .Where(x => x.TaxonomyId == command.TaxonomyId && taxonIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var update in command.Request.Taxons)
            {
                var taxon = taxons.FirstOrDefault(x => x.Id == update.Id);
                if (taxon != null)
                {
                    taxon.Position = update.Position;
                    context.Set<Taxon>().Update(taxon);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
