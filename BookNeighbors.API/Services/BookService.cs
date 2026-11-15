
using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Results;
using BookNeighbors.DTO.Books;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookNeighbors.Api.Services;

public class BookService(ApplicationDbContext _dbContext) : IBookService
{
    public async Task<List<Book>> GetBooksByUserIdAsync(Guid userId)
    {
        return await _dbContext.Books
            .AsNoTracking()
            .Include(b => b.BookCategories)
            .Where(b => b.Library.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(Guid id, Guid userId)
    {
        return await _dbContext.Books
            .AsNoTracking()
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);
    }

    public async Task<Book?> CreateBookAsync(
        Guid userId, BookRequest request)
    {
        var library = await _dbContext.Libraries
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.UserId == userId);

        if (library is null)
            return null;

        var categoryIds = await ValidateCategoriesAsync(request.CategoryIds);

        var book = new Book
        {
            Id = Guid.NewGuid(),
            LibraryId = library.Id,
            Title = request.Title.Trim(),
            Author = request.Author.Trim(),
            ISBN = request.ISBN?.Trim(),
            Description = request.Description,
            PublicationYear = request.PublicationYear,
            ImageUrl = request.ImageUrl,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublished = request.IsPublished,
            BookCategories = categoryIds.Select(id => new BookCategory
            {
                CategoryId = id
            }).ToList()
        };

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        return book;
    }

    public async Task<Book?> UpdateBookAsync(
        Guid id, Guid userId, BookRequest request)
    {
        var book = await _dbContext.Books
            .Include(b => b.BookCategories)
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);

        if (book is null)
            return null;

        var categoryIds = await ValidateCategoriesAsync(request.CategoryIds);

        book.Title = request.Title.Trim();
        book.Author = request.Author.Trim();
        book.ISBN = request.ISBN?.Trim();
        book.Description = request.Description;
        book.PublicationYear = request.PublicationYear;
        book.ImageUrl = request.ImageUrl;
        book.ThumbnailUrl = request.ThumbnailUrl;
        book.IsPublished = request.IsPublished;
        book.UpdatedAt = DateTime.UtcNow;

        // Rimuove le categorie non più selezionate
        var selectedIds = categoryIds.ToHashSet();

        foreach (var category in book.BookCategories
                     .Where(c => !selectedIds.Contains(c.CategoryId))
                     .ToList())
        {
            _dbContext.BookCategories.Remove(category);
        }

        // Aggiunge soltanto le nuove categorie
        var existingIds = book.BookCategories
            .Select(c => c.CategoryId)
            .ToHashSet();

        foreach (var categoryId in selectedIds.Except(existingIds))
        {
            book.BookCategories.Add(new BookCategory
            {
                BookId = book.Id,
                CategoryId = categoryId
            });
        }

        await _dbContext.SaveChangesAsync();

        return book;
    }

    private async Task<Guid[]> ValidateCategoriesAsync(
        IEnumerable<Guid> categoryIds)
    {
        var ids = categoryIds.Distinct().ToArray();

        if (ids.Length == 0)
            return ids;

        var count = await _dbContext.Categories
            .CountAsync(c => ids.Contains(c.Id));

        if (count != ids.Length)
            throw new ArgumentException(
                "Una o più categorie selezionate non esistono.");

        return ids;
    }
    public async Task<DeleteBookResult> DeleteBookAsync(
        Guid id, Guid userId)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);

        if (book is null)
            return DeleteBookResult.NotFound;

        var hasLoans = await _dbContext.LoanRequests
            .AnyAsync(l => l.BookId == id);

        if (hasLoans)
            return DeleteBookResult.HasLoanRequests;

        _dbContext.Books.Remove(book);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException pgEx &&
            pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation &&
            pgEx.ConstraintName == "FK_LoanRequests_Books_book_id")
        {
            return DeleteBookResult.HasLoanRequests;
        }

        return DeleteBookResult.Deleted;
    }
    public async Task<bool> UnpublishBookAsync(Guid id, Guid userId)
    {
        var book = await _dbContext.Books
            .FirstOrDefaultAsync(b =>
                b.Id == id && b.Library.UserId == userId);

        if (book is null)
            return false;

        book.IsPublished = false;
        book.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return true;
    }
}
