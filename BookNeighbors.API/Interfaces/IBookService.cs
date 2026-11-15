using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.DTO.Books;

namespace BookNeighbors.Api.Interfaces;

public interface IBookService
{
    Task<List<Book>> GetBooksByUserIdAsync(Guid userId);

    Task<Book?> GetBookByIdAsync(Guid id, Guid userId);

    Task<Book?> CreateBookAsync(Guid userId, BookRequest request);

    Task<Book?> UpdateBookAsync(Guid id, Guid userId, BookRequest request);

    Task<bool> UnpublishBookAsync(Guid id, Guid userId);

    Task<DeleteBookResult> DeleteBookAsync(Guid id, Guid userId);

    Task<CoverImageResponse?> UploadCoverAsync(Guid id, Guid userId, IFormFile file, CancellationToken cancellationToken = default);

    Task<bool> DeleteCoverAsync(Guid id, Guid userId);
}