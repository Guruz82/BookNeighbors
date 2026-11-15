 
using BookNeighbors.API.Enum;

namespace BookNeighbors.Api.Models.Results;


 

public record LoanResult(
    LoanResultCode Code,
    LoanRequest? Loan = null);