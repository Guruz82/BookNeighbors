namespace BookNeighbors.Api.Models;



/// <summary>
/// Rappresenta lo stato di una richiesta di prestito di un libro.
/// </summary>
public class LoanRequestStatus
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}