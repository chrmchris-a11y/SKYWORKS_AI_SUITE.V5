using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Skyworks.Core.Services.Orchestration;
using Skyworks.Core.Services.GRC;
using Skyworks.Core.Services;
using Skyworks.Core.Services.Risk;
using Skyworks.Core.Services.Drones;
using Skyworks.Core.Models.Drones;

namespace Skyworks.Api.Tests.Orchestration;

public class SORADrone_Autofill_Tests
{
    private class StubDroneCatalog : IDroneCatalogService
    {
        private readonly DroneSpecification _drone = new()
        {
            Id = "TEST-DRONE-1",
            Manufacturer = "TestCo",
            Model = "T100",
            CClass = EUCClass.C2,
            MTOM_kg = 5.0,
            CharacteristicDimension_m = 0.5,
            MaxSpeed_mps = 20.0
        };

        public Task<List<DroneSpecification>> GetAllDronesAsync() => Task.FromResult(new List<DroneSpecification> { _drone });
        public Task<DroneSpecification?> GetDroneByIdAsync(string droneId) => Task.FromResult<DroneSpecification?>(_drone);
        public Task<List<DroneSpecification>> SearchDronesAsync(DroneCatalogFilter filter) => Task.FromResult(new List<DroneSpecification> { _drone });
        public Task<DroneSelectionResult> GetDroneWithSORASuggestionsAsync(string droneId) => Task.FromResult(new DroneSelectionResult { Drone = _drone });
    }

    [Fact]
    public void DroneId_AutoFills_GRC_Params_And_Computes()
    {
        // Arrange
        var grc = new GRCCalculationService();
        var sail = new SAILService();
        var oso = new OSOService();
        var risk = new RiskModelingService();
        var catalog = new StubDroneCatalog();
    var orchestration = new SORAOrchestrationService(grc, sail, oso, risk, logger: null, arcValidationService: null, droneCatalog: catalog, pythonClient: null, proxyOnly: false, allowExplicitARC: true);

        var request = new SORACompleteRequest
        {
            SoraVersion = "2.5",
            DroneId = "TEST-DRONE-1",
            GroundRisk = new GroundRiskInput
            {
                PopulationDensity = 100, // <500 band
                IsControlledGroundArea = false,
                MaxCharacteristicDimension = 0, // force auto-fill
                MaxSpeed = null,
                KineticEnergy = null
            },
            AirRisk = new AirRiskInput
            {
                ExplicitARC = Skyworks.Core.Models.ARC.ARCRating.ARC_b
            },
            ImplementedOSOs = new List<OSOImplementation>()
        };

        // Act
        var result = orchestration.ExecuteComplete(request);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.NotNull(result.IntrinsicGRC);
        // With 0.5m and 20 m/s => Small category; density band <500 => iGRC row [3], col [0] => 4
        Assert.Equal(4, result.IntrinsicGRC);
        Assert.NotNull(result.FinalGRC);
        Assert.Equal(result.IntrinsicGRC, result.FinalGRC); // no mitigations applied
        Assert.Equal(Skyworks.Core.Models.ARC.ARCRating.ARC_b, result.ResidualARC);
    }
}
