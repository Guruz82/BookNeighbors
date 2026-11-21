
using BookNeighbors.API.Enum;

namespace BookNeighbors.Api.Models.Results;

/// <summary>
/// Rappresenta il risultato dell'operazione di prestito di un libro.
/// </summary>
public record LoanResult(LoanResultCode Code, LoanRequest? Loan = null);