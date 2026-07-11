namespace NotificationsAPI.Contracts.Events;

public record UserCreatedEvent(
    Guid UserId,
    string Name,
    string Email);