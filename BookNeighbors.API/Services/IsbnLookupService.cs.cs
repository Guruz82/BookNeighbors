
using System.Text.Json;
using System.Text.RegularExpressions;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Books;

namespace BookNeighbors.Api.Services;

public class IsbnLookupService(
    HttpClient httpClient,
    ILogger<IsbnLookupService> logger) : IIsbnLookupService
{
    public async Task<IsbnLookupResponse> LookupAsync(
        string isbn,
        CancellationToken cancellationToken = default)
    {
        var cleanedIsbn = new string(
            isbn.Where(c => !char.IsWhiteSpace(c) && c != '-')
                .ToArray()).ToUpperInvariant();

        if (!IsValidIsbnFormat(cleanedIsbn))
            return new(false, Message: "Formato ISBN non valido.");

        var result = await TryGoogleBooksAsync(
            cleanedIsbn, cancellationToken);

        if (result is not null)
            return result;

        result = await TryOpenLibraryAsync(
            cleanedIsbn, cancellationToken);

        return result ?? new(
            false,
            Message: "Dati non disponibili per questo ISBN.");
    }

    private async Task<IsbnLookupResponse?> TryGoogleBooksAsync(
        string isbn, CancellationToken ct)
    {
        var url =
            $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}&maxResults=1";

        using var document = await GetJsonAsync(url, ct);

        if (document is null)
            return null;

        var root = document.RootElement;

        if (!root.TryGetProperty("items", out var items) ||
            items.ValueKind != JsonValueKind.Array ||
            items.GetArrayLength() == 0)
            return null;

        if (!items[0].TryGetProperty("volumeInfo", out var info))
            return null;

        var title = GetString(info, "title");

        if (string.IsNullOrWhiteSpace(title))
            return null;

        string? thumbnail = null;

        if (info.TryGetProperty("imageLinks", out var images))
        {
            thumbnail = GetString(images, "thumbnail")
                ?? GetString(images, "smallThumbnail");

            if (thumbnail?.StartsWith("http://") == true)
                thumbnail = "https://" + thumbnail[7..];
        }

        return new IsbnLookupResponse(
            Found: true,
            Title: title,
            Author: GetAuthors(info),
            Publisher: GetString(info, "publisher"),
            PublicationYear: ExtractYear(GetString(info, "publishedDate")),
            Description: GetDescription(info),
            ThumbnailUrl: thumbnail,
            Message: "Dati recuperati da Google Books."
        );
    }

    private async Task<IsbnLookupResponse?> TryOpenLibraryAsync(
        string isbn, CancellationToken ct)
    {
        var url =
            $"https://openlibrary.org/api/volumes/brief/isbn/{isbn}.json";

        using var document = await GetJsonAsync(url, ct);

        if (document is null)
            return null;

        var root = document.RootElement;

        if (!root.TryGetProperty("records", out var records) ||
            records.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var record in records.EnumerateObject())
        {
            if (!record.Value.TryGetProperty("data", out var data))
                continue;

            var title = GetString(data, "title");

            if (string.IsNullOrWhiteSpace(title))
                continue;

            string? thumbnail = null;

            if (data.TryGetProperty("cover", out var cover))
            {
                thumbnail = GetString(cover, "medium")
                    ?? GetString(cover, "large")
                    ?? GetString(cover, "small");
            }

            return new IsbnLookupResponse(
                Found: true,
                Title: title,
                Author: GetAuthors(data),
                Publisher: GetNames(data, "publishers"),
                PublicationYear: ExtractYear(
                    GetString(data, "publish_date")),
                Description: GetDescription(data),
                ThumbnailUrl: thumbnail,
                Message: "Dati recuperati da Open Library."
            );
        }

        return null;
    }

    private async Task<JsonDocument?> GetJsonAsync(
        string url, CancellationToken ct)
    {
        try
        {
            using var response = await httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Ricerca ISBN: servizio esterno ha restituito {Status}",
                    response.StatusCode);

                return null;
            }

            await using var stream =
                await response.Content.ReadAsStreamAsync(ct);

            return await JsonDocument.ParseAsync(
                stream, cancellationToken: ct);
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Servizio ISBN non raggiungibile");
            return null;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Risposta ISBN non valida");
            return null;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            logger.LogWarning("Timeout del servizio ISBN");
            return null;
        }
    }

    private static bool IsValidIsbnFormat(string isbn) =>
        (isbn.Length == 13 && isbn.All(char.IsAsciiDigit)) ||
        (isbn.Length == 10 &&
         isbn[..9].All(char.IsAsciiDigit) &&
         (char.IsAsciiDigit(isbn[^1]) || isbn[^1] == 'X'));

    private static string? GetString(
        JsonElement element, string property)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.String)
            return value.GetString();

        return null;
    }

    private static string? GetNames(
        JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var items) ||
            items.ValueKind != JsonValueKind.Array)
            return null;

        var names = items.EnumerateArray()
            .Select(x => x.ValueKind == JsonValueKind.String
                ? x.GetString()
                : GetString(x, "name"))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return names.Count > 0 ? string.Join(", ", names) : null;
    }

    private static string? GetAuthors(JsonElement element) =>
        GetNames(element, "authors");

    private static string? GetDescription(JsonElement element)
    {
        if (!element.TryGetProperty("description", out var value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Object => GetString(value, "value"),
            _ => null
        };
    }

    private static int? ExtractYear(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;

        var match = Regex.Match(date, @"\b\d{4}\b");

        return match.Success && int.TryParse(match.Value, out var year)
            ? year
            : null;
    }
}
