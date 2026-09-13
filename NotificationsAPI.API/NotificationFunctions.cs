using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NotificationsAPI.Contracts.Events;

namespace NotificationsAPI;

public class NotificationFunctions
{
    private readonly ILogger<NotificationFunctions> _logger;

    public NotificationFunctions(ILogger<NotificationFunctions> logger)
    {
        _logger = logger;
    }

    [Function(nameof(UserCreatedTrigger))]
    public void UserCreatedTrigger(
        [RabbitMQTrigger("notifications-user-created", ConnectionStringSetting = "RabbitMQConnection")] string message)
    {
        var user = JsonSerializer.Deserialize<UserCreatedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (user is null) return;

        _logger.LogInformation($"[Serverless] Email de boas-vindas enviado para {user.Email}");
    }

    [Function(nameof(PaymentProcessedTrigger))]
    public void PaymentProcessedTrigger(
        [RabbitMQTrigger("notifications-payment-processed", ConnectionStringSetting = "RabbitMQConnection")] string message)
    {
        try
        {
            var payment = JsonSerializer.Deserialize<PaymentProcessedEvent>(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payment is null)
            {
                _logger.LogWarning("Mensagem inválida.");
                return;
            }

            // Regra de negócio trazida do código antigo
            if (payment.Status != "Approved")
            {
                _logger.LogInformation("Pagamento rejeitado. Nenhum e-mail enviado.");
                return;
            }

            _logger.LogInformation($"[Serverless] Email de confirmacao enviado para o usuario {payment.UserId} referente ao jogo {payment.GameId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar PaymentProcessedEvent.");
            // No Azure Functions, se der erro, lançamos a exceção (throw) 
            // para que a mensagem volte para a fila (NACK) e seja tentada novamente.
            throw;
        }
    }
}