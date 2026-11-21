using BookNeighbors.Api.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace BookNeighbors.Api.Validators;

public class ApplicationUserValidator : IUserValidator<ApplicationUser>
{
    /// <summary>
    /// Valida un oggetto ApplicationUser per assicurarsi che soddisfi i requisiti specificati.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        var errors = new List<IdentityError>();

        if (string.IsNullOrWhiteSpace(user.DisplayName))
        {
            errors.Add(new IdentityError
            {
                Code = "DisplayNameRequired",
                Description = "Il nome visualizzato è obbligatorio."
            });
        }

        if (!user.PrivacyNoticeAcknowledged || user.PrivacyNoticeAcknowledgedAt is null)
        {
            errors.Add(new IdentityError
            {
                Code = "PrivacyNoticeRequired",
                Description = "È necessaria la presa visione dell'informativa privacy."
            });
        }

        return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
    }
}