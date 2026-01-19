using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Rules.UpdateTaxonRule;

public static class UpdateTaxonRule
{
    public record Request
    {
        public string Type { get; init; } = null!;
        public string Value { get; init; } = null!;
        public string? MatchPolicy { get; init; }
        public string? PropertyName { get; init; }
    }

    public record Response : TaxonRuleResponse;

    public record Command(Guid TaxonomyId, Guid TaxonId, Guid Id, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request.Type).NotEmpty();
            RuleFor(x => x.Request.Value).NotEmpty();
        }
    }

    public class Handler(IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var taxon = await context.Set<Domain.Catalog.Taxonomies.Taxa.Taxon>()
                .Include(x => x.TaxonRules)
                .FirstOrDefaultAsync(x => x.Id == command.TaxonId && x.TaxonomyId == command.TaxonomyId, cancellationToken);

            if (taxon is null)
                return ReSys.Core.Domain.Catalog.Taxonomies.Taxa.TaxonErrors.NotFound(command.TaxonId);

            var rule = taxon.TaxonRules.FirstOrDefault(r => r.Id == command.Id);
            if (rule is null)
                return TaxonRuleErrors.NotFound(command.Id);

            var result = taxon.UpdateRule(
                command.Id,
                request.Type,
                request.Value,
                request.MatchPolicy,
                request.PropertyName);

            if (result.IsError)
                return result.Errors;

            context.Set<TaxonRule>().Update(rule);
            context.Set<Domain.Catalog.Taxonomies.Taxa.Taxon>().Update(taxon);

            await context.SaveChangesAsync(cancellationToken);

            return rule.Adapt<Response>();
        }
    }
}
