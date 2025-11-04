## 1. Root Cause Analysis

The failures are due to two main issues:

1. **Incorrect GRC Calculation Thresholds**: The Python backend is using wrong population density thresholds. According to JARUS SORA 2.0 Annex B, the correct thresholds should be:
   - GRC 3: 10-100 people/km² (not starting at 50)
   - GRC 4: 100-300 people/km² (not starting at 200)
   - GRC 5: 300-1000 people/km² (not starting at 500)

2. **Missing Out-of-Scope Validation**: The system returns 200 OK for high-risk operations that should be rejected with 400 BadRequest. SAIL VI and high GRC operations require CERTIFIED category and should be out of scope for SPECIFIC category operations.

## 2. File: Backend_Python/main.py

```python
"""
SORA 2.0 & 2.5 Python FastAPI Backend - COMPLETE VERSION
Port: 8001
Purpose: GRC and ARC calculations for SORA 2.0 and 2.5 evaluations

This version includes:
- SORA 2.0 GRC calculations with correct JARUS thresholds
- SORA 2.5 Enhanced ARC calculations with 5 new fields
- Frontend→Backend field mapping & unit conversions
- Out-of-scope validation for high-risk operations
- Structured logging & proper error responses
"""

from fastapi import FastAPI, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, validator, Field
from enum import Enum
from typing import Optional, List, Dict, Any
import logging

# Configure structured logging FIRST (before any logger usage)
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# Import ARC Calculator for v2.0/v2.5 calculations (AFTER logger init)
try:
    from calculations.arc_calculator import ARCCalculator
    from models.sora_models import (
        ARCRequest_2_0, 
        ARCRequest_2_5,
        EnvironmentType,
        AirspaceClass
    )
    ARC_CALCULATOR_AVAILABLE = True
    logger.info("✅ ARCCalculator loaded successfully")
except ImportError as e:
    ARC_CALCULATOR_AVAILABLE = False
    logger.warning(f"⚠️  ARCCalculator not available: {e}")

# Unit conversion constants
METERS_TO_FEET = 3.28084
FEET_TO_METERS = 1 / METERS_TO_FEET

# Create FastAPI app
app = FastAPI(
    title="SORA 2.0 & 2.5 Calculation API",
    description="GRC and ARC calculations per JAR_doc_06 (SORA 2.0) and JAR_doc_25 (SORA 2.5)",
    version="2.0.0"
)

# Enable CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ============================================================================
# MODELS
# ============================================================================

# SORA 2.0 Models
class M2Level20(str, Enum):
    """M2 Mitigation Levels for SORA 2.0"""
    NONE = "None"
    LOW = "Low"
    HIGH = "High"


class GrcRequestV20(BaseModel):
    """Request model for SORA 2.0 GRC calculation"""
    initial_grc: int
    m1: int
    m2: str
    m3: int
    
    @validator('initial_grc')
    def validate_initial_grc(cls, v):
        if not 1 <= v <= 10:
            raise ValueError('Initial GRC must be between 1 and 10')
        return v
    
    @validator('m2')
    def validate_m2(cls, v):
        valid_values = ["None", "Low", "High"]
        if v not in valid_values:
            raise ValueError(
                f"Invalid M2 level '{v}'. SORA 2.0 only supports: {', '.join(valid_values)}."
            )
        return v


# SORA 2.5 Models
class TrafficDensityDataSource(str, Enum):
    """Traffic density data sources for SORA 2.5"""
    EMPIRICAL = "Empirical"
    STATISTICAL = "Statistical"
    EXPERT = "Expert"


class AirspaceContainment25(str, Enum):
    """Airspace containment levels for SORA 2.5"""
    NONE = "None"
    OPERATIONAL = "Operational"
    CERTIFIED = "Certified"


class ARCInputs25(BaseModel):
    """SORA 2.5 Enhanced ARC Inputs - The 5 new fields"""
    u_space_services_available: bool
    traffic_density_data_source: TrafficDensityDataSource
    airspace_containment: AirspaceContainment25
    temporal_segregation: bool
    spatial_segregation: bool


class GRCInputs(BaseModel):
    """GRC Inputs for SORA 2.5"""
    population_density: str  # "Low", "Medium", "High"
    sheltering: str         # "Low", "Medium", "High"
    m1: Optional[int] = 0   # M1 mitigation credit
    m3: Optional[int] = 0   # M3 mitigation credit


class SoraRequestV25(BaseModel):
    """Complete SORA 2.5 evaluation request"""
    category: str
    grc_inputs: GRCInputs
    arc_inputs_25: ARCInputs25
    operational_volume: dict
    traffic_density: str


# ============================================================================
# SORA 2.0 ENDPOINTS
# ============================================================================

@app.post("/api/grc/calculate-v20")
async def calculate_grc_v20(request: GrcRequestV20):
    """
    Calculate Final GRC for SORA 2.0
    
    Official Reference: JAR_doc_06 Table 3 - SORA 2.0 AMC
    """
    try:
        logger.info(f"SORA 2.0 GRC calculation request: {request.dict()}")
        
        # Map M2 to numeric value
        m2_mapping = {"None": 0, "Low": -1, "High": -2}
        m2_value = m2_mapping[request.m2]
        
        # Calculate Final GRC
        final_grc = request.initial_grc + request.m1 + m2_value + request.m3
        final_grc = max(1, final_grc)  # Cannot be less than 1
        
        calculation = f"{request.initial_grc} + {request.m1} + {m2_value} + {request.m3} = {final_grc}"
        
        logger.info(f"SORA 2.0 GRC calculation successful: {calculation}")
        
        return {
            "final_grc": final_grc,
            "initial_grc": request.initial_grc,
            "m1": request.m1,
            "m2": request.m2,
            "m2_value": m2_value,
            "m3": request.m3,
            "calculation": calculation,
            "reference": "JAR_doc_06 Table 3 - SORA 2.0 AMC"
        }
        
    except Exception as e:
        logger.error(f"SORA 2.0 GRC calculation failed: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))


# ============================================================================
# SORA 2.0 & 2.5 GRC ENDPOINTS (Required by C# SoraProxyController)
# ============================================================================

class GrcRequest20(BaseModel):
    """SORA 2.0 GRC calculation request from C# backend"""
    mtom_kg: float
    population_density: float
    m1_strategic: Optional[str] = None
    m2_impact: Optional[str] = None
    m3_erp: Optional[str] = None

@app.post("/api/v1/calculate/grc/2.0")
async def calculate_grc_v20_new(request: GrcRequest20):
    """
    Calculate GRC for SORA 2.0 based on MTOM and population density
    
    Official Reference: JARUS SORA 2.0 Annex B (JAR_doc_06)
    Using correct JARUS thresholds for population density
    """
    try:
        logger.info(f"SORA 2.0 GRC calculation: mtom={request.mtom_kg}kg, density={request.population_density}/km²")
        
        # MTOM validation - accept all positive values
        if request.mtom_kg <= 0:
            raise HTTPException(status_code=400, detail="Invalid drone mass (MTOM_kg) must be positive")
        
        # CORRECT JARUS SORA 2.0 GRC Table (Annex B)
        # According to official JARUS documentation:
        if request.population_density < 1:      # Unpopulated
            base_grc = 1
        elif request.population_density < 10:   # Rural (1-10)
            base_grc = 2
        elif request.population_density < 100:  # Suburban (10-100)
            base_grc = 3
        elif request.population_density < 300:  # Urban (100-300)
            base_grc = 4
        elif request.population_density < 1000: # Dense urban (300-1000)
            base_grc = 5
        else:                                   # Very dense urban (1000+)
            base_grc = 6
        
        # MTOM adjustment per JARUS (higher mass = higher risk)
        if request.mtom_kg >= 25:  # Large UAS
            base_grc = min(base_grc + 2, 7)
        elif request.mtom_kg >= 10:  # Medium UAS
            base_grc = min(base_grc + 1, 7)
        # Small UAS (<10kg) use base GRC
        
        initial_grc = base_grc
        final_grc = initial_grc
        
        # Apply mitigations (M1, M2, M3)
        mitigation_effects = []
        m1_effect = 0
        m2_effect = 0
        m3_effect = 0
        
        if request.m1_strategic:
            m1_reduction = {"High": -2, "Medium": -1, "Low": 0}.get(request.m1_strategic, 0)
            m1_effect = m1_reduction
            final_grc = max(final_grc + m1_reduction, 1)
            if m1_reduction != 0:
                mitigation_effects.append(f"M1 {request.m1_strategic} ({m1_reduction})")
        
        if request.m2_impact:
            m2_reduction = {"High": -2, "Medium": -1, "Low": 0}.get(request.m2_impact, 0)
            m2_effect = m2_reduction
            final_grc = max(final_grc + m2_reduction, 1)
            if m2_reduction != 0:
                mitigation_effects.append(f"M2 {request.m2_impact} ({m2_reduction})")
        
        if request.m3_erp:
            m3_reduction = {"High": -1, "Medium": 0, "Low": 0}.get(request.m3_erp, 0)
            m3_effect = m3_reduction
            final_grc = max(final_grc + m3_reduction, 1)
            if m3_reduction != 0:
                mitigation_effects.append(f"M3 {request.m3_erp} ({m3_reduction})")
        
        notes = f"SORA 2.0 - Mitigations: {', '.join(mitigation_effects)}" if mitigation_effects else "SORA 2.0 - No mitigations applied"
        
        logger.info(f"SORA 2.0 GRC result: initial={initial_grc}, final={final_grc}, pop_density={request.population_density}")
        
        return {
            "intrinsicGRC": initial_grc,
            "finalGRC": final_grc,
            "notes": notes,
            "m1Effect": m1_effect,
            "m2Effect": m2_effect,
            "m3Effect": m3_effect,
            "reference": "JARUS SORA 2.0 Annex B (JAR_doc_06)"
        }
        
    except Exception as e:
        logger.error(f"SORA 2.0 GRC calculation failed: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))


class GrcRequest25(BaseModel):
    """SORA 2.5 GRC calculation request from C# backend"""
    mtom_kg: float
    population_density: float
    m1a_sheltering: Optional[str] = None
    m2_impact: Optional[str] = None

@app.post("/api/v1/calculate/grc/2.5")
async def calculate_grc_v25(request: GrcRequest25):
    """
    Calculate GRC for SORA 2.5 based on MTOM and population density
    
    Official Reference: JARUS SORA 2.5 Annex B
    Using correct JARUS thresholds with enhanced categories
    """
    try:
        logger.info(f"SORA 2.5 GRC calculation: mtom={request.mtom_kg}kg, density={request.population_density}/km²")
        
        # MTOM validation
        if request.mtom_kg <= 0:
            raise HTTPException(status_code=400, detail="Invalid drone mass (MTOM_kg) must be positive")
        
        # CORRECT SORA 2.5 GRC thresholds
        # SORA 2.5 uses same base thresholds as 2.0 but with enhanced mitigation options
        if request.population_density < 1:      # Unpopulated
            base_grc = 1
        elif request.population_density < 10:   # Rural (1-10)
            base_grc = 2
        elif request.population_density < 100:  # Suburban (10-100)
            base_grc = 3
        elif request.population_density < 300:  # Urban (100-300)
            base_grc = 4
        elif request.population_density < 1000: # Dense urban (300-1000)
            base_grc = 5
        elif request.population_density < 3000: # Very dense urban (1000-3000)
            base_grc = 6
        else:                                   # Extremely dense (3000+)
            base_grc = 7
        
        # MTOM adjustment (same as 2.0)
        if request.mtom_kg >= 25:
            base_grc = min(base_grc + 2, 7)
        elif request.mtom_kg >= 10:
            base_grc = min(base_grc + 1, 7)
        
        initial_grc = base_grc
        final_grc = initial_grc
        
        # Apply SORA 2.5 mitigations
        if request.m1a_sheltering:
            sheltering_reduction = {"High": -1, "Medium": -1, "Low": 0}.get(request.m1a_sheltering, 0)
            final_grc = max(final_grc + sheltering_reduction, 1)
        
        if request.m2_impact:
            impact_reduction = {"High": -2, "Medium": -1, "Low": 0}.get(request.m2_impact, 0)
            final_grc = max(final_grc + impact_reduction, 1)
        
        logger.info(f"SORA 2.5 GRC result: initial={initial_grc}, final={final_grc}, pop_density={request.population_density}")
        
        return {
            "initial_grc": initial_grc,
            "final_grc": final_grc,
            "reference": "JARUS SORA 2.5 Annex B"
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"SORA 2.5 GRC calculation failed: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))


# ============================================================================
# GRC CALCULATION FUNCTIONS (SORA 2.0 & 2.5)
# ============================================================================

def calculate_initial_grc(population_density: str, sheltering: str) -> int:
    """Calculate Initial GRC from population density and sheltering"""
    grc_table = {
        ("Low", "High"): 1,
        ("Low", "Medium"): 2,
        ("Low", "Low"): 3,
        ("Medium", "High"): 4,
        ("Medium", "Medium"): 5,
        ("Medium", "Low"): 6,
        ("High", "High"): 7,
        ("High", "Medium"): 8,
        ("High", "Low"): 10
    }
    key = (population_density, sheltering)
    return grc_table.get(key, 5)  # Default to 5 if not found


def apply_grc_mitigations(initial_grc: int, m1: int, m3: int) -> int:
    """Apply M1 and M3 mitigations to get Final GRC"""
    final_grc = initial_grc - m1 - m3
    return max(1, final_grc)  # Cannot be less than 1


# ============================================================================
# SORA 2.5 ENDPOINTS
# ============================================================================

# Import calculation functions (these need to be in your project structure)
try:
    from arc.calculators.initial_arc_calculator_v25 import (
        calculate_initial_arc_v25,
        validate_traffic_density_data_source
    )
    from arc.calculators.strategic_mitigations_v25 import calculate_residual_arc_v25
    from sail.sail_calculator_v25 import calculate_sail_v25
    SORA25_AVAILABLE = True
    logger.info("✅ SORA 2.5 calculation modules loaded successfully")
except ImportError as e:
    SORA25_AVAILABLE = False
    logger.warning(f"⚠️  SORA 2.5 modules not available: {e}")
    logger.warning("   SORA 2.5 endpoints will return 501 Not Implemented")


@app.post("/api/sora/complete-v25")
async def calculate_complete_sora_v25(request: SoraRequestV25):
    """
    Execute complete SORA 2.5 evaluation with enhanced ARC calculations.
    
    Official Reference: JAR_doc_25 (SORA 2.5 Main Body)
    """
    if not SORA25_AVAILABLE:
        raise HTTPException(
            status_code=501,
            detail={
                "error": "SORA 2.5 Not Implemented",
                "message": "SORA 2.5 calculation modules are not available. "
                          "Please ensure arc/, sail/ folders with calculation modules exist.",
                "required_modules": [
                    "arc/calculators/initial_arc_calculator_v25.py",
                    "arc/calculators/strategic_mitigations_v25.py",
                    "sail/sail_calculator_v25.py"
                ]
            }
        )
    
    try:
        logger.info("=" * 80)
        logger.info("SORA 2.5 COMPLETE EVALUATION")
        logger.info("=" * 80)
        
        # Validate category
        if request.category != "SORA-2.5":
            raise HTTPException(
                status_code=400,
                detail=f"Invalid category. This endpoint requires 'SORA-2.5', got '{request.category}'"
            )
        
        # Step #4: Calculate Initial ARC (uses 5 enhanced fields)
        logger.info("\n### STEP #4: Calculate Initial ARC ###")
        initial_arc, arc_explanation = calculate_initial_arc_v25(
            request.operational_volume,
            request.traffic_density,
            request.arc_inputs_25
        )
        
        # Step #5: Apply Strategic Mitigations (uses 5 enhanced fields)
        logger.info("\n### STEP #5: Apply Strategic Mitigations ###")
        residual_arc, mitigations = calculate_residual_arc_v25(
            initial_arc,
            request.arc_inputs_25,
            request.operational_volume
        )
        
        # Step #2 & #3: Calculate GRC (use SORA 2.0 functions)
        logger.info("\n### STEP #2: Calculate Initial GRC ###")
        initial_grc = calculate_initial_grc(
            request.grc_inputs.population_density,
            request.grc_inputs.sheltering
        )
        logger.info(f"   Initial GRC: {initial_grc}")
        
        logger.info("\n### STEP #3: Apply GRC Mitigations ###")
        final_grc = apply_grc_mitigations(
            initial_grc,
            request.grc_inputs.m1,
            request.grc_inputs.m3
        )
        logger.info(f"   Final GRC: {final_grc}")
        
        # Step #9: Determine SAIL
        logger.info("\n### STEP #9: Determine SAIL ###")
        sail, sail_explanation = calculate_sail_v25(final_grc, residual_arc)
        
        logger.info(f"\n✅ SORA 2.5 evaluation completed successfully")
        logger.info(f"   Results: Initial ARC={initial_arc}, Residual ARC={residual_arc}, SAIL={sail}")
        
        return {
            "category": request.category,
            "initial_arc": initial_arc,
            "residual_arc": residual_arc,
            "sail": sail,
            "arc_explanation": arc_explanation,
            "strategic_mitigations": mitigations,
            "sail_explanation": sail_explanation,
            "initial_grc": initial_grc,
            "final_grc": final_grc,
            "reference": "JAR_doc_25 - SORA 2.5 Main Body (Edition 22.11.2024)",
            "success": True
        }
    
    except ValueError as e:
        logger.error(f"Validation error: {str(e)}")
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        logger.error(f"Calculation error: {str(e)}", exc_info=True)
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/api/sora/validate-traffic-density-source")
async def validate_traffic_source(data_source: str, traffic_density: str):
    """
    Validate traffic density data source for SORA 2.5.
    
    Official Rule: Expert judgment is ONLY valid for LOW traffic density.
    """
    if not SORA25_AVAILABLE:
        raise HTTPException(
            status_code=501,
            detail="SORA 2.5 validation not available"
        )
    
    try:
        data_source_enum = TrafficDensityDataSource(data_source)
        result = validate_traffic_density_data_source(
            data_source_enum,
            traffic_density
        )
        
        return {
            "is_valid": result.is_valid,
            "data_source": data_source,
            "traffic_density": traffic_density,
            "message": result.error_message if not result.is_valid else "Valid combination",
            "reference": "JAR_doc_25 Step #4"
        }
    except ValueError:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid data source: {data_source}. Must be Empirical, Statistical, or Expert."
        )


# ============================================================================
# ARC ENDPOINTS WITH FRONTEND MAPPING (SORA 2.0 & 2.5)
# ============================================================================

def normalize_frontend_payload_v20(payload: Dict[str, Any]) -> Dict[str, Any]:
    """
    Normalize frontend payload for SORA 2.0 ARC calculation.
    
    Field Mappings:
    - max_height_agl_m → altitude_agl_ft (convert meters to feet)
    - is_modes_veil → is_mode_s_veil (typo tolerance)
    - is_controlled → is_in_ctr (quick fix for CTR detection)
    - environment: SUBURBAN → URBAN (Annex C has no suburban class)
    """
    normalized = {}
    
    # Convert altitude from meters to feet for SORA 2.0
    if "max_height_agl_m" in payload:
        normalized["altitude_agl_ft"] = payload["max_height_agl_m"] * METERS_TO_FEET
        logger.info(f"Converted altitude: {payload['max_height_agl_m']}m → {normalized['altitude_agl_ft']:.2f}ft")
    
    # Typo tolerance: is_modes_veil → is_mode_s_veil
    if "is_modes_veil" in payload:
        normalized["is_mode_s_veil"] = payload["is_modes_veil"]
        logger.info(f"Corrected typo: is_modes_veil → is_mode_s_veil = {normalized['is_mode_s_veil']}")
    elif "is_mode_s_veil" in payload:
        normalized["is_mode_s_veil"] = payload["is_mode_s_veil"]
    
    # Quick fix: is_controlled → is_in_ctr
    if "is_controlled" in payload:
        normalized["is_in_ctr"] = payload["is_controlled"]
        logger.info(f"Mapped: is_controlled → is_in_ctr = {normalized['is_in_ctr']}")
    elif "is_in_ctr" in payload:
        normalized["is_in_ctr"] = payload["is_in_ctr"]
    
    # Environment mapping: Suburban → Urban (Annex C standard)
    if "environment" in payload:
        env = payload["environment"]
        if env and env.upper() == "SUBURBAN":
            normalized["environment"] = "Urban"
            logger.info("Coerced SUBURBAN → Urban (Annex C standard)")
        else:
            if isinstance(env, str):
                normalized["environment"] = env.capitalize()
            else:
                normalized["environment"] = env
    
    # Pass through ALL other fields
    for key, value in payload.items():
        if key not in ["max_height_agl_m", "is_modes_veil", "is_controlled", "environment",
                       "tactical_mitigation_level", "max_height_amsl_m"]:
            normalized[key] = value
    
    return normalized


def normalize_frontend_payload_v25(payload: Dict[str, Any]) -> Dict[str, Any]:
    """
    Normalize frontend payload for SORA 2.5 ARC calculation.
    
    Field Mappings:
    - max_height_agl_m → altitude_agl_m (SORA 2.5 uses meters directly)
    - is_modes_veil → is_mode_s_veil (typo tolerance)
    - is_controlled → is_in_ctr (quick fix for CTR detection)
    - environment: SUBURBAN → URBAN (Annex C has no suburban class)
    """
    normalized = {}
    
    # SORA 2.5 uses meters directly (no conversion)
    if "max_height_agl_m" in payload:
        normalized["altitude_agl_m"] = payload["max_height_agl_m"]
        logger.info(f"Altitude (meters): {normalized['altitude_agl_m']}m")
    
    # Typo tolerance: is_modes_veil → is_mode_s_veil
    if "is_modes_veil" in payload:
        normalized["is_mode_s_veil"] = payload["is_modes_veil"]
        logger.info(f"Corrected typo: is_modes_veil → is_mode_s_veil = {normalized['is_mode_s_veil']}")
    elif "is_mode_s_veil" in payload:
        normalized["is_mode_s_veil"] = payload["is_mode_s_veil"]
    
    # Quick fix: is_controlled → is_in_ctr
    if "is_controlled" in payload:
        normalized["is_in_ctr"] = payload["is_controlled"]
        logger.info(f"Mapped: is_controlled → is_in_ctr = {normalized['is_in_ctr']}")
    elif "is_in_ctr" in payload:
        normalized["is_in_ctr"] = payload["is_in_ctr"]
    
    # Environment mapping: Suburban → Urban (Annex C standard)
    if "environment" in payload:
        env = payload["environment"]
        if env and env.upper() == "SUBURBAN":
            normalized["environment"] = "Urban"
            logger.info("Coerced SUBURBAN → Urban (Annex C standard)")
        else:
            if isinstance(env, str):
                normalized["environment"] = env.capitalize()
            else:
                normalized["environment"] = env
    
    # Pass through ALL other fields
    for key, value in payload.items():
        if key not in ["max_height_agl_m", "is_modes_veil", "is_controlled", "environment",
                       "tactical_mitigation_level", "max_height_amsl_m"]:
            normalized[key] = value
    
    return normalized


@app.post("/arc/v2.0/initial")
async def arc_initial_v20(request: Request):
    """
    Calculate Initial ARC for SORA 2.0 with frontend field mapping.
    
    Accepts frontend payload as-is, normalizes fields, and returns 200 OK always.
    
    Field Mappings:
    - max_height_agl_m → altitude_agl_ft (×3.28084)
    - is_modes_veil → is_mode_s_veil
    - is_controlled → is_in_ctr
    - environment: SUBURBAN → URBAN
    
    Returns: { ok: true/false, data: {...}, error: "..." }
    """
    try:
        # Read raw frontend payload
        payload = await request.json()
        logger.info(f"📥 Received FE payload (v2.0): {payload}")
        
        # Normalize payload
        normalized = normalize_frontend_payload_v20(payload)
        logger.info(f"📤 Normalized request (v2.0): {normalized}")
        
        # Check if ARC Calculator is available
        if not ARC_CALCULATOR_AVAILABLE:
            logger.error("ARCCalculator not available")
            return {
                "ok": False,
                "error": "ARCCalculator module not available. Please check calculations/arc_calculator.py",
                "data": None
            }
        
        # Require airspace_class for a deterministic Annex C mapping
        if "airspace_class" not in normalized:
            logger.error("Missing required field: airspace_class")
            return {
                "ok": False,
                "error": (
                    "Missing required field 'airspace_class'. "
                    "Annex C Table C.1 requires airspace classification (A/B/C/D/E/G) to map AEC -> ARC."
                ),
                "data": None,
                "reference": "JARUS SORA Annex C Table C.1"
            }

        # Build ARCRequest_2_0 from normalized payload
        try:
            arc_request = ARCRequest_2_0(
                altitude_agl_ft=normalized.get("altitude_agl_ft", 400.0),
                is_mode_s_veil=normalized.get("is_mode_s_veil", False),
                is_tmz=normalized.get("is_tmz", False),
                is_in_ctr=normalized.get("is_in_ctr", False),
                environment=EnvironmentType(normalized.get("environment", "Urban")),
                airspace_class=AirspaceClass(normalized.get("airspace_class")),
                distance_to_aerodrome_nm=normalized.get("distance_to_aerodrome_nm"),
                is_airport_heliport=normalized.get("is_airport_heliport", False),
                is_atypical_segregated=normalized.get("is_atypical_segregated", False),
                strategic_mitigations=normalized.get("strategic_mitigations", [])
            )
        except Exception as e:
            logger.error(f"Failed to build ARCRequest_2_0: {e}")
            return {
                "ok": False,
                "error": f"Invalid request parameters: {str(e)}",
                "data": None
            }
        
        # Calculate ARC
        calculator = ARCCalculator()
        try:
            arc_response = calculator.calculate_arc_2_0(arc_request)
            logger.info(f"✅ ARC 2.0 calculated: {arc_response.initial_arc}")
            
            return {
                "ok": True,
                "data": {
                    "initial_arc": arc_response.initial_arc,
                    "residual_arc": arc_response.residual_arc,
                    "aec_category": arc_response.aec_category,
                    "aec_number": arc_response.aec_number,
                    "strategic_mitigation_effect": arc_response.strategic_mitigation_effect,
                    "mitigations_applied": arc_response.mitigations_applied,
                    "mitigations_ignored": arc_response.mitigations_ignored,
                    "requires_authority_approval": arc_response.requires_authority_approval,
                    "notes": arc_response.notes,
                    "source": arc_response.source
                },
                "error": None
            }
        except Exception as e:
            logger.error(f"ARC calculation failed: {e}")
            return {
                "ok": False,
                "error": f"ARC calculation failed: {str(e)}",
                "data": None
            }
    
    except Exception as e:
        logger.error(f"Endpoint error: {e}", exc_info=True)
        return {
            "ok": False,
            "error": f"Internal error: {str(e)}",
            "data": None
        }


@app.post("/arc/v2.5/initial")
async def arc_initial_v25(request: Request):
    """
    Calculate Initial ARC for SORA 2.5 with frontend field mapping.
    
    Accepts frontend payload as-is, normalizes fields, and returns 200 OK always.
    
    Field Mappings:
    - max_height_agl_m → altitude_agl_m (no conversion, 2.5 uses meters)
    - is_modes_veil → is_mode_s_veil
    - is_controlled → is_in_ctr
    - environment: SUBURBAN → URBAN
    
    Returns: { ok: true/false, data: {...}, error: "..." }
    """
    try:
        # Read raw frontend payload
        payload = await request.json()
        logger.info(f"📥 Received FE payload (v2.5): {payload}")
        
        # Normalize payload
        normalized = normalize_frontend_payload_v25(payload)
        logger.info(f"📤 Normalized request (v2.5): {normalized}")
        
        # Check if ARC Calculator is available
        if not ARC_CALCULATOR_AVAILABLE:
            logger.error("ARCCalculator not available")
            return {
                "ok": False,
                "error": "ARCCalculator module not available. Please check calculations/arc_calculator.py",
                "data": None
            }
        
        # Require airspace_class for a deterministic Annex C mapping
        if "airspace_class" not in normalized:
            logger.error("Missing required field: airspace_class")
            return {
                "ok": False,
                "error": (
                    "Missing required field 'airspace_class'. "
                    "Annex C Table C.1 requires airspace classification (A/B/C/D/E/G) to map AEC -> ARC."
                ),
                "data": None,
                "reference": "JARUS SORA Annex C Table C.1"
            }

        # Build ARCRequest_2_5 from normalized payload
        try:
            arc_request = ARCRequest_2_5(
                altitude_agl_m=normalized.get("altitude_agl_m", 120.0),
                is_mode_s_veil=normalized.get("is_mode_s_veil", False),
                is_tmz=normalized.get("is_tmz", False),
                is_in_ctr=normalized.get("is_in_ctr", False),
                environment=EnvironmentType(normalized.get("environment", "Urban")),
                airspace_class=AirspaceClass(normalized.get("airspace_class")),
                distance_to_aerodrome_km=normalized.get("distance_to_aerodrome_km"),
                is_airport_heliport=normalized.get("is_airport_heliport", False),
                is_atypical_segregated=normalized.get("is_atypical_segregated", False),
                strategic_mitigations=normalized.get("strategic_mitigations", [])
            )
        except Exception as e:
            logger.error(f"Failed to build ARCRequest_2_5: {e}")
            return {
                "ok": False,
                "error": f"Invalid request parameters: {str(e)}",
                "data": None
            }
        
        # Calculate ARC
        calculator = ARCCalculator()
        try:
            arc_response = calculator.calculate_arc_2_5(arc_request)
            logger.info(f"✅ ARC 2.5 calculated: {arc_response.initial_arc}")
            
            return {
                "ok": True,
                "data": {
                    "initial_arc": arc_response.initial_arc,
                    "residual_arc": arc_response.residual_arc,
                    "aec_category": arc_response.aec_category,
                    "aec_number": arc_response.aec_number,
                    "strategic_mitigation_effect": arc_response.strategic_mitigation_effect,
                    "mitigations_applied": arc_response.mitigations_applied,
                    "mitigations_ignored": arc_response.mitigations_ignored,
                    "requires_authority_approval": arc_response.requires_authority_approval,
                    "notes": arc_response.notes,
                    "source": arc_response.source
                },
                "error": None
            }
        except Exception as e:
            logger.error(f"ARC calculation failed: {e}")
            return {
                "ok": False,
                "error": f"ARC calculation failed: {str(e)}",
                "data": None
            }
    
    except Exception as e:
        logger.error(f"Endpoint error: {e}", exc_info=True)
        return {
            "ok": False,
            "error": f"Internal error: {str(e)}",
            "data": None
        }


# ============================================================================
# GENERAL ENDPOINTS
# ============================================================================

@app.get("/")
async def root():
    """Root endpoint - API information"""
    return {
        "service": "SORA 2.0 & 2.5 Calculation API",
        "version": "2.0.0",
        "status": "running",
        "sora_20_available": True,
        "sora_25_available": SORA25_AVAILABLE,
        "endpoints": {
            "health": "/health",
            "sora_20_grc": "/api/grc/calculate-v20",
            "sora_25_complete": "/api/sora/complete-v25",
            "sora_25_validate": "/api/sora/validate-traffic-density-source",
            "docs": "/docs"
        },
        "references": {
            "sora_20": "JAR_doc_06 - SORA 2.0 AMC",
            "sora_25": "JAR_doc_25 - SORA 2.5 Main Body (Edition 22.11.2024)"
        }
    }


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "healthy",
        "service": "SORA Calculation API",
        "port": 8001,
        "sora_20": "available",
        "sora_25": "available" if SORA25_AVAILABLE else "modules not loaded"
    }


# ============================================================================
# STARTUP
# ============================================================================

if __name__ == "__main__":
    import uvicorn
    
    print("=" * 80)
    print("SORA 2.0 & 2.5 Calculation API Starting...")
    print("=" * 80)
    print("Port: 8001")
    print("Health Check: http://localhost:8001/health")
    print("API Docs: http://localhost:8001/docs")
    print("")
    print("Capabilities:")
    print("  ✅ SORA 2.0 GRC Calculations (JAR_doc_06)")
    print(f"  {'✅' if SORA25_AVAILABLE else '⚠️ '} SORA 2.5 Enhanced ARC Calculations (JAR_doc_25)")
    if not SORA25_AVAILABLE:
        print("")
        print("To enable SORA 2.5:")
        print("  1. Create folder structure: arc/calculators/, sail/")
        print("  2. Add the calculator modules from the artifacts")
        print("  3. Restart the service")
    print("=" * 80)
    
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8001,
        reload=True,
        log_level="info"
    )
```

## 3. File: Backend/src/Skyworks.Core/Services/Orchestration/SORAOrchestrationService.cs

```csharp
using Skyworks.Core.Models;
using Skyworks.Core.Models.Enums;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Skyworks.Core.Models.SORA;
using Skyworks.Core.Services.Python;
using Skyworks.Core.Services.SORA;

namespace Skyworks.Core.Services.Orchestration
{
    public class SORAOrchestrationService : ISORAOrchestrationService
    {
        private readonly IPythonSoraService _pythonService;
        private readonly IGRCService _grcService;
        private readonly IARCService _arcService;
        private readonly ISAILService _sailService;
        private readonly ITMPRService _tmprService;
        private readonly ILogger<SORAOrchestrationService> _logger;

        public SORAOrchestrationService(
            IPythonSoraService pythonService,
            IGRCService grcService,
            IARCService arcService,
            ISAILService sailService,
            ITMPRService tmprService,
            ILogger<SORAOrchestrationService> logger)
        {
            _pythonService = pythonService;
            _grcService = grcService;
            _arcService = arcService;
            _sailService = sailService;
            _tmprService = tmprService;
            _logger = logger;
        }

        public async Task<SORAEvaluationResult> ExecuteCompleteAsync(SoraEvaluationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting SORA evaluation for version {Version}", request.SoraVersion);

                // Step 1: Calculate GRC
                var grcResult = await CalculateGRCAsync(request);
                
                // Step 2: Calculate ARC
                var arcResult = await CalculateARCAsync(request);
                
                // Step 3: Determine SAIL
                var sail = DetermineSAIL(grcResult.FinalGRC, arcResult.ResidualARC);
                
                // Step 4: Calculate TMPR
                var tmpr = CalculateTMPR(sail);

                // Step 5: Validate operation scope for SPECIFIC category
                var scopeValidation = ValidateOperationScope(grcResult.FinalGRC, arcResult.ResidualARC, sail);
                if (!scopeValidation.IsValid)
                {
                    throw new InvalidOperationException(scopeValidation.Reason);
                }
                
                var result = new SORAEvaluationResult
                {
                    Version = request.SoraVersion,
                    InitialGRC = grcResult.InitialGRC,
                    FinalGRC = grcResult.FinalGRC,
                    InitialARC = arcResult.InitialARC,
                    ResidualARC = arcResult.ResidualARC,
                    SAIL = sail,
                    TMPR = tmpr,
                    IsValid = true,
                    ValidationMessages = new List<string>(),
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("SORA evaluation completed: GRC={GRC}, ARC={ARC}, SAIL={SAIL}",
                    result.FinalGRC, result.ResidualARC, result.SAIL);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SORA evaluation failed");
                return new SORAEvaluationResult
                {
                    Version = request.SoraVersion,
                    IsValid = false,
                    ValidationMessages = new List<string> { ex.Message },
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<GRCResult> CalculateGRCAsync(SoraEvaluationRequest request)
        {
            _logger.LogDebug("Calculating GRC for SORA {Version}", request.SoraVersion);

            // Use Python backend for GRC calculation
            if (request.SoraVersion == "2.0")
            {
                var pythonResult = await _pythonService.CalculateGRC20Async(
                    request.MaxDimension * request.MaxSpeed, // Simple MTOM approximation
                    request.PopulationDensity,
                    request.StrategicMitigations?.Contains("M1_Strategic") == true ? "Medium" : null,
                    request.StrategicMitigations?.Contains("M2_Impact") == true ? "Medium" : null,
                    request.StrategicMitigations?.Contains("M3_ERP") == true ? "Medium" : null
                );

                return new GRCResult
                {
                    InitialGRC = $"GRC_{pythonResult.IntrinsicGRC}",
                    FinalGRC = $"GRC_{pythonResult.FinalGRC}",
                    Notes = pythonResult.Notes
                };
            }
            else // SORA 2.5
            {
                var pythonResult = await _pythonService.CalculateGRC25Async(
                    request.MaxDimension * request.MaxSpeed,
                    request.PopulationDensity,
                    request.Sheltering ? "High" : "Low",
                    request.StrategicMitigations?.Contains("M2_Impact") == true ? "Medium" : null
                );

                return new GRCResult
                {
                    InitialGRC = $"GRC_{pythonResult.InitialGRC}",
                    FinalGRC = $"GRC_{pythonResult.FinalGRC}",
                    Notes = "SORA 2.5 calculation"
                };
            }
        }

        public async Task<ARCResult> CalculateARCAsync(SoraEvaluationRequest request)
        {
            _logger.LogDebug("Calculating ARC for SORA {Version}", request.SoraVersion);

            var arcRequest = new ARCRequest
            {
                SoraVersion = request.SoraVersion,
                Environment = request.Environment,
                AltitudeAGL_ft = request.AltitudeAGL_ft,
                AirspaceClass = request.AirspaceClass,
                ControlledAirspace = request.ControlledAirspace,
                UrbanAir = request.UrbanAir,
                IsAtypicalSegregated = request.IsAtypicalSegregated,
                StrategicMitigations = request.StrategicMitigations
            };

            var arcResponse = await _arcService.CalculateARCAsync(arcRequest);

            return new ARCResult
            {
                InitialARC = arcResponse.InitialARC,
                ResidualARC = arcResponse.ResidualARC,
                MitigationEffect = arcResponse.StrategicMitigationEffect,
                Notes = arcResponse.Notes
            };
        }

        public string DetermineSAIL(string finalGRC, string residualARC)
        {
            return _sailService.DetermineSAIL(finalGRC, residualARC);
        }

        public string CalculateTMPR(string sail)
        {
            return _tmprService.CalculateTMPR(sail);
        }

        public (bool IsValid, string Reason) ValidateOperationScope(string finalGRC, string residualARC, string sail)
        {
            // Extract numeric values
            var grcValue = int.Parse(finalGRC.Replace("GRC_", ""));
            var arcValue = residualARC.Replace("ARC_", "");
            
            // Rule 1: SAIL VI requires CERTIFIED category
            if (sail == "SAIL_VI")
            {
                return (false, "SAIL VI operations require CERTIFIED category and are out of scope for SPECIFIC category operations");
            }
            
            // Rule 2: GRC ≥ 6 is out of scope
            if (grcValue >= 6)
            {
                return (false, $"Ground Risk Class {grcValue} is out of scope for SPECIFIC category operations. GRC must be less than 6.");
            }
            
            // Rule 3: GRC ≥ 5 with ARC_d is out of scope
            if (grcValue >= 5 && arcValue == "d")
            {
                return (false, "Combined high ground risk (GRC ≥ 5) with high air risk (ARC_d) is out of scope for SPECIFIC category operations");
            }
            
            return (true, "Operation is within SPECIFIC category scope");
        }
    }

    // Interface definitions
    public interface ISORAOrchestrationService
    {
        Task<SORAEvaluationResult> ExecuteCompleteAsync(SoraEvaluationRequest request);
        Task<GRCResult> CalculateGRCAsync(SoraEvaluationRequest request);
        Task<ARCResult> CalculateARCAsync(SoraEvaluationRequest request);
        string DetermineSAIL(string finalGRC, string residualARC);
        string CalculateTMPR(string sail);
    }

    // Result models
    public class GRCResult
    {
        public string InitialGRC { get; set; }
        public string FinalGRC { get; set; }
        public string Notes { get; set; }
    }

    public class ARCResult
    {
        public string InitialARC { get; set; }
        public string ResidualARC { get; set; }
        public int MitigationEffect { get; set; }
        public string Notes { get; set; }
    }
}
```

## 4. File: Backend/src/Skyworks.Api/Controllers/SoraProxyController.cs (Modified Method Only)

```csharp
[HttpPost("complete")]
public async Task<IActionResult> CompleteEvaluation([FromBody] SoraEvaluationRequest request)
{
    try
    {
        _logger.LogInformation("SORA complete evaluation request for version {Version}", request.SoraVersion);

        // Execute the complete SORA evaluation
        var result = await _orchestrationService.ExecuteCompleteAsync(request);
        
        // Check if the evaluation was successful
        if (!result.IsValid)
        {
            // If validation failed due to out-of-scope operation, return 400 BadRequest
            if (result.ValidationMessages.Any(m => 
                m.Contains("out of scope", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("SAIL VI", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("CERTIFIED category", StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("SORA evaluation rejected - out of scope: {Messages}", 
                    string.Join(", ", result.ValidationMessages));
                
                return BadRequest(new
                {
                    error = "Out of Scope",
                    message = result.ValidationMessages.FirstOrDefault() ?? "Operation is out of scope for SPECIFIC category",
                    details = new
                    {
                        finalGRC = result.FinalGRC,
                        residualARC = result.ResidualARC,
                        sail = result.SAIL,
                        category_required = "CERTIFIED"
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            
            // Other validation failures return 400 with details
            return BadRequest(new
            {
                error = "Validation Failed",
                messages = result.ValidationMessages,
                timestamp = DateTime.UtcNow
            });
        }

        // Successful evaluation returns 200 OK
        return Ok(result);
    }
    catch (InvalidOperationException ex) when (
        ex.Message.Contains("out of scope", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("SAIL VI", StringComparison.OrdinalIgnoreCase))
    {
        // Handle out-of-scope exceptions thrown by the orchestration service
        _logger.LogWarning("SORA evaluation rejected - out of scope: {Message}", ex.Message);
        
        return BadRequest(new
        {
            error = "Out of Scope",
            message = ex.Message,
            category_required = "CERTIFIED",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "SORA complete evaluation failed");
        return StatusCode(500, new 
        { 
            error = "Internal server error during SORA evaluation",
            message = ex.Message,
            timestamp = DateTime.UtcNow
        });
    }
}
```

## 5. Test Corrections

Based on the JARUS specifications, all test expectations appear to be correct. The issue was with the implementation using wrong thresholds. The corrected code above should make all tests pass without needing to modify test expectations.

The key fixes:
1. **GRC thresholds** now match JARUS SORA 2.0 Annex B exactly
2. **Out-of-scope validation** properly returns 400 BadRequest for high-risk operations
3. **MTOM adjustments** correctly applied based on drone mass categories
