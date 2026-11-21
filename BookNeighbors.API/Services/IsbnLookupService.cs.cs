
using System.Text.Json;
using System.Text.RegularExpressions;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;

using BookNeighbors.DTO.Books;
using Microsoft.Extensions.Options;

namespace BookNeighbors.Api.Services;


public class IsbnLookupService(HttpClient httpClient, ILogger<IsbnLookupService> logger, IOptions<AppOptions> options, IConfiguration configuration) : IIsbnLookupService

{

    /// <summary>
    /// URL base per l'API di Google Books.
    /// </summary>
    private readonly string baseUrl = options.Value.ExternalServices.GoogleBooksBaseUrl ?? throw new InvalidOperationException("GoogleBooksBaseUrl non configurato.");

    /// <summary>
    /// Recupera le informazioni di un libro tramite il suo ISBN, cercando prima su Google Books e poi su Open Library se necessario.
    /// </summary>
    /// <param name="isbn"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IsbnLookupResponse> LookupAsync(string isbn, CancellationToken cancellationToken = default)
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
    /// <summary>
    /// Tenta di recuperare i dati del libro da Google Books tramite l'API pubblica.
    /// </summary>
    /// <param name="isbn"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<IsbnLookupResponse?> TryGoogleBooksAsync(string isbn, CancellationToken ct)
    {

        var url = $"{baseUrl}?q=isbn:{Uri.EscapeDataString(isbn)}&maxResults=1";

        var apiKey = configuration["ExternalServices:GoogleBooksApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            url += $"&key={Uri.EscapeDataString(apiKey)}";
        }
        System.Console.WriteLine($"URL Google Books: {url}");
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
            ImageUrl: thumbnail,
            Message: "Dati recuperati da Google Books."
        );
    }

    /// <summary>
    ///   Tenta di recuperare i dati del libro da Open Library tramite l'API pubblica.  
    /// </summary>
    /// <param name="isbn"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<IsbnLookupResponse?> TryOpenLibraryAsync(string isbn, CancellationToken ct)
    {
        var url = $"{baseUrl}?bibkeys=ISBN:{Uri.EscapeDataString(isbn)}&jscmd=data&format=json";

        using var document = await GetJsonAsync(url, ct);

        if (document is null)
            return null;

        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty($"ISBN:{isbn}", out var data) || data.ValueKind != JsonValueKind.Object)
        {
            logger.LogInformation(
                "Open Library: nessun risultato per ISBN {ISBN}", isbn);

            return null;
        }

        var title = GetString(data, "title");

        if (string.IsNullOrWhiteSpace(title))
            return null;

        string? thumbnail = null;

        if (data.TryGetProperty("cover", out var cover))
        {
            thumbnail = GetString(cover, "medium") ?? GetString(cover, "large") ?? GetString(cover, "small");
        }

        string? image = null;
        if (data.TryGetProperty("cover", out var imageLarge))
        {
            image = GetString(imageLarge, "large") ?? GetString(imageLarge, "medium") ?? GetString(imageLarge, "small");
        }


        return new IsbnLookupResponse(
            Found: true,
            Title: title,
            Author: GetNames(data, "authors"),
            Publisher: GetNames(data, "publishers"),
            PublicationYear: ExtractYear(
                GetString(data, "publish_date")),
            Description: GetDescription(data),
            ThumbnailUrl: thumbnail,
            ImageUrl: image,
            Message: "Dati recuperati da Open Library."
        );
    }

    /// <summary>
    ///  Recupera le informazioni di un libro tramite il suo ISBN, cercando prima su Google Books e poi su Open Library se necessario.  
    /// </summary>
    /// <param name="url"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task<JsonDocument?> GetJsonAsync(string url, CancellationToken ct)
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
    /// <summary>
    ///  Verifica se il formato dell'ISBN è valido (10 o 13 cifre, con eventuale 'X' finale per ISBN-10).
    /// </summary>
    /// <param name="isbn"></param>
    /// <returns></returns>
    private static bool IsValidIsbnFormat(string isbn) =>
        (isbn.Length == 13 && isbn.All(char.IsAsciiDigit)) ||
        (isbn.Length == 10 &&
         isbn[..9].All(char.IsAsciiDigit) &&
         (char.IsAsciiDigit(isbn[^1]) || isbn[^1] == 'X'));

    /// <summary>
    /// Recupera una stringa da un elemento JSON.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    private static string? GetString(
        JsonElement element, string property)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.String)
            return value.GetString();

        return null;
    }
    /// <summary>
    ///  Recupera i nomi degli autori da un elemento JSON.
    /// </summary>
    /// <param name="element"></param>
    /// <param name="property"></param>
    /// <returns></returns>
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
    /// <summary>
    ///  Estrae l'anno di pubblicazione da una stringa di data.
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private static string? GetAuthors(JsonElement element) =>
        GetNames(element, "authors");

    /// <summary>
    ///  Recupera la descrizione del libro da un elemento JSON, gestendo sia stringhe che oggetti con proprietà "value".
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
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

    /// <summary>
    ///  Estrae l'anno di pubblicazione da una stringa di data.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>

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
