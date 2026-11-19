namespace BookNeighbors.DTO.Loans;

public record LoanResponse(
    Guid Id,
    Guid BookId,
    string BookTitle,
    Guid RequesterId,
    string RequesterDisplayName,
    string Status,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? UpdatedAt
);