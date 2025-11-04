"""
SORA 2.5 ARC Models
Official Reference: JAR_doc_25 (SORA 2.5 Main Body)

This module contains Pydantic models for SORA 2.5 Enhanced ARC calculations,
including the 5 new strategic mitigation fields.
"""

from pydantic import BaseModel
from enum import Enum
from typing import Optional


class TrafficDensityDataSource(str, Enum):
    """
    Traffic density data sources for SORA 2.5.
    
    Official Reference: JAR_doc_25 Step #4 - Determination of Initial ARC
    
    Rules:
    - Empirical: Valid for all traffic densities (Low/Medium/High)
    - Statistical: Valid for all traffic densities (Low/Medium/High)
    - Expert: ONLY valid for LOW traffic density
    """
    EMPIRICAL = "Empirical"
    STATISTICAL = "Statistical"
    EXPERT = "Expert"


class AirspaceContainment25(str, Enum):
    """
    Airspace containment levels for SORA 2.5.
    
    Official Reference: JAR_doc_25 Step #5 - Strategic Mitigations
    
    Levels:
    - None: No containment measures
    - Operational: Medium robustness (NOTAMs, ANSP coordination)
    - Certified: High robustness (permanent restrictions, certified segregation)
    """
    NONE = "None"
    OPERATIONAL = "Operational"
    CERTIFIED = "Certified"


class ARCInputs25(BaseModel):
    """
    SORA 2.5 Enhanced ARC Input Fields.
    
    Official Reference: JAR_doc_25 Steps #4 and #5
    
    These 5 fields are the NEW additions in SORA 2.5 that enable:
    1. Data source validation (Step #4)
    2. Strategic mitigation credits (Step #5)
    
    Integration Mapping:
    - Field 1: u_space_services_available
      Frontend: #uSpaceServices (select: true/false)
      Backend: Used in strategic_mitigations_v25.py
      
    - Field 2: traffic_density_data_source
      Frontend: #trafficDensitySource (select: Empirical/Statistical/Expert)
      Backend: Used in initial_arc_calculator_v25.py for validation
      
    - Field 3: airspace_containment
      Frontend: #airspaceContainment25 (select: None/Operational/Certified)
      Backend: Used in strategic_mitigations_v25.py
      
    - Field 4: temporal_segregation
      Frontend: #temporalSegregation (checkbox)
      Backend: Used in strategic_mitigations_v25.py
      
    - Field 5: spatial_segregation
      Frontend: #spatialSegregation (checkbox)
      Backend: Used in strategic_mitigations_v25.py
    """
    u_space_services_available: bool
    traffic_density_data_source: TrafficDensityDataSource
    airspace_containment: AirspaceContainment25
    temporal_segregation: bool
    spatial_segregation: bool
    
    class Config:
        use_enum_values = False  # Keep enum types for validation
        schema_extra = {
            "example": {
                "u_space_services_available": True,
                "traffic_density_data_source": "Empirical",
                "airspace_containment": "Operational",
                "temporal_segregation": False,
                "spatial_segregation": True
            }
        }
