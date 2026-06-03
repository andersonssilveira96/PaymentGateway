using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Services;
using PaymentGateway.Domain.Repositories;
using PaymentGateway.Infrastructure.BankSimulator;
using PaymentGateway.Infrastructure.Persistence;

namespace PaymentGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();

        var bankSimulatorUrl = configuration["BankSimulatorUrl"] ?? "http://localhost:8080";
        services.AddHttpClient<IBankSimulatorClient, BankSimulatorClient>(client =>
            client.BaseAddress = new Uri(bankSimulatorUrl));

        return services;
    }
}
