
namespace BookNeighbors.API.Enum;
/// <summary>
/// Codici di risultato per le operazioni di prestito.
/// </summary>
public enum LoanResultCode
{
    Success,
    NotFound,
    OwnBook,
    Duplicate,
    Unavailable,
    Forbidden,
    InvalidStatus
}
