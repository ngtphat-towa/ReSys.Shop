using ErrorOr;
using ReSys.Core.Domain.Ordering;

namespace ReSys.Core.Domain.Promotions.Calculations;

/// <summary>
/// Service responsible for validating eligibility and calculating discounts for promotions.
/// </summary>
public interface IPromotionCalculator
{
    /// <summary>
    /// Evaluates a promotion against an order and returns the resulting adjustments.
    /// </summary>
    ErrorOr<PromotionCalculationResult> Calculate(Promotion promotion, Order order);
}