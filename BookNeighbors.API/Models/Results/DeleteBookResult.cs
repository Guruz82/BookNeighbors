namespace BookNeighbors.Api.Models.Results;
/// <summary>
/// Rappresenta il risultato dell'operazione di eliminazione di un libro.
/// </summary>
public enum DeleteBookResult
{
    Deleted,
    NotFound,
    HasLoanRequests
}