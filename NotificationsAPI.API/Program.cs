using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NotificationsAPI.Infrastructure.DependencyInjection; // Mantém a sua injeção

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Injetando as dependências da sua camada de infraestrutura
        services.AddInfrastructure();
    })
    .Build();

host.Run();