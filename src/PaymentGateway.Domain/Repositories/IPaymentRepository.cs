using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Domain.Repositories;

public interface IPaymentRepository
{
    void Add(Payment payment);
    Payment? Get(Guid id);
}
