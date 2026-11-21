using BookNeighbors.Api.Models;

namespace BookNeighbors.Api.Interfaces;

public interface ICategoryService
{
    /// <summary>
    /// Restituisce la lista di tutte le categorie disponibili.
    /// </summary>
    /// <returns></returns>
    Task<List<Category>> GetCategoriesAsync();
}