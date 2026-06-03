using System.Text.Json.Serialization;

namespace PaymentGateway.Infrastructure.BankSimulator;

internal record BankSimulatorApiResponse(
    [property: JsonPropertyName("authorized")] bool Authorized,
    [property: JsonPropertyName("authorization_code")] string? AuthorizationCode);
