
using BookNeighbors.Client.Handlers;
using BookNeighbors.Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;

namespace BookNeighbors.Client.Services;

/// <summary>
/// Servizio per la gestione delle notifiche in tempo reale tramite SignalR.
/// </summary>
public class NotificationRealtimeService(IHttpClientFactory factory, IOptions<AppOptions> options) : IAsyncDisposable
{
    /// <summary>
    /// Connessione SignalR per ricevere notifiche in tempo reale.
    /// </summary>
    private HubConnection? connection;

    /// <summary>
    /// Evento sollevato quando le notifiche dell'utente cambiano.
    /// </summary>
    public event Func<Task>? NotificationsChanged;

    /// <summary>
    /// Avvia la connessione SignalR. Se la connessione è già presente, non fa nulla.
    /// </summary>
    public async Task StartAsync()
    {
        if (connection is not null)
            return;

        var apiBaseUri = factory.CreateClient("BookNeighborsApi").BaseAddress ?? throw new InvalidOperationException("ApiBaseUrl non configurato.");
        var hubPath = options.Value.Services?.SignalRHubPath;

        if (string.IsNullOrWhiteSpace(hubPath))
            throw new InvalidOperationException("SignalRHubPath non configurato.");

        var hubUri = new Uri(apiBaseUri, hubPath.TrimStart('/'));

        Console.WriteLine($"SignalR notifiche: {hubUri}");

        connection = new HubConnectionBuilder().WithUrl(hubUri, options =>
        {
            options.HttpMessageHandlerFactory = handler => new SignalRCookieHandler { InnerHandler = handler };
        }).WithAutomaticReconnect().Build();

        connection.On("NotificationsChanged", async () =>
        {
            if (NotificationsChanged is not null)
                await NotificationsChanged.Invoke();
        });

        connection.Reconnected += async _ =>
        {
            if (NotificationsChanged is not null)
                await NotificationsChanged.Invoke();
        };

        try
        {
            await connection.StartAsync();
        }
        catch
        {
            await connection.DisposeAsync();
            connection = null;
            throw;
        }
    }

    /// <summary>
    /// Chiude la connessione SignalR e libera le risorse.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (connection is not null)
        {
            await connection.DisposeAsync();
            connection = null;
        }
    }
}
