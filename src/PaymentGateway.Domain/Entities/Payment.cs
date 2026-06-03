using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Domain.Entities;

public class Payment
{
    public Guid Id { get; }
    public PaymentStatus Status { get; }
    public string CardNumberLastFour { get; }
    public ExpiryDate ExpiryDate { get; }
    public Money Money { get; }

    private Payment(Guid id, PaymentStatus status, string cardNumberLastFour, ExpiryDate expiryDate, Money money)
    {
        Id = id;
        Status = status;
        CardNumberLastFour = cardNumberLastFour;
        ExpiryDate = expiryDate;
        Money = money;
    }

    public static Payment Create(PaymentStatus status, CardNumber cardNumber, ExpiryDate expiryDate, Money money)
        => new(Guid.NewGuid(), status, cardNumber.LastFour, expiryDate, money);
}
