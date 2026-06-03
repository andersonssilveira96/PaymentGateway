using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Api.Tests.Unit.Domain;

public class ExpiryDateTests
{
    [Fact]
    public void Should_Create_Valid_ExpiryDate()
    {
        // Act
        var expiryDate = new ExpiryDate(12, 2030);

        // Assert
        Assert.Equal(12, expiryDate.Month);
        Assert.Equal(2030, expiryDate.Year);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Should_Accept_All_Valid_Months(int month)
    {
        // Act
        var expiryDate = new ExpiryDate(month, 2030);

        // Assert
        Assert.Equal(month, expiryDate.Month);
    }

    [Theory]
    [InlineData(0)]   // below minimum
    [InlineData(13)]  // above maximum
    public void Should_Throw_When_Month_Is_Out_Of_Range(int month)
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new ExpiryDate(month, 2030));
    }

    [Fact]
    public void Should_Throw_When_Year_Is_In_The_Past()
    {
        // Act & Assert
        Assert.Throws<DomainException>(() => new ExpiryDate(1, 2020));
    }

    [Fact]
    public void Should_Throw_When_Month_And_Year_Combination_Is_In_The_Past()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var pastMonth = now.Month == 1 ? 12 : now.Month - 1;
        var pastMonthYear = now.Month == 1 ? now.Year - 1 : now.Year;

        // Act & Assert
        Assert.Throws<DomainException>(() => new ExpiryDate(pastMonth, pastMonthYear));
    }
}
