"""SAIL calculation data models using Pydantic V2."""

from datetime import datetime, timezone
from enum import Enum
from typing import Any, Dict, List, Literal, Optional
from pydantic import BaseModel, ConfigDict, Field, field_validator, model_validator


class SAILValue(str, Enum):
    """SAIL categories I through VI."""
    I = "I"
    II = "II"
    III = "III"
    IV = "IV"
    V = "V"
    VI = "VI"


class ARCValue(str, Enum):
    """Air Risk Class values."""
    A = "a"
    B = "b"
    C = "c"
    D = "d"


class RobustnessLevel(str, Enum):
    """OSO robustness/assurance levels."""
    NONE = "None"
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"


# Additional enums for test compatibility
class GRCLevel(Enum):
    """Ground Risk Class levels (deprecated: prefer integer grc_level with per-version validation).

    NOTE: Kept for backward compatibility only. New API models accept plain integers
    with conditional validation per SORA version (2.0: 1..7 usable, >7 ⇒ Category C; 2.5: 1..10).
    """
    GRC_1 = 1
    GRC_2 = 2
    GRC_3 = 3
    GRC_4 = 4
    GRC_5 = 5
    GRC_6 = 6
    GRC_7 = 7
    GRC_8 = 8
    # Historical enum only covered up to 8; do not extend to avoid accidental coupling.
    
    @property
    def value(self) -> int:  # type: ignore[override]
        return self._value_


class ARCLevel(str, Enum):
    """Air Risk Class levels a-d."""
    ARC_A = "a"
    ARC_B = "b"
    ARC_C = "c"
    ARC_D = "d"


class SAILLevel(str, Enum):
    """SAIL levels I-VI (alias for SAILValue for test compatibility)."""
    I = "I"
    II = "II"
    III = "III"
    IV = "IV"
    V = "V"
    VI = "VI"


class SORAVersion(str, Enum):
    """SORA standard versions."""
    SORA_2_0 = "SORA_2.0"
    SORA_2_5 = "SORA_2.5"


class DocReference(BaseModel):
    """Document reference for traceability."""
    doc_id: str = Field(description="Document identifier")
    standard: str = Field(description="Standard name (SORA_2.0 or SORA_2.5)")
    section: str = Field(description="Document section/clause")
    page: Optional[str] = Field(None, description="Page reference")
    url: Optional[str] = Field(None, description="Document URL")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "doc_id": "EASA_EAR_UAS_2024",
                "standard": "SORA_2.0",
                "section": "Annex D, Table D.1",
                "page": "Annex D-3"
            }
        }
    )


class TraceEntry(BaseModel):
    """Calculation trace entry for full traceability."""
    step: str = Field(description="Calculation step name")
    inputs: Dict[str, Any] = Field(description="Input parameters")
    result: str = Field(description="Step result")
    rule_ref: str = Field(description="Rule reference")
    doc_ref: DocReference = Field(description="Document reference")
    timestamp: str = Field(
        default_factory=lambda: datetime.now(timezone.utc).isoformat(),
        description="Calculation timestamp"
    )
    notes: Optional[str] = Field(None, description="Additional notes")
    
    model_config = ConfigDict(
        json_schema_extra={
            "example": {
                "step": "sail_determination",
                "inputs": {"grc": 4, "arc": "c"},
                "result": "III",
                "rule_ref": "EASA_AMC_Table_D1_GRC3-4_ARCC",
                "doc_ref": {
                    "doc_id": "EASA_EAR_UAS_2024",
                    "standard": "SORA_2.0",
                    "section": "Annex D, Table D.1"
                },
                "notes": "GRC 4 (range 3-4) × ARC C → SAIL III"
            }
        }
    )


class SAILInputs20(BaseModel):
    """SORA 2.0 SAIL calculation inputs."""
    version: Literal["SORA_2.0"] = "SORA_2.0"
    final_grc: int = Field(ge=1, le=8, description="Final GRC after all mitigations", alias="grc")
    final_arc: ARCValue = Field(description="Final/Residual ARC", alias="arc")
    
    model_config = ConfigDict(
        use_enum_values=True,
        populate_by_name=True,  # Allow both alias and field name
        json_schema_extra={
            "example": {
                "version": "SORA_2.0",
                "grc": 4,  # Use alias in example
                "arc": "c"
            }
        }
    )
    
    @field_validator('final_grc')
    @classmethod
    def validate_grc_range(cls, v: int) -> int:
        """Validate GRC is in range 1-8."""
        if not 1 <= v <= 8:
            raise ValueError(f"GRC must be between 1 and 8, got {v}")
        return v
    
    @property
    def grc(self) -> int:
        """Alias property for final_grc."""
        return self.final_grc
    
    @property
    def arc(self) -> ARCValue:
        """Alias property for final_arc."""
        return self.final_arc


class SAILInputs25(BaseModel):
    """SORA 2.5 SAIL calculation inputs."""
    version: Literal["SORA_2.5"] = "SORA_2.5"
    final_grc: int = Field(ge=1, le=8, description="Final GRC after all mitigations", alias="grc")
    final_arc: ARCValue = Field(description="Final/Residual ARC", alias="arc")
    
    model_config = ConfigDict(
        use_enum_values=True,
        populate_by_name=True,  # Allow both alias and field name
        json_schema_extra={
            "example": {
                "version": "SORA_2.5",
                "grc": 6,  # Use alias in example
                "arc": "b"
            }
        }
    )
    
    @field_validator('final_grc')
    @classmethod
    def validate_grc_range(cls, v: int) -> int:
        """Validate GRC is in range 1-8."""
        if not 1 <= v <= 8:
            raise ValueError(f"GRC must be between 1 and 8, got {v}")
        return v
    
    @property
    def grc(self) -> int:
        """Alias property for final_grc."""
        return self.final_grc
    
    @property
    def arc(self) -> ARCValue:
        """Alias property for final_arc."""
        return self.final_arc


class SAILResult(BaseModel):
    """SAIL calculation result with full traceability."""
    version: str = Field(description="SORA version used")
    sail: SAILValue = Field(description="Determined SAIL level")
    final_grc: int = Field(ge=1, le=8, description="Input GRC")
    final_arc: ARCValue = Field(description="Input ARC")
    
    # OSO requirements
    oso_requirements: List[str] = Field(description="Required OSO identifiers")
    oso_count: int = Field(description="Total number of OSOs required")
    robustness_levels: Dict[str, str] = Field(
        description="OSO -> robustness level mapping"
    )
    
    # Traceability
    calculation_trace: List[TraceEntry] = Field(description="Full calculation trace")
    
    model_config = ConfigDict(
        use_enum_values=True,
        json_schema_extra={
            "example": {
                "version": "SORA_2.0",
                "sail": "III",
                "final_grc": 4,
                "final_arc": "c",
                "oso_requirements": [
                    "OSO#01", "OSO#02", "OSO#03", "OSO#04", "OSO#05", "OSO#06",
                    "OSO#07", "OSO#08", "OSO#09", "OSO#10", "OSO#11", "OSO#12",
                    "OSO#13", "OSO#14", "OSO#15"
                ],
                "oso_count": 15,
                "robustness_levels": {
                    "OSO#01": "Medium",
                    "OSO#02": "Medium",
                    "OSO#03": "Low"
                },
                "calculation_trace": []
            }
        }
    )


class SAILCalculationError(Exception):
    """Base exception for SAIL calculation errors."""
    pass


class InvalidGRCError(SAILCalculationError):
    """GRC value out of valid range 1-8."""
    pass


class InvalidARCError(SAILCalculationError):
    """ARC value not in {a, b, c, d}."""
    pass


class SAILMappingError(SAILCalculationError):
    """Error during SAIL table lookup."""
    pass


# Request/Response models for test compatibility
class SAILCalculationRequest(BaseModel):
    """Request model for SAIL calculation (SORA-aware).

    Rules by version:
    - SORA 2.0: requires letter arc_level in {a..d}; accepts grc_level 1..7 (usable mapping);
      if grc_level > 7 ⇒ Category C (still 200 at router level, no SAIL).
      residual_arc_level is not allowed.
    - SORA 2.5: requires residual_arc_level in 1..10; letters are not allowed.
      grc_level must be 1..10. arc_level must be omitted.
    """

    sora_version: SORAVersion = Field(
        default=SORAVersion.SORA_2_5,
        description="SORA standard version (default 2.5 for forward-compat)"
    )
    grc_level: int = Field(
        description="Ground Risk Class level (int). Per-version validation applies."
    )
    arc_level: Optional[ARCLevel] = Field(
        default=None,
        description="Letter ARC a–d (only for SORA 2.0)."
    )
    residual_arc_level: Optional[int] = Field(
        default=None,
        ge=1,
        le=10,
        description="Numeric residual ARC (1..10) required for SORA 2.5."
    )

    model_config = ConfigDict(
        use_enum_values=True,
        extra="forbid",
        json_schema_extra={
            "examples": [
                {"sora_version": "SORA_2.0", "grc_level": 5, "arc_level": "c"},
                {"sora_version": "SORA_2.5", "grc_level": 6, "residual_arc_level": 4},
            ]
        },
    )

    @field_validator("grc_level")
    @classmethod
    def _ensure_grc_is_int(cls, v: Any) -> int:  # type: ignore[override]
        # Accept either plain int or legacy Enum values
        if isinstance(v, GRCLevel):
            return int(v.value)
        if isinstance(v, bool):  # bool is instance of int in Python
            raise ValueError("grc_level must be an integer 1..10")
        return int(v)

    @model_validator(mode="after")
    def _validate_by_version(self) -> "SAILCalculationRequest":
        v = self.sora_version
        # Normalize inputs based on version
        if v == SORAVersion.SORA_2_0:
            # arc_level required; residual_arc_level not allowed
            if self.arc_level is None:
                raise ValueError("arc_level is required for SORA 2.0")
            if self.residual_arc_level is not None:
                raise ValueError("residual_arc_level is not allowed for SORA 2.0")
            # GRC range: allow 1..10 but mapping uses 1..7; >7 handled as Category C by router
            if self.grc_level < 1:
                raise ValueError("grc_level must be >= 1 for SORA 2.0")
        elif v == SORAVersion.SORA_2_5:
            # residual_arc_level required in 1..10; letters not allowed
            if self.residual_arc_level is None:
                raise ValueError("residual_arc_level (1..10) is required for SORA 2.5")
            if self.arc_level is not None:
                raise ValueError("arc_level (letters) is not allowed for SORA 2.5")
            if not (1 <= self.grc_level <= 10):
                raise ValueError("grc_level must be between 1 and 10 for SORA 2.5")
        else:
            raise ValueError("Unsupported SORA version")
        return self


class SAILCalculationResponse(BaseModel):
    """Response model for SAIL calculation (SORA-aware).

    - sail_level: I..VI or None for Category C (2.0 with grc>7)
    - sail: deprecated alias for legacy consumers
    - oso_count: only for 2.0; 2.5 returns None
    - category: "C" when 2.0 and grc_level>7; otherwise None
    - reference: optional source string (e.g., EASA AMC/GM or JARUS Annex F)
    - warnings: optional messages (prefer 400 at router for non-compliant inputs)
    """

    sail_level: Optional[SAILLevel] = Field(
        default=None, description="Determined SAIL level (I–VI) or None for Category C"
    )
    # Deprecated alias for backwards compatibility
    sail: Optional[SAILLevel] = Field(
        default=None, description="Deprecated alias of sail_level for legacy clients"
    )

    sora_version: SORAVersion = Field(description="SORA standard version used")
    grc_level: int = Field(description="Echoed Ground Risk Class level (int)")
    arc_level: Optional[ARCLevel] = Field(
        default=None, description="Echo letter ARC (2.0 only)"
    )
    residual_arc_level: Optional[int] = Field(
        default=None, description="Echo numeric residual ARC (2.5 only)"
    )
    category: Optional[Literal["C"]] = Field(
        default=None, description="SORA 2.0 Category C when grc_level>7"
    )
    oso_count: Optional[int] = Field(
        default=None, description="Number of required OSOs (2.0 only)"
    )
    reference: Optional[str] = Field(default=None, description="Regulatory reference")
    warnings: Optional[List[str]] = Field(default=None, description="Optional warnings")

    model_config = ConfigDict(
        use_enum_values=False,
        extra="forbid",
    )

    @model_validator(mode="after")
    def _fill_legacy_alias(self) -> "SAILCalculationResponse":
        # Ensure deprecated alias is populated for legacy consumers
        if self.sail is None:
            self.sail = self.sail_level
        return self

    @field_validator("grc_level")
    @classmethod
    def _coerce_grc_level(cls, v: Any) -> int:  # type: ignore[override]
        if isinstance(v, GRCLevel):
            return int(v.value)
        if isinstance(v, bool):
            raise ValueError("grc_level must be an integer")
        return int(v)