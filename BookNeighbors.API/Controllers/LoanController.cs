
using BookNeighbors.Api.Enum;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Mappers;
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.API.Enum;
using BookNeighbors.DTO.Loans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/loan")]
public class LoanController(
    UserManager<ApplicationUser> userManager,
    ILoanService loanService) : ControllerBase
{
    private Guid? CurrentUserId =>
        Guid.TryParse(userManager.GetUserId(User), out var id)
            ? id
            : null;

    [HttpGet("sent")]
    public async Task<ActionResult<List<LoanResponse>>> GetSent()
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var loans = await loanService.GetSentAsync(userId);

        return Ok(loans.Select(LoanMapper.ToResponse).ToList());
    }

    [HttpGet("received")]
    public async Task<ActionResult<List<LoanResponse>>> GetReceived()
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var loans = await loanService.GetReceivedAsync(userId);

        return Ok(loans.Select(LoanMapper.ToResponse).ToList());
    }

    [HttpPost("{bookId:guid}")]
    public async Task<ActionResult<LoanResponse>> Create(Guid bookId)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var result = await loanService.CreateAsync(bookId, userId);

        return ToActionResult(result, created: true);
    }

    [HttpPatch("{id:guid}/accept")]
    public Task<ActionResult<LoanResponse>> Accept(Guid id) =>
        ChangeStatus(id, LoanAction.Accept);

    [HttpPatch("{id:guid}/reject")]
    public Task<ActionResult<LoanResponse>> Reject(Guid id) =>
        ChangeStatus(id, LoanAction.Reject);

    [HttpPatch("{id:guid}/cancel")]
    public Task<ActionResult<LoanResponse>> Cancel(Guid id) =>
        ChangeStatus(id, LoanAction.Cancel);

    [HttpPatch("{id:guid}/return")]
    public Task<ActionResult<LoanResponse>> Return(Guid id) =>
        ChangeStatus(id, LoanAction.Return);

    private async Task<ActionResult<LoanResponse>> ChangeStatus(
        Guid id, LoanAction action)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        var result = await loanService.ChangeStatusAsync(
            id, userId, action);

        return ToActionResult(result);
    }

    private ActionResult<LoanResponse> ToActionResult(
        LoanResult result,
        bool created = false)
    {
        return result.Code switch
        {
            LoanResultCode.Success when created =>
                StatusCode(201, LoanMapper.ToResponse(result.Loan!)),

            LoanResultCode.Success =>
                Ok(LoanMapper.ToResponse(result.Loan!)),

            LoanResultCode.NotFound => NotFound(),

            LoanResultCode.OwnBook =>
                BadRequest("Non puoi richiedere un tuo libro."),

            LoanResultCode.Duplicate =>
                Conflict("Hai già una richiesta in attesa."),

            LoanResultCode.Unavailable =>
                Conflict("Il libro non è disponibile per il prestito."),

            LoanResultCode.Forbidden => Forbid(),

            LoanResultCode.InvalidStatus =>
                Conflict("Operazione non consentita nello stato attuale."),

            _ => StatusCode(500)
        };
    }
}
