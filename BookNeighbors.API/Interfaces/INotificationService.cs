namespace BookNeighbors.Api.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Crea una nuova notifica per un utente specifico e la salva nel database.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateAsync(Guid userId, string title, string message, string? url = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// Restituisce il numero di notifiche non lette per un utente specifico.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Segna una notifica come letta per un utente specifico, se esiste.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="notificationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggiunge una nuova notifica per un utente specifico.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    void AddNotification(Guid userId, string title, string message, string? url = null);
}