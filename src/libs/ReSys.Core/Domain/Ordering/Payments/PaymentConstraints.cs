namespace ReSys.Core.Domain.Ordering.Payments;

public static class PaymentConstraints
{
    public const long AmountCentsMinValue = 0;
    public const int CurrencyMaxLength = 3;
    public const int PaymentMethodTypeMaxLength = 50;
    public const int ReferenceTransactionIdMaxLength = 100;
    public const int GatewayAuthCodeMaxLength = 50;
    public const int GatewayErrorCodeMaxLength = 100;
    public const int FailureReasonMaxLength = 500;
}
