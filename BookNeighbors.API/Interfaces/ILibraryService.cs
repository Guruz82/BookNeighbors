using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Libraries;

namespace BookNeighbors.Api.Interfaces;

public interface ILibraryService
{
    /// <summary>
    ///  Crea una nuova libreria per l'utente specificato.
    ///  Restituisce la libreria creata.
    ///  Restituisce null se l'utente ha già una libreria personale.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Library> CreateLibraryAsync(Guid userId, LibraryRequest request);
    /// <summary>
    /// Aggiorna la libreria dell'utente specificato.
    /// Restituisce la libreria aggiornata.
    /// Restituisce null se l'utente non ha una libreria personale.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Library?> UpdateLibraryAsync(Guid userId, LibraryRequest request);
    /// <summary>
    /// Restituisce la libreria dell'utente specificato, se esiste.
    /// Restituisce null se l'utente non ha una libreria personale.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Library?> GetLibraryByUserIdAsync(Guid userId);

}