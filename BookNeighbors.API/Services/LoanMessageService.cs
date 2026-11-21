
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Enum;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.DTO.LoanMessages;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;
/// <summary>
/// Servizio per la gestione dei messaggi di prestito.
/// </summary>
/// <param name="db"></param>
/// <param name="notificationService"></param>
public class LoanMessageService(ApplicationDbContext db, INotificationService notificationService) : ILoanMessageService
{
    /// <summary>
    ///  Restituisce i messaggi associati a una determinata richiesta di prestito, se l'utente è autorizzato a visualizzarli.
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<LoanMessageResponse>?> GetMessagesAsync(Guid loanId, Guid userId)
    {
        var authorized = await db.LoanRequests.AsNoTracking().AnyAsync(l => l.Id == loanId && (l.RequesterId == userId || l.Book.Library.UserId == userId));

        if (!authorized) return null;

        var messages = await db.LoanMessages.AsNoTracking().Where(m => m.LoanRequestId == loanId).OrderByDescending(m => m.CreatedAt).ThenByDescending(m => m.Id).Take(100).Select(m => new LoanMessageResponse(m.Id, m.LoanRequestId, m.SenderId, m.Sender.DisplayName, m.Text, m.CreatedAt)).ToListAsync();

        messages.Reverse();

        return messages;
    }

    /// <summary>
    /// Invia un messaggio relativo a una richiesta di prestito, se l'utente è autorizzato a farlo e la conversazione è aperta.
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="senderId"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<LoanMessageSendResult> SendMessageAsync(Guid loanId, Guid senderId, string text)
    {
        var loan = await db.LoanRequests.AsNoTracking().Where(l => l.Id == loanId && (l.RequesterId == senderId || l.Book.Library.UserId == senderId)).Select(l => new
        {
            l.RequesterId,
            OwnerId = l.Book.Library.UserId,
            BookTitle = l.Book.Title,
            Status = l.Status.Name
        }).SingleOrDefaultAsync();

        if (loan is null) return new(null, LoanMessageError.NotFound);

        if (loan.Status != "Pending" && loan.Status != "Accepted") return new(null, LoanMessageError.Closed);

        var senderName = await db.Users.AsNoTracking().Where(u => u.Id == senderId).Select(u => u.DisplayName).SingleAsync();
        var recipientId = senderId == loan.RequesterId ? loan.OwnerId : loan.RequesterId;

        var message = new LoanMessage
        {
            LoanRequestId = loanId,
            SenderId = senderId,
            Text = text.Trim()
        };

        db.LoanMessages.Add(message);
        notificationService.AddNotification(recipientId, "Nuovo messaggio", $"Hai ricevuto un messaggio relativo al libro \"{loan.BookTitle}\".", $"/prestiti/{loanId}/chat");

        await db.SaveChangesAsync();

        var response = new LoanMessageResponse(message.Id, message.LoanRequestId, senderId, senderName, message.Text, message.CreatedAt);

        return new(response, LoanMessageError.None);
    }

    /// <summary>
    /// Restituisce l'ID del destinatario di un messaggio relativo a una richiesta di prestito, se l'utente è autorizzato a inviare messaggi.
    /// </summary>
    /// <param name="loanId"></param>
    /// <param name="senderId"></param>
    /// <returns></returns>
    public async Task<Guid?> GetRecipientIdAsync(Guid loanId, Guid senderId)
    {
        var loan = await db.LoanRequests.AsNoTracking().Where(l => l.Id == loanId && (l.RequesterId == senderId || l.Book.Library.UserId == senderId)).Select(l => new
        {
            l.RequesterId,
            OwnerId = l.Book.Library.UserId
        }).SingleOrDefaultAsync();

        if (loan is null)
            return null;

        return loan.RequesterId == senderId ? loan.OwnerId : loan.RequesterId;
    }

}
