using BookNeighbors.DTO.Search;

namespace BookNeighbors.Api.Interfaces;

public interface ISearchService
{
    /// <summary>
    /// Restituisce la lista dei libri che corrispondono ai criteri di ricerca specificati.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<List<BookSearchResponse>> SearchBooksAsync(BookSearchRequest request);
}