namespace ReSys.Core.Domain.Identity.Permissions;

public static class AccessPermissionConstraints
{
    public const int MaxNameLength = 256;
    public const int MaxSegmentLength = 64; 
    public const int MinSegmentLength = 2;
    public const int MinSegments = 3; // area.resource.action
    
    public const string SegmentAllowedPattern = @"^[a-z0-9_]+$";

    public static class Display
    {
        public const int MaxDisplayNameLength = 256;
        public const int MaxDescriptionLength = 1024;
    }
}