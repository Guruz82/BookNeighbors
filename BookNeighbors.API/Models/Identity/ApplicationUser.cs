using Microsoft.AspNetCore.Identity;



namespace BookNeighbors.Api.Models.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; } 
    public string? LastName { get; set; } 
        
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; } = string.Empty;
    public bool PrivacyNoticeAcknowledged { get; set; } = false;
    public DateTime? PrivacyNoticeAcknowledgedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}