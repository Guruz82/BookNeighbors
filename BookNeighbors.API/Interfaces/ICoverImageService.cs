using BookNeighbors.DTO.Books;

namespace BookNeighbors.Api.Interfaces;

public interface ICoverImageService
{
    Task<CoverImageResponse> SaveCoverAsync(
        Guid bookId,
        IFormFile file,
        CancellationToken cancellationToken = default);

    void DeleteCoverFiles(
        Guid bookId,
        string? imageUrl,
        string? thumbnailUrl);
}