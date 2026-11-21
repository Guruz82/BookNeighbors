using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.DTO.Books;

namespace BookNeighbors.Api.Interfaces;

public interface IBookService
{
    /// <summary>
    /// Restituisce la lista dei libri pubblicati dall'utente specificato.  
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<List<Book>> GetBooksByUserIdAsync(Guid userId);
    /// <summary>
    /// Restituisce i dettagli di un libro esistente, se il libro appartiene all'utente specificato.    
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Book?> GetBookByIdAsync(Guid id, Guid userId);
    /// <summary>
    /// Crea un nuovo libro.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Book?> CreateBookAsync(Guid userId, BookRequest request);
    /// <summary>
    /// Aggiorna un libro esistente.
    /// Restituisce null se il libro non esiste o non appartiene all'utente specific
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Book?> UpdateBookAsync(Guid id, Guid userId, BookRequest request);
    /// <summary>
    /// Pubblica un libro esistente.
    /// Restituisce false se il libro non esiste o non appartiene all'utente specific
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> UnpublishBookAsync(Guid id, Guid userId);
    /// <summary>
    /// Elimina un libro esistente.
    /// Restituisce un DeleteBookResult che indica il risultato dell'operazione.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<DeleteBookResult> DeleteBookAsync(Guid id, Guid userId);
    /// <summary>
    /// Carica un'immagine di copertina per un libro esistente.
    /// Restituisce null se il libro non esiste o non appartiene all'utente specific
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CoverImageResponse?> UploadCoverAsync(Guid id, Guid userId, IFormFile file, CancellationToken cancellationToken = default);
    /// <summary>
    /// Elimina l'immagine di copertina di un libro esistente.
    /// Restituisce false se il libro non esiste o non appartiene all'utente specific
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<bool> DeleteCoverAsync(Guid id, Guid userId);
    /// <summary>
    /// Restituisce i dettagli pubblici di un libro esistente, se il libro è pubblicato.
    /// Restituisce null se il libro non esiste o non è pubblicato.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<BookDetailsResponse?> OpenPublicBookAsync(Guid bookId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Registra una visualizzazione pubblica di un libro esistente, se il libro è pubblicato.
    /// Restituisce true se la visualizzazione è stata registrata con successo, false altrimenti.
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns></returns>
    Task<BookDetailsResponse?> GetPublicDetailsAsync(Guid bookId);
    /// <summary>
    /// Registra una visualizzazione pubblica di un libro esistente, se il libro è pubblicato.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> RegisterPublicViewAsync(Guid bookId, CancellationToken cancellationToken = default);
}