using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Skyworks.Api.Tests;

/// <summary>
/// Task #7 - GET /sora/specifications Integration Tests
/// 
/// Purpose: Verify API returns EASA-compliant specifications for UI dropdowns
/// 
/// Test Coverage:
/// 1. SORA 2.5 specifications match EASA Annex B
/// 2. SORA 2.0 specifications match AMC1 Article 11
/// 3. Invalid version returns 400 Bad Request
/// </summary>
public class SpecificationsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SpecificationsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ========================================================================
    // Test 1: SORA 2.5 Specifications (EASA Annex B Compliance)
    // ========================================================================
    [Fact]
    public async Task GetSpecifications_SORA25_ReturnsEASACompliantOptions()
    {
        // Arrange
        var version = "2.5";

        // Act
        var response = await _client.GetAsync($"/api/v1/Sora/specifications?version={version}");
        response.EnsureSuccessStatusCode();
        var specs = await response.Content.ReadFromJsonAsync<SpecificationsResponse>();

        // Assert - Version
        Assert.NotNull(specs);
        Assert.Equal("SORA 2.5", specs.Version);

        // Assert - M1A options (Annex B p.8 - ONLY None + Low)
        Assert.Equal(new[] { "None", "Low" }, specs.M1aOptions);
        Assert.DoesNotContain("Medium", specs.M1aOptions!); // Medium does NOT exist
        Assert.DoesNotContain("High", specs.M1aOptions!);   // High does NOT exist

        // Assert - M1B options (Annex B p.14 - None + Medium + High)
        Assert.Equal(new[] { "None", "Medium", "High" }, specs.M1bOptions);
        Assert.DoesNotContain("Low", specs.M1bOptions!); // Low does NOT exist

        // Assert - M1C options (Annex B p.19 - ONLY None + Low)
        Assert.Equal(new[] { "None", "Low" }, specs.M1cOptions);
        Assert.DoesNotContain("Medium", specs.M1cOptions!); // Medium does NOT exist
        Assert.DoesNotContain("High", specs.M1cOptions!);   // High does NOT exist

        // Assert - M2 options (Annex B p.25 - All levels)
        Assert.Equal(new[] { "None", "Low", "Medium", "High" }, specs.M2Options);

        // Assert - Population Density options (7 total)
        Assert.Equal(7, specs.PopulationDensityOptions?.Length);
        Assert.Contains("Controlled", specs.PopulationDensityOptions!);
        Assert.Contains(">50000", specs.PopulationDensityOptions!);

        // Assert - Constraints (3 total)
        Assert.Equal(3, specs.Constraints?.Length);
        Assert.Contains(specs.Constraints!, c => c.Contains("M1(A) Low CANNOT combine with M1(B)"));

        // Assert - Reference
        Assert.Contains("SORA-MB-2.5", specs.Reference);
        Assert.Contains("13.05.2024", specs.Reference);
    }

    // ========================================================================
    // Test 2: SORA 2.0 Specifications (AMC1 Article 11 Compliance)
    // ========================================================================
    [Fact]
    public async Task GetSpecifications_SORA20_ReturnsAMC1CompliantOptions()
    {
        // Arrange
        var version = "2.0";

        // Act
        var response = await _client.GetAsync($"/api/v1/Sora/specifications?version={version}");
        response.EnsureSuccessStatusCode();
        var specs = await response.Content.ReadFromJsonAsync<SpecificationsResponse>();

        // Assert - Version
        Assert.NotNull(specs);
        Assert.Equal("SORA 2.0", specs.Version);

        // Assert - M1 options (AMC1 Table 3 - all levels)
        Assert.Equal(new[] { "None", "Low", "Medium", "High" }, specs.M1Options);

        // Assert - M2 options (AMC1 Table 4 - NO Medium)
        Assert.Equal(new[] { "None", "Low", "High" }, specs.M2Options);
        Assert.DoesNotContain("Medium", specs.M2Options!); // Medium does NOT exist in SORA 2.0

        // Assert - M3 options (AMC1 §3.5 - ERP levels)
        Assert.Equal(new[] { "None", "Adequate", "Validated" }, specs.M3Options);

        // Assert - Operation Scenarios (8 total: 4 VLOS + 4 BVLOS)
        Assert.Equal(8, specs.OperationScenarioOptions?.Length);
        Assert.Contains("VLOS_Controlled", specs.OperationScenarioOptions!);
        Assert.Contains("BVLOS_Gathering", specs.OperationScenarioOptions!);

        // Assert - Constraints (4 total)
        Assert.Equal(4, specs.Constraints?.Length);
        Assert.Contains(specs.Constraints!, c => c.Contains("EVLOS is treated as BVLOS"));
        Assert.Contains(specs.Constraints!, c => c.Contains("M3 None has penalty (+1 to final GRC)"));

        // Assert - Reference
        Assert.Contains("JAR-DEL-WG6-D.04", specs.Reference);
        Assert.Contains("30.01.2019", specs.Reference);
    }

    // ========================================================================
    // Test 3: Invalid Version Returns 400 Bad Request
    // ========================================================================
    [Fact]
    public async Task GetSpecifications_InvalidVersion_Returns400()
    {
        // Arrange
        var invalidVersion = "3.0";

        // Act
        var response = await _client.GetAsync($"/api/v1/Sora/specifications?version={invalidVersion}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid version", error);
    }

    // ========================================================================
    // Helper DTO (matches SoraController response)
    // ========================================================================
    private class SpecificationsResponse
    {
        public string? Version { get; set; }
        public string[]? PopulationDensityOptions { get; set; }
        public string[]? M1aOptions { get; set; }
        public string[]? M1bOptions { get; set; }
        public string[]? M1cOptions { get; set; }
        public string[]? M2Options { get; set; }
        public string[]? OperationScenarioOptions { get; set; }
        public string[]? M1Options { get; set; }
        public string[]? M3Options { get; set; }
        public string[]? Constraints { get; set; }
        public string? Reference { get; set; }
    }
}
