using System.ComponentModel.DataAnnotations;

namespace BookNeighbors.DTO.Account;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public bool PrivacyNoticeAcknowledged { get; set; }
}