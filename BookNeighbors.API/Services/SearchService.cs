
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.DTO.Search;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;

public class SearchService(
    ApplicationDbContext db,
    IGeoLocationService geoLocationService) : ISearchService
{
    public async Task<List<BookSearchResponse>> SearchBooksAsync(
        BookSearchRequest request)
    {
        var point = geoLocationService.CreatePoint(
            request.Latitude!.Value,
            request.Longitude!.Value);

        var radiusMeters = request.RadiusKm * 1000;

        var query = db.Books
            .AsNoTracking()
            .Where(b =>
                b.IsPublished &&
                b.Library.IsPublished &&
                b.Library.LocationConsent &&
                b.Library.Location.Distance(point) <= radiusMeters);

        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            var pattern = $"%{request.Text.Trim()}%";

            query = query.Where(b =>
                EF.Functions.ILike(b.Title, pattern) ||
                EF.Functions.ILike(b.Author, pattern) ||
                EF.Functions.ILike(b.ISBN ?? "", pattern));
        }

        if (request.CategoryId.HasValue)
        {
            var categoryId = request.CategoryId.Value;

            query = query.Where(b =>
                b.BookCategories.Any(c =>
                    c.CategoryId == categoryId));
        }

        var results = await query
            .OrderBy(b => b.Library.Location.Distance(point))
            .Take(50)
            .Select(b => new
            {
                Book = b,
                Library = b.Library,
                DistanceMeters = b.Library.Location.Distance(point)

                 IsOnLoan = db.LoanRequests.Any(l =>
            l.BookId == b.Id &&
            l.Status.Name == "Accepted")
            })
            .ToListAsync();

        return results.Select(x => new BookSearchResponse(
            x.Book.Id,
            x.Book.LibraryId,
            x.Library.Name,
            x.Library.City,
            x.Book.Title,
            x.Book.Author,
            x.Book.ThumbnailUrl,
            x.Library.Location.Y,
            x.Library.Location.X,
            Math.Round(x.DistanceMeters / 1000, 2),
            x.IsLoaned
        )).ToList();
    }
}
