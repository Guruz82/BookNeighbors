using System.ComponentModel.DataAnnotations;

namespace BookNeighbors.DTO.Account;

public class AccountProfileRequest
{
    [Required, StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    public string? Bio { get; set; }
}