"""
FastAPI router for SAIL (Specific Assurance and Integrity Level) calculations.

This module exposes an APIRouter with REST API endpoints for:
- SAIL calculations based on GRC and ARC inputs (authoritative table lookup)
- OSO requirements retrieval and counts
- Support for both SORA 2.0 (EASA AMC/GM) and SORA 2.5 (JARUS) standards

Compliance notes (knowledge-first, authoritative):
- SORA 2.0 SAIL lookup follows EASA AMC/GM (Annex D, Table D.1) groupings.
- SORA 2.5 SAIL lookup follows JARUS SORA 2.5 (Annex D/Table 7) with stricter rows for GRC 6 and 7–8.
- Step 5 in SORA 2.5 uses numeric residual ARC (1..10) for the authoritative SAIL lookup. No letter binning is used on authoritative paths.
"""

from typing import List
import re
from fastapi import APIRouter, HTTPException, Query, Path
from pydantic import BaseModel, ConfigDict, Field
from sail.models.sail_models import (
    GRCLevel, ARCLevel, SAILLevel, SORAVersion,
    SAILCalculationRequest
)
from sail.calculators.sail_calculator import SAILCalculator
from sail.calculators.oso_mapper import OSOMapper
from sail.data.oso_requirements import (
    OSORequirement, get_oso_requirements_for_sail, get_oso_count_for_sail
)
# Note: SAILValidator is not wired currently; keep imports minimal to avoid noise

# Initialize APIRouter (to be included by the main FastAPI app)
router = APIRouter(prefix="/sail", tags=["SAIL"])

# Initialize components
sail_calculator = SAILCalculator()
oso_mapper = OSOMapper()


class SAILCalculationAPIRequest(BaseModel):
    """API request model for SAIL calculations."""
    model_config = ConfigDict(
        use_enum_values=True,
        validate_assignment=True,
        str_strip_whitespace=True
    )
    
    grc_level: int = Field(
        ..., ge=1, le=10,
        description="Ground Risk Class level (2.0: 1–7, 2.5: 1–10)"
    )
    # Για SORA 2.0 απαιτείται γράμμα a–d. Για SORA 2.5 απαιτείται numeric residual ARC 1..10.
    arc_level: ARCLevel | None = Field(
        None,
        description="Air Risk Class level (a-d) – απαιτείται μόνο για SORA 2.0"
    )
    residual_arc_level: int | None = Field(
        default=None,
        ge=1,
        le=10,
        description="Residual ARC 1..10 – απαιτείται μόνο για SORA 2.5"
    )
    sora_version: SORAVersion = Field(
        SORAVersion.SORA_2_5,
        description="SORA version to use for calculation"
    )


class SAILCalculationAPIResponse(BaseModel):
    """API response model for SAIL calculations (version-aware, optional fields)."""
    model_config = ConfigDict(use_enum_values=True, validate_assignment=True)

    # SAIL can be None in Category C case (for 2.0 GRC > 7 via legacy endpoints)
    sail_level: SAILLevel | None = Field(
        None,
        description="Calculated SAIL level (I–VI). None when Category C."
    )
    # Deprecated alias for backward compatibility in some clients
    sail: SAILLevel | None = Field(
        None,
        description="Deprecated alias for backward compatibility"
    )
    # Authoritative OSO count applies only to 2.0; 2.5 returns None
    oso_count: int | None = Field(
        None,
        description="OSO count (authoritative for 2.0 only)."
    )
    grc_level: GRCLevel = Field(
        ...,
        description="Input Ground Risk Class level"
    )
    # 2.0 only
    arc_level: ARCLevel | None = Field(
        None,
        description="Residual ARC letter a–d (2.0 only)"
    )
    # 2.5 only
    residual_arc_level: int | None = Field(
        default=None,
        ge=1,
        le=10,
        description="Residual ARC 1..10 (2.5 only)"
    )
    sora_version: SORAVersion = Field(
        ...,
        description="SORA version used for calculation"
    )
    # Optional human-readable source reference
    reference: str | None = None


def _norm_version(val) -> str:
    """
    Normalize SORA version to plain '2.0' or '2.5' for internal branching.
    Accepts Enum members, strings like 'SORA_2_0', 'SORA 2.5', etc.
    """
    if val is None:
        return "2.5"
    s = str(getattr(val, "value", val))
    s = s.replace("_", ".").strip()  # SORA_2_5 -> SORA.2.5
    m = re.search(r"(\d+\.\d+)", s)
    return m.group(1) if m else "2.5"


class OSORequirementsResponse(BaseModel):
    """API response model for OSO requirements."""
    model_config = ConfigDict(
        use_enum_values=True,
        validate_assignment=True
    )
    
    sail_level: SAILLevel
    sora_version: SORAVersion | None = None
    # For SORA 2.5 do not hard-code 24‑OSO motif; return None for count and provide list+robustness.
    oso_count: int | None = None
    count_source: str | None = None  # e.g., 'authoritative-2.0' | 'derived-2.5'
    requirements: List[OSORequirement]


class ErrorResponse(BaseModel):
    """Standard error response model."""
    model_config = ConfigDict(validate_assignment=True)
    
    error: str
    detail: str
    status_code: int

@router.get("/health")
async def health_check():
    """Health check endpoint."""
    return {"status": "healthy", "service": "SAIL Calculator API"}


@router.post("/calculate", response_model=SAILCalculationAPIResponse)
async def calculate_sail(request: SAILCalculationAPIRequest):
    """
    Calculate SAIL level based on GRC and ARC inputs.
    
    Args:
        request: SAIL calculation request with GRC, ARC, and SORA version
        
    Returns:
        SAIL calculation response with level and OSO count
        
    Raises:
        HTTPException: If validation fails or calculation error occurs
    """
    try:
        # Normalize inputs considering use_enum_values=True on the model
        grc_int = int(request.grc_level)
        version_val = _norm_version(request.sora_version)

        # Βασικός έλεγχος εύρους ανά έκδοση (authoritative):
        if version_val == "2.0" and grc_int > 7:
            # Contract: 2.0 with GRC>7 ⇒ Category C (χωρίς SAIL)
            return {
                "sail_level": None,
                "sail": None,
                "oso_count": None,
                "grc_level": grc_int,
                "arc_level": (request.arc_level if not hasattr(request.arc_level, "value") else ARCLevel(str(request.arc_level.value).lower())) if request.arc_level else None,
                "residual_arc_level": None,
                "sora_version": SORAVersion.SORA_2_0,
                "category": "C",
                "reference": "EASA AMC/GM SORA 2.0 – Category C trigger for GRC>7"
            }
        
        # Κλάδος SORA 2.5: απαιτείται numeric residual ARC (1..10) και επίσημος πίνακας (Annex D)
        if version_val == "2.5":
            if request.residual_arc_level is None:
                raise HTTPException(status_code=400, detail="SORA 2.5: Απαιτείται residual_arc_level (1..10) για το Step #9 (numeric ARC)")
            # High-GRC rows (9–10) ⇒ SAIL VI for any numeric ARC 1..10 (authoritative rule)
            if grc_int >= 9:
                sail_enum = SAILLevel("VI")
                return {
                    "sail_level": sail_enum,
                    "sail": sail_enum,
                    "oso_count": None,
                    "grc_level": grc_int,
                    "residual_arc_level": int(request.residual_arc_level),
                    "sora_version": SORAVersion.SORA_2_5,
                    "reference": "JARUS SORA 2.5 Annex D Table 7 – GRC 9–10 rows"
                }
            from sail.sail_calculator_v25 import calculate_sail_v25
            sail_str, _ = calculate_sail_v25(grc_int, int(request.residual_arc_level))
            # Στο 2.5 δεν επιστρέφουμε ‘σκληροκωδικομένο’ OSO count (το Annex E διαφέρει). Μπορούμε να επιστρέψουμε None ή να εκθέσουμε λίστα OSO σε ξεχωριστό endpoint.
            sail_enum = SAILLevel(sail_str)
            return {
                "sail_level": sail_enum,
                "sail": sail_enum,
                "oso_count": None,
                "grc_level": grc_int,
                "residual_arc_level": int(request.residual_arc_level),
                "sora_version": SORAVersion.SORA_2_5,
                "reference": "JAR_doc_25 Annex D (Table 7) – numeric ARC"
            }

        # Κλάδος SORA 2.0: απαιτείται γράμμα ARC a–d (Annex D Table D.1)
        if request.arc_level is None:
            raise HTTPException(status_code=400, detail="SORA 2.0: Απαιτείται arc_level a–d")
        # Ensure enums for calculator request
        arc_val = request.arc_level if not hasattr(request.arc_level, "value") else request.arc_level.value
        calc_request = SAILCalculationRequest(
            grc_level=GRCLevel(grc_int),
            arc_level=ARCLevel(str(arc_val).lower()),
            sora_version=SORAVersion.SORA_2_0
        )
        calc_response = sail_calculator.calculate_sail(calc_request)
        oso_count = oso_mapper.get_oso_count(calc_response.sail_level)
        return {
            "sail_level": calc_response.sail_level,
            "sail": calc_response.sail_level,
            "oso_count": oso_count,
            "grc_level": grc_int,
            "arc_level": ARCLevel(str(arc_val).lower()),
            "sora_version": SORAVersion.SORA_2_0,
            "reference": "EASA AMC/GM SORA 2.0 Annex D Table D.1"
        }
        
    except ValueError as e:
        # Domain/validation error – client input is invalid
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        # Unexpected failure – include minimal info to aid debugging
        raise HTTPException(status_code=500, detail=f"SAIL calculation failed: {type(e).__name__}")


@router.get("/calculate", response_model=SAILCalculationAPIResponse)
async def calculate_sail_get(
    grc_level: int = Query(..., ge=1, le=10, description="Ground Risk Class level (SORA 2.0: 1–7; accepts up to 10 to return 400 for >7)"),
    arc_level: str = Query(..., description="Air Risk Class level (a-d)"),
    sora_version: str = Query("2.5", pattern=r"^(2\.0|2\.5)$", description="SORA version (2.0 or 2.5)")
):
    """
    GET supports only SORA 2.0. For SORA 2.5 use POST /sail/calculate with residual_arc_level (1..10).

    Args:
        grc_level: Ground Risk Class level (SORA 2.0: 1–7; SORA 2.5 GET not supported)
        arc_level: Air Risk Class level (a–d, SORA 2.0 only)
        sora_version: SORA version (2.0 or 2.5)

    Returns:
        SAIL calculation response with level and OSO count (2.0 only)
    """
    try:
        # Για SORA 2.5, το GET δεν υποστηρίζει letter-based ARC. Ζήτα POST με residual_arc_level 1..10.
        if sora_version.startswith("2.5"):
            raise ValueError("SORA 2.5: Χρησιμοποίησε POST /sail/calculate με residual_arc_level (1..10)")

        # Convert string inputs to enums (normalize ARC case, allow common prefixes) – SORA 2.0 μόνο
        arc_norm = str(arc_level).strip().lower()
        # Accept variants like "ARC-a", "ARC_A", "arc-a", or just "a"
        if arc_norm.startswith("arc"):
            # remove prefixes like "arc-" or "arc_"
            arc_norm = arc_norm.replace("arc-", "").replace("arc_", "").replace("arc", "")
        arc_norm = arc_norm.strip()
        if arc_norm not in {"a", "b", "c", "d"}:
            raise ValueError(f"ARC must be one of a/b/c/d, got '{arc_level}'")

        # Έλεγχος εύρους ανά έκδοση
        if sora_version == "2.0" and grc_level > 7:
            raise ValueError("SORA 2.0: Το GRC πρέπει να είναι 1–7. Για GRC>7 απαιτείται Κατηγορία C (Certified)")

        grc_enum = GRCLevel(grc_level)
        arc_enum = ARCLevel(arc_norm)
        # Μετατροπή έκδοσης σε enum
        sora_enum = SORAVersion.SORA_2_0 if sora_version.startswith("2.0") else SORAVersion.SORA_2_5
        
        # Create request and delegate to POST handler (SORA 2.0)
        request = SAILCalculationAPIRequest(
            grc_level=grc_enum,
            arc_level=arc_enum,
            sora_version=sora_enum
        )
        
        return await calculate_sail(request)
        
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))


@router.get("/oso-requirements/{sail_level}", response_model=OSORequirementsResponse)
async def get_oso_requirements(
    sail_level: str = Path(..., pattern="^(I|II|III|IV|V|VI)$", description="SAIL level (I-VI)"),
    sora_version: str = Query("2.0", pattern=r"^(2\.0|2\.5)$", description="SORA version for OSO mapping")
):
    """
    Get OSO requirements for a specific SAIL level.
    
    Args:
        sail_level: SAIL level (I-VI)
        
    Returns:
        OSO requirements for the specified SAIL level
        
    Raises:
        HTTPException: If SAIL level is invalid
    """
    try:
        # Convert to enums
        sail_enum = SAILLevel(sail_level)
        version_enum = SORAVersion.SORA_2_0 if sora_version.startswith("2.0") else SORAVersion.SORA_2_5

        # Get requirements list with robustness for the given SAIL
        requirements = get_oso_requirements_for_sail(sail_enum)

        # Version-specific count behavior
        if version_enum == SORAVersion.SORA_2_0:
            oso_count = get_oso_count_for_sail(sail_enum)
            return OSORequirementsResponse(
                sail_level=sail_enum,
                sora_version=version_enum,
                oso_count=oso_count,
                count_source="authoritative-2.0",
                requirements=requirements
            )
        else:
            # SORA 2.5: do NOT hard-code counts; provide list+robustness only
            return OSORequirementsResponse(
                sail_level=sail_enum,
                sora_version=version_enum,
                oso_count=None,
                count_source="derived-2.5",
                requirements=requirements
            )
        
    except ValueError:
        raise HTTPException(status_code=400, detail=f"Invalid SAIL level: {sail_level}")


@router.get("/oso-requirements", response_model=List[OSORequirementsResponse])
async def get_all_oso_requirements(
    sora_version: str = Query("2.0", pattern=r"^(2\.0|2\.5)$", description="SORA version for OSO mapping")
):
    """
    Get OSO requirements for all SAIL levels.
    
    Returns:
        List of OSO requirements for all SAIL levels
    """
    try:
        responses: List[OSORequirementsResponse] = []
        version_enum = SORAVersion.SORA_2_0 if sora_version.startswith("2.0") else SORAVersion.SORA_2_5

        for sail_level in SAILLevel:
            requirements = get_oso_requirements_for_sail(sail_level)
            if version_enum == SORAVersion.SORA_2_0:
                oso_count = get_oso_count_for_sail(sail_level)
                responses.append(OSORequirementsResponse(
                    sail_level=sail_level,
                    sora_version=version_enum,
                    oso_count=oso_count,
                    count_source="authoritative-2.0",
                    requirements=requirements
                ))
            else:
                responses.append(OSORequirementsResponse(
                    sail_level=sail_level,
                    sora_version=version_enum,
                    oso_count=None,
                    count_source="derived-2.5",
                    requirements=requirements
                ))

        return responses
        
    except Exception:
        raise HTTPException(status_code=500, detail="Failed to get OSO requirements")


@router.post("/calculate/sora20", response_model=SAILCalculationAPIResponse)
async def calculate_sail_sora20(request: SAILCalculationAPIRequest):
    """
    Calculate SAIL level using SORA 2.0 (EASA AMC/GM) standard.
    
    Args:
        request: SAIL calculation request (sora_version will be overridden)
        
    Returns:
        SAIL calculation response using SORA 2.0
    """
    # Override SORA version
    request.sora_version = SORAVersion.SORA_2_0
    return await calculate_sail(request)


@router.post("/calculate/sora25", response_model=SAILCalculationAPIResponse)
async def calculate_sail_sora25(request: SAILCalculationAPIRequest):
    """
    Calculate SAIL level using SORA 2.5 (JARUS) standard.
    
    Args:
        request: SAIL calculation request (sora_version will be overridden)
        
    Returns:
        SAIL calculation response using SORA 2.5
    """
    # Override SORA version
    request.sora_version = SORAVersion.SORA_2_5
    return await calculate_sail(request)


@router.get("/calculate/sora20", response_model=SAILCalculationAPIResponse)
async def calculate_sail_sora20_get(
    grc_level: int = Query(..., ge=1, le=7, description="Ground Risk Class level (1–7)"),
    arc_level: str = Query(..., pattern="^[a-dA-D]$", description="Air Risk Class level (a-d)")
):
    """
    Calculate SAIL level using SORA 2.0 with GET request.
    
    Args:
        grc_level: Ground Risk Class level (1-8)
        arc_level: Air Risk Class level (a-d)
        
    Returns:
        SAIL calculation response using SORA 2.0
    """
    return await calculate_sail_get(grc_level, arc_level, "2.0")


@router.get("/calculate/sora25", response_model=SAILCalculationAPIResponse)
async def calculate_sail_sora25_get(
    grc_level: int = Query(..., ge=1, le=8, description="Ground Risk Class level (1-8)")
):
    """
    Calculate SAIL level using SORA 2.5 with GET request.
    
    Args:
        grc_level: Ground Risk Class level (1-8)
        arc_level: Air Risk Class level (a-d)
        
    Returns:
        400 – Ζήτησε POST /sail/calculate με residual_arc_level (1..10)
    """
    raise HTTPException(status_code=400, detail="SORA 2.5: Χρησιμοποίησε POST /sail/calculate με residual_arc_level (1..10)")


@router.get("/versions")
async def get_supported_versions():
    """
    Get supported SORA versions and their differences.
    
    Returns:
        Information about supported SORA versions
    """
    return {
        "supported_versions": ["2.0", "2.5"],
        "default_version": "2.5",
        "version_info": {
            "2.0": {
                "name": "SORA 2.0",
                "standard": "EASA AMC/GM",
                "description": "European Union Aviation Safety Agency Acceptable Means of Compliance and Guidance Material"
            },
            "2.5": {
                "name": "SORA 2.5", 
                "standard": "JARUS",
                "description": "Joint Authorities for Rulemaking on Unmanned Systems latest revision"
            }
        },
        "key_differences": {
            "step_9_lookup": "Στο SORA 2.5 το Step #9 χρησιμοποιεί numeric residual ARC (1..10) ως μοναδικό lookup key· δεν επιτρέπεται letter binning.",
            "grc_ranges": "Στο SORA 2.0 ο πίνακας SAIL καλύπτει GRC 1..7 ( >7 ⇒ Category C). Στο SORA 2.5 ο πίνακας καλύπτει GRC 1..10 (9–10 ⇒ VI).",
            "examples": [
                "(GRC=4, ARC=3 numeric) ⇒ SAIL III",
                "(GRC=6, ARC=4 numeric) ⇒ SAIL V",
                "(GRC≤2, ARC=4 numeric) ⇒ SAIL II"
            ],
            "references": {
                "uk_caa": "https://regulatorylibrary.caa.co.uk/",
                "jarus": "https://jarus-rpas.org/"
            }
        }
    }


@router.get("/mapping-table")
async def get_sail_mapping_table(
    sora_version: str = Query("2.5", pattern=r"^(2\.0|2\.5)$", description="SORA version")
):
    """
    Get the complete SAIL mapping table for a SORA version.
    
    Args:
        sora_version: SORA version (2.0 or 2.5)
        
    Returns:
        Complete SAIL mapping table
    """
    try:
        version_enum = SORAVersion.SORA_2_0 if sora_version.startswith("2.0") else SORAVersion.SORA_2_5

        mapping_table = {}
        if version_enum == SORAVersion.SORA_2_0:
            # Γράμμα-βασισμένος πίνακας (Annex D Table D.1), GRC 1..7
            for grc in GRCLevel:
                if grc.value > 7:
                    continue
                mapping_table[f"GRC_{grc.value}"] = {}
                for arc in ARCLevel:
                    req = SAILCalculationRequest(grc_level=grc, arc_level=arc, sora_version=version_enum)
                    resp = sail_calculator.calculate_sail(req)
                    mapping_table[f"GRC_{grc.value}"][arc.value] = {
                        "sail_level": resp.sail_level.value,
                        "oso_count": oso_mapper.get_oso_count(resp.sail_level)
                    }
            legend = {"GRC": "1–7", "ARC": "a–d", "SAIL": "I–VI", "OSO": "count (2.0)"}
        else:
            # Numeric ARC 1..10 (Annex D Table 7), GRC 1..8 (για 9–10 → VI)
            from sail.sail_calculator_v25 import calculate_sail_v25
            for grc in range(1, 9):
                mapping_table[f"GRC_{grc}"] = {}
                for arc_num in range(1, 11):
                    sail_str, _ = calculate_sail_v25(grc, arc_num)
                    mapping_table[f"GRC_{grc}"][str(arc_num)] = {
                        "sail_level": sail_str
                    }
            # Add explicit GRC 9 and 10 mapping (all numeric ARC -> VI)
            for grc in (9, 10):
                mapping_table[f"GRC_{grc}"] = {str(arc_num): {"sail_level": "VI"} for arc_num in range(1, 11)}
            legend = {"GRC": "1–10", "ARC": "1–10", "SAIL": "I–VI"}

        return {"sora_version": sora_version, "mapping_table": mapping_table, "legend": legend}
        
    except ValueError:
        raise HTTPException(status_code=400, detail=f"Invalid SORA version: {sora_version}")