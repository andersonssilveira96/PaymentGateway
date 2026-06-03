namespace PaymentGateway.Application.Exceptions;

public class BankUnavailableException : Exception
{
    public BankUnavailableException() : base("The acquiring bank is currently unavailable.") { }
}
