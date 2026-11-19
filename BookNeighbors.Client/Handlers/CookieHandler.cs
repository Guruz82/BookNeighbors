
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BookNeighbors.Client.Identity;

public class CookieHandler(
    IHttpClientFactory httpClientFactory) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
Console.WriteLine(
    $"CookieHandler: {request.Method} {request.RequestUri}");

        request.SetBrowserRequestCredentials(
            BrowserRequestCredentials.Include);

        request.Headers.TryAddWithoutValidation(
            "X-Requested-With", "XMLHttpRequest");

        if (request.Method == HttpMethod.Post ||
            request.Method == HttpMethod.Put ||
            request.Method == HttpMethod.Patch ||
            request.Method == HttpMethod.Delete)
        {
            using var client = httpClientFactory.CreateClient(
                "BookNeighborsAntiforgery");

            using var tokenRequest = new HttpRequestMessage(
                HttpMethod.Get,
                "api/security/antiforgery-token");

            tokenRequest.SetBrowserRequestCredentials(
                BrowserRequestCredentials.Include);

            using var response = await client.SendAsync(
                tokenRequest, cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<AntiforgeryToken>(
                    cancellationToken: cancellationToken);

            if (string.IsNullOrWhiteSpace(result?.Token))
                throw new InvalidOperationException(
                    "Token antiforgery non disponibile.");
   request.Headers.TryAddWithoutValidation(
                "X-XSRF-TOKEN", result.Token);
         
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private sealed class AntiforgeryToken
    {
        public string Token { get; set; } = string.Empty;
    }
}
