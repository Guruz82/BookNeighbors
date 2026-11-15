
using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Books;

namespace BookNeighbors.API.Mappers.BookMapper;

public static class BookMapper
{
    public static BookResponse ToResponse(Book book)
    {
        return new BookResponse(
            book.Id,
            book.LibraryId,
            book.Title,
            book.Author,
            book.ISBN,
            book.Description,
            book.PublicationYear,
            book.ImageUrl,
            book.ThumbnailUrl,
            book.IsPublished,
            book.BookCategories.Select(c => c.CategoryId).ToList(),
            book.CreatedAt,
            book.UpdatedAt
        );
    }
}
