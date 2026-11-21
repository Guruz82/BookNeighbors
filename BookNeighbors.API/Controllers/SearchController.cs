using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Search;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController(ISearchService searchService) : ControllerBase
{
    /// <summary>
    /// Restituisce i risultati della ricerca dei libri in base ai criteri specificati nella richiesta.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("books")]
    public async Task<ActionResult<List<BookSearchResponse>>> SearchBooks(
        [FromQuery] BookSearchRequest request)
    {
        var results = await searchService.SearchBooksAsync(request);

        return Ok(results);
    }
}