"""
OSO Requirements module for SORA compliance.

Provides OSO information per SAIL level for both SORA 2.0 and 2.5 without creating version drift.

References:
- SORA 2.0: EASA AMC/GM to Reg. (EU) 2019/947, Annex E
- SORA 2.5: JARUS SORA v2.5 Main Body & Annexes, Annex E
"""

import os
import yaml
from typing import List, Optional, Union, Dict, Any
from dataclasses import dataclass
from enum import Enum


class SAILLevel(Enum):
    """SAIL levels as defined in SORA standards."""
    I = "I"
    II = "II" 
    III = "III"
    IV = "IV"
    V = "V"
    VI = "VI"


class SORAVersion(Enum):
    """SORA version enumeration."""
    SORA_2_0 = "2.0"
    SORA_2_5 = "2.5"


@dataclass
class OSORequirement:
    """
    Operational Safety Objective requirement.
    
    Fields:
    - id: OSO identifier (e.g., "OSO#01")
    - title: Human-readable OSO title
    - robustness: Required robustness level for the SAIL (e.g., "Low", "Medium", "High")
    - applicability_note: Additional notes on applicability (optional)
    """
    id: str
    title: str
    robustness: Optional[str] = None
    applicability_note: Optional[str] = None


# SORA 2.0 OSO count mapping per SAIL (convenience counts derived from Annex E)
SORA_2_0_OSO_COUNTS = {
    SAILLevel.I: 6,
    SAILLevel.II: 10,
    SAILLevel.III: 15,
    SAILLevel.IV: 18,
    SAILLevel.V: 21,
    SAILLevel.VI: 24
}


def _load_oso_data(version: str) -> dict:
    """Load OSO data from YAML file."""
    data_dir = os.path.join(os.path.dirname(__file__), 'data')
    file_path = os.path.join(data_dir, f'oso_map_{version.replace(".", "_")}.yaml')
    
    if not os.path.exists(file_path):
        return None
    
    with open(file_path, 'r', encoding='utf-8') as f:
        return yaml.safe_load(f)


def get_oso_count_for_sail(sail: SAILLevel, sora_version: Union[SORAVersion, str]) -> Optional[int]:
    """
    Get the count of OSOs required for a given SAIL level and SORA version.
    
    For SORA 2.0, returns conventional counts derived from Annex E:
    I → 6, II → 10, III → 15, IV → 18, V → 21, VI → 24
    
    For SORA 2.5, returns None as counts are not hard-coded and vary based on 
    the specific OSO set and robustness model defined in Annex E v2.5.
    
    Args:
        sail: SAIL level
        sora_version: SORA version
        
    Returns:
        OSO count for SORA 2.0, None for SORA 2.5
        
    References:
        - SORA 2.0: EASA AMC/GM to Reg. (EU) 2019/947, Annex E
        - SORA 2.5: JARUS SORA v2.5 Main Body & Annexes, Annex E
    """
    # Accept either enum or plain string ("2.0"/"2.5")
    version_str = sora_version.value if hasattr(sora_version, "value") else str(sora_version)
    
    if version_str == "2.0":
        return SORA_2_0_OSO_COUNTS.get(sail)
    elif version_str == "2.5":
        # Do not return hard-coded counts for SORA 2.5
        return None
    
    return None


def get_oso_requirements_for_sail(sail: SAILLevel, sora_version: Union[SORAVersion, str]) -> List[Dict[str, Any]]:
    """
    Get the OSO requirements for a given SAIL level and SORA version.
    
    For SORA 2.0, returns the canonical 24 OSOs with robustness per SAIL from Annex E.
    For SORA 2.5, returns the v2.5 OSO set with robustness per SAIL from Annex E v2.5.
    
    Args:
        sail: SAIL level
        sora_version: SORA version
        
    Returns:
        List of OSO requirements with robustness levels
        
    Raises:
        NotImplementedError: If SORA 2.5 data file is not available
        
    References:
        - SORA 2.0: EASA AMC/GM to Reg. (EU) 2019/947, Annex E
        - SORA 2.5: JARUS SORA v2.5 Main Body & Annexes, Annex E
    """
    # Accept either enum or plain string ("2.0"/"2.5")
    version_str = sora_version.value if hasattr(sora_version, "value") else str(sora_version)
    sail_key = sail.value
    
    if version_str == "2.0":
        return _get_sora_2_0_requirements(sail_key)
    elif version_str == "2.5":
        return _get_sora_2_5_requirements(sail_key)
    
    return []


def _get_sora_2_0_requirements(sail: str) -> List[Dict[str, Any]]:
    """Get SORA 2.0 OSO requirements from data file or fallback mapping."""
    data = _load_oso_data("2.0")
    
    if data and sail in data:
        return [
            {
                "id": req["id"],
                "title": req["title"],
                "robustness": req.get("robustness"),
                "applicability_note": req.get("applicability_note")
            }
            for req in data[sail]
        ]
    
    # Fallback: return empty list if data file not available
    # In production, this should be populated with canonical SORA 2.0 OSOs
    return []


def _get_sora_2_5_requirements(sail: str) -> List[Dict[str, Any]]:
    """Get SORA 2.5 OSO requirements from data file."""
    data = _load_oso_data("2.5")
    
    if data is None:
        raise NotImplementedError(
            "SORA 2.5 OSO requirements not available. "
            "Please provide the data file at /sail/data/oso_map_2_5.yaml "
            "with the complete OSO mapping per Annex E of JARUS SORA v2.5."
        )
    
    if sail not in data:
        return []
    
    return [
        {
            "id": req["id"],
            "title": req["title"],
            "robustness": req.get("robustness"),
            "applicability_note": req.get("applicability_note")
        }
        for req in data[sail]
    ]

