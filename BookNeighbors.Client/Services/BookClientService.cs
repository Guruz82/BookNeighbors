
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BookNeighbors.DTO.Books;
using BookNeighbors.DTO.Categories;
using Microsoft.AspNetCore.Components.Forms;

namespace BookNeighbors.Client.Services;

public sealed class BookClientService(
    IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient httpClient =
        httpClientFactory.CreateClient("BookNeighborsApi");

    public async Task<List<BookResponse>> GetMineAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<BookResponse>>("api/book/mine") ?? [];
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<CategoryResponse>>("api/category") ?? [];
    }

    public async Task<IsbnLookupResponse?> LookupIsbnAsync(string isbn)
    {
        return await httpClient
            .GetFromJsonAsync<IsbnLookupResponse>(
                $"api/isbn/{Uri.EscapeDataString(isbn.Trim())}");
    }

    public async Task<BookResponse> SaveAsync(
        BookRequest request,
        Guid? bookId = null)
    {
        using var response = bookId.HasValue
            ? await httpClient.PutAsJsonAsync(
                $"api/book/{bookId.Value}", request)
            : await httpClient.PostAsJsonAsync(
                "api/book", request);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<BookResponse>()
            ?? throw new InvalidOperationException(
                "Risposta API non valida.");
    }

    public async Task<CoverImageResponse> UploadCoverAsync(
        Guid bookId,
        IBrowserFile file)
    {
        const long maxSize = 2 * 1024 * 1024;

        if (file.Size == 0 || file.Size > maxSize)
            throw new ArgumentException(
                "La copertina non deve superare 2 MB.");

        if (file.ContentType is not
            ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException(
                "Utilizza un'immagine JPG, PNG o WebP.");

        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(maxSize);
        using var fileContent = new StreamContent(stream);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);

        content.Add(fileContent, "file", file.Name);

        using var response = await httpClient.PostAsync(
            $"api/book/{bookId}/cover", content);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<CoverImageResponse>()
            ?? throw new InvalidOperationException(
                "Risposta API non valida.");
    }

    public async Task UnpublishAsync(Guid bookId)
    {
        using var response = await httpClient.PatchAsync(
            $"api/book/{bookId}/unpublish", null);

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(Guid bookId)
    {
        using var response = await httpClient.DeleteAsync(
            $"api/book/{bookId}");

        await EnsureSuccessAsync(response);
    }

    public string? GetCoverUrl(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
        {
            return uri.Scheme is "https" or "http"
                ? uri.ToString()
                : null;
        }

        return new Uri(
            httpClient.BaseAddress!,
            imageUrl.TrimStart('/')).ToString();
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        var message = response.ReasonPhrase ?? "Errore API.";

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.String)
                {
                    message = root.GetString() ?? message;
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("detail", out var detail)
                        && detail.ValueKind == JsonValueKind.String)
                    {
                        message = detail.GetString() ?? message;
                    }
                    else if (root.TryGetProperty("title", out var title)
                        && title.ValueKind == JsonValueKind.String)
                    {
                        message = title.GetString() ?? message;
                    }
                }
            }
            catch (JsonException)
            {
                message = content;
            }
        }

        throw new HttpRequestException(
            message, null, response.StatusCode);
    }
}
