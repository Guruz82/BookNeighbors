using BookNeighbors.Api.Interfaces;
using NetTopologySuite.Geometries;
namespace BookNeighbors.Api.Services;

public class GeoLocationService : IGeoLocationService
{
    /// <summary>
    /// Crea un oggetto Point a partire da latitudine e longitudine, arrotondando i valori a 3 decimali e impostando l'SRID a 4326.
    /// </summary>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <returns></returns>
    public Point CreatePoint(double latitude, double longitude)
    {
        return new Point(
            Math.Round(longitude, 3),
            Math.Round(latitude, 3))
        {
            SRID = 4326
        };
    }
}

