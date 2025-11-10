using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// Implementation of mission geometry generator.
/// Generates rule-based patterns for different mission types (Facade, Roof, Solar, Linear, Agriculture, Custom).
/// </summary>
public class MissionGeometryService : IMissionGeometryService
{
    private const double EarthRadiusMeters = 6371000.0; // Mean Earth radius in meters
    
    public Task<MissionGeometry> GenerateAsync(GeometryGenerationRequest request)
    {
        var geometry = request.MissionType switch
        {
            MissionType.Facade => GenerateFacadeGeometry(request),
            MissionType.Roof => GenerateRoofGeometry(request),
            MissionType.Solar => GenerateSolarGeometry(request),
            MissionType.Linear => GenerateLinearGeometry(request),
            MissionType.Agriculture => GenerateAgricultureGeometry(request),
            MissionType.Custom => GenerateCustomGeometry(request),
            _ => throw new InvalidOperationException($"Unsupported mission type: {request.MissionType}")
        };
        
        return Task.FromResult(geometry);
    }
    
    #region Facade Pattern
    
    private MissionGeometry GenerateFacadeGeometry(GeometryGenerationRequest request)
    {
        // Facade: 2-4 vertical scan lines in front of building
        var numLines = 3;
        var lineSpacing = 15.0; // meters between scan lines
        var buildingDistance = 50.0; // meters in front of building
        
        var waypoints = new List<object>();
        var routeCoords = new List<double[]>();
        
        // Create vertical scan lines
        for (int i = 0; i < numLines; i++)
        {
            var offsetY = (i - (numLines - 1) / 2.0) * lineSpacing;
            
            // Bottom point
            var bottomPoint = OffsetPosition(request.CenterLat, request.CenterLon, buildingDistance, offsetY);
            waypoints.Add(new { type = "Feature", geometry = new { type = "Point", coordinates = new[] { bottomPoint.lon, bottomPoint.lat, 0.0 } } });
            routeCoords.Add(new[] { bottomPoint.lon, bottomPoint.lat, 0.0 });
            
            // Top point
            var topPoint = OffsetPosition(request.CenterLat, request.CenterLon, buildingDistance, offsetY);
            waypoints.Add(new { type = "Feature", geometry = new { type = "Point", coordinates = new[] { topPoint.lon, topPoint.lat, request.MaxHeightAGL_m } } });
            routeCoords.Add(new[] { topPoint.lon, topPoint.lat, request.MaxHeightAGL_m });
        }
        
        // CGA: rectangular polygon in front of building
        var cgaWidth = numLines * lineSpacing + 20.0;
        var cgaDepth = 80.0;
        var cgaPolygon = CreateRectangularPolygon(request.CenterLat, request.CenterLon, cgaWidth, cgaDepth);
        
        // Build GeoJSON FeatureCollection
        var geoJson = new
        {
            type = "FeatureCollection",
            features = new object[]
            {
                new { type = "Feature", properties = new { name = "waypoints" }, geometry = new { type = "MultiPoint", coordinates = waypoints.Select(w => ((dynamic)w).geometry.coordinates).ToArray() } },
                new { type = "Feature", properties = new { name = "route" }, geometry = new { type = "LineString", coordinates = routeCoords.ToArray() } },
                new { type = "Feature", properties = new { name = "cga" }, geometry = new { type = "Polygon", coordinates = new[] { cgaPolygon } } }
            }
        };
        
        var routeLength = CalculateRouteLength(routeCoords);
        var cgaArea = CalculatePolygonArea(cgaPolygon);
        
        return new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            GeoJson = JsonSerializer.Serialize(geoJson),
            MaxHeightAGL_m = (decimal)request.MaxHeightAGL_m,
            RouteLength_m = (decimal)routeLength,
            CgaArea_m2 = (decimal)cgaArea
        };
    }
    
    #endregion
    
    #region Roof Pattern
    
    private MissionGeometry GenerateRoofGeometry(GeometryGenerationRequest request)
    {
        // Roof: lawn-mower pattern over rectangular footprint
        var width = request.AreaWidth_m ?? 40.0;
        var length = request.AreaLength_m ?? 40.0;
        var spacing = 12.0; // meters between parallel lines
        
        var waypoints = new List<double[]>();
        var numLines = (int)(length / spacing) + 1;
        
        for (int i = 0; i < numLines; i++)
        {
            var offsetY = -length / 2 + i * spacing;
            
            if (i % 2 == 0)
            {
                // Left to right
                var leftPoint = OffsetPosition(request.CenterLat, request.CenterLon, -width / 2, offsetY);
                var rightPoint = OffsetPosition(request.CenterLat, request.CenterLon, width / 2, offsetY);
                waypoints.Add(new[] { leftPoint.lon, leftPoint.lat, request.MaxHeightAGL_m });
                waypoints.Add(new[] { rightPoint.lon, rightPoint.lat, request.MaxHeightAGL_m });
            }
            else
            {
                // Right to left
                var rightPoint = OffsetPosition(request.CenterLat, request.CenterLon, width / 2, offsetY);
                var leftPoint = OffsetPosition(request.CenterLat, request.CenterLon, -width / 2, offsetY);
                waypoints.Add(new[] { rightPoint.lon, rightPoint.lat, request.MaxHeightAGL_m });
                waypoints.Add(new[] { leftPoint.lon, leftPoint.lat, request.MaxHeightAGL_m });
            }
        }
        
        // CGA: footprint + 10m buffer
        var cgaPolygon = CreateRectangularPolygon(request.CenterLat, request.CenterLon, width + 20, length + 20);
        
        var geoJson = new
        {
            type = "FeatureCollection",
            features = new object[]
            {
                new { type = "Feature", properties = new { name = "waypoints" }, geometry = new { type = "MultiPoint", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "route" }, geometry = new { type = "LineString", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "cga" }, geometry = new { type = "Polygon", coordinates = new[] { cgaPolygon } } }
            }
        };
        
        var routeLength = CalculateRouteLength(waypoints);
        var cgaArea = CalculatePolygonArea(cgaPolygon);
        
        return new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            GeoJson = JsonSerializer.Serialize(geoJson),
            MaxHeightAGL_m = (decimal)request.MaxHeightAGL_m,
            RouteLength_m = (decimal)routeLength,
            CgaArea_m2 = (decimal)cgaArea
        };
    }
    
    #endregion
    
    #region Solar Pattern
    
    private MissionGeometry GenerateSolarGeometry(GeometryGenerationRequest request)
    {
        // Solar: large-area lawn-mower pattern (PV park)
        var width = request.AreaWidth_m ?? 200.0; // Larger for PV parks
        var length = request.AreaLength_m ?? 300.0;
        var spacing = 15.0; // meters between panel rows
        
        var waypoints = new List<double[]>();
        var numLines = (int)(length / spacing) + 1;
        
        for (int i = 0; i < numLines; i++)
        {
            var offsetY = -length / 2 + i * spacing;
            
            if (i % 2 == 0)
            {
                var leftPoint = OffsetPosition(request.CenterLat, request.CenterLon, -width / 2, offsetY);
                var rightPoint = OffsetPosition(request.CenterLat, request.CenterLon, width / 2, offsetY);
                waypoints.Add(new[] { leftPoint.lon, leftPoint.lat, request.MaxHeightAGL_m });
                waypoints.Add(new[] { rightPoint.lon, rightPoint.lat, request.MaxHeightAGL_m });
            }
            else
            {
                var rightPoint = OffsetPosition(request.CenterLat, request.CenterLon, width / 2, offsetY);
                var leftPoint = OffsetPosition(request.CenterLat, request.CenterLon, -width / 2, offsetY);
                waypoints.Add(new[] { rightPoint.lon, rightPoint.lat, request.MaxHeightAGL_m });
                waypoints.Add(new[] { leftPoint.lon, leftPoint.lat, request.MaxHeightAGL_m });
            }
        }
        
        // CGA: entire park + buffer
        var cgaPolygon = CreateRectangularPolygon(request.CenterLat, request.CenterLon, width + 50, length + 50);
        
        var geoJson = new
        {
            type = "FeatureCollection",
            features = new object[]
            {
                new { type = "Feature", properties = new { name = "waypoints" }, geometry = new { type = "MultiPoint", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "route" }, geometry = new { type = "LineString", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "cga" }, geometry = new { type = "Polygon", coordinates = new[] { cgaPolygon } } }
            }
        };
        
        var routeLength = CalculateRouteLength(waypoints);
        var cgaArea = CalculatePolygonArea(cgaPolygon);
        
        return new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            GeoJson = JsonSerializer.Serialize(geoJson),
            MaxHeightAGL_m = (decimal)request.MaxHeightAGL_m,
            RouteLength_m = (decimal)routeLength,
            CgaArea_m2 = (decimal)cgaArea
        };
    }
    
    #endregion
    
    #region Linear Pattern
    
    private MissionGeometry GenerateLinearGeometry(GeometryGenerationRequest request)
    {
        // Linear: corridor (powerline, pipeline, road, railway)
        var corridorLength = request.AreaLength_m ?? 1000.0; // Default 1km corridor
        var waypointSpacing = 50.0; // meters between waypoints
        var corridorWidth = 100.0; // meters (50m each side)
        
        var waypoints = new List<double[]>();
        var numWaypoints = (int)(corridorLength / waypointSpacing) + 1;
        
        // Create linear route (simplified: straight line along north-south axis)
        for (int i = 0; i < numWaypoints; i++)
        {
            var offsetY = -corridorLength / 2 + i * waypointSpacing;
            var point = OffsetPosition(request.CenterLat, request.CenterLon, 0, offsetY);
            waypoints.Add(new[] { point.lon, point.lat, request.MaxHeightAGL_m });
        }
        
        // CGA: narrow corridor polygon (buffer around route)
        var cgaPolygon = CreateCorridorPolygon(waypoints, corridorWidth / 2);
        
        var geoJson = new
        {
            type = "FeatureCollection",
            features = new object[]
            {
                new { type = "Feature", properties = new { name = "waypoints" }, geometry = new { type = "MultiPoint", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "route" }, geometry = new { type = "LineString", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "cga" }, geometry = new { type = "Polygon", coordinates = new[] { cgaPolygon } } }
            }
        };
        
        var routeLength = CalculateRouteLength(waypoints);
        var cgaArea = CalculatePolygonArea(cgaPolygon);
        
        return new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            GeoJson = JsonSerializer.Serialize(geoJson),
            MaxHeightAGL_m = (decimal)request.MaxHeightAGL_m,
            RouteLength_m = (decimal)routeLength,
            CgaArea_m2 = (decimal)cgaArea
        };
    }
    
    #endregion
    
    #region Agriculture Pattern
    
    private MissionGeometry GenerateAgricultureGeometry(GeometryGenerationRequest request)
    {
        // Agriculture: large field with lawn-mower pattern
        var width = request.AreaWidth_m ?? 150.0;
        var length = request.AreaLength_m ?? 200.0;
        var spacing = 25.0; // meters between lines (wider for agriculture)
        
        var waypoints = new List<double[]>();
        var numLines = (int)(length / spacing) + 1;
        
        for (int i = 0; i < numLines; i++)
        {
            var offsetY = -length / 2 + i * spacing;
            
            if (i % 2 == 0)
            {
                var leftPoint = OffsetPosition(request.CenterLat, request.CenterLon, -width / 2, offsetY);
                var rightPoint = OffsetPosition(request.CenterLat, request.CenterLon, width / 2, offsetY);
                waypoints.Add(new[] { leftPoint.lon, leftPoint.lat, request.MaxHeightAGL_m });
                waypoints.Add(new[] { rightPoint.lon, rightPoint.lat, request.MaxHeightAGL_m });
            }
            else
            {
                var rightPoint = OffsetPosition(request.CenterLat, request.CenterLon, width / 2, offsetY);
                var leftPoint = OffsetPosition(request.CenterLat, request.CenterLon, -width / 2, offsetY);
                waypoints.Add(new[] { rightPoint.lon, rightPoint.lat, request.MaxHeightAGL_m });
                waypoints.Add(new[] { leftPoint.lon, leftPoint.lat, request.MaxHeightAGL_m });
            }
        }
        
        // CGA: field footprint (no extra buffer for rural agriculture)
        var cgaPolygon = CreateRectangularPolygon(request.CenterLat, request.CenterLon, width, length);
        
        var geoJson = new
        {
            type = "FeatureCollection",
            features = new object[]
            {
                new { type = "Feature", properties = new { name = "waypoints" }, geometry = new { type = "MultiPoint", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "route" }, geometry = new { type = "LineString", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "cga" }, geometry = new { type = "Polygon", coordinates = new[] { cgaPolygon } } }
            }
        };
        
        var routeLength = CalculateRouteLength(waypoints);
        var cgaArea = CalculatePolygonArea(cgaPolygon);
        
        return new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            GeoJson = JsonSerializer.Serialize(geoJson),
            MaxHeightAGL_m = (decimal)request.MaxHeightAGL_m,
            RouteLength_m = (decimal)routeLength,
            CgaArea_m2 = (decimal)cgaArea
        };
    }
    
    #endregion
    
    #region Custom Pattern
    
    private MissionGeometry GenerateCustomGeometry(GeometryGenerationRequest request)
    {
        if (!request.UseImportedGeometry || string.IsNullOrWhiteSpace(request.ImportedGeoJson))
        {
            throw new InvalidOperationException("Custom mission requires imported geometry (ImportedGeoJson must be provided when UseImportedGeometry=true)");
        }
        
        // Parse imported GeoJSON
        JsonDocument importedDoc;
        try
        {
            importedDoc = JsonDocument.Parse(request.ImportedGeoJson);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid GeoJSON format: {ex.Message}", ex);
        }
        
        // Extract route from imported geometry (simplified: assume LineString or MultiPoint)
        var waypoints = new List<double[]>();
        
        if (importedDoc.RootElement.TryGetProperty("type", out var typeElement))
        {
            var geometryType = typeElement.GetString();
            
            if (geometryType == "LineString" && importedDoc.RootElement.TryGetProperty("coordinates", out var coords))
            {
                foreach (var coord in coords.EnumerateArray())
                {
                    var coordArray = coord.EnumerateArray().Select(c => c.GetDouble()).ToArray();
                    waypoints.Add(coordArray);
                }
            }
            else if (geometryType == "FeatureCollection" && importedDoc.RootElement.TryGetProperty("features", out var features))
            {
                // Extract first LineString feature
                foreach (var feature in features.EnumerateArray())
                {
                    if (feature.TryGetProperty("geometry", out var geometry) &&
                        geometry.TryGetProperty("type", out var geoType) &&
                        geoType.GetString() == "LineString" &&
                        geometry.TryGetProperty("coordinates", out var geoCoords))
                    {
                        foreach (var coord in geoCoords.EnumerateArray())
                        {
                            var coordArray = coord.EnumerateArray().Select(c => c.GetDouble()).ToArray();
                            waypoints.Add(coordArray);
                        }
                        break;
                    }
                }
            }
        }
        
        if (waypoints.Count == 0)
        {
            throw new InvalidOperationException("No valid route found in imported GeoJSON (expected LineString or FeatureCollection with LineString)");
        }
        
        // Generate simple CGA around imported route
        var cgaPolygon = CreateCorridorPolygon(waypoints, 50.0); // 50m buffer
        
        var geoJson = new
        {
            type = "FeatureCollection",
            features = new object[]
            {
                new { type = "Feature", properties = new { name = "waypoints" }, geometry = new { type = "MultiPoint", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "route" }, geometry = new { type = "LineString", coordinates = waypoints.ToArray() } },
                new { type = "Feature", properties = new { name = "cga" }, geometry = new { type = "Polygon", coordinates = new[] { cgaPolygon } } }
            }
        };
        
        var routeLength = CalculateRouteLength(waypoints);
        var cgaArea = CalculatePolygonArea(cgaPolygon);
        
        return new MissionGeometry
        {
            MissionGeometryId = Guid.NewGuid(),
            GeoJson = JsonSerializer.Serialize(geoJson),
            MaxHeightAGL_m = (decimal)request.MaxHeightAGL_m,
            RouteLength_m = (decimal)routeLength,
            CgaArea_m2 = (decimal)cgaArea
        };
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Offset a position by meters in X (east-west) and Y (north-south) directions
    /// </summary>
    private (double lat, double lon) OffsetPosition(double lat, double lon, double offsetX_m, double offsetY_m)
    {
        var latRadians = lat * Math.PI / 180.0;
        
        // Calculate new position
        var newLat = lat + (offsetY_m / EarthRadiusMeters) * (180.0 / Math.PI);
        var newLon = lon + (offsetX_m / (EarthRadiusMeters * Math.Cos(latRadians))) * (180.0 / Math.PI);
        
        return (newLat, newLon);
    }
    
    /// <summary>
    /// Create rectangular polygon around center point
    /// </summary>
    private double[][] CreateRectangularPolygon(double centerLat, double centerLon, double width_m, double length_m)
    {
        var halfWidth = width_m / 2;
        var halfLength = length_m / 2;
        
        var topLeft = OffsetPosition(centerLat, centerLon, -halfWidth, halfLength);
        var topRight = OffsetPosition(centerLat, centerLon, halfWidth, halfLength);
        var bottomRight = OffsetPosition(centerLat, centerLon, halfWidth, -halfLength);
        var bottomLeft = OffsetPosition(centerLat, centerLon, -halfWidth, -halfLength);
        
        return new[]
        {
            new[] { topLeft.lon, topLeft.lat },
            new[] { topRight.lon, topRight.lat },
            new[] { bottomRight.lon, bottomRight.lat },
            new[] { bottomLeft.lon, bottomLeft.lat },
            new[] { topLeft.lon, topLeft.lat } // Close polygon
        };
    }
    
    /// <summary>
    /// Create corridor polygon (buffer around route)
    /// </summary>
    private double[][] CreateCorridorPolygon(List<double[]> waypoints, double bufferWidth_m)
    {
        if (waypoints.Count < 2)
        {
            // Fallback: create small square
            var fallbackLat = waypoints[0][1];
            var fallbackLon = waypoints[0][0];
            return CreateRectangularPolygon(fallbackLat, fallbackLon, bufferWidth_m * 2, bufferWidth_m * 2);
        }
        
        // Simplified corridor: create rectangle along first and last waypoint
        var firstLat = waypoints.First()[1];
        var firstLon = waypoints.First()[0];
        var lastLat = waypoints.Last()[1];
        var lastLon = waypoints.Last()[0];
        
        var centerLat = (firstLat + lastLat) / 2;
        var centerLon = (firstLon + lastLon) / 2;
        
        var distance = HaversineDistance(firstLat, firstLon, lastLat, lastLon);
        
        return CreateRectangularPolygon(centerLat, centerLon, bufferWidth_m * 2, distance + bufferWidth_m * 2);
    }
    
    /// <summary>
    /// Calculate total route length using Haversine formula
    /// </summary>
    private double CalculateRouteLength(List<double[]> waypoints)
    {
        if (waypoints.Count < 2) return 0;
        
        double totalLength = 0;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            var lat1 = waypoints[i][1];
            var lon1 = waypoints[i][0];
            var lat2 = waypoints[i + 1][1];
            var lon2 = waypoints[i + 1][0];
            
            totalLength += HaversineDistance(lat1, lon1, lat2, lon2);
        }
        
        return totalLength;
    }
    
    /// <summary>
    /// Haversine distance between two points
    /// </summary>
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusMeters * c;
    }
    
    /// <summary>
    /// Calculate polygon area (simplified approximation)
    /// </summary>
    private double CalculatePolygonArea(double[][] polygon)
    {
        if (polygon.Length < 3) return 0;
        
        // Shoelace formula (approximation for small areas)
        double area = 0;
        int n = polygon.Length - 1; // Exclude closing point
        
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += polygon[i][0] * polygon[j][1];
            area -= polygon[j][0] * polygon[i][1];
        }
        
        area = Math.Abs(area) / 2.0;
        
        // Convert from degrees² to m² (rough approximation at mid-latitudes)
        var avgLat = polygon.Take(n).Average(p => p[1]);
        var latRadians = avgLat * Math.PI / 180.0;
        var metersPerDegreeLat = EarthRadiusMeters * Math.PI / 180.0;
        var metersPerDegreeLon = metersPerDegreeLat * Math.Cos(latRadians);
        
        return area * metersPerDegreeLat * metersPerDegreeLon;
    }
    
    #endregion
}
