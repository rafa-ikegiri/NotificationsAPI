using Microsoft.Extensions.DependencyInjection;
using NotificationsAPI.Infrastructure.Messaging;

namespace NotificationsAPI.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddSingleton<PaymentProcessedConsumer>();
        services.AddSingleton<UserCreatedConsumer>();

        return services;
    }
}