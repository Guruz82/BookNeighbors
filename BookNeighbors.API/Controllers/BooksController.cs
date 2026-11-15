
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
public class BooksController(
    UserManager<ApplicationUser> userManager,
    IBookService bookService) : ControllerBase
{
    private Guid? CurrentUserId =>
        Guid.TryParse(userManager.GetUserId(User), out var id)
            ? id
            : null;

    [HttpGet("mine")]
    public async Task<ActionResult<List<BookResponse>>> GetMine()
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var books = await bookService.GetBooksByUserIdAsync(userId);

        return Ok(books.Select(BookMapper.ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookResponse>> GetById(Guid id)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var book = await bookService.GetBookByIdAsync(id, userId);

        if (book is null)
            return NotFound();

        return Ok(BookMapper.ToResponse(book));
    }

    [HttpPost]
    public async Task<ActionResult<BookResponse>> Create(
        [FromBody] BookRequest request)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        try
        {
            var book = await bookService.CreateBookAsync(userId, request);

            if (book is null)
                return Conflict("Devi creare prima una libreria personale.");

            return CreatedAtAction(
                nameof(GetById),
                new { id = book.Id },
                BookMapper.ToResponse(book));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BookResponse>> Update(
        Guid id, [FromBody] BookRequest request)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        try
        {
            var book = await bookService.UpdateBookAsync(
                id, userId, request);

            if (book is null)
                return NotFound();

            return Ok(BookMapper.ToResponse(book));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

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

            DeleteBookResult.HasLoanRequests =>
                Conflict("Impossibile eliminare un libro con richieste di prestito associate."),

            _ => StatusCode(500)
        };
    }


    [HttpPost("{id:guid}/cover")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<ActionResult<CoverImageResponse>> UploadCover(
        Guid id,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        if (file is null)
            return BadRequest("Seleziona un'immagine.");

        try
        {
            var result = await bookService.UploadCoverAsync(
                id, userId, file, cancellationToken);

            if (result is null)
                return NotFound();

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}/cover")]
    public async Task<IActionResult> DeleteCover(Guid id)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var deleted = await bookService.DeleteCoverAsync(id, userId);

        return deleted ? NoContent() : NotFound();
    }

}
