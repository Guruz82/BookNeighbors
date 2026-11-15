using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/isbn")]
public class IsbnController(
    IIsbnLookupService isbnLookupService) : ControllerBase
{
    [HttpGet("{isbn}")]
    public async Task<ActionResult<IsbnLookupResponse>> Lookup(
        string isbn,
        CancellationToken cancellationToken)
    {
        var result = await isbnLookupService.LookupAsync(
            isbn, cancellationToken);

        return Ok(result);
    }
}