using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Api.Models.Responses;

/// <summary>Result of a processed payment.</summary>
public class PostPaymentResponse
{
    /// <summary>Unique payment identifier — use this to retrieve the payment later.</summary>
    public Guid Id { get; set; }

    /// <summary>Outcome of the payment: Authorized or Declined.</summary>
    /// <example>Authorized</example>
    public PaymentStatus Status { get; set; }

    /// <summary>Last four digits of the card number used for the payment.</summary>
    /// <example>8877</example>
    public string CardNumberLastFour { get; set; } = default!;

    /// <summary>Card expiry month.</summary>
    /// <example>4</example>
    public int ExpiryMonth { get; set; }

    /// <summary>Card expiry year.</summary>
    /// <example>2030</example>
    public int ExpiryYear { get; set; }

    /// <summary>ISO 4217 currency code.</summary>
    /// <example>BRL</example>
    public string Currency { get; set; } = default!;

    /// <summary>Payment amount in the minor currency unit (e.g. £10.50 = 1050).</summary>
    /// <example>1050</example>
    public int Amount { get; set; }
}
