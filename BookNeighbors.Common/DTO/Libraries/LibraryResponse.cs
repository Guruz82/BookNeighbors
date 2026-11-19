namespace BookNeighbors.DTO.Libraries;

public record LibraryResponse(
    Guid Id,
    string Name,
    string? City,
    string? AreaName,
    double Latitude,
    double Longitude,
    bool LocationConsent,
    bool IsPublished
);
