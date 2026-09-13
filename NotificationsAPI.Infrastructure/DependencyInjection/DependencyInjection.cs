using Microsoft.Extensions.DependencyInjection;

namespace NotificationsAPI.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {      
        return services;
    }
}