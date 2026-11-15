using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Search;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController(
    ISearchService searchService) : ControllerBase
{
    [HttpGet("books")]
    public async Task<ActionResult<List<BookSearchResponse>>> SearchBooks(
        [FromQuery] BookSearchRequest request)
    {
        var results = await searchService.SearchBooksAsync(request);

        return Ok(results);
    }
}