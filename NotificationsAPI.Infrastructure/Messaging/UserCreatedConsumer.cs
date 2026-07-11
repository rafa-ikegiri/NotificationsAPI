using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationsAPI.Contracts.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationsAPI.Infrastructure.Messaging;

public class UserCreatedConsumer
{
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(
        ILogger<UserCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public void Start()
    {
        _logger.LogInformation(
            "Consumer UserCreated iniciado.");

        var factory = new ConnectionFactory
        {
            HostName =
        Environment.GetEnvironmentVariable(
            "RABBITMQ_HOST")
        ?? "localhost"
        };

        IConnection? connection = null;

        while (connection is null)
        {
            try
            {
                connection = factory.CreateConnection();

                _logger.LogInformation(
                    "Conectado ao RabbitMQ.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "RabbitMQ indisponível. Nova tentativa em 5 segundos.");

                Thread.Sleep(5000);
            }
        }

        var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: "user-created-exchange",
            type: ExchangeType.Fanout,
            durable: true);

        channel.QueueDeclare(
            queue: "notifications-user-created",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueBind(
            queue: "notifications-user-created",
            exchange: "user-created-exchange",
            routingKey: "");

        var consumer =
            new EventingBasicConsumer(channel);

        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();

            var message =
                Encoding.UTF8.GetString(body);

            var user =
                JsonSerializer.Deserialize<UserCreatedEvent>(
                    message);

            if (user is null)
            {
                return;
            }

            _logger.LogInformation(
                "Email de boas-vindas enviado para {Email}",
                user.Email);
        };

        channel.BasicConsume(
            queue: "notifications-user-created",
            autoAck: true,
            consumer: consumer);
    }
}