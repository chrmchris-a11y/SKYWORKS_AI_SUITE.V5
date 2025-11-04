"""
SORA 2.5 Strategic Mitigations Calculator
Official Reference: JAR_doc_25 (SORA 2.5 Main Body) - Step #5

This module applies strategic mitigations to reduce Initial ARC to Residual ARC,
using the 5 new SORA 2.5 enhanced ARC input fields.
"""

from typing import Tuple, List, Dict
import logging

from models.arc_models import (
    ARCInputs25,
    TrafficDensityDataSource,
    AirspaceContainment25
)

logger = logging.getLogger(__name__)


def calculate_u_space_credit(
    initial_arc: int,
    operational_volume: Dict
) -> int:
    """
    Calculate ARC reduction credit for U-space services.
    
    Official Reference: JAR_doc_34 (Annex H) Section H.2.3.2
    
    U-space services provide strategic deconfliction through:
    - Geofencing and no-fly zones
    - Flight planning and authorization
    - Tracking and monitoring
    - Conflict detection and resolution
    
    Credit Requirements:
    1. U-space services must be available (already checked)
    2. Operational volume must be in U-space designated airspace
    3. Service Level Agreement (SLA) with provider
    4. OSO #13 compliance (external services adequate)
    
    Args:
        initial_arc: The Initial ARC value
        operational_volume: Dict containing 'in_u_space_airspace' flag
        
    Returns:
        ARC credit (negative value for reduction, 0 for no credit)
    """
    # Check if operational volume is in U-space designated airspace
    if not operational_volume.get('in_u_space_airspace', False):
        logger.info("U-space services available but not in U-space airspace - No credit")
        return 0
    
    # U-space services provide strategic deconfliction
    # Credit depends on Initial ARC level
    if initial_arc <= 6:
        logger.info("U-space services credit: -1 ARC (effective for ARC ≤ 6)")
        return -1  # One ARC level reduction
    else:
        logger.info("U-space services: Limited effectiveness for very high ARC (>6)")
        return 0  # Limited effectiveness for very high ARC


def calculate_airspace_containment_credit(
    containment: AirspaceContainment25,
    initial_arc: int
) -> int:
    """
    Calculate ARC reduction credit for airspace containment.
    
    Official Reference: JAR_doc_25 Step #5 - Strategic Mitigations
    
    Airspace containment restricts manned aircraft access to operational volume:
    
    - None: No containment (0 credit)
    - Operational: Medium robustness containment (-1 credit if ARC ≤ 6)
      Examples: NOTAMs, coordinated with ANSP, temporary restrictions
      Requirements: ANSP approval, coordination procedures, monitoring
      
    - Certified: High robustness containment (-2 credit if ARC ≤ 4, -1 if ARC ≤ 8)
      Examples: Permanent restricted zones, certified segregation
      Requirements: Competent authority approval, technical barriers, certified procedures
    
    Args:
        containment: The airspace containment level
        initial_arc: The Initial ARC value
        
    Returns:
        ARC credit (negative value for reduction, 0 for no credit)
    """
    if containment == AirspaceContainment25.NONE:
        logger.info("Airspace Containment: None - No credit")
        return 0  # No reduction
    
    elif containment == AirspaceContainment25.OPERATIONAL:
        # Operational containment (Medium robustness)
        if initial_arc <= 6:
            logger.info("Airspace Containment: Operational - Credit: -1 ARC (ARC ≤ 6)")
            return -1  # One ARC level reduction
        else:
            logger.info("Airspace Containment: Operational - Limited effectiveness for ARC > 6")
            return 0
    
    elif containment == AirspaceContainment25.CERTIFIED:
        # Certified containment (High robustness)
        if initial_arc <= 4:
            logger.info("Airspace Containment: Certified - Credit: -2 ARC (ARC ≤ 4)")
            return -2  # Two ARC levels reduction
        elif initial_arc <= 8:
            logger.info("Airspace Containment: Certified - Credit: -1 ARC (4 < ARC ≤ 8)")
            return -1  # One ARC level reduction
        else:
            logger.info("Airspace Containment: Certified - Limited effectiveness for ARC > 8")
            return 0
    
    return 0


def calculate_temporal_segregation_credit(
    temporal_segregation: bool,
    initial_arc: int,
    traffic_density: str
) -> int:
    """
    Calculate ARC reduction credit for temporal segregation.
    
    Official Reference: SORA Annex C - Strategic Mitigation by Operational Restrictions
    (Chronology-based restrictions)
    
    Temporal segregation means operations during periods when manned aviation 
    activity is significantly reduced:
    - Night operations (when VFR traffic minimal)
    - Non-business hours (when general aviation inactive)
    - Weather conditions prohibitive to manned aviation
    - Coordinated time windows with ANSP
    
    Requirements:
    - Time-based operational restrictions in ConOps
    - Evidence of reduced manned traffic during selected times
    - Procedures to ensure compliance with time windows
    - Contingency plans for time window violations
    
    Args:
        temporal_segregation: Whether temporal segregation is applied
        initial_arc: The Initial ARC value
        traffic_density: Traffic density level (for context)
        
    Returns:
        ARC credit (negative value for reduction, 0 for no credit)
    """
    if not temporal_segregation:
        return 0
    
    # Temporal segregation is effective if documented evidence shows
    # reduced manned traffic during operational time windows
    if initial_arc <= 6:
        logger.info(
            "Temporal Segregation: Credit -1 ARC (effective for ARC ≤ 6)\n"
            "  Requires: Time-based restrictions in ConOps, evidence of traffic reduction"
        )
        return -1  # One ARC level reduction
    else:
        logger.info(
            "Temporal Segregation: Limited effectiveness for very high ARC (>6)\n"
            "  Even at night/off-hours, some traffic remains in high-density areas"
        )
        return 0


def calculate_spatial_segregation_credit(
    spatial_segregation: bool,
    initial_arc: int,
    operational_volume: Dict
) -> int:
    """
    Calculate ARC reduction credit for spatial segregation.
    
    Official Reference: SORA Annex C - Strategic Mitigation by Operational Restrictions
    (Boundary-based restrictions)
    
    Spatial segregation means operations in airspace with defined boundaries 
    that separate from manned aviation:
    - Operations below minimum safe altitude for manned aircraft
    - Operations in designated UAS zones
    - Lateral separation from airways and approach paths
    - Vertical separation (altitude bands)
    - Operations in controlled ground areas (private property)
    
    Requirements:
    - Clearly defined operational boundaries in ConOps
    - Analysis showing separation from manned traffic routes
    - Compliance with airspace structure (CTR, ATZ, FRZ, etc.)
    - Monitoring to ensure containment within boundaries
    
    Args:
        spatial_segregation: Whether spatial segregation is applied
        initial_arc: The Initial ARC value
        operational_volume: Dict with operational parameters (for validation)
        
    Returns:
        ARC credit (negative value for reduction, 0 for no credit)
    """
    if not spatial_segregation:
        return 0
    
    # Spatial segregation is effective if documented analysis shows
    # spatial separation from manned traffic routes
    if initial_arc <= 6:
        logger.info(
            "Spatial Segregation: Credit -1 ARC (effective for ARC ≤ 6)\n"
            "  Requires: Boundary-defined volume in ConOps, separation analysis"
        )
        return -1  # One ARC level reduction
    else:
        logger.info(
            "Spatial Segregation: Limited effectiveness for very high ARC (>6)\n"
            "  Difficult to achieve full separation in congested airspace"
        )
        return 0


def calculate_residual_arc_v25(
    initial_arc: int,
    arc_inputs: ARCInputs25,
    operational_volume: Dict
) -> Tuple[int, List[str]]:
    """
    Calculate Residual ARC after applying strategic mitigations.
    
    Official Reference: JAR_doc_25 Step #5 - Application of Strategic Mitigations
    
    Formula:
        Residual ARC = Initial ARC + Strategic Mitigation Credits
        
    where Strategic Mitigation Credits (all negative or 0) come from:
    1. U-space Services (if available and in U-space airspace)
    2. Airspace Containment (None/Operational/Certified)
    3. Temporal Segregation (time-based restrictions)
    4. Spatial Segregation (boundary-based restrictions)
    
    Notes:
    - Credits are cumulative (all applicable mitigations add)
    - Residual ARC cannot be less than 1 (minimum value)
    - Each mitigation has effectiveness limits based on Initial ARC
    
    Args:
        initial_arc: The Initial ARC value (from Step #4)
        arc_inputs: SORA 2.5 ARC inputs containing all 5 fields
        operational_volume: Dict with operational parameters
        
    Returns:
        Tuple of (residual_arc: int, mitigations: List[str])
        where mitigations is a list of explanation strings
    """
    logger.info("=" * 70)
    logger.info("SORA 2.5 Strategic Mitigations - Step #5")
    logger.info("=" * 70)
    logger.info(f"Initial ARC: {initial_arc}")
    
    mitigations = []
    total_credit = 0
    
    # 1. U-space Services
    if arc_inputs.u_space_services_available:
        logger.info("\n### Evaluating U-space Services ###")
        credit = calculate_u_space_credit(initial_arc, operational_volume)
        total_credit += credit
        if credit < 0:
            mitigations.append(
                f"✓ U-space Services: {credit} ARC\n"
                f"  Reference: JAR_doc_34 (Annex H) Section H.2.3.2"
            )
        elif credit == 0 and not operational_volume.get('in_u_space_airspace', False):
            mitigations.append(
                f"✗ U-space Services: 0 ARC (Not in U-space airspace)"
            )
    
    # 2. Airspace Containment
    logger.info("\n### Evaluating Airspace Containment ###")
    containment_credit = calculate_airspace_containment_credit(
        arc_inputs.airspace_containment,
        initial_arc
    )
    total_credit += containment_credit
    if containment_credit < 0:
        mitigations.append(
            f"✓ Airspace Containment ({arc_inputs.airspace_containment.value}): "
            f"{containment_credit} ARC\n"
            f"  Reference: JAR_doc_25 Step #5 - Strategic Mitigations"
        )
    elif arc_inputs.airspace_containment != AirspaceContainment25.NONE:
        mitigations.append(
            f"✗ Airspace Containment ({arc_inputs.airspace_containment.value}): "
            f"0 ARC (Limited effectiveness for this ARC level)"
        )
    
    # 3. Temporal Segregation
    if arc_inputs.temporal_segregation:
        logger.info("\n### Evaluating Temporal Segregation ###")
        temporal_credit = calculate_temporal_segregation_credit(
            arc_inputs.temporal_segregation,
            initial_arc,
            operational_volume.get('traffic_density', 'Medium')
        )
        total_credit += temporal_credit
        if temporal_credit < 0:
            mitigations.append(
                f"✓ Temporal Segregation: {temporal_credit} ARC\n"
                f"  Reference: SORA Annex C - Strategic Mitigations (Chronology-based)"
            )
        else:
            mitigations.append(
                f"✗ Temporal Segregation: 0 ARC (Limited effectiveness for this ARC level)"
            )
    
    # 4. Spatial Segregation
    if arc_inputs.spatial_segregation:
        logger.info("\n### Evaluating Spatial Segregation ###")
        spatial_credit = calculate_spatial_segregation_credit(
            arc_inputs.spatial_segregation,
            initial_arc,
            operational_volume
        )
        total_credit += spatial_credit
        if spatial_credit < 0:
            mitigations.append(
                f"✓ Spatial Segregation: {spatial_credit} ARC\n"
                f"  Reference: SORA Annex C - Strategic Mitigations (Boundary-based)"
            )
        else:
            mitigations.append(
                f"✗ Spatial Segregation: 0 ARC (Limited effectiveness for this ARC level)"
            )
    
    # Calculate Residual ARC (cannot be less than 1)
    residual_arc = max(1, initial_arc + total_credit)
    
    # Add summary
    summary = (
        f"\n{'=' * 70}\n"
        f"Residual ARC Calculation:\n"
        f"  Initial ARC: {initial_arc}\n"
        f"  Strategic Credits: {total_credit}\n"
        f"  Residual ARC: {residual_arc} (max of 1 or {initial_arc + total_credit})\n"
        f"{'=' * 70}"
    )
    mitigations.append(summary)
    
    logger.info(summary)
    
    return residual_arc, mitigations


# Example usage and testing
if __name__ == "__main__":
    # Configure logging for testing
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s'
    )
    
    # Test case 1: All mitigations applied
    print("\n" + "=" * 80)
    print("TEST CASE 1: All Mitigations Applied")
    print("=" * 80)
    
    arc_inputs_1 = ARCInputs25(
        u_space_services_available=True,
        traffic_density_data_source=TrafficDensityDataSource.EMPIRICAL,
        airspace_containment=AirspaceContainment25.OPERATIONAL,
        temporal_segregation=True,
        spatial_segregation=True
    )
    
    operational_volume_1 = {
        'in_u_space_airspace': True,
        'traffic_density': 'Low',
        'max_altitude': 120
    }
    
    residual_arc, mitigations = calculate_residual_arc_v25(
        initial_arc=5,
        arc_inputs=arc_inputs_1,
        operational_volume=operational_volume_1
    )
    
    print(f"\nRESULT: Residual ARC = {residual_arc}")
    print("\nMitigations Applied:")
    for mitigation in mitigations:
        print(mitigation)
    
    # Test case 2: Only Certified containment
    print("\n" + "=" * 80)
    print("TEST CASE 2: Only Certified Airspace Containment")
    print("=" * 80)
    
    arc_inputs_2 = ARCInputs25(
        u_space_services_available=False,
        traffic_density_data_source=TrafficDensityDataSource.STATISTICAL,
        airspace_containment=AirspaceContainment25.CERTIFIED,
        temporal_segregation=False,
        spatial_segregation=False
    )
    
    operational_volume_2 = {
        'in_u_space_airspace': False,
        'traffic_density': 'Medium'
    }
    
    residual_arc, mitigations = calculate_residual_arc_v25(
        initial_arc=4,
        arc_inputs=arc_inputs_2,
        operational_volume=operational_volume_2
    )
    
    print(f"\nRESULT: Residual ARC = {residual_arc}")
    print("\nMitigations Applied:")
    for mitigation in mitigations:
        print(mitigation)
    
    # Test case 3: High Initial ARC (limited mitigation effectiveness)
    print("\n" + "=" * 80)
    print("TEST CASE 3: High Initial ARC (Limited Mitigation Effectiveness)")
    print("=" * 80)
    
    arc_inputs_3 = ARCInputs25(
        u_space_services_available=True,
        traffic_density_data_source=TrafficDensityDataSource.EMPIRICAL,
        airspace_containment=AirspaceContainment25.CERTIFIED,
        temporal_segregation=True,
        spatial_segregation=True
    )
    
    operational_volume_3 = {
        'in_u_space_airspace': True,
        'traffic_density': 'High'
    }
    
    residual_arc, mitigations = calculate_residual_arc_v25(
        initial_arc=9,  # Very high Initial ARC
        arc_inputs=arc_inputs_3,
        operational_volume=operational_volume_3
    )
    
    print(f"\nRESULT: Residual ARC = {residual_arc}")
    print("\nMitigations Applied:")
    for mitigation in mitigations:
        print(mitigation)
