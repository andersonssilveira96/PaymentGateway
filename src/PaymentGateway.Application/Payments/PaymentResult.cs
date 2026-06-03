using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Payments;

public record PaymentResult(
    Guid Id,
    PaymentStatus Status,
    string? CardNumberLastFour,
    int ExpiryMonth,
    int ExpiryYear,
    string? Currency,
    int Amount)
{
    public static PaymentResult CreateRejected() =>
        new(Guid.Empty, PaymentStatus.Rejected, null, 0, 0, null, 0);
}
