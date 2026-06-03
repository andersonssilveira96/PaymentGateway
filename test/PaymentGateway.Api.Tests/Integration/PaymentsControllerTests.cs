using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Application.Payments;
using PaymentGateway.Application.Services;

namespace PaymentGateway.Api.Tests.Integration;

public class PaymentsControllerTests
{
    private static WebApplicationFactory<Program> CreateFactory(bool bankAuthorizes = true) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBankSimulatorClient));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddSingleton<IBankSimulatorClient>(new StubBankSimulatorClient(bankAuthorizes));
                }));

    [Fact]
    public async Task RetrievesAPaymentSuccessfully()
    {
        // Arrange
        var client = CreateFactory().CreateClient();
        var postRequest = new PostPaymentRequest
        {
            CardNumber = "2222405343248877",
            ExpiryMonth = 4,
            ExpiryYear = 2030,
            Currency = "BRL",
            Amount = 100,
            Cvv = "123"
        };
        var postResponse = await client.PostAsJsonAsync("/api/Payments", postRequest);
        var created = await postResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Act
        var response = await client.GetAsync($"/api/Payments/{created!.Id}");
        var paymentResponse = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(paymentResponse);
    }

    [Fact]
    public async Task Returns404IfPaymentNotFound()
    {
        // Arrange
        var client = CreateFactory().CreateClient();

        // Act
        var response = await client.GetAsync($"/api/Payments/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("""{ "cardNumber": "2222405343248877", "expiryMonth": 4, "expiryYear": 2030, "currency": "BRL", "amount": 10.50, "cvv": "123" }""")]
    [InlineData("""{ "cardNumber": "2222405343248877", "expiryMonth": 4, "expiryYear": 2030, "currency": "BRL", "amount": "abc", "cvv": "123" }""")]
    public async Task Returns400_When_Amount_Is_Not_An_Integer(string json)
    {
        // Arrange
        var client = CreateFactory().CreateClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/Payments", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed class StubBankSimulatorClient : IBankSimulatorClient
    {
        private readonly bool _authorized;
        public StubBankSimulatorClient(bool authorized) => _authorized = authorized;
        public Task<BankSimulatorResponse> ProcessPaymentAsync(ProcessPaymentRequest request) =>
            Task.FromResult(new BankSimulatorResponse(_authorized, "stub-auth-code"));
    }
}
