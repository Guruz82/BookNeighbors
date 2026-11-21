namespace BookNeighbors.Api.Models;

/// <summary>
/// Rappresenta la visualizzazione di un libro da parte di un utente.
/// </summary>

public class BookView
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}