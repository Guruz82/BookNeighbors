
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.API.Mappers;
using BookNeighbors.DTO.Books;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/book")]
public class BooksController(UserManager<ApplicationUser> userManager, IBookService bookService, ICoverUrlResolver coverUrlResolver) : ControllerBase
{
    /// <summary>
    /// Ottiene l'ID dell'utente corrente dall'oggetto UserManager.
    /// </summary>
    private Guid? CurrentUserId => Guid.TryParse(userManager.GetUserId(User), out var id) ? id : null;
    /// <summary>
    /// Restituisce la lista dei libri dell'utente corrente, se esiste.
    /// </summary>
    /// <returns></returns>
    [HttpGet("mine")]
    public async Task<ActionResult<List<BookResponse>>> GetMine()
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var books = await bookService.GetBooksByUserIdAsync(userId);

        return Ok(books.Select(b => BookMapper.ToResponse(b, coverUrlResolver)).ToList());
    }
    /// <summary>
    /// Recupera un libro per ID, se appartiene all'utente autenticato.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookResponse>> GetById(Guid id)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var book = await bookService.GetBookByIdAsync(id, userId);

        if (book is null)
            return NotFound();

        return Ok(BookMapper.ToResponse(book, coverUrlResolver));
    }
    /// <summary>
    ///  Crea un nuovo libro per l'utente autenticato. Restituisce Conflict se l'utente non ha una libreria personale. Restituisce BadRequest se i dati forniti non sono validi.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create([FromBody] BookRequest request)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        try
        {
            var book = await bookService.CreateBookAsync(userId, request);

            if (book is null)
                return Conflict("Devi creare prima una libreria personale.");

            return CreatedAtAction(nameof(GetById), new { id = book.Id }, BookMapper.ToResponse(book, coverUrlResolver));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /// <summary>
    ///  Aggiorna un libro esistente, se appartiene all'utente autenticato.
    ///  Se il libro non esiste o non appartiene all'utente, restituisce NotFound.
    ///  Se i dati forniti non sono validi, restituisce BadRequest
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookResponse>> Update(Guid id, [FromBody] BookRequest request)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        try
        {
            var book = await bookService.UpdateBookAsync(id, userId, request);

            if (book is null)
                return NotFound();

            return Ok(BookMapper.ToResponse(book, coverUrlResolver));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    /// <summary>
    /// Imposta lo stato di pubblicazione di un libro, se appartiene all'utente autenticato.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPatch("{id:guid}/unpublish")]
    public async Task<IActionResult> Unpublish(Guid id)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var result = await bookService.UnpublishBookAsync(id, userId);

        if (!result)
            return NotFound();

        return NoContent();
    }



    /// <summary>
    /// Elimina un libro, se appartiene all'utente autenticato e non ha richieste di prestito associate.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var result = await bookService.DeleteBookAsync(id, userId);

        return result switch
        {
            DeleteBookResult.Deleted => NoContent(),
            DeleteBookResult.NotFound => NotFound(),
            DeleteBookResult.HasLoanRequests => Conflict("Impossibile eliminare un libro con richieste di prestito associate."),
            _ => StatusCode(500)
        };
    }

    /// <summary>
    /// Carica la copertina di un libro. L'immagine deve essere in formato JPEG o PNG e non deve superare i 3 MB.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>

    [HttpPost("{id:guid}/cover")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<ActionResult<CoverImageResponse>> UploadCover(Guid id, [FromForm] IFormFile? file, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        if (file is null)
            return BadRequest("Seleziona un'immagine.");

        try
        {
            var result = await bookService.UploadCoverAsync(id, userId, file, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Elimina la copertina di un libro, se esiste.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}/cover")]
    public async Task<IActionResult> DeleteCover(Guid id)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var deleted = await bookService.DeleteCoverAsync(id, userId);

        return deleted ? NoContent() : NotFound();
    }

    /// <summary>
    /// Recupera un libro pubblico per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("{id:guid}/open")]
    public async Task<ActionResult<BookDetailsResponse>> Open(Guid id, CancellationToken cancellationToken)
    {
        var book = await bookService.OpenPublicBookAsync(id, cancellationToken);

        if (book is null)
            return NotFound();

        return Ok(book);
    }

    /// <summary>
    /// Recupera i dettagli pubblici di un libro per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("public/{id:guid}")]
    public async Task<ActionResult<BookDetailsResponse>> GetPublicDetails(Guid id)
    {
        var book = await bookService.GetPublicDetailsAsync(id);

        if (book is null)
            return NotFound();

        return Ok(book);
    }

    /// <summary>
    ///  Registra una visualizzazione pubblica di un libro, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("public/{id:guid}/view")]
    public async Task<IActionResult> RegisterView(Guid id, CancellationToken cancellationToken)
    {
        var registered = await bookService.RegisterPublicViewAsync(id, cancellationToken);

        return registered ? NoContent() : NotFound();
    }
}
