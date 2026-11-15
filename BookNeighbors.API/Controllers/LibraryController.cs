 

using BookNeighbors.Api.Models;
using BookNeighbors.Api.Models.Identity;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.API.Mappers.LibraryMapper;
using BookNeighbors.DTO.Libraries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;


namespace BookNeighbors.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/library")]
public class LibraryController( UserManager<ApplicationUser> userManager, ILibraryService _libraryService ) : ControllerBase
{


    
    private Guid? CurrentUserId =>
        Guid.TryParse(userManager.GetUserId(User), out var id)
            ? id
            : null;

    [HttpGet("mine")]
    public async Task<ActionResult<LibraryResponse>> GetMine()
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        // Recupera la libreria dell'utente corrente
        var library =  await _libraryService.GetLibraryByUserIdAsync(userId);

        return library is null
            ? NotFound()
            : Ok(ToResponse(library));
    }

   

    [HttpPost]
    public async Task<ActionResult<LibraryResponse>> Create([FromBody] LibraryRequest request)
    {
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Il nome è obbligatorio.");

        if (!ValidLocation(request))
            return BadRequest("Coordinate non valide.");

        if (request.IsPublished && !request.LocationConsent)
            return BadRequest("È necessario autorizzare la condivisione della posizione.");
        try
        {
        var library = await _libraryService.CreateLibraryAsync(userId, request);
 
            return CreatedAtAction(
                nameof(GetMine),
                routeValues: null,
                value: ToResponse(library));
        }
        catch (DbUpdateException ex) when (
                ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                pgEx.ConstraintName == "IX_Libraries_user_id")
            {
                return Conflict("L'utente possiede già una libreria personale.");
            }
      
    }

    [HttpPut]
    public async Task<ActionResult<LibraryResponse>> Update([FromBody] LibraryRequest request)
    {           
        if (CurrentUserId is not Guid userId)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Il nome è obbligatorio.");

        if (!ValidLocation(request))
            return BadRequest("Coordinate non valide.");

        if (request.IsPublished && !request.LocationConsent)
            return BadRequest("È necessario autorizzare la condivisione della posizione.");

  

        var library = await _libraryService.UpdateLibraryAsync(userId, request);

        if (library is null)
            return NotFound();

    

        return Ok(ToResponse(library));
    }

    private static bool ValidLocation(LibraryRequest request) =>
        request.Latitude is double lat &&
        request.Longitude is double lon &&
        double.IsFinite(lat) &&
        double.IsFinite(lon) &&
        lat is >= -90 and <= 90 &&
        lon is >= -180 and <= 180;

   private  static LibraryResponse ToResponse(Library library) => LibraryMapper.ToResponse(library);
    
}

