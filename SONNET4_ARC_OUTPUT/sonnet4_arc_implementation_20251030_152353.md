I'll implement the complete ARC calculation module according to your detailed specification. Let me start with SORA 2.0 and then extend to SORA 2.5.

## 1. YAML Rule Files

### arc_rules_sora_2_0.yaml

```yaml
version: "EASA_SORA_2.0_AMC_2024-07"
source: "EASA Easy Access Rules for UAS (AMC/GM) - AMC1 Article 11 Annex C"
publication_date: "2024-07-01"
doc_id: "EASA_EAR_UAS_2024"
rules_hash: "sha256:abc123def456"

initial_arc_rules:
  - condition:
      airspace_class: ["G", "ATZ"]
      air_traffic_density: [1, 2]
    result: "a"
    rule_ref: "Table_C.1_Row_1_Low_Density_Class_G_ATZ"
    doc_ref: 
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.1, Row 1"
    
  - condition:
      airspace_class: ["G", "E"]
      air_traffic_density: [3]
    result: "b"
    rule_ref: "Table_C.1_Row_2_Medium_Density_Uncontrolled"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.1, Row 2"
    
  - condition:
      airspace_class: ["E", "D"]
      air_traffic_density: [4]
    result: "c"
    rule_ref: "Table_C.1_Row_3_High_Density_Controlled"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.1, Row 3"
    
  - condition:
      airspace_class: ["C", "CTR", "TMA"]
    result: "d"
    rule_ref: "Table_C.1_Row_4_Complex_Airspace"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.1, Row 4"
    
  - condition:
      air_traffic_density: [5]
    result: "d"
    rule_ref: "Table_C.1_Row_5_Very_High_Density"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.1, Row 5"
    
  # Note: Thresholds must be extracted from EASA Table C.1 - placeholders used here
  - condition:
      proximity_aerodrome: "Inside"
      operational_altitude_agl_m_gt: 120  # [EXTRACT_FROM_TABLE_C1]
    result: "d"
    rule_ref: "Table_C.1_Aerodrome_Proximity_Inside"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.1, Aerodrome proximity rule"
    notes: "Threshold value to be verified from exact EASA AMC/GM Table C.1"
    
  - condition:
      proximity_aerodrome: "Near"
      operational_altitude_agl_m_gt: 60   # [EXTRACT_FROM_TABLE_C1]
    result: "c"
    rule_ref: "Table_C.1_Aerodrome_Proximity_Near"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.1, Aerodrome proximity rule"
    notes: "Threshold value to be verified from exact EASA AMC/GM Table C.1"

default_by_airspace:
  G: "a"
  E: "b"
  D: "c"
  C: "d"
  CTR: "d"
  TMA: "d"
  TIZ: "c"
  ATZ: "a"
  RMZ: "c"
  TMZ: "c"
  rule_ref: "Table_C.1_Default_Fallback"

strategic_mitigations:
  airspace_segregation:
    Full_Certified:
      reduction: 2
      rule_ref: "Table_C.2_Segregation_Certified"
      doc_ref:
        doc_id: "EASA_EAR_UAS_2024"
        annex: "Annex C"
        section: "Table C.2, Certified segregation"
      notes: "Requires certified containment system meeting Annex C criteria"
    Partial:
      reduction: 1
      rule_ref: "Table_C.2_Segregation_Partial"
      doc_ref:
        doc_id: "EASA_EAR_UAS_2024"
        annex: "Annex C"
        section: "Table C.2, Partial segregation"
    None:
      reduction: 0
      
  operational_limitation_boundary:
    reduction: 1
    rule_ref: "Table_C.2_Boundary_Limitation"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.2, Boundary limitations"
    notes: "Time/area restrictions reducing encounter probability"
    
  operational_limitation_chronology:
    reduction: 1
    rule_ref: "Table_C.2_Chronology_Limitation"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.2, Chronology limitations"
    notes: "Off-peak hours operation with reduced traffic"
    
  procedural_coordination:
    reduction: 1
    rule_ref: "Table_C.2_Procedural_Coordination"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.2, Procedural coordination"
    notes: "ATC or airspace user coordination procedures"
    
  geo_fencing:
    reduction: 0
    role: "supporting_evidence"
    rule_ref: "Table_C.2_Geo_Fencing_Support"
    doc_ref:
      doc_id: "EASA_EAR_UAS_2024"
      annex: "Annex C"
      section: "Table C.2, Note (c)"
    notes: "Electronic geo-fencing provides supporting evidence only - enables claiming reduction within cap when combined with other mitigations"

reduction_caps:
  without_certification: 1
  with_certification: 2
  rule_ref: "Table_C.2_Note_a"
  doc_ref:
    doc_id: "EASA_EAR_UAS_2024"
    annex: "Annex C"
    section: "Table C.2, Note (a)"

arc_floor: "a"
rule_ref: "Table_C.2_Minimum_Floor"
```

### arc_rules_sora_2_5.yaml

```yaml
version: "JARUS_SORA_2.5_Annex_C_v1.0"
source: "JARUS SORA v2.5 - Annex C v1.0: Strategic Mitigation of Air Risk"
publication_date: "2024-01-15"
doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
rules_hash: "sha256:def456ghi789"

initial_arc_rules_25:
  - condition:
      airspace_class: ["G"]
      air_traffic_density: [1, 2]
      u_space_services_available: false
    result: "a"
    rule_ref: "Annex_C_Section_2.1_Low_Density_Class_G"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 2.1"
    
  - condition:
      airspace_class: ["G"]
      air_traffic_density: [1, 2]
      u_space_services_available: true
    result: "a"
    rule_ref: "Annex_C_Section_2.2_U_Space_Low"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 2.2"
    notes: "U-space services may support further reduction through service arrangements"
    
  - condition:
      airspace_class: ["E", "D"]
      air_traffic_density: [3, 4]
    result: "c"
    rule_ref: "Annex_C_Section_2.3_Medium_Density_Controlled"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 2.3"
    
  - condition:
      airspace_class: ["C", "CTR", "TMA"]
    result: "d"
    rule_ref: "Annex_C_Section_2.4_High_Complexity_Airspace"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 2.4"
    
  - condition:
      airspace_class: ["U-space"]
      air_traffic_density: [1, 2, 3]
    result: "b"
    rule_ref: "Annex_C_Section_2.5_U_Space_Designation"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 2.5"

default_by_airspace_25:
  G: "a"
  E: "b"
  D: "c"
  C: "d"
  CTR: "d"
  TMA: "d"
  U-space: "b"
  rule_ref: "Annex_C_Section_2_Default"

strategic_mitigations_25:
  airspace_containment:
    Certified:
      reduction: 2
      rule_ref: "Annex_C_Section_3.1_Containment_Certified"
      doc_ref:
        doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
        annex: "Annex C"
        section: "Section 3.1"
      evidence: "Certified containment system + validation data"
    Operational:
      reduction: 1
      rule_ref: "Annex_C_Section_3.1_Containment_Operational"
      doc_ref:
        doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
        annex: "Annex C"
        section: "Section 3.1"
      evidence: "Operational data showing containment effectiveness"
    None:
      reduction: 0
      
  temporal_segregation:
    reduction: 1
    rule_ref: "Annex_C_Section_3.2_Temporal"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 3.2"
    evidence: "Traffic data showing <50% nominal density during operation"
    
  spatial_segregation:
    reduction: 1
    rule_ref: "Annex_C_Section_3.3_Spatial"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 3.3"
    evidence: "Geographic separation data + coordination agreements"
    
  u_space_services:
    reduction: 0  # Eligible for up to 1 class reduction within cap
    role: "service_arrangement_evidence"
    rule_ref: "Annex_C_Section_3.5_U_Space"
    doc_ref:
      doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
      annex: "Annex C"
      section: "Section 3.5"
    evidence: "U-space service provider SLA + tracking data + competent authority acceptance"
    notes: "U-space services may support claiming up to 1 class reduction under service arrangements, subject to cap"

reduction_caps_25:
  without_certification: 1
  with_certification: 2
  with_u_space: 2  # U-space equivalent to certification
  rule_ref: "Annex_C_Section_3.6_Caps"
  doc_ref:
    doc_id: "JARUS_SORA_v2.5_Annex_C_v1.0"
    annex: "Annex C"
    section: "Section 3.6"

arc_floor: "a"
```

## 2. Python Models

### arc_models.py

```python
from pydantic import BaseModel, Field, validator
from enum import Enum
from typing import Optional, List, Dict, Any
from datetime import datetime

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
    timestamp: str = Field(default_factory=lambda: datetime.utcnow().isoformat())
    notes: Optional[str] = Field(None, description="Additional context")

class ARCInputs20(BaseModel):
    """SORA 2.0 ARC calculation inputs"""
    version: str = Field(default="SORA_2.0", const=True)
    operation_type: OperationType
    airspace_class: AirspaceClass20
    air_traffic_density: int = Field(ge=1, le=5, description="1=Very Low, 5=Very High")
    proximity_aerodrome: ProximityAerodrome
    operational_altitude_agl_m: int = Field(ge=0, le=500, description="Altitude Above Ground Level in meters")
    time_of_operation: Optional[TimeOfOperation] = None
    operational_volume_defined: bool = Field(..., description="Is 3D operational volume clearly defined?")
    adjacent_airspace_characterised: bool = Field(..., description="Adjacent airspace risks assessed?")
    
    # Strategic Mitigations
    airspace_segregation: AirspaceSegregation = AirspaceSegregation.NONE
    operational_limitation_boundary: bool = Field(False, description="Time/area restrictions applied")
    operational_limitation_chronology: bool = Field(False, description="Off-peak hours restriction")
    procedural_coordination: bool = Field(False, description="ATC coordination in place")
    geo_fencing: bool = Field(False, description="Electronic geo-fencing (supporting evidence only)")

    @validator('operation_type', 'airspace_class', pre=True)
    def normalize_case(cls, v):
        """Normalize case for enum matching"""
        if isinstance(v, str):
            return v.upper() if v in ['G', 'E', 'D', 'C'] else v.title().replace(' ', '_').replace('-', '_')
        return v

    class Config:
        use_enum_values = True
        schema_extra = {
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

class ARCInputs25(BaseModel):
    """SORA 2.5 ARC calculation inputs"""
    version: str = Field(default="SORA_2.5", const=True)
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

    @validator('traffic_density_data_source')
    def validate_density_data_source(cls, v, values):
        """Expert opinion not sufficient for medium/high density"""
        air_traffic_density = values.get('air_traffic_density', 1)
        if air_traffic_density > 2 and v == TrafficDensityDataSource.EXPERT:
            raise ValueError("Medium/High traffic density requires Empirical or Statistical data, not Expert opinion")
        return v

    @validator('u_space_services')
    def validate_u_space_services(cls, v, values):
        """U-space services require U-space airspace"""
        if v and values.get('airspace_class') != AirspaceClass25.U_SPACE:
            raise ValueError("U-space services only available in designated U-space airspace")
        return v

    class Config:
        use_enum_values = True

class ARCResult(BaseModel):
    """ARC calculation result"""
    version: str = Field(..., description="SORA version used")
    initial_arc: ARCValue = Field(..., description="Initial ARC before mitigations")
    residual_arc: ARCValue = Field(..., description="Final ARC after strategic mitigations")
    total_reduction: int = Field(ge=0, le=3, description="Total classes reduced (integer only)")
    mitigations_applied: List[str] = Field(..., description="List of applied mitigation names")
    supporting_evidence: List[str] = Field(default_factory=list, description="Supporting evidence items")
    calculation_trace: List[TraceEntry] = Field(..., description="Detailed calculation trace")
    
    class Config:
        schema_extra = {
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
```

## 3. Core Calculator

### arc_calculator.py

```python
import yaml
from pathlib import Path
from typing import Dict, List, Tuple, Optional, Any
from datetime import datetime
import logging

from .arc_models import (
    ARCInputs20, ARCInputs25, ARCResult, TraceEntry, ARCValue
)

logger = logging.getLogger(__name__)

class ARCCalculator:
    """Base ARC Calculator with common functionality"""
    
    def __init__(self, rules_path: Path = None):
        if rules_path is None:
            rules_path = Path(__file__).parent / "rules"
        self.rules_path = rules_path
        
    def load_yaml_rules(self, filename: str) -> Dict[str, Any]:
        """Load YAML rule file"""
        file_path = self.rules_path / filename
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                return yaml.safe_load(f)
        except FileNotFoundError:
            logger.error(f"Rule file not found: {file_path}")
            raise
        except yaml.YAMLError as e:
            logger.error(f"YAML parsing error in {file_path}: {e}")
            raise
    
    def arc_to_numeric(self, arc: str) -> int:
        """Convert ARC letter to numeric value"""
        mapping = {"a": 0, "b": 1, "c": 2, "d": 3}
        return mapping.get(arc.lower(), 0)
    
    def numeric_to_arc(self, value: int) -> str:
        """Convert numeric value to ARC letter"""
        mapping = {0: "a", 1: "b", 2: "c", 3: "d"}
        return mapping.get(max(0, min(3, int(value))), "a")
    
    def evaluate_condition(self, condition: Dict[str, Any], inputs: Dict[str, Any]) -> bool:
        """Evaluate a rule condition against inputs"""
        for key, expected in condition.items():
            if key.endswith('_gt'):
                # Greater than condition (e.g., operational_altitude_agl_m_gt)
                base_key = key[:-3]  # Remove '_gt' suffix
                if base_key not in inputs:
                    continue
                if isinstance(expected, list):
                    if not any(inputs[base_key] > val for val in expected):
                        return False
                else:
                    if inputs[base_key] <= expected:
                        return False
            elif key.endswith('_lt'):
                # Less than condition
                base_key = key[:-3]  # Remove '_lt' suffix
                if base_key not in inputs:
                    continue
                if isinstance(expected, list):
                    if not any(inputs[base_key] < val for val in expected):
                        return False
                else:
                    if inputs[base_key] >= expected:
                        return False
            else:
                # Exact match condition
                if key not in inputs:
                    continue
                input_val = inputs[key]
                if isinstance(expected, list):
                    if input_val not in expected:
                        return False
                else:
                    if input_val != expected:
                        return False
        return True


class ARCCalculator20(ARCCalculator):
    """SORA 2.0 ARC Calculator"""
    
    def __init__(self, rules_path: Path = None):
        super().__init__(rules_path)
        self.rules = self.load_yaml_rules("arc_rules_sora_2_0.yaml")
    
    def calculate(self, inputs: ARCInputs20) -> ARCResult:
        """Calculate ARC for SORA 2.0"""
        inputs_dict = inputs.dict()
        trace = []
        
        # Step 1: Determine Initial ARC
        initial_arc, initial_trace = self.determine_initial_arc(inputs_dict)
        trace.extend(initial_trace)
        
        # Step 2: Apply Strategic Mitigations
        residual_arc, reduction, mitigation_trace, mitigations_applied, supporting_evidence = \
            self.apply_strategic_mitigations(initial_arc, inputs_dict)
        trace.extend(mitigation_trace)
        
        return ARCResult(
            version=self.rules["version"],
            initial_arc=ARCValue(initial_arc),
            residual_arc=ARCValue(residual_arc),
            total_reduction=reduction,
            mitigations_applied=mitigations_applied,
            supporting_evidence=supporting_evidence,
            calculation_trace=trace
        )
    
    def determine_initial_arc(self, inputs: Dict[str, Any]) -> Tuple[str, List[TraceEntry]]:
        """Determine Initial ARC per EASA Table C.1"""
        trace = []
        
        # Evaluate rules in order
        for rule in self.rules["initial_arc_rules"]:
            if self.evaluate_condition(rule["condition"], inputs):
                trace_entry = TraceEntry(
                    step="initial_arc_determination",
                    inputs={k: v for k, v in inputs.items() if k in rule["condition"]},
                    result=rule["result"],
                    rule_ref=rule["rule_ref"],
                    doc_ref=rule["doc_ref"],
                    notes=rule.get("notes")
                )
                trace.append(trace_entry)
                logger.info(f"Initial ARC determined: {rule['result']} via {rule['rule_ref']}")
                return rule["result"], trace
        
        # Fallback to default by airspace
        default_arc = self.rules["default_by_airspace"].get(inputs["airspace_class"], "b")
        trace_entry = TraceEntry(
            step="initial_arc_determination_fallback",
            inputs={"airspace_class": inputs["airspace_class"]},
            result=default_arc,
            rule_ref=self.rules["default_by_airspace"]["rule_ref"],
            doc_ref={
                "doc_id": self.rules["doc_id"],
                "annex": "Annex C",
                "section": "Default by airspace class"
            },
            notes="Fallback rule applied - no specific condition matched"
        )
        trace.append(trace_entry)
        logger.info(f"Initial ARC via fallback: {default_arc}")
        return default_arc, trace
    
    def apply_strategic_mitigations(self, initial_arc: str, inputs: Dict[str, Any]) -> \
            Tuple[str, int, List[TraceEntry], List[str], List[str]]:
        """Apply strategic mitigations per EASA Table C.2"""
        trace = []
        mitigations_applied = []
        supporting_evidence = []
        total_reduction = 0
        
        mitigations = self.rules["strategic_mitigations"]
        
        # 1. Airspace Segregation
        segregation = inputs.get("airspace_segregation", "None")
        if segregation in mitigations["airspace_segregation"]:
            seg_config = mitigations["airspace_segregation"][segregation]
            reduction = seg_config["reduction"]
            if reduction > 0:
                total_reduction += reduction
                mitigations_applied.append(f"{segregation}_Segregation")
                
                trace.append(TraceEntry(
                    step="strategic_mitigation_segregation",
                    inputs={"airspace_segregation": segregation},
                    result=f"Reduction: {reduction}",
                    rule_ref=seg_config["rule_ref"],
                    doc_ref=seg_config["doc_ref"],
                    delta=-reduction,
                    notes=seg_config.get("notes")
                ))
        
        # 2. Operational Limitations - Boundary
        if inputs.get("operational_limitation_boundary", False):
            boundary_config = mitigations["operational_limitation_boundary"]
            reduction = boundary_config["reduction"]
            supporting_evidence.append("Boundary_Limitation")
            
            trace.append(TraceEntry(
                step="strategic_mitigation_boundary",
                inputs={"operational_limitation_boundary": True},
                result=f"Supporting evidence: Boundary limitation",
                rule_ref=boundary_config["rule_ref"],
                doc_ref=boundary_config["doc_ref"],
                delta=0,  # Supporting evidence only within cap
                notes="Supporting evidence for reduction within cap"
            ))
        
        # 3. Operational Limitations - Chronology
        if inputs.get("operational_limitation_chronology", False):
            chrono_config = mitigations["operational_limitation_chronology"]
            supporting_evidence.append("Chronology_Limitation")
            
            trace.append(TraceEntry(
                step="strategic_mitigation_chronology",
                inputs={"operational_limitation_chronology": True},
                result=f"Supporting evidence: Chronology limitation",
                rule_ref=chrono_config["rule_ref"],
                doc_ref=chrono_config["doc_ref"],
                delta=0,
                notes="Supporting evidence for reduction within cap"
            ))
        
        # 4. Procedural Coordination
        if inputs.get("procedural_coordination", False):
            proc_config = mitigations["procedural_coordination"]
            supporting_evidence.append("Procedural_Coordination")
            
            trace.append(TraceEntry(
                step="strategic_mitigation_procedural",
                inputs={"procedural_coordination": True},
                result=f"Supporting evidence: Procedural coordination",
                rule_ref=proc_config["rule_ref"],
                doc_ref=proc_config["doc_ref"],
                delta=0,
                notes="Supporting evidence for reduction within cap"
            ))
        
        # 5. Geo-fencing (Supporting Evidence Only)
        if inputs.get("geo_fencing", False):
            geo_config = mitigations["geo_fencing"]
            supporting_evidence.append("Geo_fencing")
            
            trace.append(TraceEntry(
                step="strategic_mitigation_geofencing",
                inputs={"geo_fencing": True},
                result="Supporting evidence: Geo-fencing",
                rule_ref=geo_config["rule_ref"],
                doc_ref=geo_config["doc_ref"],
                delta=0,
                notes=geo_config["notes"]
            ))
        
        # Apply Reduction Caps
        caps = self.rules["reduction_caps"]
        if segregation == "Full_Certified":
            cap = caps["with_certification"]
        else:
            cap = caps["without_certification"]
        
        capped_reduction = min(total_reduction, cap)
        
        if capped_reduction != total_reduction:
            trace.append(TraceEntry(
                step="reduction_cap_applied",
                inputs={"total_calculated_reduction": total_reduction, "cap": cap},
                result=f"Reduction capped at {capped_reduction}",
                rule_ref=caps["rule_ref"],
                doc_ref=caps["doc_ref"],
                delta=-(total_reduction - capped_reduction),
                notes=f"Applied cap: {cap} classes maximum"
            ))
        
        # Calculate residual ARC
        initial_numeric = self.arc_to_numeric(initial_arc)
        residual_numeric = max(0, initial_numeric - capped_reduction)  # Floor at 'a'
        residual_arc = self.numeric_to_arc(residual_numeric)
        
        # Final calculation trace
        trace.append(TraceEntry(
            step="residual_arc_calculation",
            inputs={
                "initial_arc": initial_arc,
                "total_reduction": capped_reduction
            },
            result=residual_arc,
            rule_ref=f"{caps['rule_ref']}_Final_Calculation",
            doc_ref=caps["doc_ref"],
            notes=f"Initial {initial_arc} - {capped_reduction} = {residual_arc}"
        ))
        
        logger.info(f"Strategic mitigations applied: {mitigations_applied}")
        logger.info(f"Supporting evidence: {supporting_evidence}")
        logger.info(f"Total reduction: {capped_reduction} (cap: {cap})")
        
        return residual_arc, capped_reduction, trace, mitigations_applied, supporting_evidence


class ARCCalculator25(ARCCalculator):
    """SORA 2.5 ARC Calculator"""
    
    def __init__(self, rules_path: Path = None):
        super().__init__(rules_path)
        self.rules = self.load_yaml_rules("arc_rules_sora_2_5.yaml")
    
    def calculate(self, inputs: ARCInputs25) -> ARCResult:
        """Calculate ARC for SORA 2.5"""
        inputs_dict = inputs.dict()
        trace = []
        
        # Step 1: Determine Initial ARC
        initial_arc, initial_trace = self.determine_initial_arc(inputs_dict)
        trace.extend(initial_trace)
        
        # Step 2: Apply Strategic Mitigations
        residual_arc, reduction, mitigation_trace, mitigations_applied, supporting_evidence = \
            self.apply_strategic_mitigations_25(initial_arc, inputs_dict)
        trace.extend(mitigation_trace)
        
        return ARCResult(
            version=self.rules["version"],
            initial_arc=ARCValue(initial_arc),
            residual_arc=ARCValue(residual_arc),
            total_reduction=reduction,
            mitigations_applied=mitigations_applied,
            supporting_evidence=supporting_evidence,
            calculation_trace=trace
        )
    
    def determine_initial_arc(self, inputs: Dict[str, Any]) -> Tuple[str, List[TraceEntry]]:
        """Determine Initial ARC per JARUS SORA 2.5"""
        trace = []
        
        # Evaluate rules in order
        for rule in self.rules["initial_arc_rules_25"]:
            if self.evaluate_condition(rule["condition"], inputs):
                trace_entry = TraceEntry(
                    step="initial_arc_determination_25",
                    inputs={k: v for k, v in inputs.items() if k in rule["condition"]},
                    result=rule["result"],
                    rule_ref=rule["rule_ref"],
                    doc_ref=rule["doc_ref"],
                    notes=rule.get("notes")
                )
                trace.append(trace_entry)
                return rule["result"], trace
        
        # Fallback
        default_arc = self.rules["default_by_airspace_25"].get(inputs["airspace_class"], "b")
        trace_entry = TraceEntry(
            step="initial_arc_determination_fallback_25",
            inputs={"airspace_class": inputs["airspace_class"]},
            result=default_arc,
            rule_ref=self.rules["default_by_airspace_25"]["rule_ref"],
            doc_ref={
                "doc_id": self.rules["doc_id"],
                "annex": "Annex C",
                "section": "Section 2 Default"
            }
        )
        trace.append(trace_entry)
        return default_arc, trace
    
    def apply_strategic_mitigations_25(self, initial_arc: str, inputs: Dict[str, Any]) -> \
            Tuple[str, int, List[TraceEntry], List[str], List[str]]:
        """Apply SORA 2.5 strategic mitigations per Annex C"""
        trace = []
        mitigations_applied = []
        supporting_evidence = []
        total_reduction = 0
        
        mitigations = self.rules["strategic_mitigations_25"]
        
        # 1. Airspace Containment
        containment = inputs.get("airspace_containment", "None")
        if containment in mitigations["airspace_containment"]:
            contain_config = mitigations["airspace_containment"][containment]
            reduction = contain_config["reduction"]
            if reduction > 0:
                total_reduction += reduction
                mitigations_applied.append(f"{containment}_Containment")
                
                trace.append(TraceEntry(
                    step="strategic_mitigation_containment_25",
                    inputs={"airspace_containment": containment},
                    result=f"Reduction: {reduction}",
                    rule_ref=contain_config["rule_ref"],
                    doc_ref=contain_config["doc_ref"],
                    delta=-reduction,
                    notes=contain_config.get("evidence")
                ))
        
        # 2. Temporal Segregation
        if inputs.get("temporal_segregation", False):
            temporal_config = mitigations["temporal_segregation"]
            supporting_evidence.append("Temporal_Segregation")
            
            trace.append(TraceEntry(
                step="strategic_mitigation_temporal_25",
                inputs={"temporal_segregation": True},
                result="Supporting evidence: Temporal segregation",
                rule_ref=temporal_config["rule_ref"],
                doc_ref=temporal_config["doc_ref"],
                delta=0,
                notes=temporal_config.get("evidence")
            ))
        
        # 3. Spatial Segregation
        if inputs.get("spatial_segregation", False):
            spatial_config = mitigations["spatial_segregation"]
            supporting_evidence.append("Spatial_Segregation")
            
            trace.append(TraceEntry(
                step="strategic_mitigation_spatial_25",
                inputs={"spatial_segregation": True},
                result="Supporting evidence: Spatial segregation",
                rule_ref=spatial_config["rule_ref"],
                doc_ref=spatial_config["doc_ref"],
                delta=0,
                notes=spatial_config.get("evidence")
            ))
        
        # 4. U-space Services (Supporting Evidence)
        if inputs.get("u_space_services", False):
            uspace_config = mitigations["u_space_services"]
            supporting_evidence.append("U_space_Services")
            
            trace.append(TraceEntry(
                step="strategic_mitigation_uspace_25",
                inputs={"u_space_services": True},
                result="Supporting evidence: U-space services",
                rule_ref=uspace_config["rule_ref"],
                doc_ref=uspace_config["doc_ref"],
                delta=0,
                notes=uspace_config["notes"]
            ))
        
        # Apply Reduction Caps
        caps = self.rules["reduction_caps_25"]
        if containment == "Certified":
            cap = caps["with_certification"]
        elif inputs.get("u_space_services", False) and inputs.get("airspace_class") == "U-space":
            cap = caps["with_u_space"]
        else:
            cap = caps["without_certification"]
        
        capped_reduction = min(total_reduction, cap)
        
        if capped_reduction != total_reduction:
            trace.append(TraceEntry(
                step="reduction_cap_applied_25",
                inputs={"total_calculated_reduction": total_reduction, "cap": cap},
                result=f"Reduction capped at {capped_reduction}",
                rule_ref=caps["rule_ref"],
                doc_ref=caps["doc_ref"],
                delta=-(total_reduction - capped_reduction)
            ))
        
        # Calculate residual ARC
        initial_numeric = self.arc_to_numeric(initial_arc)
        residual_numeric = max(0, initial_numeric - capped_reduction)
        residual_arc = self.numeric_to_arc(residual_numeric)
        
        trace.append(TraceEntry(
            step="residual_arc_calculation_25",
            inputs={
                "initial_arc": initial_arc,
                "total_reduction": capped_reduction
            },
            result=residual_arc,
            rule_ref=f"{caps['rule_ref']}_Final_Calculation",
            doc_ref=caps["doc_ref"]
        ))
        
        return residual_arc, capped_reduction, trace, mitigations_applied, supporting_evidence
```

## 4. Validator

### arc_validator.py

```python
from typing import Dict, Any, List
from pydantic import ValidationError

class ARCValidator:
    """ARC input validation logic"""
    
    @staticmethod
    def validate_sora_20_inputs(inputs: Dict[str, Any]) -> List[str]:
        """Validate SORA 2.0 specific business rules"""
        errors = []
        
        # Proximity + altitude validation
        if inputs.get("proximity_aerodrome") == "Inside" and inputs.get("operational_altitude_agl_m") is None:
            errors.append("Operational altitude required when inside aerodrome ATZ")
        
        # Certified segregation evidence
        if inputs.get("airspace_segregation") == "Full_Certified":
            # In real implementation, check for evidence documentation
            pass  # Would check evidence_provided field
        
        # Altitude range
        altitude = inputs.get("operational_altitude_agl_m", 0)
        if altitude < 0 or altitude > 500:
            errors.append("Altitude must be 0-500m AGL for SORA assessment")
        
        # Traffic density range
        density = inputs.get("air_traffic_density", 1)
        if density not in [1, 2, 3, 4, 5]:
            errors.append("Air traffic density must be 1-5")
            
        return errors
    
    @staticmethod
    def validate_sora_25_inputs(inputs: Dict[str, Any]) -> List[str]:
        """Validate SORA 2.5 specific business rules"""
        errors = ARCValidator.validate_sora_20_inputs(inputs)  # Base validation
        
        # Traffic density data source validation
        density = inputs.get("air_traffic_density", 1)
        data_source = inputs.get("traffic_density_data_source")
        
        if density > 2 and data_source == "Expert":
            errors.append("Medium/High traffic density requires Empirical or Statistical data, not Expert opinion")
        
        # U-space services validation
        if inputs.get("u_space_services", False) and inputs.get("airspace_class") != "U-space":
            errors.append("U-space services only available in designated U-space airspace")
            
        return errors
    
    @staticmethod
    def validate_arc_result(result: Dict[str, Any]) -> List[str]:
        """Validate ARC calculation result"""
        errors = []
        
        # ARC values validation
        valid_arcs = ["a", "b", "c", "d"]
        if result.get("initial_arc") not in valid_arcs:
            errors.append("Invalid initial ARC value")
        
        if result.get("residual_arc") not in valid_arcs:
            errors.append("Invalid residual ARC value")
        
        # Reduction validation
        reduction = result.get("total_reduction", 0)
        if reduction < 0 or reduction > 3:
            errors.append("Total reduction must be 0-3 classes")
        
        # Integer reduction validation
        if not isinstance(reduction, int):
            errors.append("Total reduction must be integer (no fractional classes)")
            
        return errors
```

## 5. FastAPI Endpoints

### arc_api.py

```python
from fastapi import APIRouter, HTTPException, status
from pathlib import Path
import logging

from .arc_models import ARCInputs20, ARCInputs25, ARCResult
from .arc_calculator import ARCCalculator20, ARCCalculator25
from .arc_validator import ARCValidator

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/sora", tags=["ARC - Air Risk Class"])

# Initialize calculators
calc_20 = ARCCalculator20()
calc_25 = ARCCalculator25()

@router.post("/2.0/arc", response_model=ARCResult, status_code=200)
async def calculate_arc_sora_20(request: ARCInputs20):
    """
    Calculate ARC (Air Risk Class) per EASA SORA 2.0
    
    Determines Initial ARC from operational parameters and applies strategic 
    mitigations with strict reduction caps per EASA AMC/GM Annex C.
    
    Key Rules:
    - Initial ARC (a-d) determined from Table C.1
    - Strategic mitigations applied per Table C.2  
    - Default cap: ≤1 class reduction
    - With certified segregation: ≤2 classes
    - Geo-fencing = supporting evidence only (0 direct reduction)
    - Returns residual ARC + full calculation trace
    """
    try:
        # Additional validation
        validation_errors = ARCValidator.validate_sora_20_inputs(request.dict())
        if validation_errors:
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail={"errors": validation_errors}
            )
        
        # Calculate ARC
        result = calc_20.calculate(request)
        
        # Validate result
        result_errors = ARCValidator.validate_arc_result(result.dict())
        if result_errors:
            logger.error(f"Invalid ARC calculation result: {result_errors}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal calculation error"
            )
        
        logger.info(f"SORA 2.0 ARC calculated: {result.initial_arc} -> {result.residual_arc}")
        return result
        
    except ValidationError as e:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"ARC 2.0 calculation failed: {e}", extra={"request": request.dict()})
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal calculation error"
        )

@router.post("/2.5/arc", response_model=ARCResult, status_code=200)
async def calculate_arc_sora_25(request: ARCInputs25):
    """
    Calculate ARC per JARUS SORA 2.5 Annex C
    
    Enhanced strategic mitigation mechanisms with:
    - U-space services support (as service arrangement evidence)
    - Data-driven traffic density assessment
    - Improved containment mechanisms
    - Full traceability to JARUS Annex C
    
    Key Rules:
    - U-space services = supporting evidence within cap (not automatic -1)
    - Certified containment ≤2 classes
    - DAA excluded from strategic ARC (tactical layer)
    - Full integer reductions only
    """
    try:
        # Additional validation
        validation_errors = ARCValidator.validate_sora_25_inputs(request.dict())
        if validation_errors:
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail={"errors": validation_errors}
            )
        
        # Calculate ARC
        result = calc_25.calculate(request)
        
        # Validate result
        result_errors = ARCValidator.validate_arc_result(result.dict())
        if result_errors:
            logger.error(f"Invalid ARC calculation result: {result_errors}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Internal calculation error"
            )
        
        logger.info(f"SORA 2.5 ARC calculated: {result.initial_arc} -> {result.residual_arc}")
        return result
        
    except ValidationError as e:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"ARC 2.5 calculation failed: {e}", extra={"request": request.dict()})
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal calculation error"
        )

@router.get("/arc/versions", status_code=200)
async def get_arc_versions():
    """Get available ARC calculation versions and rule information"""
    return {
        "versions": {
            "SORA_2.0": {
                "source": "EASA_EAR_UAS_2024",
                "publication_date": "2024-07-01",
                "annex_c_version": "AMC1_Article_11_2024-07",
                "rules_hash": "sha256:abc123def456",
                "endpoint": "/sora/2.0/arc"
            },
            "SORA_2.5": {
                "source": "JARUS_SORA_v2.5_Annex_C_v1.0", 
                "publication_date": "2024-01-15",
                "annex_c_version": "v1.0",
                "rules_hash": "sha256:def456ghi789",
                "endpoint": "/sora/2.5/arc"
            }
        },
        "supported_features": {
            "initial_arc_determination": True,
            "strategic_mitigations": True,
            "calculation_trace": True,
            "validation": True,
            "integer_reductions_only": True,
            "daa_excluded_from_strategic": True
        }
    }
```

## 6. Test Files

### test_arc_calculator.py

```python
import pytest
from pathlib import Path

from sora_calc.arc_models import ARCInputs20, ARCInputs25, ARCValue
from sora_calc.arc_calculator import ARCCalculator20, ARCCalculator25
from sora_calc.arc_validator import ARCValidator

class TestARCCalculator20:
    """Test SORA 2.0 ARC calculations"""
    
    @pytest.fixture
    def calculator(self):
        return ARCCalculator20()
    
    def test_golden_case_1_low_risk_vlos(self, calculator):
        """Golden Test Case 1: SORA 2.0 - Low Risk VLOS"""
        inputs = ARCInputs20(
            operation_type="VLOS",
            airspace_class="G",
            air_traffic_density=1,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=50,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="None",
            operational_limitation_boundary=False,
            operational_limitation_chronology=False,
            procedural_coordination=False,
            geo_fencing=False
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.A
        assert result.residual_arc == ARCValue.A
        assert result.total_reduction == 0
        assert result.mitigations_applied == []
        assert len(result.calculation_trace) >= 1
        
        # Verify trace contains proper references
        initial_trace = result.calculation_trace[0]
        assert initial_trace.step == "initial_arc_determination"
        assert "EASA_EAR_UAS_2024" in initial_trace.doc_ref["doc_id"]
    
    def test_golden_case_2_high_risk_with_mitigations(self, calculator):
        """Golden Test Case 2: SORA 2.0 - High Risk with Mitigations"""
        inputs = ARCInputs20(
            operation_type="BVLOS",
            airspace_class="E",
            air_traffic_density=4,
            proximity_aerodrome="Near",
            operational_altitude_agl_m=150,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Full_Certified",
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=False,
            geo_fencing=True
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.C
        assert result.residual_arc == ARCValue.A  # Reduced by 2 classes (certified cap)
        assert result.total_reduction == 2
        assert "Full_Certified_Segregation" in result.mitigations_applied
        assert "Boundary_Limitation" in result.supporting_evidence
        assert "Chronology_Limitation" in result.supporting_evidence
        assert "Geo_fencing" in result.supporting_evidence
        
        # Verify geo-fencing is supporting evidence only (0 direct reduction)
        geo_trace = [t for t in result.calculation_trace if t.step == "strategic_mitigation_geofencing"]
        assert len(geo_trace) == 1
        assert geo_trace[0].delta == 0
    
    def test_reduction_cap_without_certification(self, calculator):
        """Test reduction cap enforcement without certification"""
        inputs = ARCInputs20(
            operation_type="BVLOS",
            airspace_class="D",
            air_traffic_density=4,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=100,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Partial",  # Only 1 class allowed
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=True,
            geo_fencing=False
        )
        
        result = calculator.calculate(inputs)
        
        # Should be capped at 1 class reduction without certification
        assert result.total_reduction <= 1
        
        # Check cap was applied
        cap_trace = [t for t in result.calculation_trace if t.step == "reduction_cap_applied"]
        if len(cap_trace) > 0:
            assert cap_trace[0].delta < 0  # Negative delta indicates reduction was capped
    
    def test_arc_floor_never_below_a(self, calculator):
        """Test that ARC never goes below 'a' regardless of mitigations"""
        inputs = ARCInputs20(
            operation_type="VLOS",
            airspace_class="G",
            air_traffic_density=1,  # Already at 'a'
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=30,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Full_Certified",  # 2 class reduction
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=True,
            geo_fencing=True
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.A
        assert result.residual_arc == ARCValue.A  # Cannot go below 'a'
        assert result.total_reduction >= 0
    
    def test_geo_fencing_supporting_evidence_only(self, calculator):
        """Test geo-fencing provides 0 direct reduction"""
        inputs = ARCInputs20(
            operation_type="EVLOS",
            airspace_class="E",
            air_traffic_density=3,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=120,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="None",
            operational_limitation_boundary=False,
            operational_limitation_chronology=False,
            procedural_coordination=False,
            geo_fencing=True  # Only geo-fencing enabled
        )
        
        result = calculator.calculate(inputs)
        
        # Geo-fencing alone should provide 0 reduction
        assert result.total_reduction == 0
        assert "Geo_fencing" in result.supporting_evidence
        assert result.mitigations_applied == []  # No direct mitigations
    
    def test_integer_reductions_only(self, calculator):
        """Test that all reductions are integers (no fractional)"""
        inputs = ARCInputs20(
            operation_type="BVLOS",
            airspace_class="C",
            air_traffic_density=5,
            proximity_aerodrome="Inside",
            operational_altitude_agl_m=200,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Full_Certified",
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=True,
            geo_fencing=True
        )
        
        result = calculator.calculate(inputs)
        
        # Total reduction must be integer
        assert isinstance(result.total_reduction, int)
        assert result.total_reduction == int(result.total_reduction)
        
        # All trace deltas must be integers
        for trace_entry in result.calculation_trace:
            if trace_entry.delta is not None:
                assert isinstance(trace_entry.delta, int)


class TestARCCalculator25:
    """Test SORA 2.5 ARC calculations"""
    
    @pytest.fixture
    def calculator(self):
        return ARCCalculator25()
    
    def test_golden_case_3_u_space_operation(self, calculator):
        """Golden Test Case 3: SORA 2.5 - U-space Operation"""
        inputs = ARCInputs25(
            operation_type="BVLOS",
            airspace_class="U-space",
            air_traffic_density=3,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=100,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            u_space_services_available=True,
            traffic_density_data_source="Empirical",
            airspace_containment="Certified",
            temporal_segregation=False,
            spatial_segregation=False,
            u_space_services=True
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.B
        assert result.residual_arc == ARCValue.A  # Reduced by certified containment
        assert result.total_reduction <= 2  # Certified cap
        assert "Certified_Containment" in result.mitigations_applied
        assert "U_space_Services" in result.supporting_evidence
        
        # U-space should be supporting evidence, not direct reduction
        uspace_trace = [t for t in result.calculation_trace if t.step == "strategic_mitigation_uspace_25"]
        if uspace_trace:
            assert uspace_trace[0].delta == 0
    
    def test_u_space_services_supporting_evidence(self, calculator):
        """Test U-space services as supporting evidence within cap"""
        inputs = ARCInputs25(
            operation_type="EVLOS",
            airspace_class="U-space",
            air_traffic_density=2,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=80,
            operational_volume_defined=True,
            adjacent_airspace_characterise
