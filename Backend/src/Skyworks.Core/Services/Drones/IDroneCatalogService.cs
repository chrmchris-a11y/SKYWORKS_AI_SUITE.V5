using Skyworks.Core.Models.Drones;

namespace Skyworks.Core.Services.Drones;

/// <summary>
/// Service interface for drone catalog operations.
/// </summary>
public interface IDroneCatalogService
{
    Task<List<DroneSpecification>> GetAllDronesAsync();
    Task<DroneSpecification?> GetDroneByIdAsync(string droneId);
    Task<List<DroneSpecification>> SearchDronesAsync(DroneCatalogFilter filter);
    Task<DroneSelectionResult> GetDroneWithSORASuggestionsAsync(string droneId);
}
