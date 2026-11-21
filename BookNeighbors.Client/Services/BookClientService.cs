
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BookNeighbors.DTO.Books;
using BookNeighbors.DTO.Categories;
using Microsoft.AspNetCore.Components.Forms;

namespace BookNeighbors.Client.Services;

/// <summary>
/// Servizio client per la gestione dei libri.
/// Fornisce metodi per interagire con l'API BookNeighbors per operazioni sui libri, come recuperare, salvare, caricare copertine e gestire lo stato dei libri.
/// Implementa l'interfaccia IBookClientService.
/// </summary>
/// <param name="httpClientFactory"></param>
public sealed class BookClientService(
    IHttpClientFactory httpClientFactory)
{
      
     // Client HTTP utilizzato per le richieste all'API BookNeighbors.
    
    private readonly HttpClient httpClient =  httpClientFactory.CreateClient("BookNeighborsApi");
    /// <summary>
    /// Recupera tutti i libri dell'utente autenticato.
    /// Restituisce un elenco vuoto se l'utente non ha libri.
    /// </summary>
    /// <returns></returns>
    public async Task<List<BookResponse>> GetMineAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<BookResponse>>("api/book/mine") ?? [];
    }
    /// <summary>
    /// Recupera tutte le categorie disponibili.
    /// Restituisce un elenco vuoto se non ci sono categorie.
    /// </summary>
    /// <returns></returns>
    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        return await httpClient
            .GetFromJsonAsync<List<CategoryResponse>>("api/category") ?? [];
    }
    /// <summary>
    /// Recupera le informazioni di un libro pubblico per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    /// Restituisce null se il libro non esiste o non è pubblico.
    /// </summary>
    /// <param name="isbn"></param>
    /// <returns></returns>
    public async Task<IsbnLookupResponse?> LookupIsbnAsync(string isbn)
    {
        return await httpClient
            .GetFromJsonAsync<IsbnLookupResponse>(
                $"api/isbn/{Uri.EscapeDataString(isbn.Trim())}");
    }
    /// <summary>
    ///  Salva un libro. Se bookId è null, crea un nuovo libro; altrimenti aggiorna il libro esistente.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bookId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// <summary>
    ///   Carica la copertina di un libro. La copertina deve essere un'immagine JPG, PNG o WebP e non deve superare 2 MB.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
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
    /// <summary>
    /// Imposta un libro come non pubblicato.
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns></returns>
    public async Task UnpublishAsync(Guid bookId)
    {
        using var response = await httpClient.PatchAsync(
            $"api/book/{bookId}/unpublish", null);

        await EnsureSuccessAsync(response);
    }
    /// <summary>
    ///  Elimina un libro. Restituisce un'eccezione se l'eliminazione non è consentita.
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns></returns>
    public async Task DeleteAsync(Guid bookId)
    {
        using var response = await httpClient.DeleteAsync(
            $"api/book/{bookId}");

        await EnsureSuccessAsync(response);
    }
    /// <summary>
    ///   Restituisce l'URL completo della copertina di un libro, se disponibile.
    /// </summary>
    /// <param name="imageUrl"></param>
    /// <returns></returns>
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

    /// <summary>
    ///  Verifica se la risposta HTTP è un successo. In caso contrario, legge il contenuto della risposta e genera un'eccezione HttpRequestException con un messaggio appropriato.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    /// <exception cref="HttpRequestException"></exception>
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

     /// <summary>
     ///   Recupera un libro pubblico per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.    
     /// </summary>
     /// <param name="bookId"></param>
     /// <returns></returns>
     /// <exception cref="InvalidOperationException"></exception>
    public async Task<BookDetailsResponse> OpenPublicBookAsync(
        Guid bookId)
    {
        using var response = await httpClient.PostAsync(
            $"api/book/{bookId}/open",
            null);

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<BookDetailsResponse>()
            ?? throw new InvalidOperationException(
                "Impossibile recuperare i dettagli del libro.");
    }

    /// <summary>
    ///  Recupera i dettagli pubblici di un libro per ID, se appartiene a una libreria pubblicata e con consenso alla geolocalizzazione.
    /// </summary>
    /// <param name="bookId"></param>
    /// <returns></returns>
    public async Task<BookDetailsResponse?> GetPublicDetailsAsync(Guid bookId)
    {
        using var response = await httpClient.GetAsync(
            $"api/book/public/{bookId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<BookDetailsResponse>();
    }

    public async Task RegisterViewAsync(Guid bookId)
    {
        using var response = await httpClient.PostAsync(
            $"api/book/public/{bookId}/view",
            null);

        response.EnsureSuccessStatusCode();
    }
}
