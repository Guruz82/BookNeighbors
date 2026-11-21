namespace BookNeighbors.DTO.Account;

public record AccountProfileResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string? Bio,
    bool PrivacyNoticeAcknowledged
);