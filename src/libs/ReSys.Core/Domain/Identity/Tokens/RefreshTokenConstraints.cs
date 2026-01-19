namespace ReSys.Core.Domain.Identity.Tokens;

public static class RefreshTokenConstraints
{
    public const int TokenBytes = 64;
    public const int IpAddressMaxLength = 45;
    public const int TokenHashMaxLength = 255;
    public const int RevokedReasonMaxLength = 500;
}