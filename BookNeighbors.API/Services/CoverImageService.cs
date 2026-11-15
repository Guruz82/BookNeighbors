/*using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace bookneighbors.Services;

public class CoverImageResult
{
    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string ThumbnailFileName { get; set; } = string.Empty;

    public string ThumbnailFilePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
}

public class CoverImageService
{
    private readonly IWebHostEnvironment _environment;

    public CoverImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<CoverImageResult> SaveCoverAsync(IFormFile file)
    {
        var allowedContentTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        if (!allowedContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException("Formato immagine non valido. Usa JPG, PNG o WEBP.");
        }

        const long maxFileSize = 2 * 1024 * 1024;

        if (file.Length > maxFileSize)
        {
            throw new InvalidOperationException("L'immagine non può superare i 2 MB.");
        }

        var uploadsFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "covers"
        );

        var thumbnailsFolder = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "covers",
            "thumbs"
        );

        Directory.CreateDirectory(uploadsFolder);
        Directory.CreateDirectory(thumbnailsFolder);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        var fileName = $"{Guid.NewGuid()}{extension}";
        var thumbnailFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_thumb.jpg";

        var filePath = Path.Combine(uploadsFolder, fileName);
        var thumbnailPath = Path.Combine(thumbnailsFolder, thumbnailFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        CreateThumbnail(filePath, thumbnailPath, maxWidth: 220, maxHeight: 320);

        return new CoverImageResult
        {
            FileName = fileName,
            FilePath = $"/uploads/covers/{fileName}",
            ThumbnailFileName = thumbnailFileName,
            ThumbnailFilePath = $"/uploads/covers/thumbs/{thumbnailFileName}",
            ContentType = file.ContentType
        };
    }

    public void DeleteCoverFiles(string? filePath, string? thumbnailFilePath)
    {
        DeletePhysicalFile(filePath);
        DeletePhysicalFile(thumbnailFilePath);
    }

    private static void CreateThumbnail(
        string sourcePath,
        string destinationPath,
        int maxWidth,
        int maxHeight)
    {
        using var inputStream = File.OpenRead(sourcePath);
        using var original = SKBitmap.Decode(inputStream);

        if (original is null)
        {
            throw new InvalidOperationException("Immagine non valida o non leggibile.");
        }

        var ratioX = (double)maxWidth / original.Width;
        var ratioY = (double)maxHeight / original.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = Math.Max(1, (int)(original.Width * ratio));
        var newHeight = Math.Max(1, (int)(original.Height * ratio));

        using var resized = original.Resize(
            new SKImageInfo(newWidth, newHeight),
            SKFilterQuality.Medium
        );

        if (resized is null)
        {
            throw new InvalidOperationException("Errore durante la generazione della thumbnail.");
        }

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 85);

        using var outputStream = File.OpenWrite(destinationPath);
        data.SaveTo(outputStream);
    }

    private void DeletePhysicalFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var relativePath = filePath.TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var fullPath = Path.Combine(_environment.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
*/