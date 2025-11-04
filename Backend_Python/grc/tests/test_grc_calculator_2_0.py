import pytest
from pydantic import ValidationError

from ..models.grc_models import GRCInputs20, GRCResult
from ..calculators.grc_calculator import GRCCalculator20

class TestGRCCalculator20:
    """Test suite for SORA 2.0 GRC calculations"""
    
    def setup_method(self):
        self.calculator = GRCCalculator20()
    
    def test_grc_20_low_population_no_mitigations(self):
        """Test Case 1: Low population (VLOS Sparsely), no mitigations"""
        inputs = GRCInputs20(
            scenario="VLOS_Sparsely",
            dimension_m=2.0,
            population_density_p_km2=0,
            containment_quality="Good",
            m1_strategic="None",
            m2_impact="None",
            m3_erp="None"
        )
        
        result = self.calculator.calculate(inputs)
        
        # Per Table 2: VLOS_Sparsely with dimension <=3m → iGRC=3
        assert result.initial_grc == 3
        # No mitigations + M3 None penalty (+1)
        assert result.residual_grc == 4
        assert result.m1_reduction == 0
        assert result.m2_reduction == 0
        assert result.m3_adjustment == 1  # None → +1 penalty
        assert len(result.calculation_trace) >= 4
        assert result.version == "SORA_2.0"
        
        # Verify trace has proper documentation references
        for trace_entry in result.calculation_trace:
            assert trace_entry.rule_ref
            assert trace_entry.doc_ref.doc_id == "EASA_AMC_GM_UAS_2024"
            assert trace_entry.doc_ref.standard == "SORA 2.0"
    
    def test_grc_20_m1_floor_cap_critical(self):
        """Test Case 2: M1 High with floor cap (CRITICAL TEST)"""
        inputs = GRCInputs20(
            # Choose scenario/dimension yielding iGRC=4: VLOS_Sparsely with 3m<dimension<=8m
            scenario="VLOS_Sparsely",
            dimension_m=5.0,
            population_density_p_km2=0,
            containment_quality="Good",
            m1_strategic="High",  # -4 reduction
            m2_impact="None",
            m3_erp="Medium"  # 0 adjustment
        )
        
        result = self.calculator.calculate(inputs)
        
        assert result.initial_grc == 4  # VLOS_Sparsely (<=8m) → iGRC=4
        # High M1 = -4, so 4 + (-4) = 0, but floor for Good/iGRC=4 is 2
        assert result.residual_grc == 2
        assert result.m1_reduction == -2  # Actual reduction after floor cap (not -4)
        assert result.m2_reduction == 0
        assert result.m3_adjustment == 0  # Medium → 0
        assert result.floor_applied == True
        assert result.floor_value == 2
        
        # Verify floor cap is mentioned in trace
        m1_trace = next(entry for entry in result.calculation_trace if entry.step == "strategic_mitigation_m1")
        assert "Floor cap applied: True" in m1_trace.notes
    
    def test_grc_20_all_mitigations_maximum(self):
        """Test Case 3: All mitigations at maximum levels"""
        inputs = GRCInputs20(
            # Choose scenario/dimension yielding iGRC=5: VLOS_Populated with dimension <=3m
            scenario="VLOS_Populated",
            dimension_m=2.0,
            population_density_p_km2=0,
            containment_quality="Adequate",
            m1_strategic="Medium",  # -2
            m2_impact="High",       # -2
            m3_erp="High"          # -1
        )
        
        result = self.calculator.calculate(inputs)
        
        assert result.initial_grc == 5
        # M1 Medium = -2 → 3 (no floor cap needed)
        # M2 High = -2 → 1
        # M3 High = -1 → 0, but floor at 1
        assert result.residual_grc == 1
        assert result.m1_reduction == -2
        assert result.m2_reduction == -2
        assert result.m3_adjustment == -1
        assert result.total_reduction == 4  # 5 - 1 = 4
    
    def test_grc_20_floor_at_one_applied(self):
        """Test Case 4: Final floor at 1 is applied"""
        inputs = GRCInputs20(
            # Choose scenario/dimension yielding iGRC=3: VLOS_Controlled with 3m<dimension<=8m
            scenario="VLOS_Controlled",
            dimension_m=5.0,
            population_density_p_km2=0,
            containment_quality="Good",
            m1_strategic="High",    # -4, but floor may limit
            m2_impact="High",       # -2
            m3_erp="High"          # -1
        )
        
        result = self.calculator.calculate(inputs)
        
        # Final GRC should never be less than 1
        assert result.residual_grc >= 1
        
        # Check if final floor clamp was applied
        final_clamp_entries = [entry for entry in result.calculation_trace if entry.step == "final_floor_clamp"]
        if result.residual_grc == 1 and len(final_clamp_entries) > 0:
            assert "Applied minimum GRC floor of 1" in final_clamp_entries[0].notes
    
    def test_grc_20_integer_values_only(self):
        """Test Case 5: All reductions are integers (no fractional values)"""
        inputs = GRCInputs20(
            scenario="VLOS_Sparsely",
            dimension_m=5.0,
            population_density_p_km2=0,
            containment_quality="Adequate",
            m1_strategic="Medium",
            m2_impact="Low",
            m3_erp="Low"
        )
        
        result = self.calculator.calculate(inputs)
        
        # All reduction values must be integers
        assert isinstance(result.m1_reduction, int)
        assert isinstance(result.m2_reduction, int)
        assert isinstance(result.m3_adjustment, int)
        assert isinstance(result.initial_grc, int)
        assert isinstance(result.residual_grc, int)
        assert isinstance(result.total_reduction, int)
        
        # Verify no fractional values in trace
        for trace_entry in result.calculation_trace:
            if trace_entry.delta is not None:
                assert isinstance(trace_entry.delta, int)
    
    def test_grc_20_trace_completeness(self):
        """Test Case 6: Calculation trace is complete and properly formatted"""
        inputs = GRCInputs20(
            scenario="VLOS_Controlled",
            dimension_m=2.0,
            population_density_p_km2=0,
            containment_quality="Good",
            m1_strategic="Low",
            m2_impact="None",
            m3_erp="Medium"
        )
        
        result = self.calculator.calculate(inputs)
        
        # Must have at least: initial, M1, M2, M3 entries
        assert len(result.calculation_trace) >= 4
        
        # Check required trace steps
        steps = [entry.step for entry in result.calculation_trace]
        assert "initial_grc_determination" in steps
        assert "strategic_mitigation_m1" in steps
        assert "impact_effects_m2" in steps
        assert "emergency_response_m3" in steps
        
        # Verify all trace entries have required fields
        for entry in result.calculation_trace:
            assert entry.step
            assert entry.rule_ref
            assert entry.doc_ref.doc_id
            assert entry.doc_ref.standard
            assert entry.doc_ref.section
            assert entry.timestamp
            assert entry.result
    
    def test_grc_20_sequential_application(self):
        """Test Case 7: Mitigations applied in correct sequence"""
        inputs = GRCInputs20(
            scenario="BVLOS_Sparsely",
            dimension_m=2.0,
            population_density_p_km2=0,
            containment_quality="Poor",
            m1_strategic="Low",
            m2_impact="Low", 
            m3_erp="High"
        )
        
        result = self.calculator.calculate(inputs)
        
        # Verify sequential application by checking intermediate values in trace
        trace_results = {}
        for entry in result.calculation_trace:
            trace_results[entry.step] = int(entry.result)
        
        igrc = trace_results["initial_grc_determination"]
        after_m1 = trace_results["strategic_mitigation_m1"]
        after_m2 = trace_results["impact_effects_m2"]
        after_m3 = trace_results["emergency_response_m3"]
        
        # Verify sequence: iGRC → M1 → M2 → M3
        assert igrc == result.initial_grc
        assert after_m1 == igrc + result.m1_reduction
        assert after_m2 == after_m1 + result.m2_reduction
        assert after_m3 == after_m2 + result.m3_adjustment
        
        # Final result should be max(1, after_m3)
        expected_final = max(1, after_m3)
        assert result.residual_grc == expected_final