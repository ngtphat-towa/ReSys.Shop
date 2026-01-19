namespace ReSys.Core.Domain.Ordering.Shipments;

public static class ShipmentConstraints
{
    public const int NumberMaxLength = 50;
    public const int TrackingNumberMaxLength = 100;
    public const int PackageIdMaxLength = 255;
    
    public const string NumberPrefix = "SHP-";
}
