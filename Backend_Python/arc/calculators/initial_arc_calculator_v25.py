"""
SORA 2.5 Initial ARC Calculator
Official Reference: JAR_doc_25 (SORA 2.5 Main Body) - Step #4

This module calculates the Initial Air Risk Class (ARC) for SORA 2.5 evaluations,
including validation of traffic density data sources per JARUS specifications.
"""

from typing import Tuple, Dict
from enum import Enum
import logging

# Assuming these models exist in your Backend_Python/models/arc_models.py
from models.arc_models import (
    ARCInputs25,
    TrafficDensityDataSource,
    AirspaceContainment25
)

logger = logging.getLogger(__name__)


class ValidationResult:
    """Validation result container"""
    def __init__(self, is_valid: bool, error_message: str = ""):
        self.is_valid = is_valid
        self.error_message = error_message
    
    @classmethod
    def success(cls):
        return cls(True, "")
    
    @classmethod
    def fail(cls, message: str):
        return cls(False, message)


def validate_traffic_density_data_source(
    data_source: TrafficDensityDataSource,
    traffic_density: str
) -> ValidationResult:
    """
    Validate that traffic density data source is appropriate for the traffic density level.
    
    Official Rule (JAR_doc_25 Step #4):
    - Empirical data: Valid for all traffic densities (Low/Medium/High)
    - Statistical data: Valid for all traffic densities (Low/Medium/High)
    - Expert judgment: ONLY valid for LOW traffic density
    
    Rationale: Expert judgment lacks sufficient data for accurate classification
    of medium/high density airspace.
    
    Args:
        data_source: The data source used to determine traffic density
        traffic_density: The traffic density level ("Low", "Medium", "High")
        
    Returns:
        ValidationResult indicating if the combination is valid
        
    Raises:
        None - returns validation result
    """
    # Normalize traffic density to handle case variations
    traffic_density_normalized = traffic_density.strip().title()
    
    if data_source == TrafficDensityDataSource.EXPERT:
        if traffic_density_normalized != "Low":
            return ValidationResult.fail(
                f"Expert judgment data source is only valid for LOW traffic density. "
                f"Current density: {traffic_density}. "
                f"You must use Empirical or Statistical data sources for Medium/High density. "
                f"Reference: JAR_doc_25 Step #4 - Determination of Initial ARC"
            )
    
    logger.info(f"Traffic density data source validated: {data_source.value} for {traffic_density_normalized} density")
    return ValidationResult.success()


def get_assurance_level(data_source: TrafficDensityDataSource) -> str:
    """
    Map traffic density data source to assurance level.
    
    Assurance levels affect OSO robustness requirements:
    - Empirical: High assurance (requires high robustness OSO)
    - Statistical: Medium assurance (requires medium robustness OSO)
    - Expert: Low assurance (requires low robustness OSO, LOW density only)
    
    Args:
        data_source: The data source used
        
    Returns:
        Assurance level string ("High", "Medium", "Low")
    """
    mapping = {
        TrafficDensityDataSource.EMPIRICAL: "High",
        TrafficDensityDataSource.STATISTICAL: "Medium",
        TrafficDensityDataSource.EXPERT: "Low"
    }
    return mapping.get(data_source, "Medium")


def categorize_altitude(max_altitude_m: float) -> str:
    """
    Categorize operational altitude into bands.
    
    Altitude bands (SORA 2.5 classification - use 150 m pivot for VLL):
    - Low: 0-150 m AGL
    - Medium: >150 m up to 500 m
    - High: >500 m AGL or AMSL operations
    
    Args:
        max_altitude_m: Maximum altitude in meters
        
    Returns:
        Altitude band string ("Low_Altitude", "Medium_Altitude", "High_Altitude")
    """
    # SORA 2.5 uses a 150 m pivot (see Annex C Table C.1 headings)
    if max_altitude_m <= 150:
        return "Low_Altitude"
    elif max_altitude_m <= 500:
        return "Medium_Altitude"
    else:
        return "High_Altitude"


def calculate_base_arc(
    operational_volume: Dict,
    traffic_density: str
) -> int:
    """
    Calculate base Initial ARC using standard SORA table.
    
    Official Reference: JAR_doc_25 Step #4, Table (simplified version)
    
    The Initial ARC depends on:
    1. Traffic density (Low/Medium/High)
    2. Operational altitude band
    3. Proximity to airports/airways
    4. Airspace class
    
    This is a simplified implementation. Full implementation should use
    complete JAR_doc_25 table with all factors.
    
    Args:
        operational_volume: Dictionary containing:
            - max_altitude: Maximum altitude in meters
            - proximity_to_airport: Distance to nearest airport (km)
            - airspace_class: Airspace class (A-G)
        traffic_density: Traffic density level ("Low", "Medium", "High")
        
    Returns:
        Initial ARC value (1-10)
    """
    # Normalize traffic density
    traffic_density_normalized = traffic_density.strip().title()
    
    # Get altitude band
    max_altitude = operational_volume.get('max_altitude', 120)
    altitude_band = categorize_altitude(max_altitude)
    
    # Simplified ARC table (JAR_doc_25 has more detailed table)
    # Format: (traffic_density, altitude_band) -> ARC
    arc_table = {
        ("Low", "Low_Altitude"): 1,
        ("Low", "Medium_Altitude"): 2,
        ("Low", "High_Altitude"): 3,
        ("Medium", "Low_Altitude"): 3,
        ("Medium", "Medium_Altitude"): 4,
        ("Medium", "High_Altitude"): 5,
        ("High", "Low_Altitude"): 5,
        ("High", "Medium_Altitude"): 6,
        ("High", "High_Altitude"): 8,
    }
    
    key = (traffic_density_normalized, altitude_band)
    base_arc = arc_table.get(key, 4)  # Default to ARC 4 if not found
    
    # Adjust for proximity to airport (if very close, increase ARC)
    proximity_km = operational_volume.get('proximity_to_airport', 999)
    if proximity_km < 5:  # Within 5km of airport
        base_arc = min(10, base_arc + 1)
        logger.info(f"ARC increased by 1 due to proximity to airport (<5km)")
    
    logger.info(
        f"Base ARC calculated: {base_arc} "
        f"(Density: {traffic_density_normalized}, Altitude: {altitude_band})"
    )
    
    return base_arc


def calculate_initial_arc_v25(
    operational_volume: Dict,
    traffic_density: str,
    arc_inputs: ARCInputs25
) -> Tuple[int, str]:
    """
    Calculate Initial ARC for SORA 2.5.
    
    Official Reference: JAR_doc_25 Step #4 - Determination of Initial ARC
    
    Initial ARC depends on:
    1. Intrinsic airspace characteristics (traffic density, airspace class)
    2. Operational volume (size, altitude, proximity to airports)
    3. Data source reliability (affects robustness/assurance level)
    
    Args:
        operational_volume: Dictionary containing operational parameters:
            - max_altitude: Maximum altitude in meters
            - proximity_to_airport: Distance to nearest airport (km)
            - airspace_class: Airspace class (A-G)
        traffic_density: Traffic density level ("Low", "Medium", "High")
        arc_inputs: SORA 2.5 ARC inputs containing:
            - u_space_services_available
            - traffic_density_data_source ← Used for validation
            - airspace_containment
            - temporal_segregation
            - spatial_segregation
    
    Returns:
        Tuple of (initial_arc_value: int, explanation: str)
        
    Raises:
        ValueError: If traffic density data source validation fails
    """
    logger.info("=" * 70)
    logger.info("SORA 2.5 Initial ARC Calculation - Step #4")
    logger.info("=" * 70)
    
    # Step 1: Validate traffic density data source
    validation = validate_traffic_density_data_source(
        arc_inputs.traffic_density_data_source,
        traffic_density
    )
    
    if not validation.is_valid:
        logger.error(f"Validation failed: {validation.error_message}")
        raise ValueError(validation.error_message)
    
    # Step 2: Calculate base Initial ARC from table
    base_arc = calculate_base_arc(operational_volume, traffic_density)
    
    # Step 3: Get assurance level from data source
    assurance_level = get_assurance_level(arc_inputs.traffic_density_data_source)
    
    # Build explanation
    explanation = (
        f"Initial ARC: {base_arc}\n"
        f"├─ Traffic Density: {traffic_density.title()}\n"
        f"├─ Altitude: {operational_volume.get('max_altitude', 'N/A')}m "
        f"({categorize_altitude(operational_volume.get('max_altitude', 120))})\n"
        f"├─ Data Source: {arc_inputs.traffic_density_data_source.value}\n"
        f"└─ Assurance Level: {assurance_level}\n"
        f"\n"
        f"Reference: JAR_doc_25 Step #4 - Determination of Initial ARC"
    )
    
    logger.info(f"\n{explanation}")
    logger.info("=" * 70)
    
    return base_arc, explanation


# Example usage and testing
if __name__ == "__main__":
    # Configure logging for testing
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
    )
    
    # Test case 1: Valid - Empirical data with Medium density
    print("\n### Test Case 1: Empirical + Medium Density (Valid)")
    arc_inputs_1 = ARCInputs25(
        u_space_services_available=False,
        traffic_density_data_source=TrafficDensityDataSource.EMPIRICAL,
        airspace_containment=AirspaceContainment25.NONE,
        temporal_segregation=False,
        spatial_segregation=False
    )
    
    operational_volume_1 = {
        'max_altitude': 120,
        'proximity_to_airport': 10,
        'airspace_class': 'G'
    }
    
    try:
        initial_arc, explanation = calculate_initial_arc_v25(
            operational_volume_1,
            "Medium",
            arc_inputs_1
        )
        print(f"✅ SUCCESS: Initial ARC = {initial_arc}")
        print(explanation)
    except ValueError as e:
        print(f"❌ ERROR: {e}")
    
    # Test case 2: Invalid - Expert data with Medium density
    print("\n### Test Case 2: Expert + Medium Density (Should Fail)")
    arc_inputs_2 = ARCInputs25(
        u_space_services_available=False,
        traffic_density_data_source=TrafficDensityDataSource.EXPERT,
        airspace_containment=AirspaceContainment25.NONE,
        temporal_segregation=False,
        spatial_segregation=False
    )
    
    try:
        initial_arc, explanation = calculate_initial_arc_v25(
            operational_volume_1,
            "Medium",  # ← This should trigger validation error
            arc_inputs_2
        )
        print(f"❌ UNEXPECTED: Should have failed but got Initial ARC = {initial_arc}")
    except ValueError as e:
        print(f"✅ EXPECTED ERROR: {e}")
    
    # Test case 3: Valid - Expert data with Low density
    print("\n### Test Case 3: Expert + Low Density (Valid)")
    try:
        initial_arc, explanation = calculate_initial_arc_v25(
            operational_volume_1,
            "Low",  # ← Expert is valid for Low density
            arc_inputs_2
        )
        print(f"✅ SUCCESS: Initial ARC = {initial_arc}")
        print(explanation)
    except ValueError as e:
        print(f"❌ ERROR: {e}")
