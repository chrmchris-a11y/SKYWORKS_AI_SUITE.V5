using Skyworks.Core.Models.Airspace;

namespace Skyworks.Core.Utils.Geometry;

public static class GeometryUtils
{
    // Ray-casting algorithm for point-in-polygon (WGS-84 assumed planar small area)
    public static bool IsPointInPolygon(GeoPoint point, IReadOnlyList<GeoPoint> polygon)
    {
        int n = polygon.Count;
        if (n < 3) return false;

        bool inside = false;
        double x = point.Longitude;
        double y = point.Latitude;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            double xi = polygon[i].Longitude, yi = polygon[i].Latitude;
            double xj = polygon[j].Longitude, yj = polygon[j].Latitude;

            bool intersect = ((yi > y) != (yj > y)) &&
                             (x < (xj - xi) * (y - yi) / (yj - yi + 0.0) + xi);
            if (intersect) inside = !inside;
        }
        return inside;
    }

    // Haversine distance in meters
    public static double HaversineMeters(GeoPoint a, GeoPoint b)
    {
        const double R = 6371000; // Earth radius meters
        double dLat = DegreesToRadians(b.Latitude - a.Latitude);
        double dLon = DegreesToRadians(b.Longitude - a.Longitude);
        double lat1 = DegreesToRadians(a.Latitude);
        double lat2 = DegreesToRadians(b.Latitude);
        double sinDLat = Math.Sin(dLat / 2);
        double sinDLon = Math.Sin(dLon / 2);
        double h = sinDLat * sinDLat + Math.Cos(lat1) * Math.Cos(lat2) * sinDLon * sinDLon;
        double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h)));
        return R * c;
    }

    public static double DegreesToRadians(double deg) => deg * Math.PI / 180.0;

    public static bool AltitudeWithinBand(double? altitudeMeters, double? lowerMeters, double? upperMeters)
    {
        if (!altitudeMeters.HasValue) return true; // if point altitude unknown, consider match
        if (lowerMeters.HasValue && altitudeMeters.Value < lowerMeters.Value) return false;
        if (upperMeters.HasValue && altitudeMeters.Value > upperMeters.Value) return false;
        return true;
    }
}
