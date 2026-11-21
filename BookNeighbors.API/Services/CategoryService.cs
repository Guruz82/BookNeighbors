using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookNeighbors.Api.Services;

public class CategoryService(ApplicationDbContext dbContext) : ICategoryService
{
    /// <summary>
    /// Restituisce tutte le categorie disponibili.
    /// </summary>
    /// <returns></returns>
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}