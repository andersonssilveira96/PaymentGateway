using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Requests;

/// <summary>Request body for processing a card payment.</summary>
public class PostPaymentRequest
{
    /// <summary>Full card number — 14 to 19 numeric digits.</summary>
    /// <example>2222405343248877</example>
    [Required]
    public string CardNumber { get; set; } = default!;

    /// <summary>Card expiry month — must be between 1 and 12.</summary>
    /// <example>4</example>
    public int ExpiryMonth { get; set; }

    /// <summary>Card expiry year — the combination of month and year must be in the future.</summary>
    /// <example>2030</example>
    public int ExpiryYear { get; set; }

    /// <summary>ISO 4217 currency code — accepted values: USD, BRL, EUR.</summary>
    /// <example>BRL</example>
    [Required]
    public string Currency { get; set; } = default!;

    /// <summary>Payment amount in the minor currency unit (e.g. £10.50 = 1050).</summary>
    /// <example>1050</example>
    public int Amount { get; set; }

    /// <summary>Card verification value — 3 or 4 numeric digits.</summary>
    /// <example>123</example>
    [Required]
    public string Cvv { get; set; } = default!;
}
