namespace BookNeighbors.Client.Models.Maps;

public sealed record MapMarker(
    double Latitude,
    double Longitude,
    string Title,
    string? Description = null
);