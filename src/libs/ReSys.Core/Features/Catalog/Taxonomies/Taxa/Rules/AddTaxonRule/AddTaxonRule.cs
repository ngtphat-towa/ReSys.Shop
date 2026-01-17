using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Mapster;

using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.AddTaxonRule;

public static class AddTaxonRule
{
    public record Request
    {
        public string Type { get; init; } = null!;
        public string Value { get; init; } = null!;
        public string? MatchPolicy { get; init; }
        public string? PropertyName { get; init; }
    }

    public record Response : TaxonRuleResponse;

    public record Command(Guid TaxonomyId, Guid TaxonId, Request Request) : IRequest<ErrorOr<Response>>;

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

            var taxon = await context.Set<Taxon>()
                .Include(x => x.TaxonRules)
                .FirstOrDefaultAsync(x => x.Id == command.TaxonId && x.TaxonomyId == command.TaxonomyId, cancellationToken);

            if (taxon is null)
                return TaxonErrors.NotFound(command.TaxonId);

            var ruleResult = taxon.AddRule(
                request.Type,
                request.Value,
                request.MatchPolicy,
                request.PropertyName);

            if (ruleResult.IsError)
                return ruleResult.Errors;

            context.Set<TaxonRule>().Add(ruleResult.Value);

            await context.SaveChangesAsync(cancellationToken);

            return ruleResult.Value.Adapt<Response>();
        }
    }
}