from typing import List, Dict, Any
import yaml
import os
from pathlib import Path

class GRCValidator:
    """Validation logic for GRC inputs and outputs"""
    
    def __init__(self):
        self.rules_20 = self._load_rules("grc_rules_sora_2_0.yaml")
        self.rules_25 = self._load_rules("grc_rules_sora_2_5.yaml")
    
    def _load_rules(self, filename: str) -> Dict[str, Any]:
        """Load YAML rules file"""
        rules_path = Path(__file__).parent.parent / "rules" / filename
        with open(rules_path, 'r') as f:
            return yaml.safe_load(f)
    
    def validate_sora_20_inputs(self, data: Dict[str, Any]) -> List[str]:
        """Validate SORA 2.0 inputs"""
        errors = []
        
        # Population density validation
        pop_density = data.get("population_density_p_km2", 0)
        if pop_density < 0:
            errors.append("Population density cannot be negative")
        
        # M1 level validation
        valid_m1 = ["None", "Low", "Medium", "High"]
        m1_level = data.get("m1_strategic")
        if m1_level not in valid_m1:
            errors.append(f"M1 strategic level must be one of: {valid_m1}")
        
        # M2 level validation
        valid_m2 = ["None", "Low", "High"]
        m2_level = data.get("m2_impact")
        if m2_level not in valid_m2:
            errors.append(f"M2 impact level must be one of: {valid_m2}")
        
        # M3 level validation
        valid_m3 = ["None", "Low", "Medium", "High"]
        m3_level = data.get("m3_erp")
        if m3_level not in valid_m3:
            errors.append(f"M3 ERP level must be one of: {valid_m3}")
        
        # Containment quality validation
        valid_containment = ["Poor", "Adequate", "Good"]
        containment = data.get("containment_quality")
        if containment not in valid_containment:
            errors.append(f"Containment quality must be one of: {valid_containment}")
        
        return errors
    
    def validate_sora_25_inputs(self, data: Dict[str, Any]) -> List[str]:
        """Validate SORA 2.5 inputs with N/A constraint checking"""
        errors = []
        
        # Dimension validation
        dimension = data.get("characteristic_dimension_m", 0)
        if dimension <= 0:
            errors.append("Characteristic dimension must be positive")
        
        # Speed validation
        speed = data.get("max_speed_mps", 0)
        if speed <= 0:
            errors.append("Maximum speed must be positive")
        
        # Population density validation
        pop_density = data.get("population_density_p_km2", 0)
        if pop_density < 0:
            errors.append("Population density cannot be negative")
        
        # N/A constraint validation (CRITICAL for SORA 2.5)
        na_constraints = self.rules_25.get("na_constraints", {})
        
        # M1A validation - High is N/A
        m1a_level = data.get("m1a_sheltering")
        if m1a_level in na_constraints.get("m1a_invalid", []):
            errors.append("M1A High is N/A per JARUS SORA 2.5")
        
        # M1B validation - Low is N/A
        m1b_level = data.get("m1b_operational")
        if m1b_level in na_constraints.get("m1b_invalid", []):
            errors.append("M1B Low is N/A per JARUS SORA 2.5")
        
        # M1C validation - Medium/High are N/A
        m1c_level = data.get("m1c_ground_observation")
        if m1c_level in na_constraints.get("m1c_invalid", []):
            errors.append("M1C Medium/High are N/A per JARUS SORA 2.5")
        
        # M2 validation - Low is N/A
        m2_level = data.get("m2_impact")
        if m2_level in na_constraints.get("m2_invalid", []):
            errors.append("M2 Low is N/A per JARUS SORA 2.5")
        
        return errors
    
    def validate_grc_result(self, result: Dict[str, Any]) -> List[str]:
        """Validate GRC calculation result"""
        errors = []
        
        # Initial GRC validation
        initial_grc = result.get("initial_grc", 0)
        if not (1 <= initial_grc <= 8):
            errors.append("Initial GRC must be between 1 and 8")
        
        # Residual GRC validation
        residual_grc = result.get("residual_grc", 0)
        if not (1 <= residual_grc <= 8):
            errors.append("Residual GRC must be between 1 and 8")
        
        # Floor constraint validation
        if residual_grc < 1:
            errors.append("Residual GRC cannot be less than 1")
        
        # Total reduction validation
        total_reduction = result.get("total_reduction", -1)
        if total_reduction < 0:
            errors.append("Total reduction cannot be negative")
        
        # Trace validation
        trace = result.get("calculation_trace", [])
        if len(trace) < 2:
            errors.append("Calculation trace must have at least initial and final entries")
        
        # Validate all trace entries have required fields
        for i, entry in enumerate(trace):
            if not entry.get("rule_ref"):
                errors.append(f"Trace entry {i} missing rule_ref")
            if not entry.get("doc_ref"):
                errors.append(f"Trace entry {i} missing doc_ref")
        
        return errors