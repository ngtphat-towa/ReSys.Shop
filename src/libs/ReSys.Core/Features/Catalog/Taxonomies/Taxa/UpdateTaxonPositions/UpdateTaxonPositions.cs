using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Features.Catalog.Taxonomies.Services;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.UpdateTaxonPositions;

public static class UpdateTaxonPositions
{
    public record TaxonPosition(Guid Id, int Position, Guid? ParentId);
    public record Request(IEnumerable<TaxonPosition> Positions);
    public record Command(Guid TaxonomyId, Request Request) : IRequest<ErrorOr<Success>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Positions).NotEmpty();
        }
    }

    public class Handler(
        IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Success>>
    {
        public async Task<ErrorOr<Success>> Handle(Command command, CancellationToken cancellationToken)
        {
            var taxonomy = await context.Set<Taxonomy>()
                .Include(t => t.Taxons)
                .FirstOrDefaultAsync(t => t.Id == command.TaxonomyId, cancellationToken);

            if (taxonomy == null)
                return TaxonomyErrors.NotFound(command.TaxonomyId);

            foreach (var pos in command.Request.Positions)
            {
                var taxon = taxonomy.Taxons.FirstOrDefault(t => t.Id == pos.Id);
                if (taxon != null)
                {
                    taxon.SetPosition(pos.Position);
                    if (taxon.ParentId != pos.ParentId)
                    {
                        var parentResult = taxon.SetParent(pos.ParentId);
                        if (parentResult.IsError) return parentResult.Errors;
                    }
                    context.Set<Taxon>().Update(taxon);
                }
            }

            context.Set<Taxonomy>().Update(taxonomy);
            await context.SaveChangesAsync(cancellationToken);

            // Rebuild hierarchy via events

            return Result.Success;
        }
    }
}