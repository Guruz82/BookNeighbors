
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.Api.Services;
using BookNeighbors.DTO.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/statistics/my-loans")]
public class PersonalLoanStatisticsController(UserManager<ApplicationUser> userManager, IPersonalLoanStatisticsService statisticsService) : ControllerBase
{
    /// <summary>
    /// Restituisce le statistiche relative ai prestiti personali dell'utente autenticato.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<PersonalLoanStatisticsResponse>> Get()
    {
        var userIdValue = userManager.GetUserId(User);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var statistics = await statisticsService.GetAsync(userId);

        return Ok(statistics);
    }
}
