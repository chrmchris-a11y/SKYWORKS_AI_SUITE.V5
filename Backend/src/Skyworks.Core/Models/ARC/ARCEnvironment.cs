namespace Skyworks.Core.Models.ARC;

/// <summary>
/// ICAO Airspace Classification
/// Reference: ICAO Annex 2, Annex 11
/// </summary>
public enum AirspaceClass
{
    /// <summary>Class A: IFR only, all aircraft subject to ATC service</summary>
    A = 0,
    /// <summary>Class B: IFR and VFR, all aircraft subject to ATC service, ATC clearance required</summary>
    B = 1,
    /// <summary>Class C: IFR and VFR, all aircraft subject to ATC service, ATC clearance for IFR, two-way radio for VFR</summary>
    C = 2,
    /// <summary>Class D: IFR and VFR, ATC service provided, two-way radio required</summary>
    D = 3,
    /// <summary>Class E: IFR and VFR, IFR subject to ATC, VFR receives flight information</summary>
    E = 4,
    /// <summary>Class F: IFR and VFR, flight information service provided (advisory, rare)</summary>
    F = 5,
    /// <summary>Class G: IFR and VFR, uncontrolled airspace, flight information on request</summary>
    G = 6
}

public enum AirspaceControl
{
    Uncontrolled = 0,
    Controlled = 1
}

public enum LocationType
{
    NonAirport = 0,
    Airport = 1,
    Heliport = 2
}

/// <summary>
/// Environment classification per JARUS SORA Table 3
/// Population density thresholds
/// </summary>
public enum EnvironmentType
{
    /// <summary>Urban: > 1,500 people/km² per JARUS SORA v2.0 Table 3</summary>
    Urban = 0,
    /// <summary>Suburban: 500-1,500 people/km²</summary>
    Suburban = 1,
    /// <summary>Rural/Sparsely Populated: < 500 people/km²</summary>
    Rural = 2,
    /// <summary>Industrial: Industrial zones (treated as Urban for ARC)</summary>
    Industrial = 3
}

public enum AirspaceTypicality
{
    Typical = 0,
    AtypicalSegregated = 1
}

/// <summary>
/// Complete ARC Environment Input for authoritative SORA 2.0/2.5 ARC determination
/// Reference: SORA 2.0 Annex C Table 1, SORA 2.5 Step #4 Figure 6
/// </summary>
public class ARCEnvironmentInput
{
    // ════════════════════════════════════════════════════════════════════
    // ALTITUDE PARAMETERS
    // ════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Maximum operation height above ground level (meters)
    /// Critical threshold: 500 ft (152m) separates VLL from low-level ops
    /// </summary>
    public double MaxHeightAGL { get; set; } = 120.0; // meters (default: 400 ft)
    
    /// <summary>
    /// Maximum operation height above mean sea level (meters)
    /// Used for FL 600 (18,000m / 59,000 ft) check
    /// </summary>
    public double MaxHeightAMSL { get; set; } = 500.0; // meters
    
    // ════════════════════════════════════════════════════════════════════
    // AIRSPACE CLASSIFICATION
    // ════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// ICAO Airspace Class (A-G)
    /// Used to determine if airspace is controlled (B/C/D) vs uncontrolled (G)
    /// </summary>
    public AirspaceClass AirspaceClass { get; set; } = AirspaceClass.G;
    
    /// <summary>
    /// Simplified controlled/uncontrolled flag
    /// Controlled: Class A, B, C, D, E (with restrictions)
    /// Uncontrolled: Class F, G
    /// </summary>
    public AirspaceControl AirspaceControl { get; set; } = AirspaceControl.Uncontrolled;
    
    /// <summary>
    /// Mode-S Veil or Transponder Mandatory Zone (TMZ)
    /// Typically around major airports, requires Mode-S transponder
    /// </summary>
    public bool IsModeS_Veil { get; set; } = false;
    
    /// <summary>
    /// Transponder Mandatory Zone (TMZ)
    /// Airspace where transponder is mandatory
    /// </summary>
    public bool IsTMZ { get; set; } = false;
    
    // ════════════════════════════════════════════════════════════════════
    // ENVIRONMENT & POPULATION
    // ════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Environment type (Urban/Suburban/Rural)
    /// Urban: > 1,500 people/km² (SORA Table 3)
    /// </summary>
    public EnvironmentType Environment { get; set; } = EnvironmentType.Rural;
    
    /// <summary>
    /// Population density (people per km²)
    /// Used to auto-determine Environment if not explicitly set
    /// Thresholds: >1500 = Urban, 500-1500 = Suburban, <500 = Rural
    /// </summary>
    public double PopulationDensity { get; set; } = 100.0; // people/km²
    
    // ════════════════════════════════════════════════════════════════════
    // LOCATION TYPE
    // ════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Location type (Airport, Heliport, NonAirport)
    /// Airport/Heliport environments have higher traffic density
    /// </summary>
    public LocationType LocationType { get; set; } = LocationType.NonAirport;
    
    // ════════════════════════════════════════════════════════════════════
    // SPECIAL CONDITIONS
    // ════════════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Atypical/Segregated Airspace per SORA Annex G Section 3.20
    /// If TRUE and accepted by authority → ARC-a (no TMPR required)
    /// Requires authority acceptance
    /// </summary>
    public AirspaceTypicality Typicality { get; set; } = AirspaceTypicality.Typical;
    
    /// <summary>
    /// Is operation in Atypical/Segregated airspace?
    /// Convenience property for Typicality == AtypicalSegregated
    /// </summary>
    public bool IsAtypicalSegregated => Typicality == AirspaceTypicality.AtypicalSegregated;

    // ════════════════════════════════════════════════════════════════════
    // AERODROME PROXIMITY (OPTIONAL ENHANCEMENTS)
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Whether the operational volume is near an aerodrome (advisory). Optional.
    /// </summary>
    public bool IsNearAerodrome { get; set; } = false;

    /// <summary>
    /// Distance to nearest aerodrome in kilometers, if known (advisory). Optional.
    /// </summary>
    public double? DistanceToAerodrome_km { get; set; }

    /// <summary>
    /// Operation inside a CTR (Control Zone). Optional.
    /// </summary>
    public bool IsInCTR { get; set; } = false;
}
