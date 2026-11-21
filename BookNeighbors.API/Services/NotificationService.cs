
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;

/// <summary>
/// Servizio per la gestione delle notifiche.
/// </summary>
public class NotificationService(ApplicationDbContext db) : INotificationService
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
    public async Task CreateAsync(Guid userId, string title, string message, string? url = null, CancellationToken cancellationToken = default)
    {
        var notification = new Notification { UserId = userId, Title = title, Message = message, Url = url };
        db.Notifications.Add(notification);
        await db.SaveChangesAsync(cancellationToken);
    }
    /// <summary>
    /// Restituisce il numero di notifiche non lette per un utente specifico.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }
    /// <summary>
    /// Segna una notifica come letta per un utente specifico, se esiste.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="notificationId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await db.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);
        if (notification is null) return false;
        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
    /// <summary>
    /// Aggiunge una nuova notifica per un utente specifico senza salvare immediatamente nel database.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="title"></param>
    /// <param name="message"></param>
    /// <param name="url"></param>
    public void AddNotification(Guid userId, string title, string message, string? url = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Url = url,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        db.Notifications.Add(notification);
    }

}
