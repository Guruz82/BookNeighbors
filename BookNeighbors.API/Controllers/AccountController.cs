
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.DTO.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController(UserManager<ApplicationUser> userManager) : ControllerBase
{

    /// <summary>
    /// Endpoint per la registrazione di un nuovo utente.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest("Il nome visualizzato è obbligatorio.");

        if (!request.PrivacyNoticeAcknowledged)
            return BadRequest("Devi confermare la presa visione dell'informativa privacy.");

        var email = request.Email.Trim();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PrivacyNoticeAcknowledged = true,
            PrivacyNoticeAcknowledgedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return IdentityErrors(result);

        return StatusCode(StatusCodes.Status201Created);
    }



    /// <summary>
    /// Endpoint per ottenere le informazioni del profilo dell'utente autenticato.
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AccountProfileResponse>> GetMe()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        return Ok(ToResponse(user));
    }


    /// <summary>
    /// Endpoint per aggiornare le informazioni del profilo dell'utente autenticato.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>

    [Authorize]
    [HttpPut("me")]
    public async Task<ActionResult<AccountProfileResponse>> UpdateMe([FromBody] AccountProfileRequest request)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest("Il nome visualizzato è obbligatorio.");

        user.DisplayName = request.DisplayName.Trim();
        user.Bio = request.Bio?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return IdentityErrors(result);

        return Ok(ToResponse(user));
    }



    /// <summary>
    /// Endpoint per ottenere le informazioni di autenticazione dell'utente autenticato, inclusi ID, email e ruoli.
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet("auth-info")]
    public async Task<IActionResult> GetAuthInfo()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
            return Unauthorized();

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            Roles = roles
        });
    }



    /// <summary>
    /// Restituisce un oggetto BadRequestObjectResult contenente gli errori di Identity in un formato leggibile.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private BadRequestObjectResult IdentityErrors(IdentityResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray());

        return BadRequest(new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Operazione non riuscita"
        });
    }


    /// <summary>
    /// Converte un oggetto ApplicationUser in un oggetto AccountProfileResponse.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private static AccountProfileResponse ToResponse(ApplicationUser user) => new(
        user.Id,
        user.Email ?? string.Empty,
        user.DisplayName,
        user.Bio,
        user.PrivacyNoticeAcknowledged
    );
}
