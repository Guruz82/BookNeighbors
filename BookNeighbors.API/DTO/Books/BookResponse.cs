namespace BookNeighbors.DTO.Books;

public record BookResponse(
    Guid Id,
    Guid LibraryId,
    string Title,
    string Author,
    string? ISBN,
    string? Description,
    int? PublicationYear,
    string? ImageUrl,
    string? ThumbnailUrl,
    bool IsPublished,
    List<Guid> CategoryIds,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);