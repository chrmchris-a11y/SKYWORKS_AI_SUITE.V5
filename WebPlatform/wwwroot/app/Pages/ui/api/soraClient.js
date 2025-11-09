/**
 * SORA API Client - HTTP client for SORA calculations
 * 
 * Purpose: Single source-of-truth for SORA calculations via .NET backend
 * Replaces ad-hoc browser-side calculations with validated backend API
 * 
 * References:
 * - Backend: Skyworks.Api/Controllers/SoraController.cs
 * - Endpoint: POST /api/v1/sora/calculate
 * - Tests: 80/80 PASSING (70 SORA Calculator + 10 GIS Integration)
 * 
 * Type Definitions (JSDoc):
 * 
 * @typedef {Object} DroneSpecs
 * @property {number} mtom_kg - Maximum Take-Off Mass (kg)
 * @property {number} maxSpeed_ms - Maximum speed (m/s)
 * @property {number} characteristicDimension_m - Characteristic dimension (m)
 * 
 * @typedef {Object} SoraCalculationRequest
 * @property {"2.0"|"2.5"} soraVersion - SORA version
 * @property {DroneSpecs} drone - Drone specifications
 * @property {string} [populationDensity] - SORA 2.5: Population density category
 * @property {string} [m1a] - SORA 2.5: M1(A) Sheltering mitigation
 * @property {string} [m1b] - SORA 2.5: M1(B) Operational Restrictions
 * @property {string} [m1c] - SORA 2.5: M1(C) Ground Observation
 * @property {string} [m2] - SORA 2.5: M2 Impact Dynamics
 * @property {string} [operationScenario] - SORA 2.0: Operation scenario
 * @property {string} [m1] - SORA 2.0: M1 Strategic mitigation
 * @property {string} [m2_20] - SORA 2.0: M2 Impact Dynamics
 * @property {string} [m3] - SORA 2.0: M3 ERP
 * @property {number} [altitude_ft] - Altitude (feet)
 * @property {boolean} [controlledAirspace] - Operating in controlled airspace
 * @property {boolean} [airportEnvironment] - Near airport
 * @property {boolean} [populatedArea] - Over populated area
 * @property {boolean} [atypicalAirspace] - Atypical airspace
 * @property {boolean} [isVLOS] - Visual Line of Sight
 * 
 * @typedef {Object} SoraCalculationResponse
 * @property {number} initialGRC - Initial Ground Risk Class (1-7+)
 * @property {number} finalGRC - Final GRC after mitigations
 * @property {number} aec - Aircraft Encounter Category (1-12)
 * @property {string} initialARC - Initial Air Risk Class (ARC-a/b/c/d)
 * @property {string} residualARC - Residual ARC after mitigations
 * @property {number} [tmpr] - TMPR target (SORA 2.5 BVLOS only)
 * @property {string} sail - SAIL level (I/II/III/IV/V/VI/Category C)
 * @property {string[]} warnings - Warnings generated during calculation
 * @property {string[]} [errors] - Errors if calculation failed
 * @property {Object} [calculationSteps] - Optional detailed breakdown
 * 
 * @typedef {Object} SoraValidationRequest
 * @property {"2.0"|"2.5"} soraVersion - SORA version
 * @property {DroneSpecs} drone - Drone specifications
 * @property {string} [populationDensity] - Population density
 * @property {string} [m1a] - M1(A) mitigation
 * @property {string} [m1b] - M1(B) mitigation
 * @property {string} [m1c] - M1(C) mitigation
 * @property {string} [m2] - M2 mitigation
 * @property {string} [operationScenario] - Operation scenario
 * @property {string} [m1] - M1 mitigation (2.0)
 * @property {string} [m3] - M3 mitigation (2.0)
 * 
 * @typedef {Object} SoraValidationResponse
 * @property {boolean} valid - Whether parameters are valid
 * @property {string[]} [errors] - Validation errors
 * @property {string} [message] - Validation message
 * 
 * @typedef {Object} SoraSpecificationsResponse
 * @property {string} version - SORA version
 * @property {string[]} [populationDensityOptions] - Available population density options
 * @property {string[]} [operationScenarioOptions] - Available operation scenarios
 * @property {string[]} [m1aOptions] - M1(A) mitigation options
 * @property {string[]} [m1bOptions] - M1(B) mitigation options
 * @property {string[]} [m1cOptions] - M1(C) mitigation options
 * @property {string[]} [m2Options] - M2 mitigation options
 * @property {string[]} [m1Options] - M1 mitigation options (2.0)
 * @property {string[]} [m3Options] - M3 mitigation options (2.0)
 * @property {string[]} constraints - Validation constraints
 * @property {string} reference - Reference documentation
 */

// =============================================================================
// API Client
// =============================================================================

export class SoraApiClient {
  /**
   * @param {string} baseUrl - Base URL for SORA API
   */
  constructor(baseUrl = "/api/v1/sora") {
    this.baseUrl = baseUrl;
  }
  
  /**
   * Calculate SORA assessment (Ground Risk, Air Risk, SAIL)
   * 
   * @param {SoraCalculationRequest} request - SORA calculation parameters
   * @returns {Promise<SoraCalculationResponse>} Complete SORA assessment
   * @throws {Error} If HTTP request fails or backend returns error
   * 
   * @example
   * const client = new SoraApiClient();
   * const result = await client.calculate({
   *   soraVersion: "2.5",
   *   drone: {
   *     mtom_kg: 0.249,
   *     maxSpeed_ms: 16,
   *     characteristicDimension_m: 0.213
   *   },
   *   populationDensity: "<500",
   *   m1a: "Low",
   *   m2: "Medium",
   *   altitude_ft: 400,
   *   isVLOS: true
   * });
   * 
   * console.log(`SAIL: ${result.sail}, Final GRC: ${result.finalGRC}`);
   */
  async calculate(request) {
    const response = await fetch(`${this.baseUrl}/calculate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify(request)
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(
        `SORA API error (${response.status}): ${errorData.error || response.statusText}`
      );
    }
    
    return response.json();
  }
  
  /**
   * Validate SORA parameters without calculating
   * 
   * @param {SoraValidationRequest} request - Parameters to validate
   * @returns {Promise<SoraValidationResponse>} Validation result with errors if invalid
   * 
   * @example
   * const client = new SoraApiClient();
   * const validation = await client.validate({
   *   soraVersion: "2.5",
   *   drone: { mtom_kg: 0.249, maxSpeed_ms: 16, characteristicDimension_m: 0.213 },
   *   m1a: "Medium",
   *   m1b: "High" // ERROR: M1A Medium cannot combine with M1B
   * });
   * 
   * if (!validation.valid) {
   *   console.error("Validation errors:", validation.errors);
   * }
   */
  async validate(request) {
    const response = await fetch(`${this.baseUrl}/validate`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify(request)
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(
        `SORA validation error (${response.status}): ${errorData.error || response.statusText}`
      );
    }
    
    return response.json();
  }
  
  /**
   * Get SORA field specifications (dropdown options, validation rules)
   * 
   * @param {string} version - SORA version ("2.0" or "2.5")
   * @returns {Promise<SoraSpecificationsResponse>} Available options and constraints
   * 
   * @example
   * const client = new SoraApiClient();
   * const specs = await client.getSpecifications("2.5");
   * 
   * console.log("M1A options:", specs.m1aOptions); // ["None", "Low", "Medium"]
   * console.log("Constraints:", specs.constraints);
   */
  async getSpecifications(version = "2.5") {
    const response = await fetch(`${this.baseUrl}/specifications?version=${version}`, {
      method: "GET",
      headers: {
        "Accept": "application/json"
      }
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(
        `SORA specifications error (${response.status}): ${errorData.error || response.statusText}`
      );
    }
    
    return response.json();
  }
}

// =============================================================================
// Singleton Instance (for convenience)
// =============================================================================

/**
 * Default SORA API client instance
 * 
 * Usage:
 * import { soraApi } from './soraClient.js';
 * 
 * const result = await soraApi.calculate({ ... });
 */
export const soraApi = new SoraApiClient();

// =============================================================================
// Utility Functions
// =============================================================================

/**
 * Build SORA 2.5 request from mission parameters
 * 
 * @param {Object} params - Mission parameters
 * @param {DroneSpecs} params.drone - Drone specifications
 * @param {string} params.populationDensity - Population density
 * @param {string} [params.m1a] - M1(A) mitigation
 * @param {string} [params.m1b] - M1(B) mitigation
 * @param {string} [params.m1c] - M1(C) mitigation
 * @param {string} [params.m2] - M2 mitigation
 * @param {number} [params.altitude_ft] - Altitude (feet)
 * @param {boolean} [params.controlledAirspace] - Controlled airspace
 * @param {boolean} [params.airportEnvironment] - Airport environment
 * @param {boolean} [params.populatedArea] - Populated area
 * @param {boolean} [params.atypicalAirspace] - Atypical airspace
 * @param {boolean} [params.isVLOS] - VLOS operation
 * @returns {SoraCalculationRequest} SORA 2.5 calculation request
 */
export function buildSora25Request(params) {
  return {
    soraVersion: "2.5",
    ...params
  };
}

/**
 * Build SORA 2.0 request from mission parameters
 * 
 * @param {Object} params - Mission parameters
 * @param {DroneSpecs} params.drone - Drone specifications
 * @param {string} params.operationScenario - Operation scenario
 * @param {string} [params.m1] - M1 mitigation
 * @param {string} [params.m2_20] - M2 mitigation
 * @param {string} [params.m3] - M3 mitigation
 * @param {number} [params.altitude_ft] - Altitude (feet)
 * @param {boolean} [params.controlledAirspace] - Controlled airspace
 * @param {boolean} [params.airportEnvironment] - Airport environment
 * @param {boolean} [params.populatedArea] - Populated area
 * @param {boolean} [params.atypicalAirspace] - Atypical airspace
 * @param {boolean} [params.isVLOS] - VLOS operation
 * @returns {SoraCalculationRequest} SORA 2.0 calculation request
 */
export function buildSora20Request(params) {
  return {
    soraVersion: "2.0",
    ...params
  };
}

/**
 * Format SAIL for display
 * 
 * @param {string} sail - SAIL level (I/II/III/IV/V/VI/Category C)
 * @returns {string} Formatted SAIL string
 */
export function formatSAIL(sail) {
  if (sail === "Category C") return "Cat C";
  return `SAIL ${sail}`;
}

/**
 * Get SAIL severity level (for color coding)
 * 
 * @param {string} sail - SAIL level (I/II/III/IV/V/VI/Category C)
 * @returns {string} Severity level ("low"/"medium"/"high"/"critical")
 */
export function getSAILSeverity(sail) {
  switch (sail) {
    case "I":
    case "II":
      return "low";
    case "III":
    case "IV":
      return "medium";
    case "V":
    case "VI":
      return "high";
    case "Category C":
      return "critical";
    default:
      return "medium";
  }
}
