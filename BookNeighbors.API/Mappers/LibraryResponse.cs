using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Libraries;

namespace BookNeighbors.API.Mappers;

public static class LibraryMapper
{
    /// <summary>
    /// Mappa un oggetto Library in un oggetto LibraryResponse.
    /// </summary>
    /// <param name="library"></param>
    /// <returns></returns>
    public static LibraryResponse ToResponse(Library library)
    {
        return new LibraryResponse(
            library.Id,
            library.Name,
            library.City,
            library.AreaName,
            library.Location.Y,
            library.Location.X,
            library.LocationConsent,
            library.IsPublished
        );
    }
}