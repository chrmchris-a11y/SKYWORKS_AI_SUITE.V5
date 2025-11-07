/**
 * SKYWORKS SORA Validation Schemas (Zod)
 * Runtime validation for UI inputs before POST to backend
 * Rejects any value outside allowed enums
 */

// Note: This is a TypeScript-style schema file
// For pure JS runtime, we'll validate manually in app.js

export const SORA_25_SCHEMAS = {
  m1a: {
    allowed: ["None", "Low"],
    validate: (val) => {
      if (!["None", "Low"].includes(val)) {
        throw new Error(`Invalid M1A value: ${val}. Allowed: None, Low`);
      }
      return val;
    }
  },
  m1b: {
    allowed: ["None", "Medium", "High"],
    validate: (val) => {
      if (!["None", "Medium", "High"].includes(val)) {
        throw new Error(`Invalid M1B value: ${val}. Allowed: None, Medium, High`);
      }
      return val;
    }
  },
  m1c: {
    allowed: ["None", "Low"],
    validate: (val) => {
      if (!["None", "Low"].includes(val)) {
        throw new Error(`Invalid M1C value: ${val}. Allowed: None, Low`);
      }
      return val;
    }
  },
  m2: {
    allowed: ["None", "Low", "Medium", "High"],
    validate: (val) => {
      if (!["None", "Low", "Medium", "High"].includes(val)) {
        throw new Error(`Invalid M2 value: ${val}. Allowed: None, Low, Medium, High`);
      }
      return val;
    }
  }
};

export const SORA_20_SCHEMAS = {
  m1: {
    allowed: ["None", "Low", "Medium", "High"],
    validate: (val) => {
      if (!["None", "Low", "Medium", "High"].includes(val)) {
        throw new Error(`Invalid M1 value: ${val}. Allowed: None, Low, Medium, High`);
      }
      return val;
    }
  },
  m2: {
    allowed: ["None", "Low", "High"], // NO MEDIUM!
    validate: (val) => {
      if (!["None", "Low", "High"].includes(val)) {
        throw new Error(`Invalid M2 value: ${val}. Allowed: None, Low, High (NO MEDIUM in SORA 2.0)`);
      }
      return val;
    }
  },
  m3: {
    allowed: ["None", "Adequate", "Validated"],
    validate: (val) => {
      if (!["None", "Adequate", "Validated"].includes(val)) {
        throw new Error(`Invalid M3 value: ${val}. Allowed: None, Adequate, Validated`);
      }
      return val;
    }
  }
};

export const COMMON_SCHEMAS = {
  airspaceClass: {
    allowed: ["A", "B", "C", "D", "E", "F", "G"],
    validate: (val) => {
      if (!["A", "B", "C", "D", "E", "F", "G"].includes(val)) {
        throw new Error(`Invalid Airspace Class: ${val}`);
      }
      return val;
    }
  },
  specialZones: {
    allowed: ["CTR", "TMA", "ATZ", "RMZ", "TMZ", "Prohibited", "Restricted", "Danger", "TSA", "TRA", "CBA", "UAS_Geo_Zone"],
    validate: (vals) => {
      const allowed = ["CTR", "TMA", "ATZ", "RMZ", "TMZ", "Prohibited", "Restricted", "Danger", "TSA", "TRA", "CBA", "UAS_Geo_Zone"];
      for (const v of vals) {
        if (!allowed.includes(v)) {
          throw new Error(`Invalid Special Zone: ${v}. EU airspace only!`);
        }
      }
      return vals;
    }
  },
  typicality: {
    allowed: ["Typical", "Atypical"],
    validate: (val) => {
      if (!["Typical", "Atypical"].includes(val)) {
        throw new Error(`Invalid Typicality: ${val}`);
      }
      return val;
    }
  },
  trafficDensitySource: {
    allowed: ["Empirical", "Modelled", "ANSP"],
    validate: (val) => {
      if (!["Empirical", "Modelled", "ANSP"].includes(val)) {
        throw new Error(`Invalid Traffic Density Source: ${val}`);
      }
      return val;
    }
  },
  airspaceContainment: {
    allowed: ["None", "Horizontal", "Vertical", "Horizontal_and_Vertical"],
    validate: (val) => {
      if (!["None", "Horizontal", "Vertical", "Horizontal_and_Vertical"].includes(val)) {
        throw new Error(`Invalid Airspace Containment: ${val}`);
      }
      return val;
    }
  },
  aec: {
    allowed: ["AEC_1", "AEC_2", "AEC_3", "AEC_4", "AEC_5", "AEC_6", "AEC_7", "AEC_8", "AEC_9", "AEC_10", "AEC_11", "AEC_12"],
    validate: (val) => {
      if (!["AEC_1", "AEC_2", "AEC_3", "AEC_4", "AEC_5", "AEC_6", "AEC_7", "AEC_8", "AEC_9", "AEC_10", "AEC_11", "AEC_12"].includes(val)) {
        throw new Error(`Invalid AEC: ${val}`);
      }
      return val;
    }
  }
};

/**
 * Validate full SORA request payload
 */
export function validateSoraRequest(payload) {
  const errors = [];

  try {
    // Common fields
    COMMON_SCHEMAS.airspaceClass.validate(payload.common.airspaceClass);
    COMMON_SCHEMAS.typicality.validate(payload.common.typicality);
    COMMON_SCHEMAS.trafficDensitySource.validate(payload.common.trafficDensitySource);
    COMMON_SCHEMAS.airspaceContainment.validate(payload.common.airspaceContainment);
    COMMON_SCHEMAS.aec.validate(payload.common.aec);
    COMMON_SCHEMAS.specialZones.validate(payload.common.specialZones);

    // GRC fields (version-specific)
    if (payload.soraVersion === "2.5") {
      SORA_25_SCHEMAS.m1a.validate(payload.grc.m1a);
      SORA_25_SCHEMAS.m1b.validate(payload.grc.m1b);
      SORA_25_SCHEMAS.m1c.validate(payload.grc.m1c);
      SORA_25_SCHEMAS.m2.validate(payload.grc.m2);
    } else if (payload.soraVersion === "2.0") {
      SORA_20_SCHEMAS.m1.validate(payload.grc.m1);
      SORA_20_SCHEMAS.m2.validate(payload.grc.m2);
      SORA_20_SCHEMAS.m3.validate(payload.grc.m3);
    }
  } catch (err) {
    errors.push(err.message);
  }

  return {
    valid: errors.length === 0,
    errors
  };
}
