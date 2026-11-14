namespace BookNeighbors.Api.Models;
public class BookView
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
     public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}