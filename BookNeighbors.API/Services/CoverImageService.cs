
using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Books;
using SkiaSharp;

namespace BookNeighbors.Api.Services;

public class CoverImageService(IWebHostEnvironment environment)
    : ICoverImageService
{
    private const long MaxFileSize = 2 * 1024 * 1024;

    private string CoversFolder => Path.Combine(
        environment.WebRootPath
            ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
        "uploads",
        "covers");

    public async Task<CoverImageResponse> SaveCoverAsync(
        Guid bookId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Length == 0 || file.Length > MaxFileSize)
            throw new ArgumentException(
                "L'immagine deve essere valida e non superare 2 MB.");

        if (file.ContentType is not
            ("image/jpeg" or "image/png" or "image/webp"))
            throw new ArgumentException(
                "Formato non valido. Utilizza JPG, PNG o WebP.");

        using var stream = file.OpenReadStream();
        using var codec = SKCodec.Create(stream);

        if (codec is null ||
            codec.EncodedFormat is not
                (SKEncodedImageFormat.Jpeg or
                 SKEncodedImageFormat.Png or
                 SKEncodedImageFormat.Webp))
            throw new ArgumentException("Immagine non valida.");

        if (codec.Info.Width > 6000 ||
            codec.Info.Height > 6000 ||
            (long)codec.Info.Width * codec.Info.Height > 16000000)
            throw new ArgumentException("Dimensioni immagine eccessive.");

        using var original = SKBitmap.Decode(codec)
            ?? throw new ArgumentException("Immagine non leggibile.");

        // Generazione della miniatura
        const int maxWidth = 220;
        const int maxHeight = 320;

        var ratio = Math.Min(
            1.0,
            Math.Min(
                (double)maxWidth / original.Width,
                (double)maxHeight / original.Height));

        var width = Math.Max(1, (int)(original.Width * ratio));
        var height = Math.Max(1, (int)(original.Height * ratio));

        using var thumbnail = original.Resize(
            new SKImageInfo(width, height),
            new SKSamplingOptions(SKFilterMode.Linear))
            ?? throw new InvalidOperationException(
                "Errore nella generazione della miniatura.");

        using var fullImage = SKImage.FromBitmap(original);
        using var thumbnailImage = SKImage.FromBitmap(thumbnail);

        using var fullData = fullImage.Encode(
            SKEncodedImageFormat.Jpeg, 85);

        using var thumbnailData = thumbnailImage.Encode(
            SKEncodedImageFormat.Jpeg, 80);

        if (fullData is null || thumbnailData is null)
            throw new InvalidOperationException(
                "Errore nella codifica dell'immagine.");

        var directory = Path.Combine(
            CoversFolder, bookId.ToString("N"));

        Directory.CreateDirectory(directory);

        var name = Guid.NewGuid().ToString("N");
        var imageName = $"{name}.jpg";
        var thumbnailName = $"{name}_thumb.jpg";

        var imagePath = Path.Combine(directory, imageName);
        var thumbnailPath = Path.Combine(directory, thumbnailName);

        try
        {
            await File.WriteAllBytesAsync(
                imagePath, fullData.ToArray(), cancellationToken);

            await File.WriteAllBytesAsync(
                thumbnailPath, thumbnailData.ToArray(), cancellationToken);
        }
        catch
        {
            File.Delete(imagePath);
            File.Delete(thumbnailPath);
            throw;
        }

        var urlPrefix = $"/uploads/covers/{bookId:N}";

        return new CoverImageResponse(
            $"{urlPrefix}/{imageName}",
            $"{urlPrefix}/{thumbnailName}"
        );
    }

    public void DeleteCoverFiles(
        Guid bookId,
        string? imageUrl,
        string? thumbnailUrl)
    {
        DeleteFile(bookId, imageUrl);
        DeleteFile(bookId, thumbnailUrl);
    }

    private void DeleteFile(Guid bookId, string? url)
    {
        var prefix = $"/uploads/covers/{bookId:N}/";

        if (url is null ||
            !url.StartsWith(prefix, StringComparison.Ordinal))
            return;

        var fileName = url[prefix.Length..];

        // Accetta esclusivamente i nomi generati dal servizio
        if (!fileName.EndsWith(".jpg", StringComparison.Ordinal))
            return;

        var name = Path.GetFileNameWithoutExtension(fileName);

        if (name.EndsWith("_thumb", StringComparison.Ordinal))
            name = name[..^6];

        if (!Guid.TryParseExact(name, "N", out _) ||
            fileName.Contains('/') ||
            fileName.Contains('\\'))
            return;

        var path = Path.Combine(
            CoversFolder,
            bookId.ToString("N"),
            fileName);

        File.Delete(path);
    }
}
