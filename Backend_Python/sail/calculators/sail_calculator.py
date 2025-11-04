"""SAIL calculators for SORA 2.0 and SORA 2.5."""

from typing import Dict, List, Tuple, Union
from ..models.sail_models import (
    SAILValue, ARCValue, SAILInputs20, SAILInputs25, SAILResult,
    TraceEntry, DocReference, InvalidGRCError, InvalidARCError, SAILMappingError,
    GRCLevel, ARCLevel, SAILLevel, SORAVersion,
    SAILCalculationRequest, SAILCalculationResponse
)
from ..data.sail_tables_20 import SAIL_TABLE_20, OSO_BY_SAIL_20
from ..data.sail_tables_25 import SAIL_TABLE_25, OSO_BY_SAIL_25
from ..data.oso_requirements import OSO_ROBUSTNESS_MATRIX


# Wrapper class for test compatibility
class SAILCalculator:
    """Unified SAIL calculator that handles both SORA 2.0 and 2.5."""
    
    def __init__(self):
        self.calc_20 = None  # Will be initialized when needed
        self.calc_25 = None  # Will be initialized when needed
    
    def calculate_sail(self, request: SAILCalculationRequest) -> SAILCalculationResponse:
        """Calculate SAIL level from GRC and ARC using specified SORA version."""
        # Convert enum to int/str
        grc_int = request.grc_level.value if isinstance(request.grc_level.value, int) else int(str(request.grc_level.value).split('_')[-1])
        arc_str = request.arc_level.value
        
        # Choose calculator based on SORA version
        if request.sora_version == SORAVersion.SORA_2_0:
            table = SAIL_TABLE_20
            oso_map = OSO_BY_SAIL_20
        else:  # SORA_2_5
            table = SAIL_TABLE_25
            oso_map = OSO_BY_SAIL_25
        
        # Look up SAIL
        key = (grc_int, arc_str)
        if key not in table:
            raise SAILMappingError(f"No SAIL mapping for GRC {grc_int} × ARC {arc_str}")
        
        sail_str = table[key]
        sail_level = SAILLevel(sail_str)
        oso_count = oso_map[sail_str]
        
        return SAILCalculationResponse(
            sail_level=sail_level,
            oso_count=oso_count,
            sora_version=request.sora_version,
            grc_level=request.grc_level,
            arc_level=request.arc_level
        )


class SAILCalculator20:
    """SORA 2.0 SAIL calculator with full traceability."""
    
    def __init__(self):
        """Initialize SORA 2.0 calculator."""
        self.sail_table = SAIL_TABLE_20
        self.oso_by_sail = OSO_BY_SAIL_20
        self.version = "SORA_2.0"
    
    def calculate(self, inputs: SAILInputs20) -> SAILResult:
        """
        Calculate SAIL from GRC and ARC using SORA 2.0.
        
        Args:
            inputs: SORA 2.0 SAIL calculation inputs
            
        Returns:
            SAILResult with determined SAIL, OSO requirements, and trace
            
        Raises:
            InvalidGRCError: If GRC is out of range
            InvalidARCError: If ARC is invalid
            SAILMappingError: If SAIL lookup fails
        """
        trace = []
        
        try:
            # Step 1: Validate inputs
            self._validate_inputs(inputs.final_grc, inputs.final_arc, trace)
            
            # Step 2: Determine SAIL from table
            sail = self._determine_sail(inputs.final_grc, inputs.final_arc, trace)
            
            # Step 3: Get OSO requirements
            oso_requirements = self._get_oso_requirements(sail, trace)
            
            # Step 4: Get robustness levels
            robustness_levels = self._get_robustness_levels(sail, oso_requirements, trace)
            
            return SAILResult(
                version=self.version,
                sail=sail,
                final_grc=inputs.final_grc,
                final_arc=inputs.final_arc,
                oso_requirements=oso_requirements,
                oso_count=len(oso_requirements),
                robustness_levels=robustness_levels,
                calculation_trace=trace
            )
            
        except Exception as e:
            trace.append(TraceEntry(
                step="error",
                inputs={"grc": inputs.final_grc, "arc": inputs.final_arc.value},
                result=f"Error: {str(e)}",
                rule_ref="ERROR_HANDLING",
                doc_ref=DocReference(
                    doc_id="EASA_EAR_UAS_2024",
                    standard="SORA_2.0",
                    section="Error Handling"
                ),
                notes=f"Calculation failed: {str(e)}"
            ))
            raise
    
    def _validate_inputs(self, grc: int, arc: ARCValue, trace: List[TraceEntry]) -> None:
        """Validate SAIL calculation inputs."""
        if not 1 <= grc <= 8:
            raise InvalidGRCError(f"GRC must be 1-8, got {grc}")
        
        if arc.value.lower() not in ["a", "b", "c", "d"]:
            raise InvalidARCError(f"ARC must be a/b/c/d, got {arc.value}")
        
        trace.append(TraceEntry(
            step="input_validation",
            inputs={"grc": grc, "arc": arc.value},
            result="Valid inputs",
            rule_ref="SORA20_INPUT_VALIDATION",
            doc_ref=DocReference(
                doc_id="EASA_EAR_UAS_2024",
                standard="SORA_2.0",
                section="Step 9 - SAIL Determination"
            ),
            notes=f"GRC {grc} and ARC {arc.value.upper()} are valid"
        ))
    
    def _determine_sail(self, grc: int, arc: ARCValue, trace: List[TraceEntry]) -> SAILValue:
        """Determine SAIL from GRC and ARC using SORA 2.0 lookup table."""
        arc_lower = arc.value.lower()
        
        # Find matching GRC range
        for grc_range, arc_map in self.sail_table.items():
            if isinstance(grc_range, tuple):
                if grc_range[0] <= grc <= grc_range[1]:
                    sail = arc_map[arc_lower]
                    grc_ref = f"{grc_range[0]}-{grc_range[1]}"
                    break
            else:
                if grc == grc_range:
                    sail = arc_map[arc_lower]
                    grc_ref = str(grc_range)
                    break
        else:
            raise SAILMappingError(f"No SAIL mapping found for GRC {grc}, ARC {arc.value}")
        
        trace.append(TraceEntry(
            step="sail_determination",
            inputs={"grc": grc, "arc": arc.value},
            result=sail.value,
            rule_ref=f"EASA_AMC_Table_D1_GRC{grc_ref}_ARC{arc.value.upper()}",
            doc_ref=DocReference(
                doc_id="EASA_EAR_UAS_2024",
                standard="SORA_2.0",
                section="Annex D, Table D.1",
                page="Annex D-3"
            ),
            notes=f"GRC {grc} (range {grc_ref}) × ARC {arc.value.upper()} → SAIL {sail.value}"
        ))
        
        return sail
    
    def _get_oso_requirements(self, sail: SAILValue, trace: List[TraceEntry]) -> List[str]:
        """Get OSO requirements for the determined SAIL."""
        oso_requirements = self.oso_by_sail[sail]
        
        trace.append(TraceEntry(
            step="oso_requirements",
            inputs={"sail": sail.value},
            result=f"{len(oso_requirements)} OSOs required",
            rule_ref=f"EASA_AMC_OSO_SAIL_{sail.value}",
            doc_ref=DocReference(
                doc_id="EASA_EAR_UAS_2024",
                standard="SORA_2.0",
                section="Annex D, OSO Requirements"
            ),
            notes=f"SAIL {sail.value} requires {len(oso_requirements)} OSOs"
        ))
        
        return oso_requirements
    
    def _get_robustness_levels(self, sail: SAILValue, oso_requirements: List[str], 
                             trace: List[TraceEntry]) -> Dict[str, str]:
        """Get robustness levels for each OSO based on SAIL."""
        robustness = {}
        
        for oso in oso_requirements:
            if oso in OSO_ROBUSTNESS_MATRIX and sail.value in OSO_ROBUSTNESS_MATRIX[oso]:
                level = OSO_ROBUSTNESS_MATRIX[oso][sail.value]
            else:
                # Fallback heuristic
                if sail in [SAILValue.I, SAILValue.II]:
                    level = "Low"
                elif sail in [SAILValue.III, SAILValue.IV]:
                    level = "Medium"
                else:
                    level = "High"
            
            robustness[oso] = level
        
        trace.append(TraceEntry(
            step="robustness_determination",
            inputs={"sail": sail.value, "oso_count": len(oso_requirements)},
            result=f"Robustness levels assigned for {len(oso_requirements)} OSOs",
            rule_ref="EASA_AMC_Table_D2_Robustness",
            doc_ref=DocReference(
                doc_id="EASA_EAR_UAS_2024",
                standard="SORA_2.0",
                section="Annex D, Table D.2"
            ),
            notes=f"Robustness levels determined for SAIL {sail.value}"
        ))
        
        return robustness


class SAILCalculator25:
    """SORA 2.5 SAIL calculator with full traceability."""
    
    def __init__(self):
        """Initialize SORA 2.5 calculator."""
        self.sail_table = SAIL_TABLE_25
        self.oso_by_sail = OSO_BY_SAIL_25
        self.version = "SORA_2.5"
    
    def calculate(self, inputs: SAILInputs25) -> SAILResult:
        """
        Calculate SAIL from GRC and ARC using SORA 2.5.
        
        Args:
            inputs: SORA 2.5 SAIL calculation inputs
            
        Returns:
            SAILResult with determined SAIL, OSO requirements, and trace
            
        Raises:
            InvalidGRCError: If GRC is out of range
            InvalidARCError: If ARC is invalid
            SAILMappingError: If SAIL lookup fails
        """
        trace = []
        
        try:
            # Step 1: Validate inputs
            self._validate_inputs(inputs.final_grc, inputs.final_arc, trace)
            
            # Step 2: Determine SAIL from table
            sail = self._determine_sail(inputs.final_grc, inputs.final_arc, trace)
            
            # Step 3: Get OSO requirements
            oso_requirements = self._get_oso_requirements(sail, trace)
            
            # Step 4: Get robustness levels
            robustness_levels = self._get_robustness_levels(sail, oso_requirements, trace)
            
            return SAILResult(
                version=self.version,
                sail=sail,
                final_grc=inputs.final_grc,
                final_arc=inputs.final_arc,
                oso_requirements=oso_requirements,
                oso_count=len(oso_requirements),
                robustness_levels=robustness_levels,
                calculation_trace=trace
            )
            
        except Exception as e:
            trace.append(TraceEntry(
                step="error",
                inputs={"grc": inputs.final_grc, "arc": inputs.final_arc.value},
                result=f"Error: {str(e)}",
                rule_ref="ERROR_HANDLING",
                doc_ref=DocReference(
                    doc_id="JARUS_SORA_MB_2.5",
                    standard="SORA_2.5",
                    section="Error Handling"
                ),
                notes=f"Calculation failed: {str(e)}"
            ))
            raise
    
    def _validate_inputs(self, grc: int, arc: ARCValue, trace: List[TraceEntry]) -> None:
        """Validate SAIL calculation inputs."""
        if not 1 <= grc <= 8:
            raise InvalidGRCError(f"GRC must be 1-8, got {grc}")
        
        if arc.value.lower() not in ["a", "b", "c", "d"]:
            raise InvalidARCError(f"ARC must be a/b/c/d, got {arc.value}")
        
        trace.append(TraceEntry(
            step="input_validation",
            inputs={"grc": grc, "arc": arc.value},
            result="Valid inputs",
            rule_ref="SORA25_INPUT_VALIDATION",
            doc_ref=DocReference(
                doc_id="JARUS_SORA_MB_2.5",
                standard="SORA_2.5",
                section="Main Body - SAIL Determination"
            ),
            notes=f"GRC {grc} and ARC {arc.value.upper()} are valid"
        ))
    
    def _determine_sail(self, grc: int, arc: ARCValue, trace: List[TraceEntry]) -> SAILValue:
        """Determine SAIL using SORA 2.5 table."""
        arc_lower = arc.value.lower()
        
        # SORA 2.5 specific logic with different GRC groupings
        if grc in (1, 2):
            sail = self.sail_table[(1, 2)][arc_lower]
            grc_ref = "1-2"
        elif grc in (3, 4):
            sail = self.sail_table[(3, 4)][arc_lower]
            grc_ref = "3-4"
        elif grc == 5:
            sail = self.sail_table[5][arc_lower]
            grc_ref = "5"
        elif grc == 6:
            sail = self.sail_table[6][arc_lower]
            grc_ref = "6"
        elif grc in (7, 8):
            sail = self.sail_table[(7, 8)][arc_lower]
            grc_ref = "7-8"
        else:
            raise SAILMappingError(f"GRC {grc} out of valid range 1-8")
        
        trace.append(TraceEntry(
            step="sail_determination",
            inputs={"grc": grc, "arc": arc.value},
            result=sail.value,
            rule_ref=f"JARUS_SORA25_Annex_D_Table_GRC{grc_ref}_ARC{arc.value.upper()}",
            doc_ref=DocReference(
                doc_id="JARUS_SORA_MB_2.5",
                standard="SORA_2.5",
                section="Annex D v1.0, SAIL Table"
            ),
            notes=f"GRC {grc} × ARC {arc.value.upper()} → SAIL {sail.value} (SORA 2.5)"
        ))
        
        return sail
    
    def _get_oso_requirements(self, sail: SAILValue, trace: List[TraceEntry]) -> List[str]:
        """Get OSO requirements for the determined SAIL."""
        oso_requirements = self.oso_by_sail[sail]
        
        trace.append(TraceEntry(
            step="oso_requirements",
            inputs={"sail": sail.value},
            result=f"{len(oso_requirements)} OSOs required",
            rule_ref=f"JARUS_SORA25_OSO_SAIL_{sail.value}",
            doc_ref=DocReference(
                doc_id="JARUS_SORA_MB_2.5",
                standard="SORA_2.5",
                section="Annex D v1.0, OSO Requirements"
            ),
            notes=f"SAIL {sail.value} requires {len(oso_requirements)} OSOs"
        ))
        
        return oso_requirements
    
    def _get_robustness_levels(self, sail: SAILValue, oso_requirements: List[str],
                             trace: List[TraceEntry]) -> Dict[str, str]:
        """Get robustness levels for SORA 2.5."""
        robustness = {}
        
        for oso in oso_requirements:
            if oso in OSO_ROBUSTNESS_MATRIX and sail.value in OSO_ROBUSTNESS_MATRIX[oso]:
                level = OSO_ROBUSTNESS_MATRIX[oso][sail.value]
            else:
                # Fallback heuristic
                if sail in [SAILValue.I, SAILValue.II]:
                    level = "Low"
                elif sail in [SAILValue.III, SAILValue.IV]:
                    level = "Medium"
                else:
                    level = "High"
            
            robustness[oso] = level
        
        trace.append(TraceEntry(
            step="robustness_determination",
            inputs={"sail": sail.value, "oso_count": len(oso_requirements)},
            result=f"Robustness levels assigned for {len(oso_requirements)} OSOs",
            rule_ref="JARUS_SORA25_Annex_D_Robustness",
            doc_ref=DocReference(
                doc_id="JARUS_SORA_MB_2.5",
                standard="SORA_2.5",
                section="Annex D v1.0, Robustness Requirements"
            ),
            notes=f"Robustness levels determined for SAIL {sail.value}"
        ))
        
        return robustness