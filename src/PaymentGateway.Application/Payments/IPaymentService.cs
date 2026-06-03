namespace PaymentGateway.Application.Payments;

public interface IPaymentService
{
    Task<PaymentResult> ProcessAsync(ProcessPaymentRequest request);
    Task<PaymentResult?> GetAsync(Guid id);
}
