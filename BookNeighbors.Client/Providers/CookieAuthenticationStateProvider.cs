
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using BookNeighbors.Client.Interfaces;
using BookNeighbors.Client.Models.Identity;
using BookNeighbors.DTO.Account;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookNeighbors.Client.Providers;

public class CookieAuthenticationStateProvider(
    IHttpClientFactory httpClientFactory,
    ILogger<CookieAuthenticationStateProvider> logger)
    : AuthenticationStateProvider, IAccountManagement
{
    private readonly HttpClient httpClient =
        httpClientFactory.CreateClient("BookNeighborsApi");

    private readonly ClaimsPrincipal unauthenticated =
        new(new ClaimsIdentity());

    // Registrazione
    public async Task<FormResult> RegisterAsync(
        RegisterRequest request)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "api/account/register", request);

            if (response.IsSuccessStatusCode)
                return new FormResult { Succeeded = true };

            return new FormResult
            {
                Succeeded = false,
                ErrorList = await ReadErrorsAsync(response)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore durante la registrazione");

            return new FormResult
            {
                Succeeded = false,
                ErrorList = ["Impossibile completare la registrazione."]
            };
        }
    }

    // Login tramite cookie Identity
    public async Task<FormResult> LoginAsync(
        string email,
        string password)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                "login?useCookies=true",
                new
                {
                    email,
                    password
                });

            if (!response.IsSuccessStatusCode)
            {
                return new FormResult
                {
                    Succeeded = false,
                    ErrorList = ["Email o password non validi."]
                };
            }

            NotifyAuthenticationStateChanged(
                GetAuthenticationStateAsync());

            return new FormResult { Succeeded = true };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore durante il login");

            return new FormResult
            {
                Succeeded = false,
                ErrorList = ["Impossibile effettuare il login."]
            };
        }
    }

    // Recupero dello stato di autenticazione
    public override async Task<AuthenticationState>
        GetAuthenticationStateAsync()
    {
        try
        {
            using var response =
                await httpClient.GetAsync("manage/info");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return new AuthenticationState(unauthenticated);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Verifica autenticazione fallita: {StatusCode}",
                    response.StatusCode);

                return new AuthenticationState(unauthenticated);
            }

            var userInfo =
                await response.Content.ReadFromJsonAsync<UserInfo>();

            if (userInfo is null ||
                string.IsNullOrWhiteSpace(userInfo.Email))
            {
                return new AuthenticationState(unauthenticated);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, userInfo.Email),
                new(ClaimTypes.Email, userInfo.Email)
            };

            var identity = new ClaimsIdentity(
                claims,
                nameof(CookieAuthenticationStateProvider));

            return new AuthenticationState(
                new ClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Errore durante il recupero dello stato di autenticazione");

            return new AuthenticationState(unauthenticated);
        }
    }

    // Logout
    public async Task LogoutAsync()
    {
        using var response = await httpClient.PostAsJsonAsync(
            "logout", new { });

        response.EnsureSuccessStatusCode();

        NotifyAuthenticationStateChanged(
            Task.FromResult(
                new AuthenticationState(unauthenticated)));
    }

    // Verifica autenticazione
    public async Task<bool> CheckAuthenticatedAsync()
    {
        var state = await GetAuthenticationStateAsync();

        return state.User.Identity?.IsAuthenticated == true;
    }

    // Lettura errori restituiti dalle API
    private static async Task<string[]> ReadErrorsAsync(
        HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
            return ["Operazione non riuscita."];

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.String)
                return [root.GetString() ?? "Operazione non riuscita."];

            if (root.ValueKind != JsonValueKind.Object)
                return ["Operazione non riuscita."];

            var errors = new List<string>();

            if (root.TryGetProperty("errors", out var errorList) &&
                errorList.ValueKind == JsonValueKind.Object)
            {
                foreach (var error in errorList.EnumerateObject())
                {
                    if (error.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in error.Value.EnumerateArray())
                        {
                            if (item.ValueKind == JsonValueKind.String)
                            {
                                var message = item.GetString();

                                if (!string.IsNullOrWhiteSpace(message))
                                    errors.Add(message);
                            }
                        }
                    }
                    else if (error.Value.ValueKind == JsonValueKind.String)
                    {
                        var message = error.Value.GetString();

                        if (!string.IsNullOrWhiteSpace(message))
                            errors.Add(message);
                    }
                }
            }

            if (errors.Count > 0)
                return errors.ToArray();

            if (root.TryGetProperty("detail", out var detail) &&
                detail.ValueKind == JsonValueKind.String)
            {
                return [detail.GetString() ?? "Operazione non riuscita."];
            }

            if (root.TryGetProperty("title", out var title) &&
                title.ValueKind == JsonValueKind.String)
            {
                return [title.GetString() ?? "Operazione non riuscita."];
            }
        }
        catch (JsonException)
        {
            return [content];
        }

        return ["Operazione non riuscita."];
    }
}
