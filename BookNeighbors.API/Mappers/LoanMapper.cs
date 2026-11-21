using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Loans;

namespace BookNeighbors.Api.Mappers;

public static class LoanMapper
{
    /// <summary>
    /// Mappa un oggetto LoanRequest in un oggetto LoanResponse.    
    /// </summary>
    /// <param name="loan"></param>
    /// <returns></returns>
    public static LoanResponse ToResponse(LoanRequest loan) => new(
        loan.Id,
        loan.BookId,
        loan.Book.Title,
        loan.RequesterId,
        loan.Requester.DisplayName,
        loan.Status.Name,
        loan.CreatedAt,
        loan.DueDate,
        loan.UpdatedAt
    );
}