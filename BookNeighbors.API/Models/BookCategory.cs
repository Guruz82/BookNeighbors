namespace BookNeighbors.Api.Models;

/// <summary>
/// Rappresenta la relazione tra un libro e una categoria.
/// </summary>
public class BookCategory
{
    public Guid BookId { get; set; }
    public Guid CategoryId { get; set; }
    public Book Book { get; set; } = null!;
    public Category Category { get; set; } = null!;

}