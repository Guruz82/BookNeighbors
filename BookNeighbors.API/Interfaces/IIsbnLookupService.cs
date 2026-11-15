using BookNeighbors.DTO.Books;

namespace BookNeighbors.Api.Interfaces;

public interface IIsbnLookupService
{
    Task<IsbnLookupResponse> LookupAsync(
        string isbn,
        CancellationToken cancellationToken = default);
}