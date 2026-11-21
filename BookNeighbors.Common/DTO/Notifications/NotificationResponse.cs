namespace BookNeighbors.DTO.Notifications;

public record NotificationResponse(Guid Id, string Title, string Message, string? Url, bool IsRead, DateTime CreatedAt);
