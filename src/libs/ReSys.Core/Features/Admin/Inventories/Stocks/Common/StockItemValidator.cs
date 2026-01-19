using FluentValidation;
using ReSys.Core.Domain.Inventories.Stocks;

namespace ReSys.Core.Features.Admin.Inventories.Stocks.Common;

public class StockAdjustmentValidator : AbstractValidator<StockAdjustmentRequest>
{
    public StockAdjustmentValidator()
    {
        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage(StockItemErrors.ZeroQuantityMovement.Description);
            
        RuleFor(x => x.Type).IsInEnum();
        
        RuleFor(x => x.Reason)
            .MaximumLength(StockItemConstraints.Movements.MaxReasonLength);
            
        RuleFor(x => x.Reference)
            .MaximumLength(StockItemConstraints.Movements.MaxReferenceLength);
            
        RuleFor(x => x.UnitCost)
            .GreaterThanOrEqualTo(0);
    }
}

public class BackorderPolicyValidator : AbstractValidator<BackorderPolicyRequest>
{
    public BackorderPolicyValidator()
    {
        RuleFor(x => x.BackorderLimit).GreaterThanOrEqualTo(0);
    }
}
