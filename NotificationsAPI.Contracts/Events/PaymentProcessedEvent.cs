namespace NotificationsAPI.Contracts.Events;

public record PaymentProcessedEvent(
    Guid PaymentId,
    Guid UserId,
    Guid GameId,
    decimal Amount,
    string Status);