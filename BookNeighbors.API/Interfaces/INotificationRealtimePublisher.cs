
namespace BookNeighbors.Api.Interfaces;

public interface INotificationRealtimePublisher
{
    /// <summary>
    /// Pubblica un evento di notifica in tempo reale per un utente specifico.
    /// </summary>
    /// <param name="userId">L'ID dell'utente per il quale pubblicare l'evento.</param>
    /// <returns></returns>
    Task PublishAsync(Guid userId);

    /// <summary>
    /// Pubblica un evento di notifica in tempo reale per un insieme di utenti specifici.
    /// </summary>
    /// <param name="userIds">Gli ID degli utenti per i quali pubblicare l'evento.</param>
    /// <returns></returns>
    Task PublishAsync(IEnumerable<Guid> userIds);
}