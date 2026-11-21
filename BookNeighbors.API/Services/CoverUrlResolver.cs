using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using Microsoft.Extensions.Options;

namespace BookNeighbors.Api.Services;

public class CoverUrlResolver(IOptions<AppOptions> options) : ICoverUrlResolver
{

    /// <summary>
    ///  Risolve l'URL dell'immagine di copertina del libro. aggiungendo il par di oracle alle url relative
    /// </summary>
    /// <param name="imageUrl"></param>
    /// <returns></returns>
    public string? Resolve(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return null;

        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            return uri.Scheme is "https" or "http" ? uri.ToString() : null;

        var readUrl = options.Value.OracleStorage.ReadUrl;
        var objectName = string.Join("/", imageUrl.TrimStart('/').Split('/').Select(Uri.EscapeDataString));

        return $"{readUrl.TrimEnd('/')}/{objectName}";
    }
}