using BookNeighbors.Api.Data;
using BookNeighbors.Api.Interfaces;
using BookNeighbors.Api.Models;
using BookNeighbors.Api.Services;
using BookNeighbors.DTO.Libraries;
using Microsoft.EntityFrameworkCore;


namespace BookNeighbors.Api.Services;


public class LibraryService(ApplicationDbContext _dbContext, IGeoLocationService _geolocationService) : ILibraryService
{

    public async Task<Library?> GetLibraryByUserIdAsync(Guid userId)
    {
        return await _dbContext.Libraries
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.UserId == userId);
    }



    public async Task<Library> CreateLibraryAsync(Guid userId, LibraryRequest request)
    {

        var library = new Library
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            City = request.City?.Trim(),
            AreaName = request.AreaName?.Trim(),
            Location = _geolocationService.CreatePoint(request.Latitude!.Value, request.Longitude!.Value),
            LocationConsent = request.LocationConsent,
            IsPublished = request.IsPublished
        };

        _dbContext.Libraries.Add(library);

        await _dbContext.SaveChangesAsync();

        return library;
    }




    public async Task<Library?> UpdateLibraryAsync(Guid userId, LibraryRequest request)
    {



        var library = await _dbContext.Libraries
      .FirstOrDefaultAsync(l => l.UserId == userId);

        if (library is null)
            return null;
        library.Name = request.Name.Trim();
        library.City = request.City?.Trim();
        library.AreaName = request.AreaName?.Trim();
        library.Location = _geolocationService.CreatePoint(request.Latitude!.Value, request.Longitude!.Value);
        library.LocationConsent = request.LocationConsent;
        library.IsPublished = request.IsPublished;
        library.UpdatedAt = DateTime.UtcNow;


        await _dbContext.SaveChangesAsync();


        return library;
    }





}