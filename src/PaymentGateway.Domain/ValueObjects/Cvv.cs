using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.ValueObjects;

public record Cvv
{
    public string Value { get; }

    public Cvv(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || (value.Length != 3 && value.Length != 4)
            || !value.All(char.IsDigit))
            throw new DomainException("CVV must be 3 or 4 numeric characters.");

        Value = value;
    }
}
