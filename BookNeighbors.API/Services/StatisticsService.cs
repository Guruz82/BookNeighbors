using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Statistics;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;

public sealed class StatisticsService(ApplicationDbContext db) : IStatisticsService
{
    /// <summary>
    /// Restituisce le statistiche generali del sistema, come il numero di utenti, librerie, libri, richieste di prestito e visualizzazioni di libri.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<DashboardStatisticsResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var users = await db.Users.CountAsync(cancellationToken);

        var libraries = await db.Libraries.CountAsync(cancellationToken);

        var publishedLibraries = await db.Libraries.CountAsync(l => l.IsPublished && l.LocationConsent, cancellationToken);

        var books = await db.Books.CountAsync(cancellationToken);

        var publishedBooks = await db.Books
            .CountAsync(
                b => b.IsPublished &&
                     b.Library.IsPublished &&
                     b.Library.LocationConsent,
                cancellationToken);

        var loanRequests = await db.LoanRequests
            .CountAsync(cancellationToken);

        var activeLoans = await db.LoanRequests
            .CountAsync(
                l => l.Status.Name == "Accepted",
                cancellationToken);

        var bookViews = await db.BookViews.CountAsync(cancellationToken);

        return new DashboardStatisticsResponse(
            users,
            libraries,
            publishedLibraries,
            books,
            publishedBooks,
            loanRequests,
            activeLoans,
            bookViews
        );
    }

    /// <summary>
    /// Restituisce il numero di richieste di prestito per ciascuno stato, come "Pending", "Accepted", "Rejected", "Returned" e "Cancelled".
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<ChartDataResponse>> GetLoanStatusesAsync(
        CancellationToken cancellationToken = default)
    {
        var counts = await db.LoanRequests
            .AsNoTracking()
            .GroupBy(l => l.Status.Name)
            .Select(g => new
            {
                Name = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        string[] statuses =
        [
            "Pending",
            "Accepted",
            "Rejected",
            "Returned",
            "Cancelled"
        ];

        return statuses
            .Select(status => new ChartDataResponse(
                status,
                counts.FirstOrDefault(
                    x => x.Name == status)?.Count ?? 0
            ))
            .ToList();
    }
    /// <summary>
    /// Restituisce la tendenza delle richieste di prestito negli ultimi 30 giorni.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<ChartDataResponse>> GetLoanTrendAsync(
        CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow.Date.AddDays(-29);

        var counts = await db.LoanRequests
            .AsNoTracking()
            .Where(l => l.CreatedAt >= start)
            .GroupBy(l => l.CreatedAt.Date)
            .Select(g => new
            {
                Day = g.Key,
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var byDay = counts.ToDictionary(
            x => x.Day,
            x => x.Count
        );

        // Inserisce anche i giorni senza richieste.
        return Enumerable.Range(0, 30)
            .Select(offset =>
            {
                var day = start.AddDays(offset);

                return new ChartDataResponse(
                    day.ToString("dd/MM"),
                    byDay.GetValueOrDefault(day)
                );
            })
            .ToList();
    }

    /// <summary>
    /// Restituisce i 10 libri con più visualizzazioni registrate.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<List<ChartDataResponse>> GetTopBooksAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Books
            .AsNoTracking()
            .Select(b => new
            {
                b.Title,
                Views = db.BookViews.Count(
                    v => v.BookId == b.Id)
            })
            .Where(x => x.Views > 0)
            .OrderByDescending(x => x.Views)
            .Take(10)
            .Select(x => new ChartDataResponse(
                x.Title,
                x.Views
            ))
            .ToListAsync(cancellationToken);
    }
}