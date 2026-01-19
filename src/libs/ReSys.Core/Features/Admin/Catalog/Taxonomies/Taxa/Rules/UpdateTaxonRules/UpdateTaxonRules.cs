using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mapster;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Admin.Catalog.Taxonomies.Taxa.Rules.UpdateTaxonRules;

public static class UpdateTaxonRules
{
    public record Request
    {
        public List<TaxonRuleInput> Rules { get; init; } = [];
    }

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleForEach(x => x.Rules).SetValidator(new TaxonRuleInputValidator());
        }
    }

    public record Response(Guid TaxonId, List<TaxonRuleResponse> Rules);
    public record Command(Guid TaxonomyId, Guid TaxonId, Request Request) : IRequest<ErrorOr<Response>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Request).SetValidator(new RequestValidator());
        }
    }

    public class Handler(
        IApplicationDbContext context) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            var taxon = await context.Set<Domain.Catalog.Taxonomies.Taxa.Taxon>()
                .Include(t => t.TaxonRules)
                .FirstOrDefaultAsync(t => t.Id == command.TaxonId && t.TaxonomyId == command.TaxonomyId, ct);

            if (taxon == null)
                return ReSys.Core.Domain.Catalog.Taxonomies.Taxa.TaxonErrors.NotFound(command.TaxonId);

            var incomingRules = command.Request.Rules;
            var processedRuleIds = new HashSet<Guid>();
            var changesMade = false;

            // 1. Process Updates and Adds
            foreach (var incoming in incomingRules)
            {
                if (incoming.Id.HasValue)
                {
                    var existing = taxon.TaxonRules.FirstOrDefault(r => r.Id == incoming.Id.Value);
                    if (existing != null)
                    {
                        var updateResult = taxon.UpdateRule(existing.Id, incoming.Type, incoming.Value, incoming.MatchPolicy, incoming.PropertyName);
                        if (updateResult.IsError) return updateResult.Errors;
                        
                        context.Set<TaxonRule>().Update(existing);
                        processedRuleIds.Add(existing.Id);
                        changesMade = true;
                        continue;
                    }
                }

                // Add new rule
                var addResult = taxon.AddRule(incoming.Type, incoming.Value, incoming.MatchPolicy, incoming.PropertyName);
                if (addResult.IsError) return addResult.Errors;

                context.Set<TaxonRule>().Add(addResult.Value);
                processedRuleIds.Add(addResult.Value.Id);
                changesMade = true;
            }

            // 2. Process Removes
            var rulesToRemove = taxon.TaxonRules
                .Where(r => !processedRuleIds.Contains(r.Id))
                .ToList();

            if (rulesToRemove.Any())
            {
                foreach (var rule in rulesToRemove)
                {
                    var removeResult = taxon.RemoveRule(rule.Id);
                    if (removeResult.IsError) return removeResult.Errors;
                    context.Set<TaxonRule>().Remove(rule);
                }
                changesMade = true;
            }

            if (changesMade)
            {
                context.Set<Domain.Catalog.Taxonomies.Taxa.Taxon>().Update(taxon);
                await context.SaveChangesAsync(ct);
                // Regeneration via events
            }

            var responseTaxon = await context.Set<Domain.Catalog.Taxonomies.Taxa.Taxon>()
                .Include(t => t.TaxonRules)
                .FirstAsync(t => t.Id == command.TaxonId, ct);

            var rulesResponse = responseTaxon.TaxonRules
                .Select(r => new TaxonRuleResponse
                {
                    Id = r.Id,
                    Type = r.Type,
                    Value = r.Value,
                    MatchPolicy = r.MatchPolicy,
                    PropertyName = r.PropertyName
                })
                .ToList();

            return new Response(responseTaxon.Id, rulesResponse);
        }
    }
}
