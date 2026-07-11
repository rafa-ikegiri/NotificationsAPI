using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NotificationsAPI.Contracts.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationsAPI.Infrastructure.Messaging;

public class PaymentProcessedConsumer
{
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        ILogger<PaymentProcessedConsumer> logger)
    {
        _logger = logger;
    }

    public void Start()
    {
        _logger.LogInformation(
            "Consumer PaymentProcessed iniciado.");

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

        var channel =
            connection.CreateModel();

        channel.ExchangeDeclare(
    exchange: "payment-processed-exchange",
    type: ExchangeType.Fanout,
    durable: true);

        channel.QueueDeclare(
            queue: "notifications-payment-processed",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        channel.QueueBind(
            queue: "notifications-payment-processed",
            exchange: "payment-processed-exchange",
            routingKey: "");

        var consumer =
            new EventingBasicConsumer(channel);

        consumer.Received += (sender, e) =>
        {
            try
            {
                var body = e.Body.ToArray();

                var message =
                    Encoding.UTF8.GetString(body);

                _logger.LogInformation(
                    "PaymentProcessedEvent recebido: {Message}",
                    message);

                var payment =
                    JsonSerializer.Deserialize<PaymentProcessedEvent>(
                        message);

                if (payment is null)
                {
                    _logger.LogWarning(
                        "Mensagem inválida.");

                    return;
                }

                if (payment.Status != "Approved")
                {
                    _logger.LogInformation(
                        "Pagamento rejeitado. Nenhum e-mail enviado.");

                    return;
                }

                _logger.LogInformation(
                "Email de confirmacao enviado para o usuario {UserId} referente ao jogo {GameId}.",
                payment.UserId,
                payment.GameId);    
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar PaymentProcessedEvent.");
            }
        };


        channel.BasicConsume(
            queue: "notifications-payment-processed",
            autoAck: true,
            consumer: consumer);

    }
}