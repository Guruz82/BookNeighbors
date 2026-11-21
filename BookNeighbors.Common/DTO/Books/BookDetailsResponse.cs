namespace BookNeighbors.DTO.Books;

public record BookDetailsResponse(
    Guid Id,
    string Title,
    string Author,
    string? Description,
    string? ISBN,
    int? PublicationYear,
    string? ImageUrl,
    string? ThumbnailUrl,
    string LibraryName,
    string? City,
    bool IsLoaned
);