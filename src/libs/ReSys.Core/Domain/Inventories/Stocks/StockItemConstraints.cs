namespace ReSys.Core.Domain.Inventories.Stocks;

public static class StockItemConstraints
{
    public const int MinQuantity = -1_000_000; // Allows for extreme backorders but prevents overflow
    public const int MaxQuantity = 1_000_000;
    public const int SkuMaxLength = 50;
    
    public const int DefaultMaxBackorderQuantity = 100;
    
    // Movement constraints
    public static class Movements
    {
        public const int MaxReasonLength = 500;
        public const int MaxReferenceLength = 100;
        public const string FulfillmentReason = "Order fulfillment";
    }
}
