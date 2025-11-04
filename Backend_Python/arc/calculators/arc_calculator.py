import yaml
from pathlib import Path
from typing import Dict, List, Tuple, Optional, Any
from datetime import datetime
import logging

from arc.models.arc_models import (
    ARCInputs20, ARCInputs25, ARCResult, TraceEntry, ARCValue
)

logger = logging.getLogger(__name__)

class ARCCalculator:
    """Base ARC Calculator with common functionality"""
    
    def __init__(self, rules_path: Path = None):
        if rules_path is None:
            # Rules are in arc/rules/, not arc/calculators/rules/
            rules_path = Path(__file__).parent.parent / "rules"
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
        inputs_dict = inputs.model_dump()
        trace = []
        
        # FIX #3: Normalize environment to binary Urban/Rural (Suburban→Urban, Industrial→Urban)
        if "environment" in inputs_dict and inputs_dict["environment"]:
            env = inputs_dict["environment"].strip().lower()
            if env in ["suburban", "industrial"]:
                inputs_dict["environment"] = "Urban"
                trace.append(TraceEntry(
                    step="environment_normalization",
                    inputs={"original_environment": inputs.environment},
                    result="Urban",
                    rule_ref="SORA_2.5_Step_4_Binary_Environment",
                    doc_ref={"doc_id": "SORA_2.5_Main_Body", "section": "Step 4"},
                    notes=f"FIX: Normalized {inputs.environment} → Urban per SORA 2.5 binary rule"
                ))
            elif env == "rural":
                inputs_dict["environment"] = "Rural"
            elif env == "urban":
                inputs_dict["environment"] = "Urban"
        
        # FIX #2: Convert altitude to meters if provided in feet
        if "altitude_agl_ft" in inputs_dict and inputs_dict["altitude_agl_ft"] is not None:
            inputs_dict["altitude_agl_m"] = inputs_dict["altitude_agl_ft"] / 3.28084
        
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
        inputs_dict = inputs.model_dump()
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
