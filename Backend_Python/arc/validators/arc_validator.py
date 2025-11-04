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
