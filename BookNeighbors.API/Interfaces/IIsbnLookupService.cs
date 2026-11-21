using BookNeighbors.DTO.Books;

namespace BookNeighbors.Api.Interfaces;

public interface IIsbnLookupService
{
    /// <summary>
    /// Restituisce le informazioni di un libro a partire dal suo ISBN.
    /// </summary>
    /// <param name="isbn"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IsbnLookupResponse> LookupAsync(string isbn, CancellationToken cancellationToken = default);
}