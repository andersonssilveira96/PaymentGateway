using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Domain.Exceptions;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Domain.ValueObjects;

namespace PaymentGateway.Application.Payments;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IBankSimulatorClient _bankClient;

    public PaymentService(IPaymentRepository repository, IBankSimulatorClient bankClient)
    {
        _repository = repository;
        _bankClient = bankClient;
    }

    public async Task<PaymentResult> ProcessAsync(ProcessPaymentRequest request)
    {
        try
        {
            if (!Enum.TryParse<Currency>(request.Currency, ignoreCase: true, out var currency))
                throw new DomainException("Currency must be one of: USD, EUR, BRL.");

            var cardNumber = new CardNumber(request.CardNumber);
            var expiryDate = new ExpiryDate(request.ExpiryMonth, request.ExpiryYear);
            var money = new Money(request.Amount, currency);
            _ = new Cvv(request.Cvv);

            var bankResponse = await _bankClient.ProcessPaymentAsync(request);
            var status = bankResponse.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;

            var payment = Payment.Create(status, cardNumber, expiryDate, money);
            _repository.Add(payment);

            return ToResult(payment);
        }
        catch (DomainException)
        {
            return PaymentResult.CreateRejected();
        }
    }

    public Task<PaymentResult?> GetAsync(Guid id)
    {
        var payment = _repository.Get(id);
        return Task.FromResult(payment is null ? null : ToResult(payment));
    }

    private static PaymentResult ToResult(Payment p) =>
        new(p.Id, p.Status, p.CardNumberLastFour,
            p.ExpiryDate.Month, p.ExpiryDate.Year,
            p.Money.Currency.ToString(), p.Money.Amount);
}
