using BookNeighbors.DTO.Search;

namespace BookNeighbors.Api.Interfaces;

public interface ISearchService
{
    Task<List<BookSearchResponse>> SearchBooksAsync(
        BookSearchRequest request);
}