using System.ComponentModel.DataAnnotations;

namespace BookNeighbors.DTO.Account;

public class AccountProfileRequest
{
    [Required, StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

  

    public string? Bio { get; set; }
}