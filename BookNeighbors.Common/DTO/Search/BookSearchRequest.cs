using System.ComponentModel.DataAnnotations;

namespace BookNeighbors.DTO.Search;

public class BookSearchRequest
{
    [Required, Range(-90.0, 90.0)]
    public double? Latitude { get; set; }

    [Required, Range(-180.0, 180.0)]
    public double? Longitude { get; set; }

    [Range(1, 100)]
    public double RadiusKm { get; set; } = 10;

    [StringLength(100)]
    public string? Text { get; set; }

    public Guid? CategoryId { get; set; }
}