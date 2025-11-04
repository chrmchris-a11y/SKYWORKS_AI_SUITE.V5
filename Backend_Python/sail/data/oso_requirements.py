"""
OSO (Operational Safety Objectives) requirements and robustness levels for SAIL ratings.

This module defines the 24 OSO requirements as specified in JARUS SORA and EASA AMC/GM,
including robustness level assignments and differences between SORA 2.0 and 2.5.
"""

from typing import Dict, List
from enum import Enum
from pydantic import BaseModel, ConfigDict
from sail.models.sail_models import SAILLevel


class RobustnessLevel(str, Enum):
    """OSO robustness levels."""
    LOW = "Low"
    MEDIUM = "Medium"
    HIGH = "High"


class OSORequirement(BaseModel):
    """Individual OSO requirement definition."""
    model_config = ConfigDict(
        frozen=True,
        use_enum_values=True,
        validate_assignment=True
    )
    
    oso_id: str
    title: str
    description: str
    category: str
    robustness_levels: Dict[SAILLevel, RobustnessLevel]


# OSO #1-24 complete definitions
OSO_REQUIREMENTS: List[OSORequirement] = [
    OSORequirement(
        oso_id="OSO #1",
        title="Procedures for flight crew training, currency and recency",
        description="Establish procedures to ensure that flight crew training, currency and recency are commensurate with the operation.",
        category="Competency",
        robustness_levels={
            SAILLevel.I: RobustnessLevel.LOW,
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.MEDIUM,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.HIGH,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #2",
        title="Procedures for flight crew qualifications",
        description="Establish procedures to ensure that the flight crew qualifications are commensurate with the operation.",
        category="Competency",
        robustness_levels={
            SAILLevel.I: RobustnessLevel.LOW,
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.MEDIUM,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.HIGH,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #3",
        title="Training procedures for remote crew to competently handle foreseeable emergencies",
        description="Establish training procedures for remote crew to ensure they can competently handle foreseeable emergencies and abnormal situations.",
        category="Competency",
        robustness_levels={
            SAILLevel.I: RobustnessLevel.LOW,
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.MEDIUM,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.HIGH,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #4",
        title="Operating procedures designed to minimise risks to third parties",
        description="Design operating procedures to minimise risks to third parties (persons and property) on the ground and in the air.",
        category="Procedures",
        robustness_levels={
            SAILLevel.I: RobustnessLevel.LOW,
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.MEDIUM,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.HIGH,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #5",
        title="Ground crew procedures and training",
        description="Develop procedures to ensure ground crew training and competency are commensurate with the operation.",
        category="Competency",
        robustness_levels={
            SAILLevel.I: RobustnessLevel.LOW,
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.MEDIUM,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.HIGH,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #6",
        title="Procedures to ensure flight crew knowledge of airspace constraints",
        description="Establish procedures to ensure that flight crew have knowledge of the operational environment and airspace constraints.",
        category="Procedures",
        robustness_levels={
            SAILLevel.I: RobustnessLevel.LOW,
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.MEDIUM,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.HIGH,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #7",
        title="Procedures for air traffic control coordination",
        description="Establish procedures for coordination with relevant air traffic control units.",
        category="Procedures",
        robustness_levels={
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #8",
        title="Procedures for management of hazards and risks to third parties",
        description="Define procedures for identification, assessment and management of hazards and associated risks to third parties.",
        category="Risk Management",
        robustness_levels={
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #9",
        title="Inspection of the UAS to ensure airworthiness",
        description="Establish procedures for inspection of the UAS to ensure continuing airworthiness.",
        category="Technical",
        robustness_levels={
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #10",
        title="Maintenance procedures",
        description="Establish maintenance procedures for the UAS to ensure continuing airworthiness.",
        category="Technical",
        robustness_levels={
            SAILLevel.II: RobustnessLevel.LOW,
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.MEDIUM,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #11",
        title="Procedures for safe recovery from emergency and abnormal situations",
        description="Define procedures for safe recovery of the UAS from emergency and abnormal situations including manual takeover.",
        category="Emergency Procedures",
        robustness_levels={
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #12",
        title="Compliance with aviation regulations and procedures",
        description="Ensure compliance with relevant aviation regulations and procedures for the intended operation.",
        category="Regulatory",
        robustness_levels={
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #13",
        title="External services supporting UAS operations",
        description="Ensure that external services supporting UAS operations are adequate for the intended operation.",
        category="External Services",
        robustness_levels={
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #14",
        title="Procedures to handle degraded external systems",
        description="Define procedures to handle the degradation or loss of external systems supporting UAS operations.",
        category="External Services",
        robustness_levels={
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #15",
        title="Procedures for information exchange with ATC and other airspace users",
        description="Establish procedures for information exchange between the remote crew, ATC and other airspace users.",
        category="Communication",
        robustness_levels={
            SAILLevel.III: RobustnessLevel.LOW,
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #16",
        title="Multi crew coordination procedures",
        description="Establish multi crew coordination procedures and training when multiple crew members are involved.",
        category="Crew Coordination",
        robustness_levels={
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #17",
        title="Procedures for deterministic response to loss of control",
        description="Define procedures to ensure a deterministic response of the UAS to a set of credible control link loss scenarios.",
        category="Technical",
        robustness_levels={
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #18",
        title="Procedures for autonomous operation contingency management",
        description="Define procedures for handling contingencies during autonomous operations including system degradations.",
        category="Autonomous Operations",
        robustness_levels={
            SAILLevel.IV: RobustnessLevel.LOW,
            SAILLevel.V: RobustnessLevel.MEDIUM,
            SAILLevel.VI: RobustnessLevel.HIGH
        }
    ),
    OSORequirement(
        oso_id="OSO #19",
        title="Safe recovery capability during adverse meteorological conditions",
        description="Ensure safe recovery capability of the UAS during operations in adverse meteorological conditions.",
        category="Weather",
        robustness_levels={
            SAILLevel.V: RobustnessLevel.LOW,
            SAILLevel.VI: RobustnessLevel.MEDIUM
        }
    ),
    OSORequirement(
        oso_id="OSO #20",
        title="Operational control of the UAS",
        description="Ensure that operational control of the UAS is maintained throughout the flight.",
        category="Control",
        robustness_levels={
            SAILLevel.V: RobustnessLevel.LOW,
            SAILLevel.VI: RobustnessLevel.MEDIUM
        }
    ),
    OSORequirement(
        oso_id="OSO #21",
        title="Procedures for management of deteriorating operating conditions",
        description="Define procedures for management of deteriorating operating conditions that may affect UAS operations.",
        category="Risk Management",
        robustness_levels={
            SAILLevel.V: RobustnessLevel.LOW,
            SAILLevel.VI: RobustnessLevel.MEDIUM
        }
    ),
    OSORequirement(
        oso_id="OSO #22",
        title="Capability to keep the UAS within designated airspace",
        description="Ensure the capability to keep the UAS within the designated airspace and volumes.",
        category="Navigation",
        robustness_levels={
            SAILLevel.VI: RobustnessLevel.LOW
        }
    ),
    OSORequirement(
        oso_id="OSO #23",
        title="Capability to monitor traffic and avoid collisions",
        description="Ensure the capability to monitor traffic and avoid collisions with other aircraft.",
        category="Collision Avoidance",
        robustness_levels={
            SAILLevel.VI: RobustnessLevel.LOW
        }
    ),
    OSORequirement(
        oso_id="OSO #24",
        title="Capability to avoid controlled flight into terrain",
        description="Ensure the capability to avoid controlled flight into terrain or obstacles.",
        category="Terrain Avoidance",
        robustness_levels={
            SAILLevel.VI: RobustnessLevel.LOW
        }
    )
]


# SAIL level to required OSO count mapping
SAIL_OSO_COUNT: Dict[SAILLevel, int] = {
    SAILLevel.I: 6,
    SAILLevel.II: 10,
    SAILLevel.III: 15,
    SAILLevel.IV: 18,
    SAILLevel.V: 21,
    SAILLevel.VI: 24
}


# SAIL level to applicable OSO requirements mapping
SAIL_OSO_REQUIREMENTS: Dict[SAILLevel, List[str]] = {
    SAILLevel.I: [
        "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6"
    ],
    SAILLevel.II: [
        "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
        "OSO #7", "OSO #8", "OSO #9", "OSO #10"
    ],
    SAILLevel.III: [
        "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
        "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
        "OSO #13", "OSO #14", "OSO #15"
    ],
    SAILLevel.IV: [
        "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
        "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
        "OSO #13", "OSO #14", "OSO #15", "OSO #16", "OSO #17", "OSO #18"
    ],
    SAILLevel.V: [
        "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
        "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
        "OSO #13", "OSO #14", "OSO #15", "OSO #16", "OSO #17", "OSO #18",
        "OSO #19", "OSO #20", "OSO #21"
    ],
    SAILLevel.VI: [
        "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
        "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
        "OSO #13", "OSO #14", "OSO #15", "OSO #16", "OSO #17", "OSO #18",
        "OSO #19", "OSO #20", "OSO #21", "OSO #22", "OSO #23", "OSO #24"
    ]
}


def get_oso_requirements_for_sail(sail_level: SAILLevel) -> List[OSORequirement]:
    """
    Get OSO requirements for a specific SAIL level.
    
    Args:
        sail_level: The SAIL level
        
    Returns:
        List of applicable OSO requirements
        
    Raises:
        ValueError: If SAIL level is invalid
    """
    if sail_level not in SAIL_OSO_REQUIREMENTS:
        raise ValueError(f"Invalid SAIL level: {sail_level}")
        
    oso_ids = SAIL_OSO_REQUIREMENTS[sail_level]
    oso_map = {req.oso_id: req for req in OSO_REQUIREMENTS}
    
    return [oso_map[oso_id] for oso_id in oso_ids if oso_id in oso_map]


def get_oso_count_for_sail(sail_level: SAILLevel) -> int:
    """
    Get the number of required OSOs for a SAIL level.
    
    Args:
        sail_level: The SAIL level
        
    Returns:
        Number of required OSOs
        
    Raises:
        ValueError: If SAIL level is invalid
    """
    if sail_level not in SAIL_OSO_COUNT:
        raise ValueError(f"Invalid SAIL level: {sail_level}")
        
    return SAIL_OSO_COUNT[sail_level]


def validate_oso_completeness():
    """
    Validate that OSO requirements and mappings are complete and consistent.
    
    Raises:
        ValueError: If validation fails
    """
    # Check OSO count consistency
    for sail_level in SAILLevel:
        expected_count = SAIL_OSO_COUNT[sail_level]
        actual_count = len(SAIL_OSO_REQUIREMENTS[sail_level])
        
        if expected_count != actual_count:
            raise ValueError(
                f"OSO count mismatch for {sail_level}: expected {expected_count}, got {actual_count}"
            )
    
    # Check all referenced OSOs exist
    all_oso_ids = {req.oso_id for req in OSO_REQUIREMENTS}
    for sail_level, oso_ids in SAIL_OSO_REQUIREMENTS.items():
        for oso_id in oso_ids:
            if oso_id not in all_oso_ids:
                raise ValueError(f"Referenced OSO {oso_id} not found in requirements")
    
    # Validate incremental OSO requirements (higher SAIL includes all lower SAIL OSOs)
    sail_levels = [SAILLevel.I, SAILLevel.II, SAILLevel.III, SAILLevel.IV, SAILLevel.V, SAILLevel.VI]
    for i in range(1, len(sail_levels)):
        current_olos = set(SAIL_OSO_REQUIREMENTS[sail_levels[i]])
        previous_olos = set(SAIL_OSO_REQUIREMENTS[sail_levels[i-1]])
        
        if not previous_olos.issubset(current_olos):
            raise ValueError(
                f"SAIL {sail_levels[i]} requirements do not include all {sail_levels[i-1]} requirements"
            )


# Differences between SORA 2.0 and 2.5
# OSO robustness matrix: SAIL level → OSO ID → Robustness level
OSO_ROBUSTNESS_MATRIX = {
    sail_level: {
        oso.oso_id: oso.robustness_levels.get(sail_level, RobustnessLevel.LOW)
        for oso in OSO_REQUIREMENTS
        if sail_level in oso.robustness_levels
    }
    for sail_level in SAILLevel
}

# OSO descriptions by ID
OSO_DESCRIPTIONS = {
    oso.oso_id: {
        "title": oso.title,
        "description": oso.description,
        "category": oso.category
    }
    for oso in OSO_REQUIREMENTS
}

# Robustness level descriptions
ROBUSTNESS_DESCRIPTIONS = {
    RobustnessLevel.LOW: "Low level of robustness",
    RobustnessLevel.MEDIUM: "Medium level of robustness",
    RobustnessLevel.HIGH: "High level of robustness"
}

# OSO categories
OSO_CATEGORIES = list(set(oso.category for oso in OSO_REQUIREMENTS))


SORA_VERSION_DIFFERENCES = {
    "2.0_to_2.5": {
        "description": "Key differences between SORA 2.0 (EASA AMC/GM) and SORA 2.5 (JARUS)",
        "sail_mapping_changes": {
            "GRC_6": {
                "2.0": {"a": "III", "b": "III", "c": "IV", "d": "V"},
                "2.5": {"a": "IV", "b": "IV", "c": "V", "d": "VI"}
            },
            "GRC_7": {
                "2.0": {"a": "IV", "b": "IV", "c": "V", "d": "VI"},
                "2.5": {"a": "V", "b": "V", "c": "VI", "d": "VI"}
            },
            "GRC_8": {
                "2.0": {"a": "IV", "b": "IV", "c": "V", "d": "VI"},
                "2.5": {"a": "V", "b": "V", "c": "VI", "d": "VI"}
            }
        },
        "rationale": "SORA 2.5 introduces more stringent SAIL requirements for higher ground risk classes (GRC 6-8) based on operational experience and safety analysis."
    }
}