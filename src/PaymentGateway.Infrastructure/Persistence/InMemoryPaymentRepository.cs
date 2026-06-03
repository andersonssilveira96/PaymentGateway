using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Repositories;

namespace PaymentGateway.Infrastructure.Persistence;

public class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly List<Payment> _payments = [];

    public void Add(Payment payment) => _payments.Add(payment);

    public Payment? Get(Guid id) => _payments.FirstOrDefault(p => p.Id == id);
}
