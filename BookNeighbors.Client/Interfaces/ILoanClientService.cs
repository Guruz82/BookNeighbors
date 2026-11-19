using BookNeighbors.DTO.Loans;

namespace BookNeighbors.Client.Interfaces;

public interface ILoanClientService
{
    

     public  Task<List<LoanResponse>> GetSentAsync();
      public  Task<List<LoanResponse>> GetReceivedAsync();
      public  Task<LoanResponse> RequestAsync(Guid bookId);
      public  Task<LoanResponse> ChangeStatusAsync(Guid loanId, string action);

}