using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReSys.Core.Common.Data;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa;
using ReSys.Core.Domain.Catalog.Taxonomies.Taxa.Rules;
using ReSys.Core.Features.Catalog.Taxonomies.Services;
using ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.Common;

namespace ReSys.Core.Features.Catalog.Taxonomies.Taxa.Rules.UpdateTaxonRules;

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
        IApplicationDbContext context,
        ITaxonRegenerationService regenerationService,
        ILogger<Handler> logger) : IRequestHandler<Command, ErrorOr<Response>>
    {
        public async Task<ErrorOr<Response>> Handle(Command command, CancellationToken ct)
        {
            try
            {
                var taxon = await context.Set<Taxon>()
                    .Include(t => t.TaxonRules)
                    .FirstOrDefaultAsync(t => t.Id == command.TaxonId && t.TaxonomyId == command.TaxonomyId, ct);

                if (taxon == null)
                    return TaxonErrors.NotFound(command.TaxonId);

                var incomingRules = command.Request.Rules;
                var processedRuleIds = new HashSet<Guid>();
                var changesMade = false;

                // 1. Process Updates and Adds
                foreach (var incoming in incomingRules)
                {
                    if (incoming.Id.HasValue)
                    {
                        // Potential Update
                        var existing = taxon.TaxonRules.FirstOrDefault(r => r.Id == incoming.Id.Value);
                        if (existing != null)
                        {
                            // Check if there are actual changes
                            bool hasChanges = existing.Type != incoming.Type ||
                                            existing.Value != incoming.Value ||
                                            existing.MatchPolicy != incoming.MatchPolicy ||
                                            existing.PropertyName != incoming.PropertyName;

                            if (hasChanges)
                            {
                                var updateResult = existing.Update(incoming.Type, incoming.Value, incoming.MatchPolicy, incoming.PropertyName);
                                if (updateResult.IsError) return updateResult.Errors;
                                changesMade = true;
                            }

                            processedRuleIds.Add(existing.Id);
                            continue;
                        }
                    }

                    // Check if an exact match exists to avoid duplicate adds
                    var exactMatch = taxon.TaxonRules.FirstOrDefault(r =>
                        !processedRuleIds.Contains(r.Id) &&
                        r.Type == incoming.Type &&
                        r.MatchPolicy == incoming.MatchPolicy &&
                        r.PropertyName == incoming.PropertyName &&
                        r.Value == incoming.Value);

                    if (exactMatch != null)
                    {
                        processedRuleIds.Add(exactMatch.Id);
                        continue;
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
                    }
                    changesMade = true;
                }

                // 3. Save and regenerate only if changes were made
                if (changesMade)
                {
                    await context.SaveChangesAsync(ct);

                    var regenResult = await regenerationService.RegenerateProductsForTaxonAsync(taxon.Id, ct);
                    if (regenResult.IsError) return regenResult.Errors;
                }

                // 4. Build response - reload the taxon to get fresh data
                var responseTaxon = await context.Set<Taxon>()
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating rules for taxon {TaxonId}", command.TaxonId);
                return Error.Failure("TaxonRules.UpdateFailed", $"Failed to update taxon rules: {ex.Message}");
            }
        }
    }
}