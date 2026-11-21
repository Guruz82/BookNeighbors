
using BookNeighbors.Api.Hubs;
using BookNeighbors.Api.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BookNeighbors.Api.Services;

public class NotificationRealtimePublisher(IHubContext<LoanChatHub> hub) : INotificationRealtimePublisher
{
    /// <summary>
    /// Pubblica una notifica in tempo reale a un utente specifico.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task PublishAsync(Guid userId)
    {
        await hub.Clients.Group(LoanChatHub.UserGroup(userId)).SendAsync("NotificationsChanged");
    }
    /// <summary>
    /// Pubblica notifiche in tempo reale a più utenti specifici.
    /// </summary>
    /// <param name="userIds"></param>
    /// <returns></returns>
    public async Task PublishAsync(IEnumerable<Guid> userIds)
    {
        foreach (var userId in userIds.Distinct())
        {
            await PublishAsync(userId);
        }
    }
}
