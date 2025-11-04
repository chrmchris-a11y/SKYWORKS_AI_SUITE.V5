"""OSO requirements mapper for SAIL levels."""

from typing import Dict, List
from ..models.sail_models import SAILValue, RobustnessLevel
from ..data.oso_requirements import (
    OSO_DESCRIPTIONS, OSO_ROBUSTNESS_MATRIX, 
    ROBUSTNESS_DESCRIPTIONS, OSO_CATEGORIES
)


class OSOMapper:
    """Maps SAIL levels to OSO requirements and robustness levels."""
    
    def __init__(self, sora_version: str = "2.0"):
        """Initialize OSO mapper with SORA version."""
        self.sora_version = sora_version
        self.oso_descriptions = OSO_DESCRIPTIONS
        self.robustness_matrix = OSO_ROBUSTNESS_MATRIX
        self.robustness_descriptions = ROBUSTNESS_DESCRIPTIONS
        self.oso_categories = OSO_CATEGORIES
    
    def get_oso_list(self, sail: SAILValue) -> List[str]:
        """
        Get list of OSO identifiers for a SAIL level.
        
        Args:
            sail: SAIL level (I-VI)
            
        Returns:
            List of required OSO identifiers
        """
        # OSO requirements are cumulative
        if sail == SAILValue.I:
            return ["OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6"]
        elif sail == SAILValue.II:
            return [
                "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
                "OSO #7", "OSO #8", "OSO #9", "OSO #10"
            ]
        elif sail == SAILValue.III:
            return [
                "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
                "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
                "OSO #13", "OSO #14", "OSO #15"
            ]
        elif sail == SAILValue.IV:
            return [
                "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
                "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
                "OSO #13", "OSO #14", "OSO #15", "OSO #16", "OSO #17", "OSO #18"
            ]
        elif sail == SAILValue.V:
            return [
                "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
                "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
                "OSO #13", "OSO #14", "OSO #15", "OSO #16", "OSO #17", "OSO #18",
                "OSO #19", "OSO #20", "OSO #21"
            ]
        elif sail == SAILValue.VI:
            return [
                "OSO #1", "OSO #2", "OSO #3", "OSO #4", "OSO #5", "OSO #6",
                "OSO #7", "OSO #8", "OSO #9", "OSO #10", "OSO #11", "OSO #12",
                "OSO #13", "OSO #14", "OSO #15", "OSO #16", "OSO #17", "OSO #18",
                "OSO #19", "OSO #20", "OSO #21", "OSO #22", "OSO #23", "OSO #24"
            ]
        else:
            raise ValueError(f"Invalid SAIL value: {sail}")
    
    def get_oso_requirements(self, sail: SAILValue) -> Dict[str, str]:
        """
        Get OSO requirements with robustness levels for a SAIL level.
        
        Args:
            sail: SAIL level (I-VI)
            
        Returns:
            Dictionary mapping OSO ID to robustness level
        """
        oso_list = self.get_oso_list(sail)
        return self.get_robustness_levels(sail, oso_list)
    
    def get_robustness_levels(self, sail: SAILValue, oso_list: List[str]) -> Dict[str, 'RobustnessLevel']:
        """
        Get robustness levels for OSOs based on SAIL.
        
        Args:
            sail: SAIL level (I-VI)
            oso_list: List of OSO identifiers
            
        Returns:
            Dictionary mapping OSO to RobustnessLevel enum
        """
        from sail.data.oso_requirements import RobustnessLevel
        robustness = {}
        
        # robustness_matrix is indexed by SAIL, then by OSO ID
        sail_matrix = self.robustness_matrix.get(sail.value, {})
        
        for oso in oso_list:
            if oso in sail_matrix:
                level_value = sail_matrix[oso]
                # Convert string to RobustnessLevel enum if needed
                if isinstance(level_value, str):
                    robustness[oso] = RobustnessLevel(level_value)
                else:
                    robustness[oso] = level_value
            else:
                # Fallback based on SAIL level
                robustness[oso] = self._get_default_robustness(sail)
        
        return robustness
    
    def _get_default_robustness(self, sail: SAILValue) -> 'RobustnessLevel':
        """Get default robustness level for a SAIL."""
        from sail.data.oso_requirements import RobustnessLevel
        if sail in [SAILValue.I, SAILValue.II]:
            return RobustnessLevel.LOW
        elif sail in [SAILValue.III, SAILValue.IV]:
            return RobustnessLevel.MEDIUM
        else:
            return RobustnessLevel.HIGH
    
    def get_oso_description(self, oso_id: str) -> str:
        """Get description for an OSO."""
        return self.oso_descriptions.get(oso_id, f"Description not available for {oso_id}")
    
    def get_oso_category(self, oso_id: str) -> str:
        """Get category for an OSO."""
        return self.oso_categories.get(oso_id, "Uncategorized")
    
    def get_robustness_description(self, level: str) -> str:
        """Get description for a robustness level."""
        return self.robustness_descriptions.get(level, f"Description not available for {level}")
    
    def validate_oso_count(self, sail: SAILValue, oso_count: int) -> bool:
        """
        Validate that OSO count matches expected for SAIL.
        
        Args:
            sail: SAIL level
            oso_count: Number of OSOs
            
        Returns:
            True if count is correct, False otherwise
        """
        expected_counts = {
            SAILValue.I: 6,
            SAILValue.II: 10,
            SAILValue.III: 15,
            SAILValue.IV: 18,
            SAILValue.V: 21,
            SAILValue.VI: 24
        }
        
        return oso_count == expected_counts.get(sail, 0)
    
    def get_expected_oso_count(self, sail: SAILValue) -> int:
        """Get expected OSO count for a SAIL level."""
        expected_counts = {
            SAILValue.I: 6,
            SAILValue.II: 10,
            SAILValue.III: 15,
            SAILValue.IV: 18,
            SAILValue.V: 21,
            SAILValue.VI: 24
        }
        
        return expected_counts.get(sail, 0)
    
    def get_oso_count(self, sail) -> int:
        """Alias for get_expected_oso_count() - accepts SAILValue or SAILLevel."""
        # Convert SAILLevel to SAILValue if needed
        if hasattr(sail, 'value'):
            sail_str = sail.value
            sail_val = SAILValue(sail_str)
        else:
            sail_val = sail
        return self.get_expected_oso_count(sail_val)
    
    def get_all_osos(self) -> List[str]:
        """Get all available OSO identifiers."""
        return [f"OSO#{i:02d}" for i in range(1, 25)]
    
    def get_osos_by_category(self, category: str) -> List[str]:
        """Get OSOs belonging to a specific category."""
        return [oso for oso, cat in self.oso_categories.items() if cat == category]
    
    def get_available_categories(self) -> List[str]:
        """Get all available OSO categories."""
        return list(set(self.oso_categories.values()))