
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.DTO.Books;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookNeighbors.Api.Services;

/// <summary>
/// Servizio per la gestione dei libri.
/// Implementa l'interfaccia IBookService.
/// </summary>
/// <param name="_dbContext"></param>
/// <param name="_coverImageService"></param>
/// <param name="_logger"></param>
public class BookService(ApplicationDbContext _dbContext, ICoverImageService _coverImageService, ICoverUrlResolver coverUrlResolver, ILogger<BookService> _logger) : IBookService
{
    /// <summary>
    /// Recupera tutti i libri di un utente, ordinati per data di creazione decrescente.
    /// Restituisce un elenco vuoto se l'utente non ha libri.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<Book>> GetBooksByUserIdAsync(Guid userId)
    {
        return await _dbContext.Books
            .AsNoTracking()
            .Include(b => b.BookCategories)
            .Where(b => b.Library.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }
    /// <summary>
    /// Recupera un libro per ID, se appartiene all'utente.
    /// Restituisce null se il libro non esiste o non appartiene all'utente.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Book?> GetBookByIdAsync(Guid id, Guid userId)
    {
        return await _dbContext.Books
            .AsNoTracking()
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);
    }


    /// <summary>
    /// Crea un libro e le sue categorie.
    /// Restituisce null se l'utente non ha una libreria.
    /// Lancia un'eccezione se una o più categorie non esistono.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Book?> CreateBookAsync(Guid userId, BookRequest request)
    {
        var library = await _dbContext.Libraries
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.UserId == userId);

        if (library is null)
            return null;

        var categoryIds = await ValidateCategoriesAsync(request.CategoryIds);

        var book = new Book
        {
            Id = Guid.NewGuid(),
            LibraryId = library.Id,
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            ISBN = request.ISBN?.Trim(),
            Description = request.Description,
            PublicationYear = request.PublicationYear,
            ImageUrl = request.ImageUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublished = request.IsPublished,
            BookCategories = categoryIds.Select(id => new BookCategory
            {
                CategoryId = id
            }).ToList()
        };

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        return book;
    }
    /// <summary>
    /// Aggiorna un libro e le sue categorie.
    /// Restituisce null se il libro non esiste o non appartiene all'utente.
    /// Lancia un'eccezione se una o più categorie non esistono.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<Book?> UpdateBookAsync(
        Guid id, Guid userId, BookRequest request)
    {
        var book = await _dbContext.Books
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);

        if (book is null)
            return null;

        var categoryIds = await ValidateCategoriesAsync(request.CategoryIds);

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.ISBN = request.ISBN?.Trim();
        book.Description = request.Description;
        book.PublicationYear = request.PublicationYear;
        book.ImageUrl = request.ImageUrl;
        book.ThumbnailUrl = request.ThumbnailUrl;
        book.IsPublished = request.IsPublished;
        book.UpdatedAt = DateTime.UtcNow;

        // Rimuove le categorie non più selezionate
        var selectedIds = categoryIds.ToHashSet();

        foreach (var category in book.BookCategories
                     .Where(c => !selectedIds.Contains(c.CategoryId))
                     .ToList())
        {
            _dbContext.BookCategories.Remove(category);
        }

        // Aggiunge soltanto le nuove categorie
        var existingIds = book.BookCategories
            .Select(c => c.CategoryId)
            .ToHashSet();

        foreach (var categoryId in selectedIds.Except(existingIds))
        {
            book.BookCategories.Add(new BookCategory
            {
                BookId = book.Id,
                CategoryId = categoryId
            });
        }

        await _dbContext.SaveChangesAsync();

        return book;
    }
    /// <summary>
    /// Valida che tutte le categorie esistano nel database.
    /// Restituisce un array di ID univoci.
    /// Se una o più categorie non esistono, lancia un'eccezione.
    /// </summary>
    /// <param name="categoryIds"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task<Guid[]> ValidateCategoriesAsync(
        IEnumerable<Guid> categoryIds)
    {
        var ids = categoryIds.Distinct().ToArray();

        if (ids.Length == 0)
            return ids;

        var count = await _dbContext.Categories
            .CountAsync(c => ids.Contains(c.Id));

        if (count != ids.Length)
            throw new ArgumentException(
                "Una o più categorie selezionate non esistono.");

        return ids;
    }

    /// <summary>
    /// Elimina un libro se non ha richieste di prestito.
    /// Restituisce un risultato che indica se l'eliminazione è avvenuta con successo.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<DeleteBookResult> DeleteBookAsync(
        Guid id, Guid userId)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);

        if (book is null)
            return DeleteBookResult.NotFound;

        var hasLoans = await _dbContext.LoanRequests
            .AnyAsync(l => l.BookId == id);

        if (hasLoans)
            return DeleteBookResult.HasLoanRequests;

        _dbContext.Books.Remove(book);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation &&
            pgEx.ConstraintName == "FK_LoanRequests_Books_book_id")
        {
            return DeleteBookResult.HasLoanRequests;
        }

        return DeleteBookResult.Deleted;
    }

    /// <summary>
    /// Imposta un libro come non pubblicato.   
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> UnpublishBookAsync(Guid id, Guid userId)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);

        if (book is null)
            return false;

        book.IsPublished = false;
        book.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }
    /// <summary>
    /// Carica la copertina di un libro e genera il thumbnail.
    /// Se il libro aveva già una copertina, la sostituisce.
    /// Se il salvataggio del DB fallisce, elimina i nuovi file.
    /// Se il salvataggio del DB ha successo, elimina i vecchi file.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<CoverImageResponse?> UploadCoverAsync(Guid id, Guid userId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId,
                cancellationToken);

        if (book is null)
            return null;

        var oldImageUrl = book.ImageUrl;
        var oldThumbnailUrl = book.ThumbnailUrl;

        var result = await _coverImageService.SaveCoverAsync(
            book.Id, file, cancellationToken);

        book.ImageUrl = result.ImageUrl;
        book.ThumbnailUrl = result.ThumbnailUrl;
        book.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Se il DB non salva, eliminiamo i nuovi file
            _coverImageService.DeleteCoverFiles(
                book.Id, result.ImageUrl, result.ThumbnailUrl);

            throw;
        }

        // Eliminiamo la vecchia copertina solo dopo il salvataggio
        TryDeleteFiles(book.Id, oldImageUrl, oldThumbnailUrl);

        return new CoverImageResponse(coverUrlResolver.Resolve(result.ImageUrl), coverUrlResolver.Resolve(result.ThumbnailUrl));
    }

    /// <summary>
    /// Elimina la copertina di un libro, se esiste.
    /// Restituisce true se l'eliminazione è avvenuta con successo.
    /// Restituisce false se il libro non esiste o non appartiene all'utente.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteCoverAsync(Guid id, Guid userId)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);

        if (book is null)
            return false;

        var oldImageUrl = book.ImageUrl;
        var oldThumbnailUrl = book.ThumbnailUrl;

        book.ImageUrl = null;
        book.ThumbnailUrl = null;
        book.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        TryDeleteFiles(book.Id, oldImageUrl, oldThumbnailUrl);

        return true;
    }
    /// <summary>
    ///     Elimina i file della copertina e del thumbnail, se esistono.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="imageUrl"></param>
    /// <param name="thumbnailUrl"></param>
    private void TryDeleteFiles(Guid bookId, string? imageUrl, string? thumbnailUrl)
    {
        try
        {
            _coverImageService.DeleteCoverFiles(
                bookId, imageUrl, thumbnailUrl);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(
                ex, "Impossibile eliminare i file del libro {BookId}",
                bookId);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(
                ex, "Accesso negato ai file del libro {BookId}",
                bookId);
        }
    }

    /// <summary>
    ///  Recupera un libro pubblico per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    ///  Registra l'apertura del libro.
    ///  Restituisce null se il libro non esiste o non è pubblico.  
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<BookDetailsResponse?> OpenPublicBookAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var book = await GetPublicDetailsCoreAsync(bookId, cancellationToken);

        if (book is null)
            return null;

        // Registra l'apertura del libro.
        var view = new BookView
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ViewedAt = DateTime.UtcNow
        };

        _dbContext.BookViews.Add(view);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return book;
    }


    /// <summary>
    ///  Restituisce i dettagli pubblici di un libro per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    ///  Restituisce null se il libro non esiste o non è pubblico.
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns></returns>
    public async Task<BookDetailsResponse?> GetPublicDetailsAsync(Guid bookId)
    {
        return await GetPublicDetailsCoreAsync(bookId, CancellationToken.None);
    }

    /// <summary>
    ///  Restituisce i dettagli pubblici di un libro per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    ///  Restituisce null se il libro non esiste o non è pubblico.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<BookDetailsResponse?> GetPublicDetailsCoreAsync(Guid bookId, CancellationToken cancellationToken)
    {
        // Sono consultabili soltanto i libri pubblici appartenenti a librerie pubblicate.
        var book = await _dbContext.Books
            .AsNoTracking()
            .Where(b => b.Id == bookId && b.IsPublished && b.Library.IsPublished && b.Library.LocationConsent)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Author,
                b.Description,
                b.ISBN,
                b.PublicationYear,
                b.ImageUrl,
                b.ThumbnailUrl,
                LibraryName = b.Library.Name,
                b.Library.City,
                IsLoaned = _dbContext.LoanRequests.Any(l => l.BookId == b.Id && l.Status.Name == "Accepted")
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (book is null)
            return null;

        return new BookDetailsResponse(
            book.Id,
            book.Title,
            book.Author,
            book.Description,
            book.ISBN,
            book.PublicationYear,
            coverUrlResolver.Resolve(book.ImageUrl),
            coverUrlResolver.Resolve(book.ThumbnailUrl),
            book.LibraryName,
            book.City,
            book.IsLoaned
        );
    }
    /// <summary>
    /// Registra la visualizzazione di un libro pubblico.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> RegisterPublicViewAsync(
        Guid bookId,
        CancellationToken cancellationToken = default)
    {
        // Verifica che il libro sia pubblicamente consultabile.
        var exists = await _dbContext.Books
            .AsNoTracking()
            .AnyAsync(b =>
                b.Id == bookId &&
                b.IsPublished &&
                b.Library.IsPublished &&
                b.Library.LocationConsent,
                cancellationToken);

        if (!exists)
            return false;

        // Registra la visualizzazione.
        var view = new BookView
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            ViewedAt = DateTime.UtcNow
        };

        _dbContext.BookViews.Add(view);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
