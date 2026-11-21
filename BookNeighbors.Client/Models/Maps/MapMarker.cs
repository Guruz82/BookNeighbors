namespace BookNeighbors.Client.Models.Maps;

/// <summary>
/// Rappresenta un marcatore sulla mappa.
/// </summary>
public sealed record MapMarker(
    double Latitude,
    double Longitude,
    string Title,
    string? Description = null
);