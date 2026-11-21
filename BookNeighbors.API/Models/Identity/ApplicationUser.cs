using Microsoft.AspNetCore.Identity;



namespace BookNeighbors.Api.Models.Identity;

public class ApplicationUser : IdentityUser<Guid>
{

    /// <summary>
    /// Restituisce o imposta il nome visualizzato dell'utente.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>
    ///  Restituisce o imposta la biografia dell'utente.
    /// </summary>
    public string? Bio { get; set; } = string.Empty;
    /// <summary>
    ///  Flag che indica se l'utente ha riconosciuto l'informativa sulla privacy.
    /// </summary>
    public bool PrivacyNoticeAcknowledged { get; set; } = false;
    /// <summary>
    /// data e ora in cui l'utente ha riconosciuto l'informativa sulla privacy
    /// </summary>
    public DateTime? PrivacyNoticeAcknowledgedAt { get; set; }
    /// <summary>
    /// data creazione dell'utente
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// data modifica dell'utente
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}