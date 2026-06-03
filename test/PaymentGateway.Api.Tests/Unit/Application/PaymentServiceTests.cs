using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Payments;
using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.ValueObjects;
using Currency = PaymentGateway.Domain.Enums.Currency;

namespace PaymentGateway.Api.Tests.Unit.Application;

public class PaymentServiceTests
{
    private readonly IPaymentRepository _repository;
    private readonly IBankSimulatorClient _bankClient;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _repository = Substitute.For<IPaymentRepository>();
        _bankClient = Substitute.For<IBankSimulatorClient>();
        _service = new PaymentService(_repository, _bankClient);
    }

    private static ProcessPaymentRequest ValidRequest() => new(
        CardNumber: "2222405343248877",
        ExpiryMonth: 12,
        ExpiryYear: 2030,
        Currency: "USD",
        Amount: 100,
        Cvv: "123"
    );

    [Fact]
    public async Task ProcessAsync_Should_Return_Authorized_When_Bank_Authorizes()
    {
        // Arrange
        _bankClient.ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>())
            .Returns(new BankSimulatorResponse(true, "auth-code-123"));

        // Act
        var result = await _service.ProcessAsync(ValidRequest());

        // Assert
        Assert.Equal(PaymentStatus.Authorized, result.Status);
    }

    [Fact]
    public async Task ProcessAsync_Should_Return_Declined_When_Bank_Declines()
    {
        // Arrange
        _bankClient.ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>())
            .Returns(new BankSimulatorResponse(false, null));

        // Act
        var result = await _service.ProcessAsync(ValidRequest());

        // Assert
        Assert.Equal(PaymentStatus.Declined, result.Status);
    }

    [Fact]
    public async Task ProcessAsync_Should_Store_Payment_After_Bank_Responds()
    {
        // Arrange
        _bankClient.ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>())
            .Returns(new BankSimulatorResponse(true, "auth-code-123"));

        // Act
        await _service.ProcessAsync(ValidRequest());

        // Assert
        _repository.Received(1).Add(Arg.Any<Payment>());
    }

    [Fact]
    public async Task ProcessAsync_Should_Return_Correct_Card_And_Expiry_Details()
    {
        // Arrange
        _bankClient.ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>())
            .Returns(new BankSimulatorResponse(true, "auth-code-123"));

        // Act
        var result = await _service.ProcessAsync(ValidRequest());

        // Assert
        Assert.Equal("8877", result.CardNumberLastFour);
        Assert.Equal(12, result.ExpiryMonth);
        Assert.Equal(2030, result.ExpiryYear);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(100, result.Amount);
    }

    [Fact]
    public async Task ProcessAsync_Should_Propagate_BankUnavailableException()
    {
        // Arrange
        _bankClient.ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>())
            .ThrowsAsync(new BankUnavailableException());

        // Act & Assert
        await Assert.ThrowsAsync<BankUnavailableException>(() => _service.ProcessAsync(ValidRequest()));
    }

    [Theory]
    [InlineData("1234567890123")]         // 13 digits — too short
    [InlineData("12345678901234567890")]  // 20 digits — too long
    [InlineData("1234abcd56789012")]      // contains letters
    [InlineData("")]                      // empty
    public async Task ProcessAsync_Should_Return_Rejected_When_CardNumber_Is_Invalid(string cardNumber)
    {
        // Arrange
        var request = ValidRequest() with { CardNumber = cardNumber };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected, result.Status);
        _ = _bankClient.DidNotReceive().ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>());
    }

    [Theory]
    [InlineData("12")]     // 2 digits — too short
    [InlineData("12345")]  // 5 digits — too long
    [InlineData("abc")]    // non-numeric
    [InlineData("")]       // empty
    public async Task ProcessAsync_Should_Return_Rejected_When_Cvv_Is_Invalid(string cvv)
    {
        // Arrange
        var request = ValidRequest() with { Cvv = cvv };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected, result.Status);
        _ = _bankClient.DidNotReceive().ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>());
    }

    [Fact]
    public async Task ProcessAsync_Should_Return_Rejected_When_ExpiryDate_Is_In_The_Past()
    {
        // Arrange
        var request = ValidRequest() with { ExpiryMonth = 1, ExpiryYear = 2020 };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected, result.Status);
        _ = _bankClient.DidNotReceive().ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public async Task ProcessAsync_Should_Return_Rejected_When_Amount_Is_Not_Positive(int amount)
    {
        // Arrange
        var request = ValidRequest() with { Amount = amount };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected, result.Status);
        _ = _bankClient.DidNotReceive().ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>());
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("")]
    public async Task ProcessAsync_Should_Return_Rejected_When_Currency_Is_Not_Supported(string currency)
    {
        // Arrange
        var request = ValidRequest() with { Currency = currency };

        // Act
        var result = await _service.ProcessAsync(request);

        // Assert
        Assert.Equal(PaymentStatus.Rejected, result.Status);
        _ = _bankClient.DidNotReceive().ProcessPaymentAsync(Arg.Any<ProcessPaymentRequest>());
    }

    [Fact]
    public async Task GetAsync_Should_Return_PaymentResult_When_Payment_Exists()
    {
        // Arrange
        var payment = Payment.Create(
            PaymentStatus.Authorized,
            new CardNumber("2222405343248877"),
            new ExpiryDate(12, 2030),
            new Money(100, Currency.USD)
        );
        _repository.Get(payment.Id).Returns(payment);

        // Act
        var result = await _service.GetAsync(payment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(payment.Id, result.Id);
        Assert.Equal(PaymentStatus.Authorized, result.Status);
        Assert.Equal("8877", result.CardNumberLastFour);
    }

    [Fact]
    public async Task GetAsync_Should_Return_Null_When_Payment_Does_Not_Exist()
    {
        // Arrange
        _repository.Get(Arg.Any<Guid>()).Returns((Payment?)null);

        // Act
        var result = await _service.GetAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }
}
