
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BookNeighbors.Client.Handlers;

public class SignalRCookieHandler : DelegatingHandler
{
    /// <summary>
    /// Imposta le credenziali della richiesta del browser per includere i cookie nella richiesta HTTP. Questo è necessario per le richieste SignalR che richiedono l'autenticazione tramite cookie.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}
