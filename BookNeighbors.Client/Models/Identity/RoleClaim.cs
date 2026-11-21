namespace BookNeighbors.Client.Models.Identity;

/// <summary>
/// Rappresenta un claim associato a un ruolo dell'utente.
/// </summary>
public class RoleClaim
{
    public string? Issuer { get; set; }
    public string? OriginalIssuer { get; set; }
    public string? Type { get; set; }
    public string? Value { get; set; }
    public string? ValueType { get; set; }
}
