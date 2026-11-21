using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;



[ApiController]
[Authorize]
[Route("api/isbn")]
public class IsbnController(IIsbnLookupService isbnLookupService) : ControllerBase
{


    /// <summary>
    /// Esegue una ricerca di informazioni su un libro tramite il suo ISBN. Restituisce i dettagli del libro se trovati.
    /// </summary>
    /// <param name="isbn"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{isbn}")]
    public async Task<ActionResult<IsbnLookupResponse>> Lookup(string isbn, CancellationToken cancellationToken)
    {
        var result = await isbnLookupService.LookupAsync(isbn, cancellationToken);

        return Ok(result);
    }
}