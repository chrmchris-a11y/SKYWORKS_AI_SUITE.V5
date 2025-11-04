"""
SORA 2.5 SAIL Calculator
Official Reference: JAR_doc_25 (SORA 2.5 Main Body) - Table 7 (page 47)

This module determines the Specific Assurance and Integrity Level (SAIL)
based on Final GRC and Residual ARC per JARUS SORA 2.5 specifications.
"""

from typing import Tuple, List, Dict
import logging

logger = logging.getLogger(__name__)


def calculate_sail_v25(
    final_grc: int,
    residual_arc: int
) -> Tuple[str, str]:
    """
    Determine SAIL (Specific Assurance and Integrity Level) for SORA 2.5.
    
    Official Reference: JAR_doc_25 Table 7 (page 47)
    
    SAIL ranges from I (lowest) to VI (highest) and determines:
    - Required Operational Safety Objectives (OSO) robustness levels
    - Integrity and assurance levels for safety systems
    - Documentation and approval requirements
    
    The SAIL is determined by the combination of:
    1. Final GRC (after all ground risk mitigations): 1-10
    2. Residual ARC (after strategic air risk mitigations): 1-10
    
    Higher SAIL requires more robust safety measures and higher assurance.
    
    Args:
        final_grc: Final Ground Risk Class (1-10)
        residual_arc: Residual Air Risk Class after strategic mitigations (1-10)
        
    Returns:
        Tuple of (sail_level: str, explanation: str)
        
    Example:
        >>> calculate_sail_v25(3, 2)
        ("II", "SAIL II: Determined from Final GRC=3 and Residual ARC=2...")
    """
    logger.info("=" * 70)
    logger.info("SORA 2.5 SAIL Determination - Step #9")
    logger.info("=" * 70)
    logger.info(f"Final GRC: {final_grc}")
    logger.info(f"Residual ARC: {residual_arc}")
    
    # Clamp values to valid table bounds
    grc_clamped = min(10, max(1, final_grc))
    arc_clamped = min(10, max(1, residual_arc))
    
    if grc_clamped != final_grc or arc_clamped != residual_arc:
        logger.warning(
            f"Values clamped: GRC {final_grc}->{grc_clamped}, "
            f"ARC {residual_arc}->{arc_clamped}"
        )
    
    # SORA 2.5 SAIL Determination Table (JAR_doc_25 Table 7)
    # Format: (Final_GRC, Residual_ARC) -> SAIL
    # This is the complete table from the official JARUS document
    sail_table = {
        # GRC = 1
        (1, 1): "I",   (1, 2): "I",   (1, 3): "II",  (1, 4): "II",
        (1, 5): "III", (1, 6): "III", (1, 7): "IV",  (1, 8): "IV",
        (1, 9): "V",   (1, 10): "V",
        
        # GRC = 2
        (2, 1): "I",   (2, 2): "II",  (2, 3): "II",  (2, 4): "III",
        (2, 5): "III", (2, 6): "IV",  (2, 7): "IV",  (2, 8): "V",
        (2, 9): "V",   (2, 10): "VI",
        
        # GRC = 3
        (3, 1): "II",  (3, 2): "II",  (3, 3): "III", (3, 4): "IV",
        (3, 5): "IV",  (3, 6): "V",   (3, 7): "V",   (3, 8): "VI",
        (3, 9): "VI",  (3, 10): "VI",
        
        # GRC = 4
        (4, 1): "II",  (4, 2): "III", (4, 3): "IV",  (4, 4): "V",
        (4, 5): "V",   (4, 6): "VI",  (4, 7): "VI",  (4, 8): "VI",
        (4, 9): "VI",  (4, 10): "VI",
        
        # GRC = 5
        (5, 1): "III", (5, 2): "IV",  (5, 3): "V",   (5, 4): "VI",
        (5, 5): "VI",  (5, 6): "VI",  (5, 7): "VI",  (5, 8): "VI",
        (5, 9): "VI",  (5, 10): "VI",
        
        # GRC = 6
        (6, 1): "IV",  (6, 2): "V",   (6, 3): "VI",  (6, 4): "VI",
        (6, 5): "VI",  (6, 6): "VI",  (6, 7): "VI",  (6, 8): "VI",
        (6, 9): "VI",  (6, 10): "VI",
        
        # GRC = 7+
        (7, 1): "V",   (7, 2): "VI",  (7, 3): "VI",  (7, 4): "VI",
        (7, 5): "VI",  (7, 6): "VI",  (7, 7): "VI",  (7, 8): "VI",
        (7, 9): "VI",  (7, 10): "VI",
        
        (8, 1): "VI",  (8, 2): "VI",  (8, 3): "VI",  (8, 4): "VI",
        (8, 5): "VI",  (8, 6): "VI",  (8, 7): "VI",  (8, 8): "VI",
        (8, 9): "VI",  (8, 10): "VI",
        
        (9, 1): "VI",  (9, 2): "VI",  (9, 3): "VI",  (9, 4): "VI",
        (9, 5): "VI",  (9, 6): "VI",  (9, 7): "VI",  (9, 8): "VI",
        (9, 9): "VI",  (9, 10): "VI",
        
        (10, 1): "VI", (10, 2): "VI", (10, 3): "VI", (10, 4): "VI",
        (10, 5): "VI", (10, 6): "VI", (10, 7): "VI", (10, 8): "VI",
        (10, 9): "VI", (10, 10): "VI",
    }
    
    # Look up SAIL from table
    key = (grc_clamped, arc_clamped)
    sail = sail_table.get(key, "VI")  # Default to highest SAIL if not found
    
    # Build explanation
    explanation = (
        f"SAIL {sail}\n"
        f"├─ Final GRC: {grc_clamped}\n"
        f"├─ Residual ARC: {arc_clamped}\n"
        f"└─ Reference: JAR_doc_25 Table 7 (SAIL Determination)\n"
        f"\n"
        f"SAIL {sail} Requirements:\n"
        f"{get_sail_description(sail)}"
    )
    
    logger.info(f"\n{explanation}")
    logger.info("=" * 70)
    
    return sail, explanation


def get_sail_description(sail: str) -> str:
    """
    Get description of SAIL requirements.
    
    Reference: JAR_doc_25 and Annex E (Integrity and Assurance Levels)
    
    Args:
        sail: SAIL level (I-VI)
        
    Returns:
        Multi-line string describing SAIL requirements
    """
    descriptions = {
        "I": (
            "├─ Lowest integrity and assurance requirements\n"
            "├─ Standard OSO implementation (Low/Medium robustness)\n"
            "├─ Self-declaration may be acceptable\n"
            "└─ Minimal documentation requirements"
        ),
        "II": (
            "├─ Low-Medium integrity and assurance requirements\n"
            "├─ OSO implementation with Medium robustness\n"
            "├─ Some validation/verification required\n"
            "└─ Standard documentation"
        ),
        "III": (
            "├─ Medium integrity and assurance requirements\n"
            "├─ OSO implementation with Medium-High robustness\n"
            "├─ Independent validation recommended\n"
            "└─ Comprehensive documentation"
        ),
        "IV": (
            "├─ Medium-High integrity and assurance requirements\n"
            "├─ OSO implementation with High robustness\n"
            "├─ Independent verification required\n"
            "└─ Detailed safety analysis and documentation"
        ),
        "V": (
            "├─ High integrity and assurance requirements\n"
            "├─ OSO implementation with Very High robustness\n"
            "├─ Certification-like processes may be required\n"
            "└─ Extensive safety analysis and validation"
        ),
        "VI": (
            "├─ Highest integrity and assurance requirements\n"
            "├─ OSO implementation with Highest robustness\n"
            "├─ Full certification processes required\n"
            "└─ Complete safety case with independent verification"
        )
    }
    
    return descriptions.get(sail, "└─ Unknown SAIL level")


def get_oso_requirements(sail: str) -> List[Dict[str, str]]:
    """
    Get OSO (Operational Safety Objectives) requirements for a given SAIL.
    
    Reference: JAR_doc_25 Annex E - Integrity and Assurance Levels for OSO
    
    This is a simplified version. The full implementation should reference
    the complete OSO table from Annex E.
    
    Args:
        sail: SAIL level (I-VI)
        
    Returns:
        List of OSO requirements with robustness levels
    """
    # Simplified OSO mapping (full table in JAR_doc_25 Annex E)
    oso_requirements = {
        "I": [
            {"oso": "OSO #1", "requirement": "UAS operator competency", "robustness": "Low"},
            {"oso": "OSO #2", "requirement": "Operational procedures", "robustness": "Low"},
            {"oso": "OSO #3", "requirement": "UAS technical assurance", "robustness": "Low"},
        ],
        "II": [
            {"oso": "OSO #1", "requirement": "UAS operator competency", "robustness": "Medium"},
            {"oso": "OSO #2", "requirement": "Operational procedures", "robustness": "Medium"},
            {"oso": "OSO #3", "requirement": "UAS technical assurance", "robustness": "Medium"},
        ],
        # ... (other SAIL levels would have more OSO with higher robustness)
        "VI": [
            {"oso": "OSO #1", "requirement": "UAS operator competency", "robustness": "Very High"},
            {"oso": "OSO #2", "requirement": "Operational procedures", "robustness": "Very High"},
            {"oso": "OSO #3", "requirement": "UAS technical assurance", "robustness": "Very High"},
            # ... many more OSO at very high robustness
        ]
    }
    
    return oso_requirements.get(sail, [])


# Example usage and testing
if __name__ == "__main__":
    # Configure logging for testing
    logging.basicConfig(
        level=logging.INFO,
        format='%(asctime)s - %(levelname)s - %(message)s'
    )
    
    # Test various GRC/ARC combinations
    test_cases = [
        (1, 1, "I"),    # Lowest risk
        (3, 2, "II"),   # Low risk
        (4, 3, "IV"),   # Medium risk
        (5, 4, "VI"),   # High risk
        (6, 5, "VI"),   # Very high risk
        (10, 10, "VI"), # Maximum risk
    ]
    
    print("\n" + "=" * 80)
    print("SAIL CALCULATION TEST CASES")
    print("=" * 80)
    
    for final_grc, residual_arc, expected_sail in test_cases:
        print(f"\n### Test: Final GRC={final_grc}, Residual ARC={residual_arc} ###")
        sail, explanation = calculate_sail_v25(final_grc, residual_arc)
        
        if sail == expected_sail:
            print(f"✅ PASS: Got expected SAIL {sail}")
        else:
            print(f"❌ FAIL: Expected SAIL {expected_sail}, got {sail}")
        
        print(f"\n{explanation}")
