using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.ValueObjects;

public record CardNumber
{
    public string Value { get; }
    public string LastFour => Value[^4..];

    public CardNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length < 14
            || value.Length > 19
            || !value.All(char.IsDigit))
            throw new DomainException("Card number must be between 14 and 19 numeric characters.");

        Value = value;
    }
}
