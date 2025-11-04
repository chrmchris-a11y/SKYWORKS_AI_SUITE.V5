// Phase 6 Step 51 — Skyworks V5
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skyworks.Core.Models.GIS;

namespace Skyworks.Core.Services.GIS;

/// <summary>
/// GIS service interface for managing operational volumes, population density zones,
/// airspace volumes, no-fly zones, and terrain elevation data.
/// Supports dual SORA compliance (EASA AMC 2.0 and JARUS 2.5).
/// </summary>
public interface IGISService
{
    // --- Operational Volume Management (OSO #17) ---

    /// <summary>Creates a new operational volume.</summary>
    Task<OperationalVolume> CreateOperationalVolumeAsync(OperationalVolume volume);

    /// <summary>Gets all operational volumes for a specific mission.</summary>
    Task<List<OperationalVolume>> GetOperationalVolumesByMissionAsync(Guid missionId);

    /// <summary>Validates an operational volume against safety requirements.</summary>
    Task<bool> ValidateOperationalVolumeAsync(OperationalVolume volume);

    /// <summary>Deletes an operational volume by ID.</summary>
    Task<bool> DeleteOperationalVolumeAsync(Guid volumeId);

    // --- Population Density Queries ---

    /// <summary>Gets population density zone for a specific point.</summary>
    Task<PopulationDensityZone?> GetPopulationDensityAsync(double latitude, double longitude);

    /// <summary>Gets all population density zones within a bounding box.</summary>
    Task<List<PopulationDensityZone>> GetPopulationDensityZonesAsync(
        double minLat, double maxLat, double minLon, double maxLon);

    /// <summary>
    /// Calculates Intrinsic GRC for a location using SORA version-specific logic.
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <param name="soraVersion">SORA version ("2.0" or "2.5").</param>
    /// <param name="uaDimension">UA maximum dimension in meters.</param>
    /// <param name="uaSpeed">UA maximum speed in m/s (SORA 2.5 only).</param>
    /// <returns>Intrinsic GRC value (1-10).</returns>
    Task<int> CalculateIntrinsicGRCAsync(
        double latitude, 
        double longitude, 
        string soraVersion, 
        double uaDimension,
        double? uaSpeed = null);

    // --- Airspace Queries ---

    /// <summary>Gets all airspace volumes containing a specific point.</summary>
    Task<List<AirspaceVolume>> GetAirspaceVolumesAsync(
        double latitude, 
        double longitude, 
        double altitudeAMSL);

    /// <summary>Determines airspace class for a specific point and altitude.</summary>
    Task<string> DetermineAirspaceClassAsync(
        double latitude, 
        double longitude, 
        double altitudeAMSL);

    /// <summary>
    /// Calculates Initial ARC using SORA decision tree (12 AEC categories).
    /// </summary>
    /// <param name="latitude">Latitude in decimal degrees.</param>
    /// <param name="longitude">Longitude in decimal degrees.</param>
    /// <param name="altitudeAGL">Altitude Above Ground Level in meters.</param>
    /// <param name="altitudeAMSL">Altitude Above Mean Sea Level in meters.</param>
    /// <param name="environment">Environment type ("Urban", "Suburban", "Rural").</param>
    /// <param name="soraVersion">SORA version ("2.0" or "2.5").</param>
    /// <returns>Initial ARC ("ARC-a", "ARC-b", "ARC-c", "ARC-d").</returns>
    Task<string> CalculateInitialARCAsync(
        double latitude, 
        double longitude, 
        double altitudeAGL, 
        double altitudeAMSL,
        string environment,
        string soraVersion);

    // --- No-Fly Zone Management ---

    /// <summary>Gets all no-fly zones within a bounding box for a specific date.</summary>
    Task<List<NoFlyZone>> GetNoFlyZonesAsync(
        double minLat, 
        double maxLat, 
        double minLon, 
        double maxLon,
        DateTime operationDate);

    /// <summary>Checks if a flight path violates any no-fly zones.</summary>
    Task<bool> CheckNoFlyZoneViolationAsync(
        string flightPathGeoJson, 
        DateTime operationDate,
        double altitudeAMSL);

    /// <summary>Creates a new no-fly zone.</summary>
    Task<NoFlyZone> CreateNoFlyZoneAsync(NoFlyZone zone);

    // --- Terrain Elevation ---

    /// <summary>Gets terrain elevation (AMSL) for a specific point.</summary>
    Task<double> GetTerrainElevationAsync(double latitude, double longitude);

    /// <summary>Gets terrain elevation profile for a list of waypoints.</summary>
    Task<List<TerrainElevation>> GetTerrainProfileAsync(
        List<(double latitude, double longitude)> waypoints);
}
