using Xunit;
using Skyworks.Infrastructure.Services.Compliance;
using Skyworks.Core.Models.Compliance;

namespace Skyworks.Api.Tests.Compliance;

public class ComplianceProvider_Tests
{
    [Fact]
    public async Task GetObligations_CTR_RequiresATCClearance()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "b",
            AirspaceType = "CTR",
            OperationType = "VLOS",
            MaxAltitudeAGL = 120,
            CountryCode = "CY",
            IsUSpaceAirspace = false
        };

        // Act
        var result = await provider.GetComplianceObligationsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.Obligations, o => o.Type == "ATC" && o.Priority == "Mandatory");
        var atcObligation = result.Obligations.First(o => o.Type == "ATC");
        Assert.Contains("clearance", atcObligation.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("48h before operation", atcObligation.Deadline);
    }

    [Fact]
    public async Task GetObligations_USpace_RequiresRegistration()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "b",
            AirspaceType = "Uncontrolled",
            OperationType = "VLOS",
            MaxAltitudeAGL = 120,
            CountryCode = "CY",
            IsUSpaceAirspace = true
        };

        // Act
        var result = await provider.GetComplianceObligationsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.Obligations, o => o.Type == "U-space" && o.Priority == "Mandatory");
        var uspaceObligation = result.Obligations.First(o => o.Type == "U-space");
        Assert.Contains("registration", uspaceObligation.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetObligations_BVLOS_RequiresAuthorization()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "c",
            AirspaceType = "Uncontrolled",
            OperationType = "BVLOS",
            MaxAltitudeAGL = 200,
            CountryCode = "CY",
            IsUSpaceAirspace = false
        };

        // Act
        var result = await provider.GetComplianceObligationsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.Obligations, o => o.Type == "BVLOS" && o.Priority == "Mandatory");
        var bvlosObligations = result.Obligations.Where(o => o.Type == "BVLOS").ToList();
        Assert.True(bvlosObligations.Count >= 2, "Should have multiple BVLOS obligations (auth + DAA + pilot cert)");
        Assert.Contains(bvlosObligations, o => o.Description.Contains("authorization", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(bvlosObligations, o => o.Description.Contains("Detect-and-Avoid", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetObligations_ARCc_RequiresOSOAndTMPR()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "c",
            AirspaceType = "Uncontrolled",
            OperationType = "VLOS",
            MaxAltitudeAGL = 120,
            CountryCode = "CY",
            IsUSpaceAirspace = false
        };

        // Act
        var result = await provider.GetComplianceObligationsAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.Obligations, o => o.Type == "OSO");
        Assert.Contains(result.Obligations, o => o.Type == "TMPR");
        Assert.Contains(result.Obligations, o => o.Type == "Documentation");
    }

    [Fact]
    public async Task GetObligations_ARCa_NoMandatoryOSOTMPR()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "a",
            AirspaceType = "Uncontrolled",
            OperationType = "VLOS",
            MaxAltitudeAGL = 50,
            CountryCode = "CY",
            IsUSpaceAirspace = false
        };

        // Act
        var result = await provider.GetComplianceObligationsAsync(request);

        // Assert
        Assert.NotNull(result);
        // ARC-a should have no OSO/TMPR obligations
        Assert.DoesNotContain(result.Obligations, o => o.Type == "OSO");
        Assert.DoesNotContain(result.Obligations, o => o.Type == "TMPR");
    }

    [Fact]
    public async Task GetChecklist_VLOS_IncludesWeatherAndNOTAM()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "b",
            AirspaceType = "Uncontrolled",
            OperationType = "VLOS",
            MaxAltitudeAGL = 120
        };

        // Act
        var checklist = await provider.GetPreFlightChecklistAsync(request);

        // Assert
        Assert.NotNull(checklist);
        Assert.Contains(checklist, c => c.Category == "Weather" && c.IsMandatory);
        Assert.Contains(checklist, c => c.Category == "Airspace" && c.Item.Contains("NOTAM"));
        Assert.Contains(checklist, c => c.Category == "Equipment" && c.Item.Contains("geo-fencing"));
    }

    [Fact]
    public async Task GetChecklist_BVLOS_IncludesDAA()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "c",
            AirspaceType = "Uncontrolled",
            OperationType = "BVLOS",
            MaxAltitudeAGL = 200
        };

        // Act
        var checklist = await provider.GetPreFlightChecklistAsync(request);

        // Assert
        Assert.NotNull(checklist);
        Assert.Contains(checklist, c => 
            c.Category == "Equipment" && 
            c.Item.Contains("detect-and-avoid", StringComparison.OrdinalIgnoreCase) && 
            c.IsMandatory);
        Assert.Contains(checklist, c => 
            c.Category == "Personnel" && 
            c.Item.Contains("BVLOS certification"));
    }

    [Fact]
    public async Task ValidateReadiness_ARCc_RequiresDocumentation()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "c",
            AirspaceType = "Uncontrolled",
            OperationType = "VLOS",
            MaxAltitudeAGL = 120
        };

        // Act
        var readiness = await provider.ValidateOperatorReadinessAsync("OP001", "M001", request);

        // Assert
        Assert.NotNull(readiness);
        Assert.Equal("OP001", readiness.OperatorId);
        Assert.Equal("M001", readiness.MissionId);
        Assert.Contains(readiness.DocumentationChecklist, d => d.Item.Contains("Operations Manual") && d.IsMandatory);
        Assert.Contains(readiness.DocumentationChecklist, d => d.Item.Contains("Risk Assessment") && d.IsMandatory);
        Assert.Contains(readiness.RequiredOSOs, o => o.Contains("High"));
        Assert.Equal(2, readiness.RequiredTMPR); // ARC-c → TMPR 2
    }

    [Fact]
    public async Task ValidateReadiness_ARCa_LowRequirements()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "a",
            AirspaceType = "Uncontrolled",
            OperationType = "VLOS",
            MaxAltitudeAGL = 50
        };

        // Act
        var readiness = await provider.ValidateOperatorReadinessAsync("OP001", "M001", request);

        // Assert
        Assert.NotNull(readiness);
        Assert.Contains(readiness.RequiredOSOs, o => o.Contains("Low"));
        Assert.Equal(1, readiness.RequiredTMPR); // ARC-a → TMPR 1
        // OpManual not mandatory for ARC-a
        Assert.DoesNotContain(readiness.DocumentationChecklist, d => 
            d.Item.Contains("Operations Manual") && d.IsMandatory);
    }

    [Fact]
    public async Task GetObligations_RestrictedAirspace_RequiresSpecialAuth()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "b",
            AirspaceType = "Restricted",
            OperationType = "VLOS",
            MaxAltitudeAGL = 100
        };

        // Act
        var result = await provider.GetComplianceObligationsAsync(request);

        // Assert
        Assert.Contains(result.Obligations, o => 
            o.Type == "Airspace Auth" && 
            o.Description.Contains("Special authorization"));
    }

    [Fact]
    public async Task GetObligations_SpecialUseAirport_RequiresCoordination()
    {
        // Arrange
        var provider = new StaticComplianceProvider();
        var request = new ComplianceRequest
        {
            ARC = "b",
            AirspaceType = "Uncontrolled",
            OperationType = "VLOS",
            MaxAltitudeAGL = 120,
            SpecialUseAreas = new List<string> { "Airport" }
        };

        // Act
        var result = await provider.GetComplianceObligationsAsync(request);

        // Assert
        Assert.Contains(result.Obligations, o => 
            o.Type == "Airspace Auth" && 
            o.Description.Contains("Airport") && 
            o.Priority == "Mandatory");
    }
}
