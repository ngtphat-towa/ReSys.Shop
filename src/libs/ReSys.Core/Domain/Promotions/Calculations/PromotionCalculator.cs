using ErrorOr;
using ReSys.Core.Domain.Ordering;
using ReSys.Core.Domain.Ordering.LineItems;
using ReSys.Core.Domain.Promotions.Actions;
using ReSys.Core.Domain.Promotions.Rules;

namespace ReSys.Core.Domain.Promotions.Calculations;

public sealed class PromotionCalculator : IPromotionCalculator
{
    public ErrorOr<PromotionCalculationResult> Calculate(Promotion promotion, Order order)
    {
        // 1. Core Eligibility (Status, Timing, Usage)
        if (!promotion.IsActive) return PromotionErrors.NotActive;
        if (promotion.IsExpired) return PromotionErrors.Expired;
        if (promotion.StartsAt.HasValue && promotion.StartsAt > DateTimeOffset.UtcNow) return PromotionErrors.NotStarted;
        if (promotion.UsageLimit.HasValue && promotion.UsageCount >= promotion.UsageLimit) return PromotionErrors.UsageLimitReached;

        // 2. Threshold Check (Minimum Order Value)
        if (promotion.MinimumOrderAmount.HasValue && (order.ItemTotalCents / 100m) < promotion.MinimumOrderAmount.Value)
            return PromotionErrors.MinimumOrderNotMet(promotion.MinimumOrderAmount.Value);

        // 3. Rule Evaluation (Global Eligibility)
        foreach (var rule in promotion.Rules)
        {
            if (!EvaluateRule(rule, order))
                return Error.Validation("Promotion.RulesNotMet", $"Rule of type {rule.Type} was not satisfied.");
        }

        // 4. Item-Level Filtering
        var eligibleItems = GetEligibleItems(promotion, order);
        
        if (!eligibleItems.Any())
            return Error.Validation("Promotion.NoEligibleItems", "The items in the cart do not qualify for this promotion.");

        // 5. Action Execution
        if (promotion.Action is null) return PromotionErrors.ActionRequired;

        var context = new PromotionCalculationContext(promotion, order, eligibleItems);
        var adjustments = CalculateAction(promotion.Action, context);

        // 6. Discount Capping
        var finalAdjustments = ApplyDiscountCap(adjustments, promotion);

        return new PromotionCalculationResult(promotion.Id, finalAdjustments);
    }

    private bool EvaluateRule(PromotionRule rule, Order order)
    {
        return rule.Type switch
        {
            PromotionRule.RuleType.MinimumQuantity => order.LineItems.Sum(li => li.Quantity) >= (rule.Parameters.Threshold ?? 0),
            PromotionRule.RuleType.ProductInclude => order.LineItems.Any(li => rule.Parameters.TargetIds.Contains(li.VariantId)),
            PromotionRule.RuleType.ProductExclude => !order.LineItems.Any(li => rule.Parameters.TargetIds.Contains(li.VariantId)),
            PromotionRule.RuleType.CategoryInclude => order.LineItems.Any(li => li.Variant.Product != null && li.Variant.Product.Classifications.Any(c => rule.Parameters.TargetIds.Contains(c.TaxonId))),
            _ => true 
        };
    }

    private List<LineItem> GetEligibleItems(Promotion promotion, Order order)
    {
        var query = order.LineItems.AsEnumerable();

        foreach (var rule in promotion.Rules)
        {
            query = rule.Type switch
            {
                PromotionRule.RuleType.ProductInclude => query.Where(li => rule.Parameters.TargetIds.Contains(li.VariantId)),
                PromotionRule.RuleType.ProductExclude => query.Where(li => !rule.Parameters.TargetIds.Contains(li.VariantId)),
                PromotionRule.RuleType.CategoryInclude => query.Where(li => li.Variant.Product != null && li.Variant.Product.Classifications.Any(c => rule.Parameters.TargetIds.Contains(c.TaxonId))),
                PromotionRule.RuleType.CategoryExclude => query.Where(li => li.Variant.Product == null || !li.Variant.Product.Classifications.Any(c => rule.Parameters.TargetIds.Contains(c.TaxonId))),
                _ => query
            };
        }

        return query.ToList();
    }

    private List<PromotionAdjustment> CalculateAction(PromotionAction action, PromotionCalculationContext context)
    {
        var result = new List<PromotionAdjustment>();
        var p = action.Parameters;

        switch (action.Type)
        {
            case Promotion.PromotionType.OrderDiscount:
                var totalItemTotalCents = context.Order.ItemTotalCents;
                var orderDiscount = p.DiscountType == Promotion.DiscountType.Percentage
                    ? (long)(totalItemTotalCents * (p.Value / 100m))
                    : (long)(p.Value * 100);
                
                result.Add(new PromotionAdjustment($"Discount: {context.Promotion.Name}", -orderDiscount));
                break;

            case Promotion.PromotionType.ItemDiscount:
                var baseAmountCents = context.EligibleItems.Sum(li => li.GetTotalCents());
                if (baseAmountCents <= 0) break;

                var totalDiscountCents = p.DiscountType == Promotion.DiscountType.Percentage
                    ? (long)(baseAmountCents * (p.Value / 100m))
                    : (long)(p.Value * 100);

                totalDiscountCents = Math.Min(totalDiscountCents, baseAmountCents);
                var remainingDiscountCents = totalDiscountCents;

                for (int i = 0; i < context.EligibleItems.Count; i++)
                {
                    var item = context.EligibleItems[i];
                    long itemDiscountCents;

                    if (i == context.EligibleItems.Count - 1)
                    {
                        itemDiscountCents = remainingDiscountCents;
                    }
                    else
                    {
                        decimal proportion = (decimal)item.GetTotalCents() / baseAmountCents;
                        itemDiscountCents = (long)Math.Round(totalDiscountCents * proportion);
                        remainingDiscountCents -= itemDiscountCents;
                    }

                    if (itemDiscountCents > 0)
                    {
                        result.Add(new PromotionAdjustment(
                            Description: $"Discount: {context.Promotion.Name}", 
                            AmountCents: -itemDiscountCents, 
                            LineItemId: item.Id));
                    }
                }
                break;

            case Promotion.PromotionType.FreeShipping:
                result.Add(new PromotionAdjustment($"Free Shipping: {context.Promotion.Name}", -context.Order.ShipmentTotalCents));
                break;
        }

        return result;
    }

    private List<PromotionAdjustment> ApplyDiscountCap(List<PromotionAdjustment> adjustments, Promotion promotion)
    {
        if (!promotion.MaximumDiscountAmount.HasValue) return adjustments;

        var totalDiscountCents = -adjustments.Sum(a => a.AmountCents);
        var capCents = (long)(promotion.MaximumDiscountAmount.Value * 100);

        if (totalDiscountCents <= capCents) return adjustments;

        return new List<PromotionAdjustment> 
        { 
            new PromotionAdjustment($"Discount: {promotion.Name} (Capped)", -capCents) 
        };
    }
}
