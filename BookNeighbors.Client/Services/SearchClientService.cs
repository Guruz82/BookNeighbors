
using System.Globalization;
using System.Net.Http.Json;
using BookNeighbors.DTO.Categories;
using BookNeighbors.DTO.Search;

namespace BookNeighbors.Client.Services;

public sealed class SearchClientService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient httpClient =
        httpClientFactory.CreateClient("BookNeighborsApi");

    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        return await httpClient.GetFromJsonAsync<List<CategoryResponse>>(
            "api/category") ?? [];
    }

    public async Task<List<BookSearchResponse>> SearchBooksAsync(
        BookSearchRequest request)
    {
        if (request.Latitude is null || request.Longitude is null)
            throw new ArgumentException("Seleziona una posizione.");

        var query = new List<string>
        {
            $"latitude={request.Latitude.Value.ToString(CultureInfo.InvariantCulture)}",
            $"longitude={request.Longitude.Value.ToString(CultureInfo.InvariantCulture)}",
            $"radiusKm={request.RadiusKm.ToString(CultureInfo.InvariantCulture)}"
        };

        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            query.Add(
                $"text={Uri.EscapeDataString(request.Text.Trim())}");
        }

        if (request.CategoryId.HasValue)
        {
            query.Add($"categoryId={request.CategoryId.Value}");
        }

        var url = "api/search/books?" + string.Join("&", query);

        using var response = await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<BookSearchResponse>>() ?? [];
    }
}
