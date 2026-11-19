namespace BookNeighbors.DTO.Books;

public record IsbnLookupResponse(
    bool Found,
    string? Title = null,
    string? Author = null,
    string? Publisher = null,
    int? PublicationYear = null,
    string? Description = null,
    string? ThumbnailUrl = null,
    string? Message = null
);