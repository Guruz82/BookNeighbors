
namespace BookNeighbors.Api.Interfaces;

public interface ICoverUrlResolver
{
    /// <summary>
    /// Risolve l'URL dell'immagine di copertina del libro.
    /// </summary>
    /// <param name="imageUrl">L'URL dell'immagine da risolvere.</param>
    /// <returns>L'URL risolto o null se non valido.</returns>
    string? Resolve(string? imageUrl);
}




