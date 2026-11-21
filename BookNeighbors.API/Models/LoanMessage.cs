
using BookNeighbors.Api.Models.Identity;

namespace BookNeighbors.Api.Models;

/// <summary>
/// Rappresenta un messaggio inviato tra utenti riguardante una richiesta di prestito di un libro.
/// </summary>
public class LoanMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoanRequestId { get; set; }
    public LoanRequest LoanRequest { get; set; } = null!;
    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
