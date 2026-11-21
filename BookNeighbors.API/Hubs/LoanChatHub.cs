
using BookNeighbors.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookNeighbors.Api.Hubs;
/// <summary>
/// Hub SignalR per la gestione delle conversazioni relative alle richieste di prestito.
/// </summary>
/// <param name="db"></param>
[Authorize]
public class LoanChatHub(ApplicationDbContext db) : Hub
{
    /// <summary>
    ///  Restituisce il nome del gruppo SignalR associato a un utente specifico.
    ///  Il nome del gruppo è costruito concatenando la stringa "user-" con l'ID dell'utente in formato stringa senza trattini.
    ///  Questo metodo viene
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static string UserGroup(Guid userId) => $"user-{userId:N}";
    public static string GroupName(Guid loanId) => $"loan-{loanId:N}";
    /// <summary>
    /// Aggiunge l'utente corrente al gruppo della conversazione relativa alla richiesta di prestito specificata.
    /// Restituisce un'eccezione HubException se l'utente non è autenticato o non è autorizzato ad accedere alla conversazione.
    /// </summary>
    /// <param name="loanId"></param>
    /// <returns></returns>
    /// <exception cref="HubException"></exception>
    public async Task JoinLoan(Guid loanId)
    {
        if (!Guid.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) throw new HubException("Utente non autenticato.");

        var authorized = await db.LoanRequests.AsNoTracking().AnyAsync(l => l.Id == loanId && (l.RequesterId == userId || l.Book.Library.UserId == userId));

        if (!authorized) throw new HubException("Non sei autorizzato ad accedere a questa conversazione.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(loanId));
    }

    /// <summary>
    /// Rimuove l'utente corrente dal gruppo della conversazione relativa alla richiesta di prestito specificata.
    /// Restituisce un'eccezione HubException se l'utente non è autenticato o non è autorizzato ad accedere alla conversazione.
    /// </summary>
    /// <param name="loanId"></param>
    /// <returns></returns>
    public async Task LeaveLoan(Guid loanId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(loanId));
    }


    /// <summary>
    /// Gestisce la connessione di un utente al hub SignalR. Aggiunge l'utente al gruppo SignalR associato al suo ID utente.
    /// Restituisce un'eccezione HubException se l'utente non è autenticato. Questo metodo viene chiamato automaticamente quando un utente si connette al hub.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="HubException"></exception>
    public override async Task OnConnectedAsync()
    {
        var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            throw new HubException("Utente non autenticato.");

        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        await base.OnConnectedAsync();
    }

}
