using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Payments;
using PaymentGateway.Application.Services;

namespace PaymentGateway.Api.Tests.Integration;

public class AmountBoundaryTests
{
    private static HttpClient CreateClient() =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBankSimulatorClient));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddSingleton<IBankSimulatorClient>(new StubBankSimulatorClient());
                }))
            .CreateClient();

    private static StringContent Json(string body) =>
        new(body, Encoding.UTF8, "application/json");

    private const string ValidBase = """
        "cardNumber": "2222405343248877",
        "expiryMonth": 4,
        "expiryYear": 2030,
        "currency": "USD",
        "cvv": "123"
        """;

    [Theory]
    [InlineData("""{ %%BASE%%, "amount": 10.50 }""",  "decimal with dot")]
    [InlineData("""{ %%BASE%%, "amount": 10.0 }""",   "decimal .0 (whole number as float)")]
    [InlineData("""{ %%BASE%%, "amount": "10,50" }""", "string with comma")]
    [InlineData("""{ %%BASE%%, "amount": "10.50" }""", "string with dot")]
    [InlineData("""{ %%BASE%%, "amount": "abc" }""",   "non-numeric string")]
    [InlineData("""{ %%BASE%%, "amount": null }""",    "null")]
    public async Task Returns400_When_Amount_Cannot_Be_Parsed_As_Integer(string template, string scenario)
    {
        // Arrange
        var client = CreateClient();
        var json = template.Replace("%%BASE%%", ValidBase);

        // Act
        var response = await client.PostAsync("/api/Payments", Json(json));

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected 400 for scenario '{scenario}' but got {(int)response.StatusCode} {response.StatusCode}");
    }

    [Theory]
    [InlineData("""{ %%BASE%%, "amount": -1 }""",  "negative integer")]
    [InlineData("""{ %%BASE%%, "amount": 0 }""",   "zero")]
    public async Task Returns422_When_Amount_Is_Non_Positive_Integer(string template, string scenario)
    {
        // Arrange
        var client = CreateClient();
        var json = template.Replace("%%BASE%%", ValidBase);

        // Act
        var response = await client.PostAsync("/api/Payments", Json(json));

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.UnprocessableEntity,
            $"Expected 422 for scenario '{scenario}' but got {(int)response.StatusCode} {response.StatusCode}");
    }

    private sealed class StubBankSimulatorClient : IBankSimulatorClient
    {
        public Task<BankSimulatorResponse> ProcessPaymentAsync(ProcessPaymentRequest request) =>
            Task.FromResult(new BankSimulatorResponse(true, "stub-auth-code"));
    }
}
