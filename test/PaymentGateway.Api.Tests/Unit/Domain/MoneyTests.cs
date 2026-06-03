using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class MoneyTests
{
    [Theory]
    [InlineData(1, Currency.USD)]
    [InlineData(100, Currency.EUR)]
    [InlineData(1050, Currency.BRL)]
    public void Should_Create_Valid_Money(int amount, Currency currency)
    {
        // Act
        var money = new Money(amount, currency);

        // Assert
        Assert.Equal(amount, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Should_Accept_Any_Positive_Amount(int amount)
    {
        // Act
        var money = new Money(amount, Currency.USD);

        // Assert
        Assert.Equal(amount, money.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Should_Throw_When_Amount_Is_Not_Positive(int amount)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new Money(amount, Currency.USD));
    }
}
