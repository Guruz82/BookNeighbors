
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Statistics;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;

public class PersonalLoanStatisticsService(ApplicationDbContext db) : IPersonalLoanStatisticsService
{
    public async Task<PersonalLoanStatisticsResponse> GetAsync(Guid userId)
    {
        var loans = db.LoanRequests.AsNoTracking();

        var lentBooks = await loans.CountAsync(l => l.Book.Library.UserId == userId && (l.Status.Name == "Accepted" || l.Status.Name == "Returned"));

        var borrowedBooks = await loans.CountAsync(l => l.RequesterId == userId && (l.Status.Name == "Accepted" || l.Status.Name == "Returned"));

        var activeLoans = await loans.CountAsync(l => (l.RequesterId == userId || l.Book.Library.UserId == userId) && l.Status.Name == "Accepted");

        var completedLoans = await loans.CountAsync(l => (l.RequesterId == userId || l.Book.Library.UserId == userId) && l.Status.Name == "Returned");

        var now = DateTime.UtcNow;
        var currentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var firstMonth = currentMonth.AddMonths(-5);
        var endDate = currentMonth.AddMonths(1);

        var recentRequests = await loans.Where(l => (l.RequesterId == userId || l.Book.Library.UserId == userId) && l.CreatedAt >= firstMonth && l.CreatedAt < endDate).Select(l => new
        {
            l.CreatedAt,
            l.RequesterId,
            OwnerId = l.Book.Library.UserId
        }).ToListAsync();

        var months = new List<PersonalLoanMonthResponse>();

        for (var i = 0; i < 6; i++)
        {
            var month = firstMonth.AddMonths(i);
            var nextMonth = month.AddMonths(1);
            var monthRequests = recentRequests.Where(l => l.CreatedAt >= month && l.CreatedAt < nextMonth).ToList();

            var sent = monthRequests.Count(l => l.RequesterId == userId);
            var received = monthRequests.Count(l => l.OwnerId == userId);

            months.Add(new PersonalLoanMonthResponse(month.ToString("MMM yy", System.Globalization.CultureInfo.GetCultureInfo("it-IT")), sent, received));
        }

        return new PersonalLoanStatisticsResponse(lentBooks, borrowedBooks, activeLoans, completedLoans, months);
    }
}
