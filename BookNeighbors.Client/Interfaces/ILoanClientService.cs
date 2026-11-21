using BookNeighbors.DTO.Loans;

namespace BookNeighbors.Client.Interfaces;

public interface ILoanClientService
{

    /// <summary>
    /// Gets the list of loans sent by the current user.
    /// </summary>
    /// <returns></returns>
    public Task<List<LoanResponse>> GetSentAsync();
    /// <summary>
    /// Gets the list of loans received by the current user.
    /// </summary>
    /// <returns></returns>
    public Task<List<LoanResponse>> GetReceivedAsync();
    /// <summary>
    /// Requests a loan for a book with the given ID.
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns></returns>
    public Task<LoanResponse> RequestAsync(Guid bookId);
    /// <summary>
    /// Changes the status of a loan with the given ID to the specified action (e.g., accept, reject, return).
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public Task<LoanResponse> ChangeStatusAsync(Guid loanId, string action);

}