using BookNeighbors.Api.Enum;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;

namespace BookNeighbors.Api.Interfaces;

public interface ILoanService
{
    Task<List<LoanRequest>> GetSentAsync(Guid userId);
    Task<List<LoanRequest>> GetReceivedAsync(Guid userId);

    Task<LoanResult> CreateAsync(Guid bookId, Guid userId);

    Task<LoanResult> ChangeStatusAsync(Guid id, Guid userId, LoanAction action);
}