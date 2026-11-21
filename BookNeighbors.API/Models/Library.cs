using BookNeighbors.Api.Models.Identity;
using NetTopologySuite.Geometries;

namespace BookNeighbors.Api.Models;

/// <summary>
/// Rappresenta una biblioteca di libri.
/// </summary>
public class Library
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? AreaName { get; set; }

    public Point Location { get; set; } = null!;

    public bool LocationConsent { get; set; } = false;
    public bool IsPublished { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
