
using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Books;
using BookNeighbors.Api.Interfaces;

namespace BookNeighbors.API.Mappers;

public static class BookMapper
{
    /// <summary>
    /// Mappa un oggetto Book in un oggetto BookResponse.
    /// </summary>
    /// <param name="book"></param>
    /// <param name="coverUrlResolver"></param>
    /// <returns></returns>
    public static BookResponse ToResponse(Book book, ICoverUrlResolver coverUrlResolver)
    {
        return new BookResponse(
            book.Id,
            book.LibraryId,
            book.Title,
            book.Author,
            book.ISBN,
            book.Description,
            book.PublicationYear,
            coverUrlResolver.Resolve(book.ImageUrl),
            coverUrlResolver.Resolve(book.ThumbnailUrl),
            book.IsPublished,
            book.BookCategories.Select(c => c.CategoryId).ToList(),
            book.CreatedAt,
            book.UpdatedAt
        );
    }
}
