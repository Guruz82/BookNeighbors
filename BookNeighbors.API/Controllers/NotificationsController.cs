
using System.Security.Claims;
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Services;
using BookNeighbors.DTO.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Authorize]
[Route("api/notifications")]
public class NotificationsController(ApplicationDbContext _db, INotificationService _notificationService) : ControllerBase
{

    /// <summary>
    /// Restituisce le notifiche dell'utente corrente, se l'utente è autenticato.
    /// Restituisce Unauthorized se l'utente non è autenticato.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var notifications = await _db.Notifications.AsNoTracking().Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).Take(50).Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Url, n.IsRead, n.CreatedAt)).ToListAsync(cancellationToken);

        return Ok(notifications);
    }


    /// <summary>
    /// Restituisce il numero di notifiche non lette dell'utente corrente, se l'utente è autenticato.
    /// Restituisce Unauthorized se l'utente non è autenticato.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);

        return Ok(count);
    }
    /// <summary>
    /// Cambia lo stato di una notifica specificata come letta dall'utente corrente, se l'utente è autenticato.
    /// Restituisce NotFound se la notifica non esiste o non appartiene all'utente corrente.
    /// Restituisce NoContent se l'operazione è stata completata con successo.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var result = await _notificationService.MarkAsReadAsync(userId, id, cancellationToken);

        if (!result) return NotFound();

        return NoContent();
    }
    /// <summary>
    /// Cambia lo stato di tutte le notifiche dell'utente corrente come lette, se l'utente è autenticato.
    /// Restituisce NoContent se l'operazione è stata completata con successo.
    /// Restituisce Unauthorized se l'utente non è autenticato.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var now = DateTime.UtcNow;

        await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true).SetProperty(n => n.ReadAt, now), cancellationToken);

        return NoContent();
    }
    /// <summary>
    /// Restituisce l'ID dell'utente corrente dall'oggetto UserManager.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }
}

