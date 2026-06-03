using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class CvvTests
{
    [Theory]
    [InlineData("123")]
    [InlineData("1234")]
    public void Should_Create_Valid_Cvv(string value)
    {
        // Act
        var cvv = new Cvv(value);

        // Assert
        Assert.Equal(value, cvv.Value);
    }

    [Theory]
    [InlineData("12")]      // 2 digits — too short
    [InlineData("12345")]   // 5 digits — too long
    [InlineData("12a")]     // contains letter
    [InlineData("12 3")]    // contains space
    [InlineData("")]        // empty
    [InlineData("   ")]     // whitespace only
    public void Should_Throw_DomainException_For_Invalid_Cvv(string value)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new Cvv(value));
    }

    [Fact]
    public void Should_Throw_DomainException_For_Null_Cvv()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new Cvv(null!));
    }
}
