using System.Threading.Tasks;
using Skyworks.Core.Models;

namespace Skyworks.Core.Services;

/// <summary>
/// SORA calculation request for internal service use.
/// Maps to the SoraController's request format.
/// </summary>
public class SoraRequest
{
    public string SoraVersion { get; set; } = "2.5";
    public double DroneMtom_kg { get; set; }
    public double DroneMaxSpeed_ms { get; set; }
    public double DroneCharacteristicDimension_m { get; set; }
    public double MaxHeightAGL_m { get; set; }
    public bool ControlledGroundArea { get; set; }
    public string Environment { get; set; } = "suburban"; // "urban", "suburban", "rural"
    public bool IsVLOS { get; set; } = true;
    public string? PopulationDensity { get; set; }
    public string? M1a_Sheltering { get; set; }
    public string? M1b_OperationalRestrictions { get; set; }
    public string? M1c_GroundObservation { get; set; }
    public string? M2_ImpactDynamics { get; set; }
}

/// <summary>
/// SORA calculation result containing GRC, ARC, and SAIL outputs.
/// </summary>
public class SoraResult
{
    public int InitialGrc { get; set; }
    public int FinalGrc { get; set; }
    public string InitialArc { get; set; } = string.Empty; // "a", "b", "c", "d" or "ARC-a", etc.
    public string ResidualArc { get; set; } = string.Empty; // "a", "b", "c", "d" or "ARC-a", etc.
    public string Sail { get; set; } = string.Empty; // "I", "II", "III", "IV", "V", "VI"
    public string InputsJson { get; set; } = string.Empty; // Full request as JSON for audit
}

/// <summary>
/// Service interface for SORA risk assessment calculations.
/// Wraps the Node.js SORA calculator (sora-calculator.js).
/// </summary>
public interface ISoraCalculatorService
{
    /// <summary>
    /// Calculates SORA risk assessment (GRC, ARC, SAIL) for a mission.
    /// </summary>
    Task<SoraResult> CalculateAsync(SoraRequest request);
}
