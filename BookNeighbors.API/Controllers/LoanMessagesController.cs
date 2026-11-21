
using BookNeighbors.Api.Enum;
using BookNeighbors.Api.Hubs;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.Api.Services;
using BookNeighbors.DTO.LoanMessages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace BookNeighbors.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/loans/{loanId:guid}/messages")]
/// <summary>
/// Controller per la gestione dei messaggi relativi alle richieste di prestito.
/// </summary>      
public class LoanMessagesController(UserManager<ApplicationUser> userManager, ILoanMessageService messageService, IHubContext<LoanChatHub> hub, ILogger<LoanMessagesController> logger, INotificationRealtimePublisher realtimePublisher) : ControllerBase
{
    private Guid? CurrentUserId => Guid.TryParse(userManager.GetUserId(User), out var id) ? id : null;

    [HttpGet]
    public async Task<ActionResult<List<LoanMessageResponse>>> GetMessages(Guid loanId)
    {
        if (CurrentUserId is not Guid userId) return Unauthorized();

        var messages = await messageService.GetMessagesAsync(loanId, userId);

        if (messages is null) return NotFound();

        return Ok(messages);
    }


    /// <summary>
    /// Invia un messaggio relativo a una richiesta di prestito.
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<LoanMessageResponse>> SendMessage(Guid loanId, [FromBody] LoanMessageRequest request)
    {
        if (CurrentUserId is not Guid userId) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Text)) return BadRequest("Il messaggio non può essere vuoto.");

        if (request.Text.Length > 2000) return BadRequest("Il messaggio non può superare 2000 caratteri.");

        var result = await messageService.SendMessageAsync(loanId, userId, request.Text);

        if (result.Error == LoanMessageError.NotFound) return NotFound();

        if (result.Error == LoanMessageError.Closed) return Conflict("La conversazione non consente più l'invio di messaggi.");

        var message = result.Message!;
        var recipientId = await messageService.GetRecipientIdAsync(loanId, userId);

        if (recipientId is Guid id)
            await realtimePublisher.PublishAsync(id);
        try
        {
            await hub.Clients.Group(LoanChatHub.GroupName(loanId)).SendAsync("ReceiveLoanMessage", message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Messaggio {MessageId} salvato, ma consegna SignalR non riuscita.", message.Id);
        }

        return Ok(message);
    }
}
