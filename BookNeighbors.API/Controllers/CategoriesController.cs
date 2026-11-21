using BookNeighbors.Api.Interfaces;
using BookNeighbors.API.Mappers;
using BookNeighbors.DTO.Categories;
using Microsoft.AspNetCore.Mvc;

namespace BookNeighbors.Api.Controllers;



[ApiController]
[Route("api/category")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{

    /// <summary>
    /// Restituisce tutte le categorie disponibili.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponse>>> GetAll()
    {
        var categories = await categoryService.GetCategoriesAsync();

        return Ok(categories
            .Select(CategoryMapper.ToResponse)
            .ToList());
    }
}