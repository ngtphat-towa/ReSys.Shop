using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Adjustments;

public static class OrderAdjustmentErrors
{
    public static Error DescriptionRequired => Error.Validation(
        code: "OrderAdjustment.DescriptionRequired",
        description: "Adjustment description is required.");
}
