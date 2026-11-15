using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Categories;

namespace BookNeighbors.API.Mappers;

public static class CategoryMapper
{
    public static CategoryResponse ToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name
        );
    }
}