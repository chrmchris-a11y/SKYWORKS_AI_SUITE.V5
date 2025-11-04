import pytest
from pydantic import ValidationError

from ..models.grc_models import GRCInputs25, GRCResult
from ..calculators.grc_calculator import GRCCalculator25

class TestGRCCalculator25:
    """Test suite for SORA 2.5 GRC calculations"""
    
    def setup_method(self):
        self.calculator = GRCCalculator25()
    
    def test_grc_25_na_validation_m1a_high(self):
        """Test Case 1: M1A High should be rejected (N/A validation)"""
        with pytest.raises(ValidationError) as exc_info:
            inputs = GRCInputs25(
                characteristic_dimension_m=2.5,
                max_speed_mps=25.0,
                population_density_p_km2=500,
                containment_quality="Good",
                m1a_sheltering="High",  # N/A! Should cause validation error
                m1b_operational="None",
                m1c_ground_observation="None",
                m2_impact="None"
            )
        
        # This should fail at Pydantic level since High is not in M1ALevel25 enum
        assert "High" in str(exc_info.value)
    
    def test_grc_25_na_validation_m1b_low(self):
        """Test Case 2: M1B Low should be rejected (N/A validation)"""
        with pytest.raises(ValidationError) as exc_info:
            inputs = GRCInputs25(
                characteristic_dimension_m=1.8,
                max_speed_mps=20.0,
                population_density_p_km2=300,
                containment_quality="Adequate",
                m1a_sheltering="None",
                m1b_operational="Low",  # N/A! Should cause validation error
                m1c_ground_observation="None",
                m2_impact="Medium"
            )
        
        # This should fail at Pydantic level since Low is not in M1BLevel25 enum
        assert "Low" in str(exc_info.value)
    
    def test_grc_25_na_validation_m1c_medium(self):
        """Test Case 3: M1C Medium should be rejected (N/A validation)"""
        with pytest.raises(ValidationError) as exc_info:
            inputs = GRCInputs25(
                characteristic_dimension_m=3.0,
                max_speed_mps=30.0,
                population_density_p_km2=800,
                containment_quality="Poor",
                m1a_sheltering="Low",
                m1b_operational="High",
                m1c_ground_observation="Medium",  # N/A! Should cause validation error
                m2_impact="High"
            )
        
        # This should fail at Pydantic level since Medium is not in M1CLevel25 enum
        assert "Medium" in str(exc_info.value)
    
    def test_grc_25_quantitative_model_basic(self):
        """Test Case 4: Basic quantitative model calculation"""
        inputs = GRCInputs25(
            characteristic_dimension_m=1.5,
            max_speed_mps=20.0,
            population_density_p_km2=300,
            environment_type="Suburban",
            containment_quality="Good",
            m1a_sheltering="Low",      # -1
            m1b_operational="Medium",  # -1
            m1c_ground_observation="Low",  # -1
            m2_impact="High"          # -2
        )
        
        result = self.calculator.calculate(inputs)
        
        # Verify quantitative model was used
        assert result.version == "SORA_2.5"
        assert 1 <= result.initial_grc <= 8
        
        # Verify M1A/M1B/M1C reductions applied
        assert result.m1a_reduction == -1
        assert result.m1b_reduction == -1
        assert result.m1c_reduction == -1
        assert result.m2_reduction == -2
        
        # Verify final GRC is at least 1
        assert result.residual_grc >= 1
        
        # Verify trace includes quantitative model step
        steps = [entry.step for entry in result.calculation_trace]
        assert "initial_grc_quantitative" in steps
        assert "strategic_mitigations_m1" in steps
        assert "impact_dynamics_m2" in steps
    
    def test_grc_25_all_none_mitigations(self):
        """Test Case 5: All mitigations set to None"""
        inputs = GRCInputs25(
            characteristic_dimension_m=2.0,
            max_speed_mps=15.0,
            population_density_p_km2=100,
            containment_quality="Adequate",
            m1a_sheltering="None",
            m1b_operational="None",
            m1c_ground_observation="None",
            m2_impact="None"
        )
        
        result = self.calculator.calculate(inputs)
        
        # All reductions should be 0
        assert result.m1a_reduction == 0
        assert result.m1b_reduction == 0
        assert result.m1c_reduction == 0
        assert result.m2_reduction == 0
        
        # Final GRC should equal initial GRC (no reductions)
        assert result.residual_grc == result.initial_grc
        assert result.total_reduction == 0
    
    def test_grc_25_maximum_mitigations(self):
        """Test Case 6: Maximum allowed mitigations"""
        inputs = GRCInputs25(
            characteristic_dimension_m=4.0,
            max_speed_mps=40.0,
            population_density_p_km2=2000,
            containment_quality="Good",
            m1a_sheltering="Medium",   # -2 (max for M1A)
            m1b_operational="High",    # -2 (max for M1B)
            m1c_ground_observation="Low",  # -1 (max for M1C)
            m2_impact="High"          # -2 (max for M2)
        )
        
        result = self.calculator.calculate(inputs)
        
        # Verify maximum reductions
        assert result.m1a_reduction == -2
        assert result.m1b_reduction == -2
        assert result.m1c_reduction == -1
        assert result.m2_reduction == -2
        
        # Total M1 reduction should be -5, M2 should be -2
        total_expected_reduction = -2 + -2 + -1 + -2  # -7
        
        # But floor caps may apply
        assert result.residual_grc >= 1
        assert result.total_reduction >= 0  # Can't be negative
    
    def test_grc_25_integer_values_only(self):
        """Test Case 7: All values are integers (no fractional)"""
        inputs = GRCInputs25(
            characteristic_dimension_m=1.2,
            max_speed_mps=18.5,
            population_density_p_km2=450,
            containment_quality="Poor",
            m1a_sheltering="Low",
            m1b_operational="Medium",
            m1c_ground_observation="None",
            m2_impact="Medium"
        )
        
        result = self.calculator.calculate(inputs)
        
        # All reduction values must be integers
        assert isinstance(result.m1a_reduction, int)
        assert isinstance(result.m1b_reduction, int)
        assert isinstance(result.m1c_reduction, int)
        assert isinstance(result.m2_reduction, int)
        assert isinstance(result.initial_grc, int)
        assert isinstance(result.residual_grc, int)
        assert isinstance(result.total_reduction, int)
        
        # Verify no fractional values in trace
        for trace_entry in result.calculation_trace:
            if trace_entry.delta is not None:
                assert isinstance(trace_entry.delta, int)
    
    def test_grc_25_trace_documentation(self):
        """Test Case 8: Trace has proper JARUS documentation references"""
        inputs = GRCInputs25(
            characteristic_dimension_m=2.2,
            max_speed_mps=22.0,
            population_density_p_km2=600,
            containment_quality="Good",
            m1a_sheltering="Medium",
            m1b_operational="High",
            m1c_ground_observation="Low",
            m2_impact="High"
        )
        
        result = self.calculator.calculate(inputs)
        
        # Verify all trace entries have JARUS documentation
        for entry in result.calculation_trace:
            assert entry.rule_ref
            assert entry.doc_ref.doc_id == "JARUS_SORA_2_5_2024"
            assert entry.doc_ref.standard == "SORA 2.5"
            assert entry.doc_ref.section
            assert entry.timestamp
        
        # Verify specific JARUS references
        quantitative_entry = next(entry for entry in result.calculation_trace if entry.step == "initial_grc_quantitative")
        assert "JARUS_SORA_2.5_Annex_F_Model" in quantitative_entry.rule_ref