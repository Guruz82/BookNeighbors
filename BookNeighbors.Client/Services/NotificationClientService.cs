
using System.Net.Http.Json;
using BookNeighbors.DTO.Notifications;

namespace BookNeighbors.Client.Services;

public class NotificationClientService(IHttpClientFactory httpClientFactory)
{
    private HttpClient Client => httpClientFactory.CreateClient("BookNeighborsApi");

    public async Task<List<NotificationDto>> GetNotificationsAsync(CancellationToken cancellationToken = default)
    {
        return await Client.GetFromJsonAsync<List<NotificationDto>>("api/notifications", cancellationToken) ?? [];
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        return await Client.GetFromJsonAsync<int>("api/notifications/unread-count", cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await Client.PatchAsync($"api/notifications/{id}/read", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        using var response = await Client.PatchAsync("api/notifications/read-all", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
