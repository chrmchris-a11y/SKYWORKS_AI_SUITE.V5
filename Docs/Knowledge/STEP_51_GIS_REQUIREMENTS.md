# STEP 51: GIS MAPPING SYSTEM - REQUIREMENTS SPECIFICATION

**Phase:** 6 - Mission Planning & Maps  
**Step:** 51 - GIS Mapping System  
**Date:** 2025-01-27  
**Status:** Requirements Phase  
**SORA Compliance:** Dual (EASA AMC 2.0 + JARUS 2.5)

---

## 📋 EXECUTIVE SUMMARY

This document defines **authoritative SORA-based requirements** for the Step 51 GIS Mapping System, extracted from official EASA/JARUS documentation and existing Skyworks V5 compliance implementation.

**Critical Requirement:** System MUST support **dual SORA compliance** (2.0 + 2.5) for all geographic features.

---

## 🎯 CORE GIS REQUIREMENTS

### 1. OPERATIONAL VOLUME MANAGEMENT (OSO #17)

**SORA 2.0 Reference:** EASA AMC UAS.SORA-10, OSO #17  
**SORA 2.5 Reference:** JARUS SORA 2.5 Main Body, Section 4.3  

#### Requirements:
- **3D Operational Volume Definition**
  - Horizontal boundaries (lat/lon polygon)
  - Vertical limits (min/max altitude AGL + AMSL)
  - Safety margins (GPS/GNSS accuracy buffers)
  - Contingency volumes (emergency containment zones)

- **Geographic Constraints**
  - Geofence boundaries
  - No-fly zones (regulatory + operational)
  - Buffer zones around critical infrastructure
  - Emergency landing zones (ELS)

- **Data Format:** GeoJSON FeatureCollection
  ```json
  {
    "type": "FeatureCollection",
    "features": [
      {
        "type": "Feature",
        "geometry": {
          "type": "Polygon",
          "coordinates": [[[lon, lat, alt], ...]]
        },
        "properties": {
          "volumeType": "operational|contingency|buffer",
          "minAltitudeAGL": 0,
          "maxAltitudeAGL": 120,
          "minAltitudeAMSL": 50,
          "maxAltitudeAMSL": 170,
          "safetyMarginHorizontal": 10,
          "soraVersion": "2.0|2.5"
        }
      }
    ]
  }
  ```

---

### 2. GROUND RISK MAPPING (GRC - Annex A/B)

**SORA 2.0 Reference:** AMC1 Article 11 Annex A Table 2, Table 3  
**SORA 2.5 Reference:** JARUS SORA 2.5 Volume II Annex B Table 2, Table 5  

#### Population Density Categories (CRITICAL)

**SORA 2.0 Categories** (Scenario-based):
1. VLOS/BVLOS over controlled ground area
2. VLOS in sparsely populated environment
3. BVLOS in sparsely populated environment
4. VLOS in populated environment
5. BVLOS in populated environment
6. VLOS over gathering of people
7. BVLOS over gathering of people

**SORA 2.5 Categories** (Density-based):
1. **Controlled Ground Area:** Operator-controlled (0 people/km²)
2. **<5 people/km²:** Extremely remote / Hard to reach
3. **<50 people/km²:** Sparsely populated
4. **<500 people/km²:** Rural
5. **<5,000 people/km²:** Suburban / Light industrial
6. **<50,000 people/km²:** High density metro / Dense industrial
7. **>50,000 people/km²:** Assemblies / Major gatherings

#### GIS Requirements:
- **Population Density Layer**
  - Resolution: 10m minimum (for ground risk modeling)
  - Data sources: Eurostat, WorldPop, national census
  - Update frequency: Quarterly (static) + real-time (events/gatherings)
  - Query API: `GET /api/gis/population-density?lat={lat}&lon={lon}`

- **Ground Risk Heatmap**
  - Color-coded zones based on GRC 1-7
  - Dynamic updates based on UA parameters (dimension, speed)
  - Shelter factor visualization (buildings, structures)
  - M1 mitigation zones (strategic ground risk reductions)

- **Data Format:** GeoTIFF raster + GeoJSON vector overlay
  ```json
  {
    "type": "Feature",
    "geometry": {"type": "Polygon", "coordinates": [...]},
    "properties": {
      "populationDensity": 2500,
      "soraCategory_2_0": "VLOS_Populated",
      "soraCategory_2_5": "<5000",
      "intrinsicGRC_2_0": 4,
      "intrinsicGRC_2_5": 5,
      "shelteringFactor": 0.3,
      "m1Applicability": true
    }
  }
  ```

---

### 3. AIRSPACE STRUCTURE MAPPING (ARC - Annex C)

**SORA 2.0 Reference:** AMC1 Article 11 Annex C Figure 4 (12 AEC categories)  
**SORA 2.5 Reference:** JARUS SORA 2.5 Volume II Annex C Figure 6  

#### Airspace Classification Requirements:

**Airspace Classes:**
- Class A: IFR only, ATC clearance required
- Class B: IFR/VFR, ATC clearance required
- Class C: IFR/VFR, ATC clearance required (VFR traffic info)
- Class D: IFR/VFR, ATC clearance required (limited VFR traffic info)
- Class E: IFR ATC, VFR flight info on request
- Class F: Flight information service (if available)
- Class G: Uncontrolled airspace

**Special Airspace Zones:**
- Mode S Transponder Mandatory Zones (TMZ)
- Radio Mandatory Zones (RMZ)
- Temporary Segregated Areas (TSA)
- Temporary Reserved Areas (TRA)
- Danger/Restricted/Prohibited Areas (D/R/P)

#### GIS Requirements:
- **Airspace Layer**
  - 3D volumes with altitude floors/ceilings
  - Real-time NOTAM integration (temporary restrictions)
  - ATC service availability indicators
  - Controlled vs uncontrolled boundaries

- **Environment Classification**
  - Urban (high-density, complex infrastructure)
  - Suburban (medium-density, mixed use)
  - Rural (low-density, agricultural/natural)
  - Airport/Heliport proximity zones

- **Data Format:** GML (Geography Markup Language) + GeoJSON
  ```json
  {
    "type": "Feature",
    "geometry": {"type": "Polygon", "coordinates": [...]},
    "properties": {
      "airspaceClass": "C",
      "floorAltitude": 0,
      "ceilingAltitude": 10000,
      "altitudeReference": "AMSL",
      "atcServiceAvailable": true,
      "modeS_Required": false,
      "environment": "Urban",
      "initialARC_2_0": "ARC-c",
      "initialARC_2_5": "ARC-c",
      "aecCategory": 8
    }
  }
  ```

---

### 4. STRATEGIC MITIGATIONS VISUALIZATION (M1/M1A/M1B/M1C)

**SORA 2.0 Reference:** AMC Table 3 (M1 Strategic Mitigations for Ground Risk)  
**SORA 2.5 Reference:** JARUS Annex B Table 5 (M1A/M1B/M1C)  

#### Mitigation Categories:

**SORA 2.0 - M1 Strategic Mitigations:**
- None: +0 GRC reduction
- Low: -1 GRC reduction
- Medium: -2 GRC reduction
- High: -4 GRC reduction (cannot reduce below column minimum)

**SORA 2.5 - Split Mitigations:**
- **M1A - Location-based:** Geographic constraints, operational boundaries
- **M1B - UA Characteristics:** Size, speed, impact energy limits
- **M1C - Sheltering:** UA won't penetrate standard dwelling (-1 GRC)

#### GIS Requirements:
- **Mitigation Zone Overlays**
  - M1A zones: Geographic no-fly buffers, time restrictions
  - M1B zones: UA characteristic constraints (max dimension, max speed)
  - M1C zones: Sheltering applicability (building density, roof strength)

- **Buffer Calculation Tools**
  - Ground risk buffers (based on UA parameters)
  - Approach/departure corridors
  - ELS (Emergency Landing Site) zones
  - Crowd assembly buffer zones

- **Data Format:** GeoJSON with mitigation metadata
  ```json
  {
    "type": "Feature",
    "geometry": {"type": "Polygon", "coordinates": [...]},
    "properties": {
      "mitigationType": "M1A|M1B|M1C",
      "reductionValue": -2,
      "constraint": "No operations after sunset",
      "bufferDistance": 50,
      "shelteringApplicable": true,
      "soraVersion": "2.5"
    }
  }
  ```

---

### 5. NO-FLY ZONES & REGULATORY RESTRICTIONS

**SORA 2.0 Reference:** OSO #18 (Adjacency to controlled ground areas)  
**SORA 2.5 Reference:** EASA Part-UAS Subpart B (Geo-awareness requirements)  

#### Categories:
- **Permanent No-Fly Zones:**
  - Airports/Heliports (5km radius default)
  - Military installations
  - Critical infrastructure (power plants, dams)
  - National parks/protected areas
  - Prisons, hospitals (privacy/safety)

- **Temporary No-Fly Zones:**
  - NOTAM-based restrictions
  - Special events (concerts, sports, political)
  - Emergency operations (fire, rescue, police)
  - Weather-related closures

- **Conditional Restrictions:**
  - Altitude limits (e.g., 120m AGL)
  - Time-based (daylight only, noise curfews)
  - Weather minimums (visibility, wind, precipitation)
  - Operational category limits (Open, Specific, Certified)

#### GIS Requirements:
- **No-Fly Zone Layer**
  - Permanent zones from national aviation authority
  - Real-time NOTAM feed integration
  - Event-based temporary zones (API-driven)
  - Conditional restriction logic engine

- **Geo-fencing Enforcement**
  - Hard geofences (flight termination)
  - Soft geofences (warnings)
  - Dynamic geofence updates (WebSocket push)

- **Data Format:** GeoJSON with restriction metadata
  ```json
  {
    "type": "Feature",
    "geometry": {"type": "Polygon", "coordinates": [...]},
    "properties": {
      "restrictionType": "permanent|temporary|conditional",
      "category": "airport|military|event|weather",
      "severity": "prohibited|restricted|caution",
      "validFrom": "2025-01-27T00:00:00Z",
      "validTo": "2025-01-27T23:59:59Z",
      "altitudeRestriction": {"min": 0, "max": 120},
      "enforceGeofence": true
    }
  }
  ```

---

### 6. TERRAIN & ELEVATION DATA

**Purpose:** Altitude reference, obstacle avoidance, AGL/AMSL conversion  

#### Requirements:
- **Digital Elevation Model (DEM)**
  - Resolution: 30m (SRTM) or 10m (EU-DEM)
  - Accuracy: ±10m vertical
  - Coverage: Global (minimum European coverage)
  - Format: GeoTIFF

- **Obstacle Database**
  - Buildings (3D models, heights)
  - Towers/masts (communication, power)
  - Wind turbines
  - Trees/vegetation (estimated heights)

- **3D Visualization**
  - Terrain elevation rendering
  - Obstacle collision detection
  - Line-of-sight analysis (VLOS verification)

---

## 🔧 TECHNICAL IMPLEMENTATION REQUIREMENTS

### Backend Services (.NET 8 C#)

#### GIS Models (`Backend/Models/GIS/`)
```csharp
// OperationalVolume.cs
public class OperationalVolume
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public GeoJsonPolygon Geometry { get; set; } // 3D polygon
    public double MinAltitudeAGL { get; set; }
    public double MaxAltitudeAGL { get; set; }
    public double MinAltitudeAMSL { get; set; }
    public double MaxAltitudeAMSL { get; set; }
    public double SafetyMarginHorizontal { get; set; }
    public string SoraVersion { get; set; } // "2.0" or "2.5"
    public List<GeoJsonPoint> ContingencyZones { get; set; }
}

// PopulationDensityZone.cs
public class PopulationDensityZone
{
    public Guid Id { get; set; }
    public GeoJsonPolygon Geometry { get; set; }
    public double PopulationDensity { get; set; }
    
    // SORA 2.0 properties
    public string Scenario_2_0 { get; set; } // "VLOS_Sparsely", etc.
    public int IntrinsicGRC_2_0 { get; set; }
    
    // SORA 2.5 properties
    public string Category_2_5 { get; set; } // "<500", "<5000", etc.
    public int IntrinsicGRC_2_5 { get; set; }
    
    public double ShelteringFactor { get; set; }
    public bool M1Applicability { get; set; }
}

// AirspaceVolume.cs
public class AirspaceVolume
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public GeoJsonPolygon Geometry { get; set; }
    public string AirspaceClass { get; set; } // A-G
    public double FloorAltitude { get; set; }
    public double CeilingAltitude { get; set; }
    public string AltitudeReference { get; set; } // "AGL" or "AMSL"
    public bool ATCServiceAvailable { get; set; }
    public bool ModeS_Required { get; set; }
    public string Environment { get; set; } // "Urban", "Suburban", "Rural"
    public string InitialARC { get; set; } // "ARC-a", "ARC-b", etc.
    public int AECCategory { get; set; } // 1-12
}

// NoFlyZone.cs
public class NoFlyZone
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public GeoJsonPolygon Geometry { get; set; }
    public string RestrictionType { get; set; } // "permanent", "temporary", "conditional"
    public string Category { get; set; } // "airport", "military", "event", "weather"
    public string Severity { get; set; } // "prohibited", "restricted", "caution"
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public AltitudeRestriction AltitudeRestriction { get; set; }
    public bool EnforceGeofence { get; set; }
}

// TerrainElevation.cs
public class TerrainElevation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double ElevationAMSL { get; set; }
    public string Source { get; set; } // "SRTM", "EU-DEM"
    public double Accuracy { get; set; }
}
```

#### GIS Service (`Backend/Services/GISService.cs`)
```csharp
public interface IGISService
{
    // Operational Volume
    Task<OperationalVolume> CreateOperationalVolumeAsync(OperationalVolumeRequest request);
    Task<List<OperationalVolume>> GetOperationalVolumesAsync(string missionId);
    Task<bool> ValidateOperationalVolumeAsync(OperationalVolume volume);
    
    // Population Density
    Task<PopulationDensityZone> GetPopulationDensityAsync(double lat, double lon);
    Task<List<PopulationDensityZone>> GetPopulationDensityZonesAsync(GeoJsonPolygon bounds);
    Task<int> CalculateIntrinsicGRCAsync(double lat, double lon, string soraVersion, double uaDimension, double uaSpeed);
    
    // Airspace
    Task<List<AirspaceVolume>> GetAirspaceVolumesAsync(double lat, double lon, double altitudeAMSL);
    Task<string> DetermineAirspaceClassAsync(double lat, double lon, double altitudeAMSL);
    Task<string> CalculateInitialARCAsync(double lat, double lon, double altitudeAGL, double altitudeAMSL, string environment, string soraVersion);
    
    // No-Fly Zones
    Task<List<NoFlyZone>> GetNoFlyZonesAsync(GeoJsonPolygon bounds, DateTime operationDate);
    Task<bool> CheckNoFlyZoneViolationAsync(GeoJsonPolygon flightPath, DateTime operationDate);
    
    // Terrain
    Task<double> GetTerrainElevationAsync(double lat, double lon);
    Task<List<TerrainElevation>> GetTerrainProfileAsync(List<GeoJsonPoint> waypoints);
}
```

#### GIS Controller (`Backend/Controllers/GISController.cs`)
```csharp
[ApiController]
[Route("api/gis")]
public class GISController : ControllerBase
{
    private readonly IGISService _gisService;
    
    [HttpPost("operational-volume")]
    public async Task<ActionResult<OperationalVolume>> CreateOperationalVolume([FromBody] OperationalVolumeRequest request) { }
    
    [HttpGet("population-density")]
    public async Task<ActionResult<PopulationDensityZone>> GetPopulationDensity([FromQuery] double lat, [FromQuery] double lon) { }
    
    [HttpGet("airspace")]
    public async Task<ActionResult<List<AirspaceVolume>>> GetAirspace([FromQuery] double lat, [FromQuery] double lon, [FromQuery] double altitude) { }
    
    [HttpGet("no-fly-zones")]
    public async Task<ActionResult<List<NoFlyZone>>> GetNoFlyZones([FromQuery] GeoJsonPolygon bounds, [FromQuery] DateTime operationDate) { }
    
    [HttpGet("terrain-elevation")]
    public async Task<ActionResult<double>> GetTerrainElevation([FromQuery] double lat, [FromQuery] double lon) { }
}
```

---

### Frontend Integration (JavaScript)

#### Mapping Libraries
- **2D Mapping:** Leaflet.js or OpenLayers
- **3D Mapping:** CesiumJS (for terrain/airspace visualization)
- **GeoJSON Support:** Native in all libraries

#### Key Features
- **Layer Management:**
  - Operational volume overlay
  - Population density heatmap
  - Airspace classification zones
  - No-fly zone boundaries
  - Terrain elevation contours

- **Interactive Tools:**
  - Draw operational volumes (polygon tool)
  - Query population density (click tool)
  - Check airspace class (hover tool)
  - Validate no-fly zones (real-time check)

- **Real-time Updates:**
  - WebSocket for NOTAM changes
  - WebSocket for temporary no-fly zones
  - WebSocket for weather restrictions

---

## 📊 DATA SOURCES

### Primary Sources
1. **Population Density:**
   - Eurostat GEOSTAT (Europe)
   - WorldPop (Global)
   - National census data (country-specific)

2. **Airspace:**
   - EAD (European AIS Database)
   - FAA NASR (US)
   - ICAO Annex 15 (global standards)

3. **Terrain:**
   - SRTM (Shuttle Radar Topography Mission) - 30m
   - EU-DEM (European Digital Elevation Model) - 25m
   - ASTER GDEM - 30m

4. **No-Fly Zones:**
   - National aviation authorities (CAAs)
   - NOTAM feeds (ICAO standard)
   - OpenAIP (crowd-sourced airspace data)

### Integration Providers (existing)
- `AirspaceProvider` (Phase 2)
- `PopulationDensityProvider` (Phase 2)
- `WeatherProvider` (Phase 3)

---

## 🧪 TESTING REQUIREMENTS

### Unit Tests
- GeoJSON geometry validation
- Population density category calculation (2.0 vs 2.5)
- Airspace class determination (12 AEC categories)
- No-fly zone intersection detection
- Terrain elevation interpolation

### Integration Tests
- Full mission validation (operational volume + restrictions)
- Multi-layer query performance (population + airspace + no-fly)
- Real-time NOTAM update handling
- WebSocket push notifications

### Acceptance Criteria
- ✅ All GeoJSON geometries valid per RFC 7946
- ✅ Population density queries < 100ms (cached)
- ✅ Airspace queries < 200ms (3D spatial index)
- ✅ No-fly zone validation < 500ms (full mission path)
- ✅ Dual SORA compliance (2.0 + 2.5) for all calculations

---

## 📝 DOCUMENTATION DELIVERABLES

1. **API Documentation:**
   - OpenAPI/Swagger spec for all GIS endpoints
   - Example requests/responses
   - Error codes and handling

2. **Integration Guide:**
   - Frontend mapping library setup
   - Layer configuration examples
   - WebSocket event handling

3. **SORA Compliance Matrix:**
   - GIS features mapped to OSO requirements
   - Evidence generation for operational authorization
   - Dual version support documentation

---

## 🔗 DEPENDENCIES

### Existing Services (Phase 5)
- `OSOFrameworkService` (OSO #17, #18, #19, #23 validation)
- `RiskMitigationService` (M1/M1A/M1B/M1C calculation)
- `GRCCalculationService` (population density → GRC mapping)
- `ARCCalculationService` (airspace → ARC mapping)

### External Providers
- Airspace data provider (EAD, FAA, ICAO)
- Population density data provider (Eurostat, WorldPop)
- Terrain elevation provider (SRTM, EU-DEM)
- NOTAM feed provider (ICAO, national CAAs)

---

## 🚀 IMPLEMENTATION PHASES

### Phase 1: Core Models & Service (3 days)
- Create GIS models (OperationalVolume, PopulationDensityZone, AirspaceVolume, NoFlyZone, TerrainElevation)
- Implement `GISService` with basic CRUD operations
- Create `GISController` with initial endpoints

### Phase 2: Data Integration (4 days)
- Integrate population density provider
- Integrate airspace data provider
- Integrate terrain elevation provider
- Implement caching layer (Redis)

### Phase 3: Frontend Mapping (4 days)
- Setup Leaflet.js / OpenLayers
- Implement layer management
- Create interactive tools (draw, query, validate)
- Add 3D visualization (CesiumJS)

### Phase 4: Testing & Documentation (3 days)
- Write unit tests (80%+ coverage)
- Write integration tests
- Generate API documentation
- Create SORA compliance matrix

**Total Estimate:** 14 days

---

## ✅ ACCEPTANCE CRITERIA

- [ ] All GIS models created and documented
- [ ] `GISService` implemented with full CRUD operations
- [ ] `GISController` endpoints tested and documented
- [ ] Population density integration working (SORA 2.0 + 2.5)
- [ ] Airspace classification working (12 AEC categories)
- [ ] No-fly zone validation working (permanent + temporary)
- [ ] Terrain elevation queries working
- [ ] Frontend mapping library integrated
- [ ] Layer management UI complete
- [ ] Interactive tools working (draw, query, validate)
- [ ] Real-time updates via WebSocket
- [ ] Unit tests 80%+ coverage
- [ ] Integration tests passing
- [ ] API documentation generated
- [ ] SORA compliance matrix updated

---

**END OF REQUIREMENTS DOCUMENT**
