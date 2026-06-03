using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.ValueObjects;

public record ExpiryDate
{
    public int Month { get; }
    public int Year { get; }

    public ExpiryDate(int month, int year)
    {
        if (month < 1 || month > 12)
            throw new DomainException("Expiry month must be between 1 and 12.");

        var now = DateTime.UtcNow;
        if (year < now.Year || (year == now.Year && month < now.Month))
            throw new DomainException("Card expiry date must be in the future.");

        Month = month;
        Year = year;
    }
}
