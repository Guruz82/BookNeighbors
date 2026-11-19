using System.ComponentModel.DataAnnotations;

namespace BookNeighbors.DTO.Books;

public sealed class BookRequest
{
    [Required, StringLength(255)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Author { get; set; } = string.Empty;

    [StringLength(17)]
    public string? ISBN { get; set; }

    public string? Description { get; set; }

    public int? PublicationYear { get; set; }

    [StringLength(1024)]
    public string? ImageUrl { get; set; }

    [StringLength(1024)]
    public string? ThumbnailUrl { get; set; }

    public bool IsPublished { get; set; }

    public List<Guid> CategoryIds { get; set; } = [];
}