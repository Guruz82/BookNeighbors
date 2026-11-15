using System.ComponentModel.DataAnnotations;

namespace BookNeighbors.DTO.Libraries;

public sealed class LibraryRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(150)]
    public string? AreaName { get; set; }

    [Required, Range(-90.0, 90.0)]
    public double? Latitude { get; set; }

    [Required, Range(-180.0, 180.0)]
    public double? Longitude { get; set; }

    public bool LocationConsent { get; set; }
    public bool IsPublished { get; set; }
}

