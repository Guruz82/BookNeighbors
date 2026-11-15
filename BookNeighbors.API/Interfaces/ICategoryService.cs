using BookNeighbors.Api.Models;

namespace BookNeighbors.Api.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync();
}