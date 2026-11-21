namespace BookNeighbors.DTO.Notifications;

public record NotificationDto(Guid Id, string Title, string Message, string? Url, bool IsRead, DateTime CreatedAt);
