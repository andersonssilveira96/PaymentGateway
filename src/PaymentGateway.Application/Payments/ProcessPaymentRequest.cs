namespace PaymentGateway.Application.Payments;

public record ProcessPaymentRequest(
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string Currency,
    int Amount,
    string Cvv);
