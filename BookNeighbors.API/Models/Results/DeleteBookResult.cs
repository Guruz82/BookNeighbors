namespace BookNeighbors.Api.Models.Results;

public enum DeleteBookResult
{
    Deleted,
    NotFound,
    HasLoanRequests
}