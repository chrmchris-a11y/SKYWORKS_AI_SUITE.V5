using Xunit;
using Skyworks.Core.Models;
using Skyworks.Core.Services;

namespace Skyworks.Api.Tests;

public class MissionTemplatesConfigTests
{
    private readonly MissionTemplatesConfig _config = new();

    [Fact]
    public void GetAll_ShouldReturn48Templates()
    {
        // Act
        var all = _config.GetAll();

        // Assert
        Assert.Equal(48, all.Count);
    }

    [Fact]
    public void GetAll_AllTemplates_ShouldHaveValidMetadata()
    {
        // Act
        var all = _config.GetAll();

        // Assert
        foreach (var template in all)
        {
            Assert.NotEqual(default(MissionCategory), template.Category);
            Assert.NotEqual(default(MissionType), template.DefaultMissionType);
            Assert.True(
                template.DefaultSoraVersion == "2.0" || template.DefaultSoraVersion == "2.5",
                $"Template {template.Code} has invalid SORA version: {template.DefaultSoraVersion}"
            );
            Assert.Contains(template.DefaultEnvironment, new[] { "urban", "suburban", "rural" });
        }
    }

    [Theory]
    [InlineData(MissionTemplateCode.PhotovoltaicParkInspection, MissionCategory.EnergyAndUtilities, MissionType.Solar, "rural", true, false)]
    [InlineData(MissionTemplateCode.SolarPanelCleaning, MissionCategory.EnergyAndUtilities, MissionType.Roof, "suburban", false, true)]
    [InlineData(MissionTemplateCode.BridgeStructuralInspection, MissionCategory.InfrastructureAndTransport, MissionType.Facade, "suburban", false, true)]
    [InlineData(MissionTemplateCode.TopographicSurvey, MissionCategory.SurveyAndConstruction, MissionType.Agriculture, "rural", true, false)]
    [InlineData(MissionTemplateCode.TrainingFlightVLOS, MissionCategory.TrainingAndTest, MissionType.Agriculture, "rural", false, false)]
    public void Get_SpecificTemplates_ShouldReturnExpectedMetadata(
        MissionTemplateCode code,
        MissionCategory expectedCategory,
        MissionType expectedType,
        string expectedEnvironment,
        bool expectedIsLargeArea,
        bool expectedIsHighStructure)
    {
        // Act
        var metadata = _config.Get(code);

        // Assert
        Assert.Equal(code, metadata.Code);
        Assert.Equal(expectedCategory, metadata.Category);
        Assert.Equal(expectedType, metadata.DefaultMissionType);
        Assert.Equal(expectedEnvironment, metadata.DefaultEnvironment);
        Assert.Equal("2.5", metadata.DefaultSoraVersion);
        Assert.Equal(expectedIsLargeArea, metadata.IsLargeArea);
        Assert.Equal(expectedIsHighStructure, metadata.IsHighStructure);
    }

    [Fact]
    public void Get_InvalidCode_ShouldThrowKeyNotFoundException()
    {
        // Act & Assert
        var invalidCode = (MissionTemplateCode)9999;
        Assert.Throws<KeyNotFoundException>(() => _config.Get(invalidCode));
    }

    [Fact]
    public void Get_PowerlineCorridorInspection_ShouldBeLinearLargeArea()
    {
        // Act
        var metadata = _config.Get(MissionTemplateCode.PowerlineCorridorInspection);

        // Assert
        Assert.Equal(MissionCategory.EnergyAndUtilities, metadata.Category);
        Assert.Equal(MissionType.Linear, metadata.DefaultMissionType);
        Assert.True(metadata.IsLinear);
        Assert.True(metadata.IsLargeArea);
        Assert.False(metadata.IsHighStructure);
    }
}
