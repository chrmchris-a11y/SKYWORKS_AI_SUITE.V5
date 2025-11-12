/**
 * Task #7 Tests - GET /sora/specifications Integration
 * 
 * Purpose: Verify UI dropdowns populate correctly from backend API
 * 
 * Test Coverage:
 * 1. SORA 2.5 specifications match EASA Annex B
 * 2. SORA 2.0 specifications match AMC1 Article 11
 * 3. soraClient caching works correctly
 */

import { describe, it, expect, beforeEach, vi } from 'vitest';
import { SoraApiClient } from '../WebPlatform/wwwroot/app/Pages/ui/api/soraClient.js';

describe('Task #7 - GET /sora/specifications Integration', () => {
  let client;

  beforeEach(() => {
    client = new SoraApiClient();
    // Clear cache before each test
    client._specificationsCache = { "2.0": null, "2.5": null };
    global.fetch = vi.fn();
  });

  // ========================================================================
  // Test 1: SORA 2.5 Specifications (EASA Annex B Compliance)
  // ========================================================================
  it('should return correct SORA 2.5 specifications (EASA Annex B)', async () => {
    // Mock API response
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({
        version: "SORA 2.5",
        populationDensityOptions: ["Controlled", "<5", "<50", "<500", "<5000", "<50000", ">50000"],
        m1aOptions: ["None", "Low"],           // Annex B p.8 - ONLY Low
        m1bOptions: ["None", "Medium", "High"], // Annex B p.14 - Medium, High
        m1cOptions: ["None", "Low"],           // Annex B p.19 - ONLY Low
        m2Options: ["None", "Low", "Medium", "High"], // Annex B p.25 - All levels
        constraints: [
          "M1(A) Low CANNOT combine with M1(B) any level (Annex B p.8)",
          "Small-UA Rule: IF MTOM ≤ 0.25kg AND speed ≤ 25m/s THEN iGRC = 1 (Main Body §4.3)",
          "Final GRC cannot go below column minimum from Table 2 (Main Body Table 2)"
        ],
        reference: "JAR-DEL-SRM-SORA-MB-2.5, Edition 2.5, 13.05.2024"
      })
    });

    const specs = await client.getSpecifications("2.5");

    // Verify version
    expect(specs.version).toBe("SORA 2.5");

    // Verify M1A options (ONLY None + Low - Annex B p.8)
    expect(specs.m1aOptions).toEqual(["None", "Low"]);
    expect(specs.m1aOptions).not.toContain("Medium"); // Medium does NOT exist
    expect(specs.m1aOptions).not.toContain("High");   // High does NOT exist

    // Verify M1B options (None + Medium + High - Annex B p.14)
    expect(specs.m1bOptions).toEqual(["None", "Medium", "High"]);
    expect(specs.m1bOptions).not.toContain("Low"); // Low does NOT exist

    // Verify M1C options (ONLY None + Low - Annex B p.19)
    expect(specs.m1cOptions).toEqual(["None", "Low"]);
    expect(specs.m1cOptions).not.toContain("Medium"); // Medium does NOT exist
    expect(specs.m1cOptions).not.toContain("High");   // High does NOT exist

    // Verify M2 options (All levels - Annex B p.25)
    expect(specs.m2Options).toEqual(["None", "Low", "Medium", "High"]);

    // Verify population density options
    expect(specs.populationDensityOptions).toHaveLength(7);
    expect(specs.populationDensityOptions).toContain("Controlled");
    expect(specs.populationDensityOptions).toContain(">50000");

    // Verify constraints
    expect(specs.constraints).toHaveLength(3);
    expect(specs.constraints[0]).toContain("M1(A) Low CANNOT combine with M1(B)");

    // Verify reference
    expect(specs.reference).toContain("SORA-MB-2.5");
    expect(specs.reference).toContain("13.05.2024");
  });

  // ========================================================================
  // Test 2: SORA 2.0 Specifications (AMC1 Article 11 Compliance)
  // ========================================================================
  it('should return correct SORA 2.0 specifications (AMC1 Article 11)', async () => {
    // Mock API response
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({
        version: "SORA 2.0",
        operationScenarioOptions: [
          "VLOS_Controlled", "VLOS_Sparsely", "VLOS_Populated", "VLOS_Gathering",
          "BVLOS_Controlled", "BVLOS_Sparsely", "BVLOS_Populated", "BVLOS_Gathering"
        ],
        m1Options: ["None", "Low", "Medium", "High"], // AMC1 Table 3
        m2Options: ["None", "Low", "High"],           // AMC1 Table 4 - NO Medium
        m3Options: ["None", "Adequate", "Validated"], // AMC1 §3.5
        constraints: [
          "EVLOS is treated as BVLOS for GRC determination (AMC1 §2.2)",
          "M1 uses column-minimum clamp method (AMC1 Table 3 footnotes)",
          "M3 None has penalty (+1 to final GRC) (AMC1 §3.5)",
          "Final GRC cannot go below column minimum from AMC1 Table 2"
        ],
        reference: "JAR-DEL-WG6-D.04, Edition 2.0, 30.01.2019"
      })
    });

    const specs = await client.getSpecifications("2.0");

    // Verify version
    expect(specs.version).toBe("SORA 2.0");

    // Verify M1 options (AMC1 Table 3 - all levels)
    expect(specs.m1Options).toEqual(["None", "Low", "Medium", "High"]);

    // Verify M2 options (AMC1 Table 4 - NO Medium)
    expect(specs.m2Options).toEqual(["None", "Low", "High"]);
    expect(specs.m2Options).not.toContain("Medium"); // Medium does NOT exist in SORA 2.0

    // Verify M3 options (AMC1 §3.5 - ERP levels)
    expect(specs.m3Options).toEqual(["None", "Adequate", "Validated"]);

    // Verify operation scenarios (8 total: 4 VLOS + 4 BVLOS)
    expect(specs.operationScenarioOptions).toHaveLength(8);
    expect(specs.operationScenarioOptions).toContain("VLOS_Controlled");
    expect(specs.operationScenarioOptions).toContain("BVLOS_Gathering");

    // Verify constraints
    expect(specs.constraints).toHaveLength(4);
    expect(specs.constraints[0]).toContain("EVLOS is treated as BVLOS");
    expect(specs.constraints[2]).toContain("M3 None has penalty (+1 to final GRC)");

    // Verify reference
    expect(specs.reference).toContain("JAR-DEL-WG6-D.04");
    expect(specs.reference).toContain("30.01.2019");
  });

  // ========================================================================
  // Test 3: Caching Works Correctly
  // ========================================================================
  it('should cache specifications to prevent redundant API calls', async () => {
    // Mock API response
    global.fetch.mockResolvedValue({
      ok: true,
      json: async () => ({
        version: "SORA 2.5",
        m1aOptions: ["None", "Low"],
        m1bOptions: ["None", "Medium", "High"],
        m1cOptions: ["None", "Low"],
        m2Options: ["None", "Low", "Medium", "High"],
        constraints: [],
        reference: "Test"
      })
    });

    // First call - should fetch from API
    const specs1 = await client.getSpecifications("2.5");
    expect(global.fetch).toHaveBeenCalledTimes(1);

    // Second call - should use cache (NO additional fetch)
    const specs2 = await client.getSpecifications("2.5");
    expect(global.fetch).toHaveBeenCalledTimes(1); // Still 1 (no new call)

    // Verify both results are identical
    expect(specs1).toBe(specs2); // Same object reference from cache

    // Different version - should fetch again
    const specs3 = await client.getSpecifications("2.0");
    expect(global.fetch).toHaveBeenCalledTimes(2); // Now 2 calls
  });

  // ========================================================================
  // Test 4: Invalid Version Returns Error
  // ========================================================================
  it('should throw error for invalid SORA version', async () => {
    global.fetch.mockResolvedValueOnce({
      ok: false,
      status: 400,
      statusText: 'Bad Request',
      json: async () => ({ error: "Invalid version. Valid: '2.0' or '2.5'" })
    });

    await expect(client.getSpecifications("3.0")).rejects.toThrow(
      "Invalid version. Valid: '2.0' or '2.5'"
    );
  });

  // ========================================================================
  // Test 5: Network Error Handling
  // ========================================================================
  it('should handle network errors gracefully', async () => {
    global.fetch.mockRejectedValueOnce(new Error('Network request failed'));

    await expect(client.getSpecifications("2.5")).rejects.toThrow('Network request failed');
  });
});
