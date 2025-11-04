"""
SKYWORKS AI SUITE - Pydantic Models for SORA Calculations
100% EASA/JARUS Compliant Data Models
"""

from enum import Enum
from typing import Optional, List
from pydantic import BaseModel, Field
try:
    # Pydantic v2
    from pydantic import field_validator
except ImportError:  # pragma: no cover - fallback for older versions
    # Define a no-op decorator to avoid import errors in unlikely v1 envs
    def field_validator(*args, **kwargs):
        def _decorator(func):
            return func
        return _decorator


class SORAVersion(str, Enum):
    """SORA Version: 2.0 or 2.5"""
    SORA_2_0 = "2.0"
    SORA_2_5 = "2.5"


class AirspaceClass(str, Enum):
    """ICAO Airspace Classification"""
    CLASS_A = "A"
    CLASS_B = "B"
    CLASS_C = "C"
    CLASS_D = "D"
    CLASS_E = "E"
    CLASS_F = "F"
    CLASS_G = "G"


class EnvironmentType(str, Enum):
    """Environment classification for SORA 2.0"""
    URBAN = "Urban"
    SUBURBAN = "Suburban"
    RURAL = "Rural"
    CONTROLLED = "Controlled"


class OperationType(str, Enum):
    """Operation type (VLOS/BVLOS)"""
    VLOS = "VLOS"
    BVLOS = "BVLOS"


class MitigationLevel(str, Enum):
    """Mitigation robustness levels"""
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"


class ARCRating(str, Enum):
    """Air Risk Class ratings"""
    ARC_a = "ARC-a"
    ARC_b = "ARC-b"
    ARC_c = "ARC-c"
    ARC_d = "ARC-d"


class SAILLevel(str, Enum):
    """Specific Assurance and Integrity Levels"""
    SAIL_I = "I"
    SAIL_II = "II"
    SAIL_III = "III"
    SAIL_IV = "IV"
    SAIL_V = "V"
    SAIL_VI = "VI"


# ============================================================================
# GRC CALCULATION MODELS
# ============================================================================

class GRCRequest_2_0(BaseModel):
    """
    Ground Risk Class calculation request - SORA 2.0
    Source: JARUS SORA 2.0 Table 2 (Page 20)
    """
    max_dimension_m: float = Field(
        ..., 
        gt=0, 
        description="Maximum characteristic dimension in meters"
    )
    operational_scenario: str = Field(
        ...,
        description="Scenario: VLOS_Controlled, VLOS_Sparsely, BVLOS_Sparsely, VLOS_Populated, BVLOS_Populated, VLOS_Gathering, BVLOS_Gathering"
    )
    m1_strategic: MitigationLevel = Field(
        default=MitigationLevel.NONE,
        description="M1: Strategic mitigations for ground risk"
    )
    m2_ground_impact: MitigationLevel = Field(
        default=MitigationLevel.MEDIUM,
        description="M2: Effects of ground impact reduced (e.g., parachute)"
    )
    m3_emergency_response: MitigationLevel = Field(
        default=MitigationLevel.MEDIUM,
        description="M3: Emergency Response Plan (ERP)"
    )

    # Normalize legacy/synonym scenario labels coming from .NET (e.g., "SparselyPopulated")
    @field_validator("operational_scenario", mode="before")
    @classmethod
    def _normalize_operational_scenario(cls, v: str) -> str:
        if v is None:
            return v
        s = str(v)
        # Normalize common synonyms used in older payloads
        s = s.replace("SparselyPopulated", "Sparsely")
        # Keep original casing/prefix (VLOS/BVLOS) intact
        return s


class GRCRequest_2_5(BaseModel):
    """
    Ground Risk Class calculation request - SORA 2.5
    Source: JARUS SORA 2.5 Table 2 (Page 34)
    """
    max_dimension_m: float = Field(
        ..., 
        gt=0, 
        description="Maximum characteristic dimension in meters"
    )
    max_speed_ms: float = Field(
        ..., 
        gt=0, 
        description="Maximum speed in meters per second"
    )
    weight_kg: Optional[float] = Field(
        None,
        gt=0,
        description="Weight in kg (for 250g rule check)"
    )
    population_density: float = Field(
        ..., 
        ge=0, 
        description="Population density in people/km²"
    )
    is_controlled_ground: bool = Field(
        default=False,
        description="Is operation over controlled ground area?"
    )
    m1_strategic: MitigationLevel = Field(
        default=MitigationLevel.NONE,
        description="M1: Strategic mitigations for ground risk"
    )
    m2_ground_impact: MitigationLevel = Field(
        default=MitigationLevel.MEDIUM,
        description="M2: Effects of ground impact reduced"
    )
    m3_emergency_response: MitigationLevel = Field(
        default=MitigationLevel.MEDIUM,
        description="M3: Emergency Response Plan"
    )


class GRCResponse(BaseModel):
    """GRC Calculation Response"""
    intrinsic_grc: int = Field(..., description="Intrinsic GRC (before mitigations)")
    final_grc: int = Field(..., description="Final GRC (after mitigations)")
    m1_effect: int = Field(..., description="M1 mitigation effect")
    m2_effect: int = Field(..., description="M2 mitigation effect")
    m3_effect: int = Field(..., description="M3 mitigation effect")
    dimension_category: str = Field(..., description="Dimension category (1m, 3m, 8m, 20m, 40m)")
    notes: str = Field(..., description="Calculation details and notes")
    source: str = Field(..., description="EASA/JARUS reference")


# ============================================================================
# ARC CALCULATION MODELS
# ============================================================================

class ARCRequest_2_0(BaseModel):
    """
    Air Risk Class calculation request - SORA 2.0
    Source: JARUS SORA 2.0 Annex C Table 1 (Page 12)
    """
    max_height_agl_m: float = Field(
        ..., 
        ge=0, 
        description="Maximum height above ground level in meters"
    )
    max_height_amsl_m: float = Field(
        ..., 
        ge=0, 
        description="Maximum height above mean sea level in meters"
    )
    airspace_class: AirspaceClass = Field(
        ...,
        description="ICAO Airspace Class (A-G)"
    )
    is_controlled: bool = Field(
        ...,
        description="Is airspace controlled?"
    )
    is_modes_veil: bool = Field(
        default=False,
        description="Is operation in Mode-S Veil?"
    )
    is_tmz: bool = Field(
        default=False,
        description="Is operation in TMZ (Transponder Mandatory Zone)?"
    )
    environment: EnvironmentType = Field(
        ...,
        description="Environment type (Urban, Suburban, Rural, Controlled)"
    )
    is_airport_heliport: bool = Field(
        default=False,
        description="Is operation in airport/heliport environment?"
    )
    is_atypical_segregated: bool = Field(
        default=False,
        description="Is airspace atypical/segregated per Annex G 3.20?"
    )
    tactical_mitigation_level: MitigationLevel = Field(
        default=MitigationLevel.NONE,
        description="Tactical mitigation level for ARC reduction"
    )


class ARCRequest_2_5(BaseModel):
    """
    Air Risk Class calculation request - SORA 2.5
    Source: JARUS SORA 2.5 Annex E Table 9 (Page 70)
    """
    max_height_agl_m: float = Field(..., ge=0)
    # Optional in some report/orchestration paths; ARC 2.5 logic doesn't require speed
    max_speed_ms: float | None = Field(default=None, description="Maximum speed in m/s (optional)")
    airspace_class: AirspaceClass
    is_controlled: bool
    is_modes_veil: bool = False
    is_tmz: bool = False
    environment: EnvironmentType
    # Optional flags present in 2.0 format but also applicable in 2.5
    is_airport_heliport: bool = False
    is_atypical_segregated: bool = False
    tactical_mitigation_level: MitigationLevel = MitigationLevel.NONE

    # Accept common casing/underscore variants for enums coming from .NET
    @field_validator("airspace_class", mode="before")
    @classmethod
    def _normalize_airspace_class(cls, v):
        if isinstance(v, AirspaceClass):
            return v
        s = str(v).strip().upper()
        mapping = {
            "A": AirspaceClass.CLASS_A,
            "B": AirspaceClass.CLASS_B,
            "C": AirspaceClass.CLASS_C,
            "D": AirspaceClass.CLASS_D,
            "E": AirspaceClass.CLASS_E,
            "F": AirspaceClass.CLASS_F,
            "G": AirspaceClass.CLASS_G,
        }
        return mapping.get(s, v)

    @field_validator("environment", mode="before")
    @classmethod
    def _normalize_environment(cls, v):
        if isinstance(v, EnvironmentType):
            return v
        s = str(v).strip().lower()
        mapping = {
            "urban": EnvironmentType.URBAN,
            "suburban": EnvironmentType.SUBURBAN,
            "rural": EnvironmentType.RURAL,
            "controlled": EnvironmentType.CONTROLLED,
        }
        return mapping.get(s, v)


class ARCResponse(BaseModel):
    """ARC Calculation Response"""
    initial_arc: ARCRating = Field(..., description="Initial ARC (before mitigations)")
    final_arc: ARCRating = Field(..., description="Final ARC (after tactical mitigations)")
    aec: int = Field(..., description="Air risk Environment Category (AEC)")
    density_rating: int = Field(..., description="Air risk density rating")
    tactical_mitigation_effect: int = Field(..., description="Tactical mitigation reduction (-1, -2, etc.)")
    notes: str = Field(..., description="Calculation details")
    source: str = Field(..., description="EASA/JARUS reference")


# ============================================================================
# SAIL CALCULATION MODELS
# ============================================================================

class SAILRequest(BaseModel):
    """
    SAIL calculation request
    Source: JARUS SORA 2.0 Table 5 / SORA 2.5 Table 7
    """
    sora_version: SORAVersion = Field(..., description="SORA version (2.0 or 2.5)")
    final_grc: int = Field(..., ge=0, le=10, description="Final GRC (0-10)")
    final_arc: ARCRating = Field(..., description="Final ARC (a/b/c/d)")

    # Accept ARC labels with underscore (e.g., "ARC_d") by normalizing to enum values ("ARC-d")
    @field_validator("final_arc", mode="before")
    @classmethod
    def _normalize_final_arc(cls, v):
        if isinstance(v, ARCRating):
            return v
        if v is None:
            return v
        s = str(v).strip()
        # Normalize common underscore form coming from .NET
        s = s.replace("_", "-").upper()
        mapping = {
            "ARC-A": ARCRating.ARC_a,
            "ARC-B": ARCRating.ARC_b,
            "ARC-C": ARCRating.ARC_c,
            "ARC-D": ARCRating.ARC_d,
        }
        return mapping.get(s, v)


class SAILResponse(BaseModel):
    """SAIL Calculation Response"""
    sail: SAILLevel = Field(..., description="SAIL level (I-VI)")
    final_grc: int = Field(..., description="Final GRC used")
    final_arc: ARCRating = Field(..., description="Final ARC used")
    sora_version: SORAVersion = Field(..., description="SORA version used")
    notes: str = Field(..., description="Matrix lookup details")
    source: str = Field(..., description="EASA/JARUS table reference")


# ============================================================================
# OSO FRAMEWORK MODELS
# ============================================================================

class OSORequest(BaseModel):
    """OSO applicability request"""
    sora_version: SORAVersion
    sail: SAILLevel
    operation_type: OperationType
    final_grc: int
    final_arc: ARCRating


class OSOItem(BaseModel):
    """Single OSO item"""
    oso_id: str = Field(..., description="OSO identifier (e.g., OSO#1)")
    title: str = Field(..., description="OSO title")
    description: str = Field(..., description="OSO description")
    integrity: str = Field(..., description="Required integrity level")
    assurance: str = Field(..., description="Required assurance level")
    is_applicable: bool = Field(..., description="Is this OSO applicable?")
    is_mandatory: bool = Field(..., description="Is this OSO mandatory?")


class OSOResponse(BaseModel):
    """OSO Framework Response"""
    applicable_osos: List[OSOItem] = Field(..., description="List of applicable OSOs")
    total_count: int = Field(..., description="Total number of applicable OSOs")
    mandatory_count: int = Field(..., description="Number of mandatory OSOs")
    sora_version: SORAVersion
    sail: SAILLevel
    notes: str
