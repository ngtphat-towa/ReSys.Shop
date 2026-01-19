using ErrorOr;

using Microsoft.Extensions.DependencyInjection;

namespace ReSys.Core.Features.Inventories.Services.Fulfillment;

/// <summary>
/// Factory responsible for providing the correct fulfillment algorithm at runtime.
/// </summary>
public class FulfillmentStrategyFactory(IServiceProvider serviceProvider)
{
    public ErrorOr<IFulfillmentStrategy> GetStrategy(FulfillmentStrategyType type)
    {
        return type switch
        {
            FulfillmentStrategyType.Greedy => serviceProvider.GetRequiredService<GreedyFulfillmentStrategy>(),

            // Placeholder for future strategies
            FulfillmentStrategyType.CostOptimized => throw new NotImplementedException("CostOptimized strategy coming in Phase 2"),
            FulfillmentStrategyType.Nearest => throw new NotImplementedException("Nearest location strategy coming in Phase 2"),

            _ => throw new ArgumentException($"Strategy {type} is not supported.", nameof(type))
        };
    }
}
