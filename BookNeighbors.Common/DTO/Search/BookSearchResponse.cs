namespace BookNeighbors.DTO.Search;

public record BookSearchResponse(
    Guid Id,
    Guid LibraryId,
    string LibraryName,
    string? City,
    string Title,
    string Author,
    string? ThumbnailUrl,
    string? ImageUrl,
    double Latitude,
    double Longitude,
    double DistanceKm,
    bool IsLoaned
);