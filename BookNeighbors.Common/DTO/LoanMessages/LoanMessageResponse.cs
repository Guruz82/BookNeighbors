
namespace BookNeighbors.DTO.LoanMessages;

public record LoanMessageResponse(Guid Id, Guid LoanRequestId, Guid SenderId, string SenderName, string Text, DateTime CreatedAt);
