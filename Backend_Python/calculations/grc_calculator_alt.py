"""
SKYWORKS AI SUITE - GRC Calculator
Ground Risk Class Calculation - 100% EASA/JARUS Compliant

SORA 2.0 Source: JARUS SORA 2.0 (JAR-DEL-WG6-D.04 Edition 2.0)
  - Table 2: Intrinsic GRC vs. scenario and characteristic dimension (Page 10)
  - Table 3: Effects of M1, M2, M3 mitigations (Page 11)

SORA 2.5 Source: JARUS SORA 2.5 (JAR-DEL-SRM-SORA-MB-2.5)
  - Table 2: Intrinsic GRC vs. population density and dimension+speed category (Page 22)
  - Table 3: Effects of M1, M2, M3 mitigations (Page 23)
"""

from typing import Optional
from ..models.sora_models import (
    GRCRequest_2_0,
    GRCRequest_2_5,
    GRCResponse,
    MitigationLevel,
)


class GRCCalculator:
    """Ground Risk Class Calculator - EASA/JARUS Authoritative Implementation"""

    # ═══════════════════════════════════════════════════════════════════════
    # SORA 2.0 - INTRINSIC GRC TABLE (Table 2, Page 10)
    # ═══════════════════════════════════════════════════════════════════════
    # Rows: Operational Scenarios
    # Columns: Characteristic Dimension (≤1m, ≤3m, ≤8m, >8m)
    # Values: Intrinsic GRC (None = undefined cell, 10 = out of scope)
    
    INTRINSIC_GRC_TABLE_2_0 = {
        "VLOS_Controlled": [1, 2, 4, 7],
        "VLOS_Sparsely": [2, 4, 6, 8],
        "BVLOS_Sparsely": [3, 5, 7, 9],
        "VLOS_Populated": [4, 6, 8, 10],
        "BVLOS_Populated": [5, 7, 9, 10],
        "VLOS_Gathering": [6, 8, 10, 10],
        "BVLOS_Gathering": [7, 9, 10, 10],
    }

    # Minimum GRC floor per dimension category (for M1 strategic mitigation)
    MIN_GRC_FLOOR_2_0 = {
        "1m": 1,
        "3m": 2,
        "8m": 3,
        ">8m": 4,
    }

    # ═══════════════════════════════════════════════════════════════════════
    # SORA 2.5 - INTRINSIC GRC TABLE (Table 2, Page 22)
    # ═══════════════════════════════════════════════════════════════════════
    # Rows: Population Density Categories (7 rows)
    # Columns: Combined Dimension+Speed Categories (5 columns)
    # Values: Intrinsic GRC (-1 = out of scope, ≥9 = out of scope)
    
    INTRINSIC_GRC_TABLE_2_5 = [
        # Row 0: Controlled ground area
        [1, 2, 3, 4, 5],
        # Row 1: <5 people/km²
        [2, 3, 4, 5, 6],
        # Row 2: <50 people/km²
        [3, 4, 5, 6, 7],
        # Row 3: <500 people/km² (Rural)
        [4, 5, 6, 7, 8],
        # Row 4: <5,000 people/km² (Suburban)
        [5, 6, 7, 8, -1],
        # Row 5: <50,000 people/km² (Urban)
        [6, 7, 8, -1, -1],
        # Row 6: ≥50,000 people/km² (Assemblies)
        [7, 8, -1, -1, -1],
    ]

    POPULATION_CATEGORIES = [
        "Controlled ground area",
        "<5 people/km²",
        "<50 people/km²",
        "<500 people/km² (Rural)",
        "<5,000 people/km² (Suburban)",
        "<50,000 people/km² (Urban)",
        "≥50,000 people/km² (Assemblies)",
    ]

    DIMENSION_SPEED_LABELS = ["1m", "3m", "8m", "20m", "40m"]

    # ═══════════════════════════════════════════════════════════════════════
    # HELPER FUNCTIONS
    # ═══════════════════════════════════════════════════════════════════════

    @staticmethod
    def _get_dimension_category_2_0(dimension_m: float) -> tuple[str, int]:
        """
        Determine dimension category for SORA 2.0
        Returns: (category_label, column_index)
        """
        epsilon = 1e-6  # For floating point comparison
        
        if dimension_m <= 1.0 + epsilon:
            return ("1m", 0)
        elif dimension_m <= 3.0 + epsilon:
            return ("3m", 1)
        elif dimension_m <= 8.0 + epsilon:
            return ("8m", 2)
        else:
            return (">8m", 3)

    @staticmethod
    def _get_dimension_speed_category_2_5(
        dimension_m: float,
        speed_ms: float
    ) -> tuple[str, int]:
        """
        Determine combined dimension+speed category for SORA 2.5
        Takes the worst-case (max) of dimension and speed categories
        
        Returns: (category_label, column_index)
        """
        epsilon = 1e-6
        
        # Dimension categories (0-4)
        if dimension_m <= 1.0 + epsilon:
            dim_cat = 0
        elif dimension_m <= 3.0 + epsilon:
            dim_cat = 1
        elif dimension_m <= 8.0 + epsilon:
            dim_cat = 2
        elif dimension_m <= 20.0 + epsilon:
            dim_cat = 3
        else:  # >20m
            dim_cat = 4
        
        # Speed categories (0-4)
        if speed_ms < 25.0:
            speed_cat = 0
        elif speed_ms < 35.0:
            speed_cat = 1
        elif speed_ms < 75.0:
            speed_cat = 2
        elif speed_ms < 120.0:
            speed_cat = 3
        else:  # ≥120 m/s
            speed_cat = 4
        
        # Worst-case (max)
        final_cat = max(dim_cat, speed_cat)
        
        return (GRCCalculator.DIMENSION_SPEED_LABELS[final_cat], final_cat)

    @staticmethod
    def _get_population_row_2_5(
        population_density: float,
        is_controlled_ground: bool
    ) -> tuple[str, int]:
        """
        Determine population density row for SORA 2.5
        Returns: (category_label, row_index)
        """
        if is_controlled_ground:
            return (GRCCalculator.POPULATION_CATEGORIES[0], 0)
        
        if population_density < 5.0:
            return (GRCCalculator.POPULATION_CATEGORIES[1], 1)
        elif population_density < 50.0:
            return (GRCCalculator.POPULATION_CATEGORIES[2], 2)
        elif population_density < 500.0:
            return (GRCCalculator.POPULATION_CATEGORIES[3], 3)
        elif population_density < 5000.0:
            return (GRCCalculator.POPULATION_CATEGORIES[4], 4)
        elif population_density < 50000.0:
            return (GRCCalculator.POPULATION_CATEGORIES[5], 5)
        else:  # ≥50,000
            return (GRCCalculator.POPULATION_CATEGORIES[6], 6)

    @staticmethod
    def _apply_mitigations(
        intrinsic_grc: int,
        m1_level: MitigationLevel,
        m2_level: MitigationLevel,
        m3_level: MitigationLevel,
        min_grc_floor: int,
    ) -> tuple[int, int, int, int]:
        """
        Apply M1, M2, M3 mitigations sequentially
        
        M1 Strategic (JARUS Table 3):
          - NONE: 0
          - LOW: -1
          - MEDIUM: -2
          - HIGH: -4
          Floor: Cannot go below min_grc_floor for the dimension category
        
        M2 Ground Impact Effects:
          - NONE/LOW/MEDIUM: 0
          - HIGH: -1
        
        M3 Emergency Response Plan:
          - NONE/LOW/MEDIUM: 0
          - HIGH: -1
        
        Returns: (final_grc, m1_effect, m2_effect, m3_effect)
        """
        # M1 Strategic Mitigation
        m1_reduction = {
            MitigationLevel.NONE: 0,
            MitigationLevel.LOW: -1,
            MitigationLevel.MEDIUM: -2,
            MitigationLevel.HIGH: -4,
        }[m1_level]
        
        grc_after_m1 = intrinsic_grc + m1_reduction
        grc_after_m1 = max(grc_after_m1, min_grc_floor)  # Apply floor
        m1_effect = grc_after_m1 - intrinsic_grc
        
        # M2 Ground Impact Effects
        m2_reduction = -1 if m2_level == MitigationLevel.HIGH else 0
        grc_after_m2 = grc_after_m1 + m2_reduction
        m2_effect = m2_reduction
        
        # M3 Emergency Response Plan
        m3_reduction = -1 if m3_level == MitigationLevel.HIGH else 0
        grc_after_m3 = grc_after_m2 + m3_reduction
        m3_effect = m3_reduction
        
        # Final GRC cannot be less than 1
        final_grc = max(1, grc_after_m3)
        
        return (final_grc, m1_effect, m2_effect, m3_effect)

    # ═══════════════════════════════════════════════════════════════════════
    # SORA 2.0 CALCULATION
    # ═══════════════════════════════════════════════════════════════════════

    def calculate_grc_2_0(self, request: GRCRequest_2_0) -> GRCResponse:
        """
        Calculate GRC for SORA 2.0
        
        Source: JARUS SORA 2.0, Table 2 (Page 10) & Table 3 (Page 11)
        
        Steps:
        1. Determine dimension category (≤1m, ≤3m, ≤8m, >8m)
        2. Look up intrinsic GRC from Table 2
        3. Check for undefined cells or out-of-scope (≥10)
        4. Apply M1, M2, M3 mitigations with floor constraints
        """
        
        # Normalize scenario name (handle "SparselyPopulated" → "Sparsely")
        scenario = request.operational_scenario
        if "SparselyPopulated" in scenario:
            scenario = scenario.replace("SparselyPopulated", "Sparsely")
        
        # Step 1: Determine dimension category
        dim_category, dim_col_index = self._get_dimension_category_2_0(
            request.max_dimension_m
        )
        
        # Step 2: Look up intrinsic GRC
        if scenario not in self.INTRINSIC_GRC_TABLE_2_0:
            raise ValueError(
                f"Unknown operational scenario: '{scenario}'. "
                f"Valid values: {list(self.INTRINSIC_GRC_TABLE_2_0.keys())}"
            )
        
        scenario_row = self.INTRINSIC_GRC_TABLE_2_0[scenario]
        intrinsic_grc = scenario_row[dim_col_index]
        
        # Step 3: Check for undefined cells (None in table)
        if intrinsic_grc is None:
            raise ValueError(
                f"Scenario '{scenario}' not defined in SORA 2.0 for dimension {dim_category}"
            )
        
        # Step 4: Check for out-of-scope (iGRC ≥ 10)
        if intrinsic_grc >= 10:
            raise ValueError(
                f"OUT_OF_SCOPE|iGRC={intrinsic_grc}|reason="
                f"Scenario '{scenario}' with dimension {dim_category} "
                f"(iGRC={intrinsic_grc}) exceeds SORA scope (Category C operation)"
            )
        
        # Step 5: Check for iGRC ≥ 9 (also out of scope)
        if intrinsic_grc >= 9:
            raise ValueError(
                f"OUT_OF_SCOPE|iGRC={intrinsic_grc}|reason="
                f"Intrinsic GRC {intrinsic_grc} requires Category C authorization"
            )
        
        # Step 6: Apply mitigations
        min_grc_floor = self.MIN_GRC_FLOOR_2_0[dim_category]
        
        final_grc, m1_effect, m2_effect, m3_effect = self._apply_mitigations(
            intrinsic_grc,
            request.m1_strategic,
            request.m2_ground_impact,
            request.m3_emergency_response,
            min_grc_floor,
        )
        
        # Build detailed notes
        notes = (
            f"SORA 2.0 GRC Calculation:\n"
            f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
            f"Operational Parameters:\n"
            f"  • Scenario: {scenario}\n"
            f"  • Max Dimension: {request.max_dimension_m:.2f}m → Category: {dim_category}\n"
            f"  • Intrinsic GRC: {intrinsic_grc}\n"
            f"  • Min GRC Floor: {min_grc_floor}\n"
            f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
            f"Mitigation Effects:\n"
            f"  • M1 Strategic ({request.m1_strategic.value}): {m1_effect:+d} "
            f"(floor-constrained at {min_grc_floor})\n"
            f"  • M2 Ground Impact ({request.m2_ground_impact.value}): {m2_effect:+d}\n"
            f"  • M3 Emergency Response ({request.m3_emergency_response.value}): {m3_effect:+d}\n"
            f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
            f"Final GRC: {final_grc}"
        )
        
        return GRCResponse(
            intrinsic_grc=intrinsic_grc,
            final_grc=final_grc,
            m1_effect=m1_effect,
            m2_effect=m2_effect,
            m3_effect=m3_effect,
            dimension_category=dim_category,
            notes=notes,
            source="JARUS SORA 2.0, Table 2 (Page 10) & Table 3 (Page 11)",
        )

    # ═══════════════════════════════════════════════════════════════════════
    # SORA 2.5 CALCULATION
    # ═══════════════════════════════════════════════════════════════════════

    def calculate_grc_2_5(self, request: GRCRequest_2_5) -> GRCResponse:
        """
        Calculate GRC for SORA 2.5
        
        Source: JARUS SORA 2.5, Table 2 (Page 22) & Table 3 (Page 23)
        
        Steps:
        1. Check 250g rule (weight ≤ 250g AND speed ≤ 25 m/s → iGRC = 1)
        2. Determine combined dimension+speed category (worst-case)
        3. Determine population density row
        4. Look up intrinsic GRC from Table 2
        5. Check for out-of-scope (-1 or ≥9)
        6. Apply M1, M2, M3 mitigations
        """
        
        # ═══════════════════════════════════════════════════════════════
        # SPECIAL RULE: 250g Exception (SORA 2.5 Table 2, Note)
        # ═══════════════════════════════════════════════════════════════
        # If weight ≤ 250g AND speed ≤ 25 m/s → intrinsic GRC = 1
        # If weight is None, infer sub-250g when dimension ≤ 0.25m AND speed ≤ 25 m/s
        
        is_250g_rule = False
        
        if request.weight_kg is not None:
            # Explicit weight provided
            if request.weight_kg <= 0.25 and request.max_speed_ms <= 25.0:
                is_250g_rule = True
        else:
            # Weight not provided - infer from dimension
            if request.max_dimension_m <= 0.25 and request.max_speed_ms <= 25.0:
                is_250g_rule = True
        
        if is_250g_rule:
            # 250g rule applies - intrinsic GRC = 1
            intrinsic_grc = 1
            dim_speed_category = "1m"
            pop_category = "N/A (250g rule)"
            min_grc_floor = 1
            
            # Apply mitigations (though rarely needed for such low risk)
            final_grc, m1_effect, m2_effect, m3_effect = self._apply_mitigations(
                intrinsic_grc,
                request.m1_strategic,
                request.m2_ground_impact,
                request.m3_emergency_response,
                min_grc_floor,
            )
            
            notes = (
                f"SORA 2.5 GRC Calculation:\n"
                f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
                f"250g RULE APPLIED:\n"
                f"  • Weight: {request.weight_kg if request.weight_kg else 'inferred'} kg\n"
                f"  • Speed: {request.max_speed_ms:.1f} m/s\n"
                f"  • Dimension: {request.max_dimension_m:.2f}m\n"
                f"  • Intrinsic GRC: {intrinsic_grc} (automatic)\n"
                f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
                f"Mitigation Effects:\n"
                f"  • M1: {m1_effect:+d}, M2: {m2_effect:+d}, M3: {m3_effect:+d}\n"
                f"  • Final GRC: {final_grc}"
            )
            
            return GRCResponse(
                intrinsic_grc=intrinsic_grc,
                final_grc=final_grc,
                m1_effect=m1_effect,
                m2_effect=m2_effect,
                m3_effect=m3_effect,
                dimension_category=dim_speed_category,
                notes=notes,
                source="JARUS SORA 2.5, Table 2 (Page 22) - 250g Rule",
            )
        
        # ═══════════════════════════════════════════════════════════════
        # STANDARD SORA 2.5 CALCULATION
        # ═══════════════════════════════════════════════════════════════
        
        # Step 1: Determine combined dimension+speed category
        dim_speed_category, dim_speed_col = self._get_dimension_speed_category_2_5(
            request.max_dimension_m,
            request.max_speed_ms,
        )
        
        # Step 2: Determine population density row
        pop_category, pop_row = self._get_population_row_2_5(
            request.population_density,
            request.is_controlled_ground,
        )
        
        # Step 3: Look up intrinsic GRC from Table 2
        intrinsic_grc = self.INTRINSIC_GRC_TABLE_2_5[pop_row][dim_speed_col]
        
        # Step 4: Check for out-of-scope (-1 in table)
        if intrinsic_grc == -1:
            raise ValueError(
                f"OUT_OF_SCOPE|iGRC=-1|reason="
                f"Operation with dimension+speed category '{dim_speed_category}' "
                f"in population '{pop_category}' is out of SORA scope (Category C)"
            )
        
        # Step 5: Check for iGRC ≥ 9
        if intrinsic_grc >= 9:
            raise ValueError(
                f"OUT_OF_SCOPE|iGRC={intrinsic_grc}|reason="
                f"Intrinsic GRC {intrinsic_grc} requires Category C authorization"
            )
        
        # Step 6: Determine M1 floor (row 0 value for this column)
        min_grc_floor = self.INTRINSIC_GRC_TABLE_2_5[0][dim_speed_col]
        
        # Step 7: Apply mitigations
        final_grc, m1_effect, m2_effect, m3_effect = self._apply_mitigations(
            intrinsic_grc,
            request.m1_strategic,
            request.m2_ground_impact,
            request.m3_emergency_response,
            min_grc_floor,
        )
        
        # Build detailed notes
        notes = (
            f"SORA 2.5 GRC Calculation:\n"
            f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
            f"Operational Parameters:\n"
            f"  • Dimension: {request.max_dimension_m:.2f}m\n"
            f"  • Speed: {request.max_speed_ms:.1f} m/s\n"
            f"  • Combined Category: {dim_speed_category}\n"
            f"  • Population Density: {request.population_density:.1f} people/km²\n"
            f"  • Population Category: {pop_category}\n"
            f"  • Controlled Ground: {'Yes' if request.is_controlled_ground else 'No'}\n"
            f"  • Intrinsic GRC: {intrinsic_grc}\n"
            f"  • Min GRC Floor: {min_grc_floor}\n"
            f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
            f"Mitigation Effects:\n"
            f"  • M1 Strategic ({request.m1_strategic.value}): {m1_effect:+d} "
            f"(floor-constrained at {min_grc_floor})\n"
            f"  • M2 Ground Impact ({request.m2_ground_impact.value}): {m2_effect:+d}\n"
            f"  • M3 Emergency Response ({request.m3_emergency_response.value}): {m3_effect:+d}\n"
            f"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n"
            f"Final GRC: {final_grc}"
        )
        
        return GRCResponse(
            intrinsic_grc=intrinsic_grc,
            final_grc=final_grc,
            m1_effect=m1_effect,
            m2_effect=m2_effect,
            m3_effect=m3_effect,
            dimension_category=dim_speed_category,
            notes=notes,
            source="JARUS SORA 2.5, Table 2 (Page 22) & Table 3 (Page 23)",
        )


# ═══════════════════════════════════════════════════════════════════════════
# VALIDATION TESTS
# ═══════════════════════════════════════════════════════════════════════════

def run_validation_tests():
    """Comprehensive validation tests for GRC calculations"""
    from enum import Enum
    
    # Mock models for testing
    class MitigationLevel(str, Enum):
        NONE = "None"
        LOW = "Low"
        MEDIUM = "Medium"
        HIGH = "High"
    
    class GRCRequest_2_0:
        def __init__(self, max_dimension_m, operational_scenario,
                     m1_strategic, m2_ground_impact, m3_emergency_response):
            self.max_dimension_m = max_dimension_m
            self.operational_scenario = operational_scenario
            self.m1_strategic = m1_strategic
            self.m2_ground_impact = m2_ground_impact
            self.m3_emergency_response = m3_emergency_response
    
    class GRCRequest_2_5:
        def __init__(self, max_dimension_m, max_speed_ms, weight_kg,
                     population_density, is_controlled_ground,
                     m1_strategic, m2_ground_impact, m3_emergency_response):
            self.max_dimension_m = max_dimension_m
            self.max_speed_ms = max_speed_ms
            self.weight_kg = weight_kg
            self.population_density = population_density
            self.is_controlled_ground = is_controlled_ground
            self.m1_strategic = m1_strategic
            self.m2_ground_impact = m2_ground_impact
            self.m3_emergency_response = m3_emergency_response
    
    calc = GRCCalculator()
    print("=" * 80)
    print("GRC CALCULATOR VALIDATION TESTS")
    print("=" * 80)
    
    # SORA 2.0 Tests
    print("\n" + "=" * 80)
    print("SORA 2.0 TESTS")
    print("=" * 80)
    
    try:
        print("\n[Test 1] 0.8m + VLOS_Controlled + No Mitigations")
        req = GRCRequest_2_0(
            max_dimension_m=0.8,
            operational_scenario="VLOS_Controlled",
            m1_strategic=MitigationLevel.NONE,
            m2_ground_impact=MitigationLevel.NONE,
            m3_emergency_response=MitigationLevel.NONE,
        )
        result = calc.calculate_grc_2_0(req)
        print(f"✅ iGRC={result.intrinsic_grc}, final={result.final_grc}, dim={result.dimension_category}")
        assert result.intrinsic_grc == 1 and result.final_grc == 1
    except Exception as e:
        print(f"❌ FAILED: {e}")
    
    try:
        print("\n[Test 2] 3.0m + BVLOS_Populated + M1 High")
        req = GRCRequest_2_0(
            max_dimension_m=3.0,
            operational_scenario="BVLOS_Populated",
            m1_strategic=MitigationLevel.HIGH,
            m2_ground_impact=MitigationLevel.NONE,
            m3_emergency_response=MitigationLevel.NONE,
        )
        result = calc.calculate_grc_2_0(req)
        print(f"✅ iGRC={result.intrinsic_grc}, final={result.final_grc} (floor={calc.MIN_GRC_FLOOR_2_0['3m']})")
        # iGRC=7, M1 High=-4, but floor at 2 for "3m"
        assert result.intrinsic_grc == 7 and result.final_grc >= 2
    except Exception as e:
        print(f"❌ FAILED: {e}")
    
    try:
        print("\n[Test 3] 8.0m + BVLOS_Populated → OUT OF SCOPE (iGRC=10)")
        req = GRCRequest_2_0(
            max_dimension_m=8.0,
            operational_scenario="BVLOS_Populated",
            m1_strategic=MitigationLevel.NONE,
            m2_ground_impact=MitigationLevel.NONE,
            m3_emergency_response=MitigationLevel.NONE,
        )
        result = calc.calculate_grc_2_0(req)
        print(f"❌ Should have raised OUT_OF_SCOPE error")
    except ValueError as e:
        if "OUT_OF_SCOPE" in str(e):
            print(f"✅ Correctly raised: {e}")
        else:
            print(f"❌ Wrong error: {e}")
    
    try:
        print("\n[Test 4] 8.0m + VLOS_Sparsely + M1 Medium + M2 High + M3 High")
        req = GRCRequest_2_0(
            max_dimension_m=8.0,
            operational_scenario="VLOS_Sparsely",
            m1_strategic=MitigationLevel.MEDIUM,
            m2_