using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Books;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System.Net.Http.Headers;

namespace BookNeighbors.Api.Services;

public class CoverImageService(IWebHostEnvironment environment, HttpClient httpClient, IOptions<AppOptions> options) : ICoverImageService
{
    private const long MaxFileSize = 2 * 1024 * 1024;

    private string CoversFolder => Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "covers");

    /// <summary>
    /// Salva l'immagine di copertina di un libro e genera una miniatura.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="file"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<CoverImageResponse> SaveCoverAsync(Guid bookId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            throw new ArgumentException("L'immagine deve essere valida e non superare 2 MB.");

        if (file.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException("Formato non valido. Utilizza JPG, PNG o WebP.");

        using var stream = file.OpenReadStream();
        using var codec = SKCodec.Create(stream);

        if (codec is null || codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
            throw new ArgumentException("Immagine non valida.");

        if (codec.Info.Width > 6000 || codec.Info.Height > 6000 || (long)codec.Info.Width * codec.Info.Height > 16000000)
            throw new ArgumentException("Dimensioni immagine eccessive.");

        using var original = SKBitmap.Decode(codec) ?? throw new ArgumentException("Immagine non leggibile.");

        const int maxWidth = 90;
        const int maxHeight = 130;

        var ratio = Math.Min(1.0, Math.Min((double)maxWidth / original.Width, (double)maxHeight / original.Height));
        var width = Math.Max(1, (int)(original.Width * ratio));
        var height = Math.Max(1, (int)(original.Height * ratio));

        using var thumbnail = original.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear)) ?? throw new InvalidOperationException("Errore nella generazione della miniatura.");
        using var fullImage = SKImage.FromBitmap(original);
        using var thumbnailImage = SKImage.FromBitmap(thumbnail);

        using var fullData = fullImage.Encode(SKEncodedImageFormat.Jpeg, 85);
        using var thumbnailData = thumbnailImage.Encode(SKEncodedImageFormat.Jpeg, 80);

        if (fullData is null || thumbnailData is null)
            throw new InvalidOperationException("Errore nella codifica dell'immagine.");

        var writeUrl = options.Value.OracleStorage.WriteUrl;
        var readUrl = options.Value.OracleStorage.ReadUrl;

        if (string.IsNullOrWhiteSpace(writeUrl) || string.IsNullOrWhiteSpace(readUrl))
            throw new InvalidOperationException("Configurazione Oracle Object Storage mancante.");

        var objectPrefix = $"covers/{bookId:N}";
        var name = Guid.NewGuid().ToString("N");

        var imageName = $"{objectPrefix}/{name}.jpg";
        var thumbnailName = $"{objectPrefix}/{name}_thumb.jpg";

        var imageWriteUrl = BuildObjectUrl(writeUrl, imageName);
        var thumbnailWriteUrl = BuildObjectUrl(writeUrl, thumbnailName);

        using var originalContent = new ByteArrayContent(fullData.ToArray());
        originalContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        using var originalResponse = await httpClient.PutAsync(imageWriteUrl, originalContent, cancellationToken);
        originalResponse.EnsureSuccessStatusCode();

        using var thumbnailContent = new ByteArrayContent(thumbnailData.ToArray());
        thumbnailContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        using var thumbnailResponse = await httpClient.PutAsync(thumbnailWriteUrl, thumbnailContent, cancellationToken);
        thumbnailResponse.EnsureSuccessStatusCode();

        return new CoverImageResponse(imageName, thumbnailName);
    }

    /// <summary>
    /// Elimina i file di copertina di un libro, se esistono e se i nomi dei file sono validi.
    /// Con la soluzione Oracle attuale elimina solo i vecchi file locali.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="imageUrl"></param>
    /// <param name="thumbnailUrl"></param>
    public void DeleteCoverFiles(Guid bookId, string? imageUrl, string? thumbnailUrl)
    {
        DeleteLocalFile(bookId, imageUrl);
        DeleteLocalFile(bookId, thumbnailUrl);
    }

    /// <summary>
    /// Elimina un file locale legacy della vecchia gestione /uploads/covers/.
    /// </summary>
    /// <param name="bookId"></param>
    /// <param name="url"></param>
    private void DeleteLocalFile(Guid bookId, string? url)
    {
        var prefix = $"/uploads/covers/{bookId:N}/";

        if (url is null || !url.StartsWith(prefix, StringComparison.Ordinal))
            return;

        var fileName = url[prefix.Length..];

        if (!fileName.EndsWith(".jpg", StringComparison.Ordinal))
            return;

        var name = Path.GetFileNameWithoutExtension(fileName);

        if (name.EndsWith("_thumb", StringComparison.Ordinal))
            name = name[..^6];

        if (!Guid.TryParseExact(name, "N", out _) || fileName.Contains('/') || fileName.Contains('\\'))
            return;

        var path = Path.Combine(CoversFolder, bookId.ToString("N"), fileName);

        if (File.Exists(path))
            File.Delete(path);
    }

    private static string BuildObjectUrl(string baseUrl, string objectName)
    {
        return $"{baseUrl.TrimEnd('/')}/{string.Join("/", objectName.Split('/').Select(Uri.EscapeDataString))}";
    }
}