
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Enum;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.API.Enum;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;

public class LoanService(ApplicationDbContext db) : ILoanService
{
    private IQueryable<LoanRequest> Details =>
        db.LoanRequests
            .AsNoTracking()
            .Include(l => l.Book)
            .Include(l => l.Status)
            .Include(l => l.Requester);

    public async Task<List<LoanRequest>> GetSentAsync(Guid userId)
    {
        return await Details
            .Where(l => l.RequesterId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<LoanRequest>> GetReceivedAsync(Guid userId)
    {
        return await Details
            .Where(l => l.Book.Library.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    private async Task<bool> LockBookAsync(Guid bookId)
    {
        // Tutte le operazioni di prestito usano questo lock
        // all'interno della rispettiva transazione.
        var books = await db.Books
            .FromSqlInterpolated(
                $"""SELECT * FROM "Books" WHERE "Id" = {bookId} FOR UPDATE""")
            .AsNoTracking()
            .ToListAsync();

        return books.Count != 0;
    }

    public async Task<LoanResult> CreateAsync(
        Guid bookId, Guid userId)
    {
        await using var transaction =
            await db.Database.BeginTransactionAsync();

        if (!await LockBookAsync(bookId))
            return new(LoanResultCode.NotFound);

        var book = await db.Books
            .AsNoTracking()
            .Where(b => b.Id == bookId)
            .Select(b => new
            {
                b.IsPublished,
                OwnerId = b.Library.UserId,
                LibraryPublished = b.Library.IsPublished,
                b.Library.LocationConsent
            })
            .SingleAsync();

        if (book.OwnerId == userId)
            return new(LoanResultCode.OwnBook);

        if (!book.IsPublished ||
            !book.LibraryPublished ||
            !book.LocationConsent)
            return new(LoanResultCode.NotFound);

        var alreadyAccepted = await db.LoanRequests
            .AnyAsync(l =>
                l.BookId == bookId &&
                l.Status.Name == "Accepted");

        if (alreadyAccepted)
            return new(LoanResultCode.Unavailable);

        var duplicate = await db.LoanRequests
            .AnyAsync(l =>
                l.BookId == bookId &&
                l.RequesterId == userId &&
                l.Status.Name == "Pending");

        if (duplicate)
            return new(LoanResultCode.Duplicate);

        var pending = await db.LoanRequestStatuses
            .SingleAsync(s => s.Name == "Pending");

        var loan = new LoanRequest
        {
            Id = Guid.NewGuid(),
            BookId = bookId,
            RequesterId = userId,
            StatusId = pending.Id
        };

        db.LoanRequests.Add(loan);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new(
            LoanResultCode.Success,
            await Details.SingleAsync(l => l.Id == loan.Id));
    }

    public async Task<LoanResult> ChangeStatusAsync(Guid id, Guid userId, LoanAction action)
    {
        // Recuperiamo l'ID del libro prima di acquisire il lock.
        var bookId = await db.LoanRequests
            .AsNoTracking()
            .Where(l => l.Id == id)
            .Select(l => (Guid?)l.BookId)
            .SingleOrDefaultAsync();

        if (bookId is null)
            return new(LoanResultCode.NotFound);

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        if (!await LockBookAsync(bookId.Value))
            return new(LoanResultCode.NotFound);

        var loan = await db.LoanRequests
            .Include(l => l.Status)
            .Include(l => l.Book)
                .ThenInclude(b => b.Library)
            .SingleOrDefaultAsync(l => l.Id == id);

        if (loan is null)
            return new(LoanResultCode.NotFound);

        var isOwner = loan.Book.Library.UserId == userId;
        var isRequester = loan.RequesterId == userId;

        if (action == LoanAction.Cancel)
        {
            if (!isRequester)
                return new(LoanResultCode.Forbidden);
        }
        else if (!isOwner)
        {
            return new(LoanResultCode.Forbidden);
        }

        var requiredStatus = action == LoanAction.Return
            ? "Accepted"
            : "Pending";

        if (loan.Status.Name != requiredStatus)
            return new(LoanResultCode.InvalidStatus);

        if (action == LoanAction.Accept)
        {
            if (!loan.Book.IsPublished ||
                !loan.Book.Library.IsPublished ||
                !loan.Book.Library.LocationConsent)
                return new(LoanResultCode.Unavailable);

            var alreadyAccepted = await db.LoanRequests
                .AnyAsync(l =>
                    l.BookId == bookId.Value &&
                    l.Id != id &&
                    l.Status.Name == "Accepted");

            if (alreadyAccepted)
                return new(LoanResultCode.Unavailable);
        }

        var newStatusName = action switch
        {
            LoanAction.Accept => "Accepted",
            LoanAction.Reject => "Rejected",
            LoanAction.Cancel => "Cancelled",
            LoanAction.Return => "Returned",
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        var status = await db.LoanRequestStatuses
            .SingleAsync(s => s.Name == newStatusName);

        loan.StatusId = status.Id;
        loan.Status = status;
        loan.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new(
            LoanResultCode.Success,
            await Details.SingleAsync(l => l.Id == loan.Id));
    }
}
