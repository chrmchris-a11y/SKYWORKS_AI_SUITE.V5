from pydantic import BaseModel, Field, field_validator, ConfigDict
from enum import Enum
from typing import Optional, List, Dict, Any, Literal
from datetime import datetime, UTC

class OperationType(str, Enum):
    VLOS = "VLOS"
    EVLOS = "EVLOS"
    BVLOS = "BVLOS"

class AirspaceClass20(str, Enum):
    G = "G"
    E = "E"
    D = "D"
    C = "C"
    CTR = "CTR"
    TMA = "TMA"
    TIZ = "TIZ"
    ATZ = "ATZ"
    RMZ = "RMZ"
    TMZ = "TMZ"

class AirspaceClass25(str, Enum):
    G = "G"
    E = "E"
    D = "D"
    C = "C"
    CTR = "CTR"
    TMA = "TMA"
    TIZ = "TIZ"
    ATZ = "ATZ"
    RMZ = "RMZ"
    TMZ = "TMZ"
    U_SPACE = "U-space"

class ProximityAerodrome(str, Enum):
    INSIDE = "Inside"
    NEAR = "Near"
    OUTSIDE = "Outside"

class TimeOfOperation(str, Enum):
    DAY = "Day"
    NIGHT = "Night"
    OFF_PEAK = "Off-peak"

class AirspaceSegregation(str, Enum):
    NONE = "None"
    PARTIAL = "Partial"
    FULL_CERTIFIED = "Full_Certified"

class AirspaceContainment25(str, Enum):
    NONE = "None"
    OPERATIONAL = "Operational"
    CERTIFIED = "Certified"

class TrafficDensityDataSource(str, Enum):
    EMPIRICAL = "Empirical"
    STATISTICAL = "Statistical"
    EXPERT = "Expert"

class ARCValue(str, Enum):
    A = "a"
    B = "b"
    C = "c"
    D = "d"

class TraceEntry(BaseModel):
    """Calculation trace entry for audit compliance"""
    step: str = Field(..., description="Calculation step name")
    inputs: Dict[str, Any] = Field(..., description="Input parameters for this step")
    result: str = Field(..., description="Output of this step")
    rule_ref: str = Field(..., description="Rule reference")
    doc_ref: Dict[str, str] = Field(..., description="Document reference with doc_id, annex, section")
    delta: Optional[int] = Field(None, description="Change applied (for mitigations)")
    timestamp: str = Field(default_factory=lambda: datetime.now(UTC).isoformat())
    notes: Optional[str] = Field(None, description="Additional context")

class ARCInputs20(BaseModel):
    """SORA 2.0 ARC calculation inputs"""
    version: Literal["SORA_2.0"] = "SORA_2.0"
    operation_type: OperationType
    airspace_class: AirspaceClass20
    
    # FIX #1: Add environment field for AEC mapping (Urban/Rural binary)
    environment: Optional[str] = Field(None, description="Environment type: Urban or Rural (Suburban→Urban)")
    
    # FIX #2: Add is_controlled flag for AEC mapping
    is_controlled: Optional[bool] = Field(None, description="Is operation in controlled airspace?")
    
    # FIX #3: Add altitude_agl_ft for C# backend compatibility
    altitude_agl_ft: Optional[float] = Field(None, description="Altitude AGL in feet (will convert to meters)")
    
    # Legacy field (deprecated but kept for backwards compatibility)
    air_traffic_density: Optional[int] = Field(None, ge=1, le=5, description="DEPRECATED: Use environment+altitude+is_controlled instead")
    
    proximity_aerodrome: ProximityAerodrome
    operational_altitude_agl_m: Optional[int] = Field(None, ge=0, le=500, description="Altitude Above Ground Level in meters")
    time_of_operation: Optional[TimeOfOperation] = None
    operational_volume_defined: bool = Field(..., description="Is 3D operational volume clearly defined?")
    adjacent_airspace_characterised: bool = Field(..., description="Adjacent airspace risks assessed?")
    
    # Strategic Mitigations
    airspace_segregation: AirspaceSegregation = AirspaceSegregation.NONE
    operational_limitation_boundary: bool = Field(False, description="Time/area restrictions applied")
    operational_limitation_chronology: bool = Field(False, description="Off-peak hours restriction")
    procedural_coordination: bool = Field(False, description="ATC coordination in place")
    geo_fencing: bool = Field(False, description="Electronic geo-fencing (supporting evidence only)")

    @field_validator('operation_type', 'airspace_class', mode='before')
    @classmethod
    def normalize_case(cls, v):
        """Normalize case for enum matching"""
        if isinstance(v, str):
            # Keep VLOS/EVLOS/BVLOS uppercase, single-letter airspace classes uppercase
            return v.upper()
        return v

    model_config = ConfigDict(
        use_enum_values=True,
        json_schema_extra={
            "example": {
                "operation_type": "BVLOS",
                "airspace_class": "E",
                "air_traffic_density": 4,
                "proximity_aerodrome": "Near",
                "operational_altitude_agl_m": 150,
                "operational_volume_defined": True,
                "adjacent_airspace_characterised": True,
                "airspace_segregation": "Full_Certified",
                "operational_limitation_boundary": True,
                "operational_limitation_chronology": True,
                "procedural_coordination": False,
                "geo_fencing": True
            }
        }
    )

class ARCInputs25(BaseModel):
    """SORA 2.5 ARC calculation inputs"""
    version: Literal["SORA_2.5"] = "SORA_2.5"
    operation_type: OperationType
    airspace_class: AirspaceClass25
    air_traffic_density: int = Field(ge=1, le=5)
    proximity_aerodrome: ProximityAerodrome
    operational_altitude_agl_m: int = Field(ge=0, le=500)
    time_of_operation: Optional[TimeOfOperation] = None
    operational_volume_defined: bool
    adjacent_airspace_characterised: bool
    
    # SORA 2.5 specific fields
    u_space_services_available: bool = Field(False, description="U-space DAA/tracking services available")
    traffic_density_data_source: TrafficDensityDataSource = Field(..., description="Method used to determine traffic density")
    
    # Enhanced Strategic Mitigations
    airspace_containment: AirspaceContainment25 = AirspaceContainment25.NONE
    temporal_segregation: bool = Field(False, description="Time-based separation with traffic data")
    spatial_segregation: bool = Field(False, description="Area-based separation with coordination")
    u_space_services: bool = Field(False, description="U-space service arrangements (supporting evidence)")

    @field_validator('traffic_density_data_source')
    @classmethod
    def validate_density_data_source(cls, v, info):
        """Expert opinion not sufficient for medium/high density"""
        air_traffic_density = info.data.get('air_traffic_density', 1)
        if air_traffic_density > 2 and v == TrafficDensityDataSource.EXPERT:
            raise ValueError("Medium/High traffic density requires Empirical or Statistical data, not Expert opinion")
        return v

    @field_validator('u_space_services')
    @classmethod
    def validate_u_space_services(cls, v, info):
        """U-space services require U-space airspace"""
        if v and info.data.get('airspace_class') != AirspaceClass25.U_SPACE:
            raise ValueError("U-space services only available in designated U-space airspace")
        return v

    model_config = ConfigDict(use_enum_values=True)

class ARCResult(BaseModel):
    """ARC calculation result"""
    version: str = Field(..., description="SORA version used")
    initial_arc: ARCValue = Field(..., description="Initial ARC before mitigations")
    residual_arc: ARCValue = Field(..., description="Final ARC after strategic mitigations")
    requires_authority_approval: bool = Field(False, description="Flag indicating authority approval is required (AEC12 / atypical or other cases)")
    total_reduction: int = Field(ge=0, le=3, description="Total classes reduced (integer only)")
    mitigations_applied: List[str] = Field(..., description="List of applied mitigation names")
    supporting_evidence: List[str] = Field(default_factory=list, description="Supporting evidence items")
    calculation_trace: List[TraceEntry] = Field(..., description="Detailed calculation trace")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "version": "EASA_SORA_2.0_AMC_2024-07",
                "initial_arc": "c",
                "residual_arc": "a",
                "total_reduction": 2,
                "mitigations_applied": ["Full_Certified_Segregation"],
                "supporting_evidence": ["Boundary_Limitation", "Chronology_Limitation", "Geo_fencing"],
                "calculation_trace": [
                    {
                        "step": "initial_arc_determination",
                        "inputs": {"airspace_class": "E", "air_traffic_density": 4},
                        "result": "c",
                        "rule_ref": "Table_C.1_Row_3_High_Density_Controlled",
                        "doc_ref": {
                            "doc_id": "EASA_EAR_UAS_2024",
                            "annex": "Annex C",
                            "section": "Table C.1, Row 3"
                        }
                    }
                ]
            }
        }
    )
