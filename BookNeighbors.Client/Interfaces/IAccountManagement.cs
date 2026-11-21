
using BookNeighbors.Client.Models.Identity;
using BookNeighbors.DTO.Account;

namespace BookNeighbors.Client.Interfaces;

/// <summary>
/// Account management services.
/// </summary>
public interface IAccountManagement
{
    /// <summary>
    /// Login service.
    /// </summary>
    /// <param name="email">User's email.</param>
    /// <param name="password">User's password.</param>
    /// <returns>The result of the request serialized to <see cref="FormResult"/>.</returns>
    public Task<FormResult> LoginAsync(string email, string password);

    /// <summary>
    /// Log out the logged in user.
    /// </summary>
    /// <returns>The asynchronous task.</returns>
    public Task LogoutAsync();

    /// <summary>
    /// Registration service.
    /// </summary>
    /// <param name="email">User's email.</param>
    /// <param name="password">User's password.</param>
    /// <returns>The result of the request serialized to <see cref="FormResult"/>.</returns>
    public Task<FormResult> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Checks if the user is authenticated.
    /// </summary>
    /// <returns></returns>
    public Task<bool> CheckAuthenticatedAsync();
    /// <summary>
    /// Gets the profile of the currently logged in user.
    /// </summary>
    /// <returns></returns>
    public Task<AccountProfileResponse?> GetProfileAsync();
    /// <summary>
    /// Updates the profile of the currently logged in user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>

    public Task<FormResult> UpdateProfileAsync(AccountProfileRequest request);

}
