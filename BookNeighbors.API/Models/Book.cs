namespace BookNeighbors.Api.Models;
/// <summary>
/// Rappresenta un libro all'interno del sistema.
/// </summary>
public class Book
{
    public Guid Id { get; set; }
    public Guid LibraryId { get; set; }
    public Library Library { get; set; } = null!;
    public ICollection<BookCategory> BookCategories { get; set; } = new List<BookCategory>();
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ISBN { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsPublished { get; set; } = false;
    public int? PublicationYear { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}