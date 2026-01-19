using ErrorOr;

namespace ReSys.Core.Domain.Ordering.Adjustments;

public static class LineItemAdjustmentErrors
{
    public static Error DescriptionRequired => Error.Validation(
        code: "LineItemAdjustment.DescriptionRequired",
        description: "Adjustment description is required.");
}
