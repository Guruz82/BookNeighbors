
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Enum;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.API.Enum;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;

public class LoanService(ApplicationDbContext db, INotificationService notificationService, INotificationRealtimePublisher realtimePublisher) : ILoanService
{
    /// <summary>
    /// Query con i dettagli delle richieste di prestito.
    /// </summary>
    private IQueryable<LoanRequest> Details => db.LoanRequests.AsNoTracking().Include(l => l.Book).Include(l => l.Status).Include(l => l.Requester);

    /// <summary>
    /// Restituisce le richieste di prestito inviate da un determinato utente.
    /// </summary>
    public async Task<List<LoanRequest>> GetSentAsync(Guid userId)
    {
        return await Details.Where(l => l.RequesterId == userId).OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Restituisce le richieste di prestito ricevute da un determinato utente.
    /// </summary>
    public async Task<List<LoanRequest>> GetReceivedAsync(Guid userId)
    {
        return await Details.Where(l => l.Book.Library.UserId == userId).OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    /// <summary>
    /// Blocca il libro durante le operazioni di prestito per evitare richieste concorrenti.
    /// </summary>
    private async Task<bool> LockBookAsync(Guid bookId)
    {
        var books = await db.Books.FromSqlInterpolated($"""SELECT * FROM "Books" WHERE "Id" = {bookId} FOR UPDATE""").AsNoTracking().ToListAsync();

        return books.Count != 0;
    }

    /// <summary>
    /// Crea una nuova richiesta di prestito e notifica il proprietario.
    /// </summary>
    public async Task<LoanResult> CreateAsync(Guid bookId, Guid userId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        if (!await LockBookAsync(bookId)) return new(LoanResultCode.NotFound);

        var book = await db.Books.AsNoTracking().Where(b => b.Id == bookId).Select(b => new
        {
            b.Title,
            b.IsPublished,
            OwnerId = b.Library.UserId,
            LibraryPublished = b.Library.IsPublished,
            b.Library.LocationConsent
        }).SingleAsync();

        if (book.OwnerId == userId) return new(LoanResultCode.OwnBook);
        if (!book.IsPublished || !book.LibraryPublished || !book.LocationConsent) return new(LoanResultCode.NotFound);

        var alreadyAccepted = await db.LoanRequests.AnyAsync(l => l.BookId == bookId && l.Status.Name == "Accepted");

        if (alreadyAccepted) return new(LoanResultCode.Unavailable);

        var duplicate = await db.LoanRequests.AnyAsync(l => l.BookId == bookId && l.RequesterId == userId && l.Status.Name == "Pending");

        if (duplicate) return new(LoanResultCode.Duplicate);

        var pending = await db.LoanRequestStatuses.SingleAsync(s => s.Name == "Pending");

        var loan = new LoanRequest
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            RequesterId = userId,
            StatusId = pending.Id
        };

        db.LoanRequests.Add(loan);

        notificationService.AddNotification(book.OwnerId, "Nuova richiesta di prestito", $"Hai ricevuto una nuova richiesta di prestito per il libro \"{book.Title}\".", "/prestiti");

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        await realtimePublisher.PublishAsync(book.OwnerId);

        return new(LoanResultCode.Success, await Details.SingleAsync(l => l.Id == loan.Id));
    }

    /// <summary>
    /// Cambia lo stato di una richiesta di prestito e invia le notifiche agli utenti interessati.
    /// </summary>
    public async Task<LoanResult> ChangeStatusAsync(Guid id, Guid userId, LoanAction action)
    {
        var bookId = await db.LoanRequests.AsNoTracking().Where(l => l.Id == id).Select(l => (Guid?)l.BookId).SingleOrDefaultAsync();

        if (bookId is null) return new(LoanResultCode.NotFound);

        var notifiedUserIds = new HashSet<Guid>();

        await using var transaction = await db.Database.BeginTransactionAsync();

        if (!await LockBookAsync(bookId.Value)) return new(LoanResultCode.NotFound);

        var loan = await db.LoanRequests.Include(l => l.Status).Include(l => l.Book).ThenInclude(b => b.Library).SingleOrDefaultAsync(l => l.Id == id);

        if (loan is null) return new(LoanResultCode.NotFound);

        var isOwner = loan.Book.Library.UserId == userId;
        var isRequester = loan.RequesterId == userId;

        if (action == LoanAction.Cancel)
        {
            if (!isRequester) return new(LoanResultCode.Forbidden);
        }
        else if (!isOwner)
        {
            return new(LoanResultCode.Forbidden);
        }

        var requiredStatus = action == LoanAction.Return ? "Accepted" : "Pending";

        if (loan.Status.Name != requiredStatus) return new(LoanResultCode.InvalidStatus);

        if (action == LoanAction.Accept)
        {
            if (!loan.Book.IsPublished || !loan.Book.Library.IsPublished || !loan.Book.Library.LocationConsent) return new(LoanResultCode.Unavailable);

            var alreadyAccepted = await db.LoanRequests.AnyAsync(l => l.BookId == bookId.Value && l.Id != id && l.Status.Name == "Accepted");

            if (alreadyAccepted) return new(LoanResultCode.Unavailable);
        }

        var newStatusName = action switch
        {
            LoanAction.Accept => "Accepted",
            LoanAction.Reject => "Rejected",
            LoanAction.Cancel => "Cancelled",
            LoanAction.Return => "Returned",
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        var status = await db.LoanRequestStatuses.SingleAsync(s => s.Name == newStatusName);

        loan.StatusId = status.Id;
        loan.Status = status;
        loan.UpdatedAt = DateTime.UtcNow;

        switch (action)
        {
            case LoanAction.Accept:
                {
                    loan.DueDate = DateTime.UtcNow.AddDays(30);

                    notificationService.AddNotification(loan.RequesterId, "Richiesta accettata", $"La tua richiesta per il libro \"{loan.Book.Title}\" è stata accettata. Scadenza prevista: {loan.DueDate:dd/MM/yyyy}.", "/prestiti");
                    notifiedUserIds.Add(loan.RequesterId);

                    var rejectedStatus = await db.LoanRequestStatuses.SingleAsync(s => s.Name == "Rejected");
                    var otherRequests = await db.LoanRequests.Include(l => l.Status).Where(l => l.BookId == loan.BookId && l.Id != loan.Id && l.Status.Name == "Pending").ToListAsync();

                    foreach (var otherRequest in otherRequests)
                    {
                        otherRequest.StatusId = rejectedStatus.Id;
                        otherRequest.Status = rejectedStatus;
                        otherRequest.UpdatedAt = DateTime.UtcNow;

                        notificationService.AddNotification(otherRequest.RequesterId, "Richiesta rifiutata", $"La tua richiesta per il libro \"{loan.Book.Title}\" è stata rifiutata perché il libro è stato assegnato a un altro utente.", "/prestiti");
                        notifiedUserIds.Add(otherRequest.RequesterId);
                    }

                    break;
                }

            case LoanAction.Reject:
                {
                    notificationService.AddNotification(loan.RequesterId, "Richiesta rifiutata", $"La tua richiesta per il libro \"{loan.Book.Title}\" è stata rifiutata.", "/prestiti");
                    notifiedUserIds.Add(loan.RequesterId);

                    break;
                }

            case LoanAction.Cancel:
                {
                    notificationService.AddNotification(loan.Book.Library.UserId, "Richiesta annullata", $"Una richiesta di prestito per il libro \"{loan.Book.Title}\" è stata annullata.", "/prestiti");
                    notifiedUserIds.Add(loan.Book.Library.UserId);

                    break;
                }

            case LoanAction.Return:
                {
                    loan.DueDate = null;

                    notificationService.AddNotification(loan.RequesterId, "Prestito concluso", $"Il prestito del libro \"{loan.Book.Title}\" è stato concluso.", "/prestiti");
                    notifiedUserIds.Add(loan.RequesterId);

                    break;
                }
        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        await realtimePublisher.PublishAsync(notifiedUserIds);

        return new(LoanResultCode.Success, await Details.SingleAsync(l => l.Id == loan.Id));
    }
}
