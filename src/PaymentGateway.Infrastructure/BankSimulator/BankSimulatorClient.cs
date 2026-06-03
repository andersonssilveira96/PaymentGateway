using System.Net;
using System.Net.Http.Json;
using Polly;
using Polly.Retry;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Payments;
using PaymentGateway.Application.Services;

namespace PaymentGateway.Infrastructure.BankSimulator;

public class BankSimulatorClient : IBankSimulatorClient
{
    private readonly HttpClient _httpClient;

    // Retry only on connection-level errors where the request is guaranteed not to have reached the bank.
    // HTTP responses (4xx/5xx) are never retried — the bank may have already processed the payment.
    private static readonly ResiliencePipeline<HttpResponseMessage> RetryPipeline =
        new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(300),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>(IsConnectionError)
            })
            .Build();

    public BankSimulatorClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BankSimulatorResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
    {
        var body = new BankSimulatorRequest(
            request.CardNumber,
            $"{request.ExpiryMonth:D2}/{request.ExpiryYear}",
            request.Currency,
            request.Amount,
            request.Cvv);

        var response = await RetryPipeline.ExecuteAsync(
            async ct => await _httpClient.PostAsJsonAsync("/payments", body, ct));

        if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            throw new BankUnavailableException();

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BankSimulatorApiResponse>();
        return new BankSimulatorResponse(result!.Authorized, result.AuthorizationCode);
    }

    private static bool IsConnectionError(HttpRequestException ex) =>
        ex.HttpRequestError is HttpRequestError.ConnectionError
                            or HttpRequestError.NameResolutionError
                            or HttpRequestError.SecureConnectionError;
}
