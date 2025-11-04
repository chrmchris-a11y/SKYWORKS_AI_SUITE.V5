// Phase 6 Step 51 — Skyworks V5
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Skyworks.Core.Models.GIS;

namespace Skyworks.Core.Services.GIS;

/// <summary>
/// GIS service implementation with demo data.
/// TODO: Replace demo data with real GIS providers (Eurostat, SRTM, EAD, NOTAM feeds).
/// </summary>
public class GISService : IGISService
{
    private readonly ILogger<GISService> _logger;
    private readonly List<OperationalVolume> _operationalVolumes = new();
    private readonly List<NoFlyZone> _noFlyZones = new();

    public GISService(ILogger<GISService> logger)
    {
        _logger = logger;
        InitializeDemoData();
    }

    // --- Operational Volume Management ---

    public Task<OperationalVolume> CreateOperationalVolumeAsync(OperationalVolume volume)
    {
        volume.Id = Guid.NewGuid();
        volume.CreatedAt = DateTime.UtcNow;
        volume.UpdatedAt = DateTime.UtcNow;
        _operationalVolumes.Add(volume);
        
        _logger.LogInformation(
            "Created operational volume {Id} for mission {MissionId} (SORA {Version})",
            volume.Id, volume.MissionId, volume.SoraVersion);
        
        return Task.FromResult(volume);
    }

    public Task<List<OperationalVolume>> GetOperationalVolumesByMissionAsync(Guid missionId)
    {
        var volumes = _operationalVolumes
            .Where(v => v.MissionId == missionId)
            .ToList();
        
        return Task.FromResult(volumes);
    }

    public Task<bool> ValidateOperationalVolumeAsync(OperationalVolume volume)
    {
        // Basic validation
        if (string.IsNullOrEmpty(volume.GeometryGeoJson))
        {
            _logger.LogWarning("Operational volume {Id} has no geometry", volume.Id);
            return Task.FromResult(false);
        }

        if (volume.MaxAltitudeAGL <= volume.MinAltitudeAGL)
        {
            _logger.LogWarning(
                "Operational volume {Id} has invalid altitude range: {Min}-{Max} AGL",
                volume.Id, volume.MinAltitudeAGL, volume.MaxAltitudeAGL);
            return Task.FromResult(false);
        }

        // SORA 2.0: Requires detailed 3D envelope
        // SORA 2.5: Simplified 2D boundaries acceptable
        if (volume.SoraVersion == "2.0" && volume.SafetyMarginHorizontal < 5.0)
        {
            _logger.LogWarning(
                "SORA 2.0 operational volume {Id} has insufficient safety margin: {Margin}m",
                volume.Id, volume.SafetyMarginHorizontal);
            return Task.FromResult(false);
        }

        _logger.LogInformation("Operational volume {Id} validated successfully", volume.Id);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteOperationalVolumeAsync(Guid volumeId)
    {
        var removed = _operationalVolumes.RemoveAll(v => v.Id == volumeId) > 0;
        if (removed)
        {
            _logger.LogInformation("Deleted operational volume {Id}", volumeId);
        }
        return Task.FromResult(removed);
    }

    // --- Population Density ---

    public Task<PopulationDensityZone?> GetPopulationDensityAsync(double latitude, double longitude)
    {
        // Demo data: Cyprus zones
        var zone = GetDemoPopulationZone(latitude, longitude);
        
        if (zone != null)
        {
            _logger.LogInformation(
                "Population density at ({Lat}, {Lon}): {Density} people/km² (SORA 2.5: {Cat})",
                latitude, longitude, zone.PopulationDensity, zone.Category_2_5);
        }
        
        return Task.FromResult(zone);
    }

    public Task<List<PopulationDensityZone>> GetPopulationDensityZonesAsync(
        double minLat, double maxLat, double minLon, double maxLon)
    {
        // TODO: Implement real spatial query with GIS provider
        var zones = GetAllDemoPopulationZones()
            .Where(z => IsWithinBounds(z, minLat, maxLat, minLon, maxLon))
            .ToList();
        
        return Task.FromResult(zones);
    }

    public Task<int> CalculateIntrinsicGRCAsync(
        double latitude, 
        double longitude, 
        string soraVersion, 
        double uaDimension,
        double? uaSpeed = null)
    {
        var zone = GetDemoPopulationZone(latitude, longitude);
        if (zone == null)
        {
            _logger.LogWarning("No population data for ({Lat}, {Lon})", latitude, longitude);
            return Task.FromResult(1); // Default to GRC 1 (controlled area)
        }

        int iGRC;
        if (soraVersion == "2.0")
        {
            iGRC = zone.IntrinsicGRC_2_0 ?? CalculateGRC_2_0(uaDimension, zone.Scenario_2_0 ?? "VLOS_Controlled");
        }
        else // SORA 2.5
        {
            iGRC = zone.IntrinsicGRC_2_5 ?? CalculateGRC_2_5(zone.PopulationDensity, uaDimension, uaSpeed ?? 0);
        }

        _logger.LogInformation(
            "Calculated iGRC={GRC} for ({Lat},{Lon}) using SORA {Version} (Dim={Dim}m, Speed={Speed}m/s)",
            iGRC, latitude, longitude, soraVersion, uaDimension, uaSpeed);

        return Task.FromResult(iGRC);
    }

    // --- Airspace ---

    public Task<List<AirspaceVolume>> GetAirspaceVolumesAsync(
        double latitude, 
        double longitude, 
        double altitudeAMSL)
    {
        // Demo data: Cyprus airspace
        var volumes = GetDemoAirspaceVolumes()
            .Where(v => altitudeAMSL >= v.FloorAltitude && altitudeAMSL <= v.CeilingAltitude)
            .ToList();
        
        return Task.FromResult(volumes);
    }

    public Task<string> DetermineAirspaceClassAsync(
        double latitude, 
        double longitude, 
        double altitudeAMSL)
    {
        var volumes = GetDemoAirspaceVolumes()
            .Where(v => altitudeAMSL >= v.FloorAltitude && altitudeAMSL <= v.CeilingAltitude)
            .OrderBy(v => v.FloorAltitude) // Lowest floor first
            .ToList();

        var airspaceClass = volumes.FirstOrDefault()?.AirspaceClass ?? "G";
        
        _logger.LogInformation(
            "Airspace class at ({Lat},{Lon},{Alt}m): {Class}",
            latitude, longitude, altitudeAMSL, airspaceClass);
        
        return Task.FromResult(airspaceClass);
    }

    public Task<string> CalculateInitialARCAsync(
        double latitude, 
        double longitude, 
        double altitudeAGL, 
        double altitudeAMSL,
        string environment,
        string soraVersion)
    {
        // Simplified ARC determination (full decision tree in Phase 5 ARCCalculationService)
        var airspaceClass = DetermineAirspaceClassAsync(latitude, longitude, altitudeAMSL).Result;
        
        string arc;
        if (airspaceClass == "A" || airspaceClass == "B" || airspaceClass == "C")
        {
            arc = "ARC-d"; // Controlled airspace
        }
        else if (altitudeAGL > 152) // Above 500ft AGL
        {
            arc = environment == "Rural" ? "ARC-c" : "ARC-c";
        }
        else // Below 500ft AGL
        {
            arc = environment == "Rural" ? "ARC-b" : "ARC-c";
        }

        _logger.LogInformation(
            "Initial ARC={ARC} at ({Lat},{Lon}) AGL={AGL}m, Airspace={Class}, Env={Env}",
            arc, latitude, longitude, altitudeAGL, airspaceClass, environment);

        return Task.FromResult(arc);
    }

    // --- No-Fly Zones ---

    public Task<List<NoFlyZone>> GetNoFlyZonesAsync(
        double minLat, 
        double maxLat, 
        double minLon, 
        double maxLon,
        DateTime operationDate)
    {
        var zones = _noFlyZones
            .Where(z => IsActive(z, operationDate))
            .ToList();
        
        return Task.FromResult(zones);
    }

    public Task<bool> CheckNoFlyZoneViolationAsync(
        string flightPathGeoJson, 
        DateTime operationDate,
        double altitudeAMSL)
    {
        // TODO: Implement real spatial intersection check
        var activeZones = _noFlyZones.Where(z => IsActive(z, operationDate)).ToList();
        
        if (activeZones.Any())
        {
            _logger.LogWarning("Flight path intersects {Count} no-fly zones", activeZones.Count);
            return Task.FromResult(true); // Violation detected
        }
        
        return Task.FromResult(false);
    }

    public Task<NoFlyZone> CreateNoFlyZoneAsync(NoFlyZone zone)
    {
        zone.Id = Guid.NewGuid();
        zone.CreatedAt = DateTime.UtcNow;
        zone.UpdatedAt = DateTime.UtcNow;
        _noFlyZones.Add(zone);
        
        _logger.LogInformation(
            "Created no-fly zone {Id}: {Name} ({Type}, {Severity})",
            zone.Id, zone.Name, zone.RestrictionType, zone.Severity);
        
        return Task.FromResult(zone);
    }

    // --- Terrain Elevation ---

    public Task<double> GetTerrainElevationAsync(double latitude, double longitude)
    {
        // Demo data: Simplified Cyprus elevation (actual: 0m coast to 1952m Troodos)
        var elevation = GetDemoTerrainElevation(latitude, longitude);
        
        _logger.LogInformation(
            "Terrain elevation at ({Lat},{Lon}): {Elev}m AMSL",
            latitude, longitude, elevation);
        
        return Task.FromResult(elevation);
    }

    public Task<List<TerrainElevation>> GetTerrainProfileAsync(
        List<(double latitude, double longitude)> waypoints)
    {
        var profile = waypoints.Select(wp => new TerrainElevation
        {
            Latitude = wp.latitude,
            Longitude = wp.longitude,
            ElevationAMSL = GetDemoTerrainElevation(wp.latitude, wp.longitude),
            Source = "Demo",
            Accuracy = 10.0,
            ResolutionMeters = 30.0,
            DataCollectedAt = DateTime.UtcNow,
            IsInterpolated = true
        }).ToList();
        
        return Task.FromResult(profile);
    }

    // --- Private Helper Methods ---

    private void InitializeDemoData()
    {
        // Initialize demo no-fly zones (Cyprus airports)
        _noFlyZones.Add(new NoFlyZone
        {
            Id = Guid.NewGuid(),
            Name = "Larnaca International Airport (LCLK)",
            GeometryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[33.6,34.85],[33.65,34.85],[33.65,34.9],[33.6,34.9],[33.6,34.85]]]}",
            RestrictionType = "permanent",
            Category = "airport",
            Severity = "prohibited",
            AltitudeRestrictionMin = 0,
            AltitudeRestrictionMax = 1500,
            EnforceGeofence = true,
            Authority = "DCA Cyprus",
            DataSource = "Demo"
        });
    }

    private PopulationDensityZone? GetDemoPopulationZone(double lat, double lon)
    {
        // Demo Cyprus zones (simplified)
        if (lat >= 34.6 && lat <= 35.2 && lon >= 32.8 && lon <= 34.6)
        {
            // Nicosia area (urban)
            return new PopulationDensityZone
            {
                Name = "Nicosia Urban",
                PopulationDensity = 8000,
                Category_2_5 = "<50000",
                IntrinsicGRC_2_5 = 6,
                Scenario_2_0 = "VLOS_Populated",
                IntrinsicGRC_2_0 = 5,
                ShelteringFactor = 0.4,
                DataSource = "Demo"
            };
        }
        else
        {
            // Rural Cyprus
            return new PopulationDensityZone
            {
                Name = "Rural Cyprus",
                PopulationDensity = 200,
                Category_2_5 = "<500",
                IntrinsicGRC_2_5 = 4,
                Scenario_2_0 = "VLOS_Sparsely",
                IntrinsicGRC_2_0 = 2,
                ShelteringFactor = 0.1,
                DataSource = "Demo"
            };
        }
    }

    private List<PopulationDensityZone> GetAllDemoPopulationZones()
    {
        return new List<PopulationDensityZone>
        {
            GetDemoPopulationZone(35.0, 33.3)!, // Nicosia
            GetDemoPopulationZone(34.8, 33.0)!  // Rural
        };
    }

    private List<AirspaceVolume> GetDemoAirspaceVolumes()
    {
        return new List<AirspaceVolume>
        {
            new AirspaceVolume
            {
                Name = "CTR Larnaca",
                AirspaceClass = "D",
                FloorAltitude = 0,
                CeilingAltitude = 1500,
                AltitudeReference = "AMSL",
                ATCServiceAvailable = true,
                Environment = "Urban",
                InitialARC = "ARC-d",
                AECCategory = 1,
                DataSource = "Demo"
            },
            new AirspaceVolume
            {
                Name = "Cyprus FIR",
                AirspaceClass = "G",
                FloorAltitude = 0,
                CeilingAltitude = 6000,
                AltitudeReference = "AMSL",
                ATCServiceAvailable = false,
                Environment = "Rural",
                InitialARC = "ARC-b",
                AECCategory = 10,
                DataSource = "Demo"
            }
        };
    }

    private double GetDemoTerrainElevation(double lat, double lon)
    {
        // Simplified: Higher elevation in Troodos mountains area
        if (lat >= 34.8 && lat <= 35.1 && lon >= 32.8 && lon <= 33.1)
        {
            return 800.0; // Troodos foothills
        }
        return 50.0; // Coastal/lowland
    }

    private int CalculateGRC_2_0(double dimension, string scenario)
    {
        // Simplified SORA 2.0 Table 2 lookup
        return scenario switch
        {
            "VLOS_Controlled" => dimension < 1 ? 1 : 2,
            "VLOS_Sparsely" => dimension < 1 ? 2 : 3,
            "VLOS_Populated" => dimension < 1 ? 4 : 5,
            _ => 1
        };
    }

    private int CalculateGRC_2_5(double popDensity, double dimension, double speed)
    {
        // Simplified SORA 2.5 Table 2 lookup
        if (popDensity < 5) return dimension < 1 ? 2 : 3;
        if (popDensity < 50) return dimension < 1 ? 3 : 4;
        if (popDensity < 500) return dimension < 1 ? 4 : 5;
        if (popDensity < 5000) return dimension < 1 ? 5 : 6;
        if (popDensity < 50000) return dimension < 1 ? 6 : 7;
        return dimension < 1 ? 7 : 8;
    }

    private bool IsWithinBounds(PopulationDensityZone zone, double minLat, double maxLat, double minLon, double maxLon)
    {
        // Simplified bounds check (real implementation needs GeoJSON parsing)
        return true;
    }

    private bool IsActive(NoFlyZone zone, DateTime operationDate)
    {
        if (zone.RestrictionType == "permanent") return true;
        
        if (zone.ValidFrom.HasValue && operationDate < zone.ValidFrom.Value)
            return false;
        
        if (zone.ValidTo.HasValue && operationDate > zone.ValidTo.Value)
            return false;
        
        return true;
    }
}
