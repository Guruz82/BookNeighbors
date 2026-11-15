using NetTopologySuite.Geometries;

namespace BookNeighbors.Api.Interfaces;

public interface IGeoLocationService
{
    Point CreatePoint(double latitude, double longitude);
}
