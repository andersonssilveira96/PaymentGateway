using PaymentGateway.Application.Payments;

namespace PaymentGateway.Application.Services;

public interface IBankSimulatorClient
{
    Task<BankSimulatorResponse> ProcessPaymentAsync(ProcessPaymentRequest request);
}

public record BankSimulatorResponse(bool Authorized, string? AuthorizationCode);
