import pytest
from pathlib import Path

from arc.models.arc_models import ARCInputs20, ARCInputs25, ARCValue
from arc.calculators.arc_calculator import ARCCalculator20, ARCCalculator25
from arc.validators.arc_validator import ARCValidator

class TestARCCalculator20:
    """Test SORA 2.0 ARC calculations"""
    
    @pytest.fixture
    def calculator(self):
        return ARCCalculator20()
    
    def test_golden_case_1_low_risk_vlos(self, calculator):
        """Golden Test Case 1: SORA 2.0 - Low Risk VLOS"""
        inputs = ARCInputs20(
            operation_type="VLOS",
            airspace_class="G",
            air_traffic_density=1,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=50,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="None",
            operational_limitation_boundary=False,
            operational_limitation_chronology=False,
            procedural_coordination=False,
            geo_fencing=False
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.A
        assert result.residual_arc == ARCValue.A
        assert result.total_reduction == 0
        assert result.mitigations_applied == []
        assert len(result.calculation_trace) >= 1
        
        # Verify trace contains proper references
        initial_trace = result.calculation_trace[0]
        assert initial_trace.step == "initial_arc_determination"
        assert "EASA_EAR_UAS_2024" in initial_trace.doc_ref["doc_id"]
    
    def test_golden_case_2_high_risk_with_mitigations(self, calculator):
        """Golden Test Case 2: SORA 2.0 - High Risk with Mitigations"""
        inputs = ARCInputs20(
            operation_type="BVLOS",
            airspace_class="E",
            air_traffic_density=4,
            proximity_aerodrome="Near",
            operational_altitude_agl_m=150,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Full_Certified",
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=False,
            geo_fencing=True
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.C
        assert result.residual_arc == ARCValue.A  # Reduced by 2 classes (certified cap)
        assert result.total_reduction == 2
        assert "Full_Certified_Segregation" in result.mitigations_applied
        assert "Boundary_Limitation" in result.supporting_evidence
        assert "Chronology_Limitation" in result.supporting_evidence
        assert "Geo_fencing" in result.supporting_evidence
        
        # Verify geo-fencing is supporting evidence only (0 direct reduction)
        geo_trace = [t for t in result.calculation_trace if t.step == "strategic_mitigation_geofencing"]
        assert len(geo_trace) == 1
        assert geo_trace[0].delta == 0
    
    def test_reduction_cap_without_certification(self, calculator):
        """Test reduction cap enforcement without certification"""
        inputs = ARCInputs20(
            operation_type="BVLOS",
            airspace_class="D",
            air_traffic_density=4,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=100,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Partial",  # Only 1 class allowed
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=True,
            geo_fencing=False
        )
        
        result = calculator.calculate(inputs)
        
        # Should be capped at 1 class reduction without certification
        assert result.total_reduction <= 1
        
        # Check cap was applied
        cap_trace = [t for t in result.calculation_trace if t.step == "reduction_cap_applied"]
        if len(cap_trace) > 0:
            assert cap_trace[0].delta < 0  # Negative delta indicates reduction was capped
    
    def test_arc_floor_never_below_a(self, calculator):
        """Test that ARC never goes below 'a' regardless of mitigations"""
        inputs = ARCInputs20(
            operation_type="VLOS",
            airspace_class="G",
            air_traffic_density=1,  # Already at 'a'
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=30,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Full_Certified",  # 2 class reduction
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=True,
            geo_fencing=True
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.A
        assert result.residual_arc == ARCValue.A  # Cannot go below 'a'
        assert result.total_reduction >= 0
    
    def test_geo_fencing_supporting_evidence_only(self, calculator):
        """Test geo-fencing provides 0 direct reduction"""
        inputs = ARCInputs20(
            operation_type="EVLOS",
            airspace_class="E",
            air_traffic_density=3,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=120,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="None",
            operational_limitation_boundary=False,
            operational_limitation_chronology=False,
            procedural_coordination=False,
            geo_fencing=True  # Only geo-fencing enabled
        )
        
        result = calculator.calculate(inputs)
        
        # Geo-fencing alone should provide 0 reduction
        assert result.total_reduction == 0
        assert "Geo_fencing" in result.supporting_evidence
        assert result.mitigations_applied == []  # No direct mitigations
    
    def test_integer_reductions_only(self, calculator):
        """Test that all reductions are integers (no fractional)"""
        inputs = ARCInputs20(
            operation_type="BVLOS",
            airspace_class="C",
            air_traffic_density=5,
            proximity_aerodrome="Inside",
            operational_altitude_agl_m=200,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            airspace_segregation="Full_Certified",
            operational_limitation_boundary=True,
            operational_limitation_chronology=True,
            procedural_coordination=True,
            geo_fencing=True
        )
        
        result = calculator.calculate(inputs)
        
        # Total reduction must be integer
        assert isinstance(result.total_reduction, int)
        assert result.total_reduction == int(result.total_reduction)
        
        # All trace deltas must be integers
        for trace_entry in result.calculation_trace:
            if trace_entry.delta is not None:
                assert isinstance(trace_entry.delta, int)


class TestARCCalculator25:
    """Test SORA 2.5 ARC calculations"""
    
    @pytest.fixture
    def calculator(self):
        return ARCCalculator25()
    
    def test_golden_case_3_u_space_operation(self, calculator):
        """Golden Test Case 3: SORA 2.5 - U-space Operation"""
        inputs = ARCInputs25(
            operation_type="BVLOS",
            airspace_class="U-space",
            air_traffic_density=3,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=100,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            u_space_services_available=True,
            traffic_density_data_source="Empirical",
            airspace_containment="Certified",
            temporal_segregation=False,
            spatial_segregation=False,
            u_space_services=True
        )
        
        result = calculator.calculate(inputs)
        
        assert result.initial_arc == ARCValue.B
        assert result.residual_arc == ARCValue.A  # Reduced by certified containment
        assert result.total_reduction <= 2  # Certified cap
        assert "Certified_Containment" in result.mitigations_applied
        assert "U_space_Services" in result.supporting_evidence
        
        # U-space should be supporting evidence, not direct reduction
        uspace_trace = [t for t in result.calculation_trace if t.step == "strategic_mitigation_uspace_25"]
        if uspace_trace:
            assert uspace_trace[0].delta == 0
    
    def test_u_space_services_supporting_evidence(self, calculator):
        """Test U-space services as supporting evidence within cap"""
        inputs = ARCInputs25(
            operation_type="EVLOS",
            airspace_class="U-space",
            air_traffic_density=2,
            proximity_aerodrome="Outside",
            operational_altitude_agl_m=80,
            operational_volume_defined=True,
            adjacent_airspace_characterised=True,
            u_space_services_available=True,
            traffic_density_data_source="Empirical",
            airspace_containment="None",
            temporal_segregation=False,
            spatial_segregation=False,
            u_space_services=True
        )
        
        result = calculator.calculate(inputs)
        
        # U-space alone (without other mitigations) should be supporting evidence only
        assert "U_space_Services" in result.supporting_evidence
        # Should not provide direct reduction without other mitigations
        assert result.total_reduction == 0 or len(result.mitigations_applied) > 0
