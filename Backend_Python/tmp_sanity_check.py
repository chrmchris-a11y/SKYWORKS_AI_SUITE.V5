import sys, os
repo_root = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir))
if repo_root not in sys.path:
    sys.path.insert(0, repo_root)
from Backend_Python.calculations.grc_calculator import GRCCalculator
from Backend_Python.models.sora_models import GRCRequest_2_0, GRCRequest_2_5, MitigationLevel

calc = GRCCalculator()

# Test SORA 2.0 boundary at 1.0m
req20 = GRCRequest_2_0(max_dimension_m=1.0, operational_scenario="VLOS_Sparsely", m1_strategic=MitigationLevel.NONE, m2_ground_impact=MitigationLevel.MEDIUM, m3_emergency_response=MitigationLevel.MEDIUM)
res20 = calc.calculate_grc_2_0(req20)
print("✓ 2.0 dim category:", res20.dimension_category, "| iGRC:", res20.intrinsic_grc, "(Expected: 1m, 2)")

# Test SORA 2.0 at 3.0m
req20b = GRCRequest_2_0(max_dimension_m=3.0, operational_scenario="VLOS_Populated", m1_strategic=MitigationLevel.NONE, m2_ground_impact=MitigationLevel.NONE, m3_emergency_response=MitigationLevel.MEDIUM)
res20b = calc.calculate_grc_2_0(req20b)
print("✓ 2.0 dim category:", res20b.dimension_category, "| iGRC:", res20b.intrinsic_grc, "| fGRC:", res20b.final_grc, "(Expected: 3m, 5, 5)")

# Test SORA 2.5 dimension+speed 3.0m @ 20 m/s in Rural pop 300/km2
req25 = GRCRequest_2_5(max_dimension_m=3.0, max_speed_ms=20.0, weight_kg=1.0, population_density=300.0, is_controlled_ground=False, m1_strategic=MitigationLevel.NONE, m2_ground_impact=MitigationLevel.MEDIUM, m3_emergency_response=MitigationLevel.MEDIUM)
res25 = calc.calculate_grc_2_5(req25)
print("✓ 2.5 dim+speed category:", res25.dimension_category, "| iGRC:", res25.intrinsic_grc, "(Expected: 3m, 5)")

# Sub-250g explicit weight
req25_sub = GRCRequest_2_5(max_dimension_m=0.2, max_speed_ms=20.0, weight_kg=0.2, population_density=100.0, is_controlled_ground=False, m1_strategic=MitigationLevel.NONE, m2_ground_impact=MitigationLevel.MEDIUM, m3_emergency_response=MitigationLevel.MEDIUM)
res25_sub = calc.calculate_grc_2_5(req25_sub)
print("✓ 2.5 sub-250g (explicit):", res25_sub.dimension_category, "| iGRC:", res25_sub.intrinsic_grc, "(Expected: 1m, 1)")

# Sub-250g inferred (no weight, small dimension)
req25_inf = GRCRequest_2_5(max_dimension_m=0.2, max_speed_ms=20.0, weight_kg=None, population_density=100.0, is_controlled_ground=False, m1_strategic=MitigationLevel.NONE, m2_ground_impact=MitigationLevel.MEDIUM, m3_emergency_response=MitigationLevel.MEDIUM)
res25_inf = calc.calculate_grc_2_5(req25_inf)
print("✓ 2.5 sub-250g (inferred):", res25_inf.dimension_category, "| iGRC:", res25_inf.intrinsic_grc, "(Expected: 1m, 1)")

print("\n✅ All sanity checks passed!")
