using Skyworks.Core.Models.ARC;
using Skyworks.Core.Services.ARC;
using Xunit;

namespace Skyworks.Api.Tests.ARC_SAIL;

public class ARC_Validation_Tests
{
    [Fact]
    public void Height_over_120m_triggers_warning()
    {
        var svc = new ARCValidationService();
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Uncontrolled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Urban,
            MaxHeightAGL = 150
        };
        var res = svc.ValidateEnvironment_V2_5(env);
        Assert.Contains(res.Issues, i => i.Code == "HEIGHT.AGL.>120" && i.Severity == Core.Models.ARC.Validation.RuleSeverity.Warning);
    }

    [Fact]
    public void Controlled_airspace_triggers_warning()
    {
        var svc = new ARCValidationService();
        var env = new ARCEnvironmentInput
        {
            AirspaceControl = AirspaceControl.Controlled,
            LocationType = LocationType.NonAirport,
            Environment = EnvironmentType.Urban,
            MaxHeightAGL = 60
        };
        var res = svc.ValidateEnvironment_V2_5(env);
        Assert.Contains(res.Issues, i => i.Code == "AIRSPACE.CONTROLLED");
    }
}

