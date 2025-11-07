// index.ts
// Skyworks SORA Compliance Library - Main Entry Point
// ✅ 100% EASA/JARUS Compliant Implementation

// Core Enums & Types
export * from './skyworks.enums';

// AEC Labels & Mappings
export * from './aec.labels';

// Calculators
export * from './calculators/grc20';
export * from './calculators/grc25';
export {
  calculateARC,
  getInitialARC,
  applyStrategicMitigations,
  validateStrategicMitigations,
  getRecommendedMitigations,
  type ARCInput,
  type ARCResult,
  type StrategicMitigations,
} from './calculators/arc';

export {
  calculateSAIL,
  getRequiredOSOs,
  validateSAILAchievability,
  type SAILInput,
  type SAILResult,
  type OSORequirement,
} from './calculators/sail';

// Mappings
export {
  SAIL_MATRIX,
  getSAIL,
  determineSAIL,
  isWithinSORAScope,
  getSAILDescription,
  type SAIL,
} from './mappings/sail.matrix';

export {
  TMPR_TARGETS,
  getTMPRTarget,
  getTMPRLevel,
  getSystemRiskRatioTarget,
  validateTMPR,
  getDAACapabilities,
} from './mappings/tmpr.targets';

// Re-export ARC type from sail.matrix to avoid duplication
export type { ARC } from './mappings/sail.matrix';

/**
 * Skyworks SORA Compliance Library
 * 
 * This library provides 100% EASA/JARUS compliant implementations of:
 * - SORA 2.0 (AMC to Article 11)
 * - SORA 2.5 (JARUS Main Body 2024-05-13)
 * 
 * Key Features:
 * ✅ AEC (Airspace Encounter Categories) 1-12 from Annex C
 * ✅ GRC calculations (Table 2 + Table 3/5) with mitigations
 * ✅ ARC determination (Initial → Residual via strategic mitigations)
 * ✅ SAIL matrix (Final GRC × Residual ARC → SAIL I-VI)
 * ✅ TMPR targets (per Annex D)
 * ✅ OSO requirements (per SAIL level)
 * 
 * All calculations reference official JARUS/EASA documents:
 * - JAR_doc_25 (SORA v2.5 Main Body)
 * - JAR_doc_27 (SORA v2.5 Annex B)
 * - Annex C v1.0 (AEC → ARC)
 * - Annex D v1.0 (TMPR)
 * - AMC1 Article 11 (SORA v2.0)
 * 
 * Backend Alignment:
 * All frontend calculations match Backend/src/Skyworks.Core/Services/GRC/GRCCalculationService.cs
 */
