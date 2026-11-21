using NetTopologySuite.Geometries;

namespace BookNeighbors.Api.Interfaces;

public interface IGeoLocationService
{
    /// <summary>
    /// Crea un oggetto Point a partire dalle coordinate geografiche specificate.
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    Point CreatePoint(double latitude, double longitude);
}
