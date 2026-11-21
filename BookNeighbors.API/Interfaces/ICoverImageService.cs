using BookNeighbors.DTO.Books;

namespace BookNeighbors.Api.Interfaces;

public interface ICoverImageService
{
    /// <summary>
    /// Salva l'immagine di copertina per il libro specificato.
    /// Restituisce un CoverImageResponse che contiene gli URL dell'immagine e della miniatura, se l'operazione ha avuto successo.
    /// Restituisce null se il libro non esiste o non appartiene all'utente specificato.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CoverImageResponse> SaveCoverAsync(Guid bookId, IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina i file dell'immagine di copertina e della miniatura per il libro specificato, se esistono.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="imageUrl"></param>
    /// <param name="thumbnailUrl"></param>
    void DeleteCoverFiles(Guid bookId, string? imageUrl, string? thumbnailUrl);
}