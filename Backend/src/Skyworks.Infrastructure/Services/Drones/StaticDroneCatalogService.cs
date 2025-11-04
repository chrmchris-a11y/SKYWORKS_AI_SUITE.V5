using System.Text.Json;
using Skyworks.Core.Models.Drones;
using Skyworks.Core.Services.Drones;

namespace Skyworks.Infrastructure.Services.Drones;

/// <summary>
/// Static drone catalog service loading from JSON files.
/// </summary>
public class StaticDroneCatalogService : IDroneCatalogService
{
    private readonly List<DroneSpecification> _drones;

    public StaticDroneCatalogService()
    {
        _drones = LoadAllDrones();
    }

    private List<DroneSpecification> LoadAllDrones()
    {
        var drones = new List<DroneSpecification>();
        var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        var files = new[]
        {
            "drones-catalog-part1.json",
            "drones-catalog-part2.json",
            "drones-catalog-part3.json",
            "drones-catalog-part4.json",
            "drones-catalog-part5.json"
        };

        foreach (var file in files)
        {
            var filePath = Path.Combine(basePath, file);
            if (!File.Exists(filePath))
            {
                // Fallback to relative path from project root
                filePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "src", "Skyworks.Infrastructure", "Data", file);
            }

            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var batch = JsonSerializer.Deserialize<List<DroneSpecification>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                });
                if (batch != null)
                {
                    drones.AddRange(batch);
                }
            }
        }

        return drones;
    }

    public Task<List<DroneSpecification>> GetAllDronesAsync()
    {
        return Task.FromResult(_drones);
    }

    public Task<DroneSpecification?> GetDroneByIdAsync(string droneId)
    {
        var drone = _drones.FirstOrDefault(d => d.Id.Equals(droneId, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(drone);
    }

    public Task<List<DroneSpecification>> SearchDronesAsync(DroneCatalogFilter filter)
    {
        IEnumerable<DroneSpecification> results = _drones;

        if (filter.CClass.HasValue)
        {
            results = results.Where(d => d.CClass == filter.CClass.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Manufacturer))
        {
            results = results.Where(d => d.Manufacturer.Contains(filter.Manufacturer, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.MaxMTOM_kg.HasValue)
        {
            results = results.Where(d => d.MTOM_kg <= filter.MaxMTOM_kg.Value);
        }

        if (filter.MinMTOM_kg.HasValue)
        {
            results = results.Where(d => d.MTOM_kg >= filter.MinMTOM_kg.Value);
        }

        if (filter.IsBVLOSCapable.HasValue)
        {
            results = results.Where(d => d.IsBVLOSCapable == filter.IsBVLOSCapable.Value);
        }

        if (filter.HasObstacleAvoidance.HasValue)
        {
            results = results.Where(d => d.HasObstacleAvoidance == filter.HasObstacleAvoidance.Value);
        }

        if (filter.MinFlightTime_min.HasValue)
        {
            results = results.Where(d => d.MaxFlightTime_min >= filter.MinFlightTime_min.Value);
        }

        return Task.FromResult(results.ToList());
    }

    public Task<DroneSelectionResult> GetDroneWithSORASuggestionsAsync(string droneId)
    {
        var drone = _drones.FirstOrDefault(d => d.Id.Equals(droneId, StringComparison.OrdinalIgnoreCase));
        if (drone == null)
        {
            return Task.FromResult<DroneSelectionResult>(null!);
        }

        var result = new DroneSelectionResult
        {
            Drone = drone,
            SuggestedInitialARC = drone.SuggestedARC,
            SuggestedGRC_Dimension = MapDimensionToGRC(drone.CharacteristicDimension_m),
            SuggestedGRC_Speed = MapSpeedToGRC(drone.MaxSpeed_mps),
            ComplianceNotes = GenerateComplianceNotes(drone)
        };

        return Task.FromResult(result);
    }

    private int MapDimensionToGRC(double dimension_m)
    {
        // SORA 2.5 dimension mapping (approximate)
        if (dimension_m < 0.3) return 1;
        if (dimension_m < 1.0) return 2;
        if (dimension_m < 3.0) return 3;
        if (dimension_m < 8.0) return 4;
        return 5;
    }

    private int MapSpeedToGRC(double speed_mps)
    {
        // SORA 2.5 speed mapping (approximate)
        if (speed_mps < 5) return 1;
        if (speed_mps < 10) return 2;
        if (speed_mps < 15) return 3;
        if (speed_mps < 25) return 4;
        return 5;
    }

    private List<string> GenerateComplianceNotes(DroneSpecification drone)
    {
        var notes = new List<string>();

        if (drone.MTOM_kg < 0.25)
        {
            notes.Add("Open Category C0: No registration required (<250g)");
        }
        else if (drone.MTOM_kg < 0.9 && drone.CClass == EUCClass.C1)
        {
            notes.Add("Open Category C1: Remote pilot competency required");
        }
        else if (drone.MTOM_kg < 4 && drone.CClass == EUCClass.C2)
        {
            notes.Add("Open Category C2: A2 Certificate required for <50m from people");
        }
        else if (drone.MTOM_kg < 25 && drone.CClass == EUCClass.C3)
        {
            notes.Add("Open Category C3: Keep 150m from residential/commercial/industrial areas");
        }
        else
        {
            notes.Add("Specific Category: SORA assessment required");
        }

        if (drone.HasRemoteID)
        {
            notes.Add("Remote ID: Compliant (EU 2019/945)");
        }
        else
        {
            notes.Add("Remote ID: Upgrade required for EU compliance");
        }

        if (drone.IsBVLOSCapable)
        {
            notes.Add("BVLOS: Requires Specific Category authorization & risk assessment");
        }

        if (drone.HasParachute)
        {
            notes.Add("Parachute: Reduces kinetic energy risk (SORA mitigation M3)");
        }

        if (drone.MTOM_kg > 25)
        {
            notes.Add("Heavy UA: Requires enhanced operational procedures");
        }

        return notes;
    }
}
