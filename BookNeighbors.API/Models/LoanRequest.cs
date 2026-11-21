using BookNeighbors.Api.Models.Identity;

namespace BookNeighbors.Api.Models;

/// <summary>
/// Rappresenta una richiesta di prestito di un libro da parte di un utente.
/// </summary>
public class LoanRequest
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;
    public Guid RequesterId { get; set; }
    public ApplicationUser Requester { get; set; } = null!;
    public Guid StatusId { get; set; }
    public LoanRequestStatus Status { get; set; } = null!;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}