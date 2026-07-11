
using NotificationsAPI.Infrastructure.DependencyInjection;
using NotificationsAPI.Infrastructure.Messaging;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
    app.MapOpenApi();


app.UseHttpsRedirection();


var paymentConsumer =
    app.Services.GetRequiredService<PaymentProcessedConsumer>();

paymentConsumer.Start();

var userConsumer =
    app.Services.GetRequiredService<UserCreatedConsumer>();

userConsumer.Start();

app.Run();

