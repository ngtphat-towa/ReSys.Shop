namespace ReSys.Core.Features.Inventories.Services.Fulfillment;

public enum FulfillmentStrategyType
{
    Greedy,         // Minimum splits, prioritize default
    CostOptimized,  // Minimize shipping + handling costs (Future)
    Nearest         // Shortest geographic distance (Future)
}
