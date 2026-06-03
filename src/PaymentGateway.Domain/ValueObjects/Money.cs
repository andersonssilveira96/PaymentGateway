using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.ValueObjects;

public record Money
{
    public int Amount { get; }
    public Currency Currency { get; }

    public Money(int amount, Currency currency)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        Amount = amount;
        Currency = currency;
    }
}
