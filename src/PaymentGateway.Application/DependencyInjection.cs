using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Payments;

namespace PaymentGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}
