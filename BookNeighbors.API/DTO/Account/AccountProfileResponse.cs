namespace BookNeighbors.DTO.Account;

public record AccountProfileResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string? FirstName,
    string? LastName,
    string? Bio,
    bool PrivacyNoticeAcknowledged
);