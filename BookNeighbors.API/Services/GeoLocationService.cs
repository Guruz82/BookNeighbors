using BookNeighbors.Api.Interfaces;
using NetTopologySuite.Geometries;
namespace BookNeighbors.Api.Services;

public   class GeoLocationService :IGeoLocationService
{
    public   Point CreatePoint(double latitude, double longitude)
    {
        return new Point(
            Math.Round(longitude, 3),
            Math.Round(latitude, 3))
        {
            SRID = 4326
        };
    }
}

