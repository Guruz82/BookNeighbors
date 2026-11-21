using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Categories;

namespace BookNeighbors.API.Mappers;

public static class CategoryMapper
{
    /// <summary>
    /// Mappa un oggetto Category in un oggetto CategoryResponse.
    /// </summary>
    /// <param name="category"></param>
    /// <returns></returns>
    public static CategoryResponse ToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name
        );
    }
}