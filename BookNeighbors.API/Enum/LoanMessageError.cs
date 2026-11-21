namespace BookNeighbors.Api.Enum;

/// <summary>
/// Rappresenta gli errori che possono verificarsi durante l'invio di un messaggio di prestito.
/// </summary>
public enum LoanMessageError
{
    None,
    NotFound,
    Closed
}