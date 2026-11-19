
using System.Net;
using System.Net.Http.Json;
using BookNeighbors.DTO.Libraries;

namespace BookNeighbors.Client.Services;

public sealed class LibraryClientService(
    IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient httpClient =
        httpClientFactory.CreateClient("BookNeighborsApi");

    public async Task<LibraryResponse?> GetMineAsync()
    {
        using var response = await httpClient.GetAsync(
            "api/library/mine");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<LibraryResponse>();
    }

    public async Task<LibraryResponse> SaveAsync(
        LibraryRequest request,
        bool exists)
    {
        using var response = exists
            ? await httpClient.PutAsJsonAsync("api/library", request)
            : await httpClient.PostAsJsonAsync("api/library", request);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                detail,
                null,
                response.StatusCode);
        }

        return await response.Content
            .ReadFromJsonAsync<LibraryResponse>()
            ?? throw new InvalidOperationException(
                "Risposta API non valida.");
    }
}
