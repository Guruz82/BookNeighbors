using BookNeighbors.Api.Models;
using BookNeighbors.DTO.Libraries;

namespace BookNeighbors.Api.Interfaces;
public interface ILibraryService
{
     Task<Library> CreateLibraryAsync( Guid userId,LibraryRequest request);
      Task<Library?> UpdateLibraryAsync( Guid userId,LibraryRequest request);
     Task<Library?> GetLibraryByUserIdAsync(Guid userId);
   
}   