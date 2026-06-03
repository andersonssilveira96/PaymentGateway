using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class CardNumberTests
{
    [Theory]
    [InlineData("12345678901234")]       // 14 digits — minimum
    [InlineData("1234567890123456789")]  // 19 digits — maximum
    [InlineData("2222405343248877")]     // 16 digits — typical Visa
    public void Should_Create_Valid_CardNumber(string value)
    {
        // Act
        var cardNumber = new CardNumber(value);

        // Assert
        Assert.Equal(value, cardNumber.Value);
    }

    [Theory]
    [InlineData("2222405343248877", "8877")]
    [InlineData("12345678901234", "1234")]
    [InlineData("1234567890123456789", "6789")]
    public void LastFour_Should_Return_Last_Four_Digits(string number, string expectedLastFour)
    {
        // Act
        var cardNumber = new CardNumber(number);

        // Assert
        Assert.Equal(expectedLastFour, cardNumber.LastFour);
    }

    [Theory]
    [InlineData("1234567890123")]          // 13 digits — too short
    [InlineData("12345678901234567890")]   // 20 digits — too long
    [InlineData("1234abcd567890")]         // contains letters
    [InlineData("1234 5678 9012 3456")]    // contains spaces
    [InlineData("")]                       // empty
    [InlineData("   ")]                    // whitespace only
    public void Should_Throw_DomainException_For_Invalid_CardNumber(string value)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new CardNumber(value));
    }

    [Fact]
    public void Should_Throw_DomainException_For_Null_CardNumber()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new CardNumber(null!));
    }
}
