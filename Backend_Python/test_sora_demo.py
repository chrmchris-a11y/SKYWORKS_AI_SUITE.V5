"""
Demo script to show EXPECTED vs ACTUAL results for SORA 2.0 and SORA 2.5
"""

from grc.calculators.grc_calculator import GRCCalculator20, GRCCalculator25
from grc.models.grc_models import GRCInputs20, GRCInputs25

print("\n" + "="*70)
print(" SORA 2.0 TEST: VLOS over Sparsely Populated, Small UAS")
print("="*70)

calc20 = GRCCalculator20()
inputs20 = GRCInputs20(
    version="SORA_2.0",
    scenario="VLOS_Sparsely",
    dimension_m=0.6,
    population_density_p_km2=50,
    m1_strategic="Medium",
    m2_impact="Low",
    m3_erp="None",
    containment_quality="Good"
)

result20 = calc20.calculate(inputs20)

print(f"\nINPUTS:")
print(f"  Scenario: {inputs20.scenario}")
print(f"  Dimension: {inputs20.dimension_m} m")
print(f"  M1: {inputs20.m1_strategic}, M2: {inputs20.m2_impact}, M3: {inputs20.m3_erp}")
print(f"  Containment: {inputs20.containment_quality}")

print(f"\nCALCULATION STEPS:")
for i, trace in enumerate(result20.calculation_trace, 1):
    print(f"  {i}. {trace.step}: {trace.result}")
    if trace.delta:
        print(f"     Delta: {trace.delta}")

print(f"\n┌─────────────────────────────────────┐")
print(f"│  EXPECTED vs ACTUAL (SORA 2.0)      │")
print(f"├─────────────────────────────────────┤")
print(f"│  Initial GRC (iGRC):                │")
print(f"│    EXPECTED:  2                     │")
print(f"│    ACTUAL:    {result20.initial_grc:<2}  {'✓ MATCH' if result20.initial_grc == 2 else '✗ MISMATCH'}              │")
print(f"│                                     │")
print(f"│  Final GRC:                         │")
print(f"│    EXPECTED:  1                     │")
print(f"│    ACTUAL:    {result20.residual_grc:<2}  {'✓ MATCH' if result20.residual_grc == 1 else '✗ MISMATCH'}              │")
print(f"└─────────────────────────────────────┘")

status20 = "PASSED ✓" if result20.initial_grc == 2 and result20.residual_grc == 1 else "FAILED ✗"
print(f"\nTEST STATUS: {status20}")

print("\n" + "="*70)
print(" SORA 2.5 TEST: Quantitative Model, Medium Risk")
print("="*70)

calc25 = GRCCalculator25()
inputs25 = GRCInputs25(
    version="SORA_2.5",
    mtom_kg=2.0,
    characteristic_dimension_m=2.0,
    max_speed_mps=25.0,
    population_density_p_km2=300,
    environment_type="Urban",
    containment_quality="Adequate",
    m1a_sheltering="Low",
    m1b_operational="None",
    m1c_ground_observation="None",
    m2_impact="Medium"
)

result25 = calc25.calculate(inputs25)

print(f"\nINPUTS:")
print(f"  MTOM: {inputs25.mtom_kg} kg")
print(f"  Dimension: {inputs25.characteristic_dimension_m} m")
print(f"  Speed: {inputs25.max_speed_mps} m/s")
print(f"  Population: {inputs25.population_density_p_km2} people/km²")
print(f"  M1A: {inputs25.m1a_sheltering}, M2: {inputs25.m2_impact}")
print(f"  Containment: {inputs25.containment_quality}")

print(f"\nCALCULATION STEPS:")
for i, trace in enumerate(result25.calculation_trace[:5], 1):  # First 5 steps
    print(f"  {i}. {trace.step}: {trace.result}")
    if trace.notes:
        print(f"     Notes: {trace.notes}")

print(f"\n┌─────────────────────────────────────┐")
print(f"│  EXPECTED vs ACTUAL (SORA 2.5)      │")
print(f"├─────────────────────────────────────┤")
print(f"│  Initial GRC (iGRC):                │")
print(f"│    EXPECTED:  3                     │")
print(f"│    ACTUAL:    {result25.initial_grc:<2}  {'✓ MATCH' if result25.initial_grc == 3 else '✗ MISMATCH'}              │")
print(f"│                                     │")
print(f"│  Final GRC:                         │")
print(f"│    EXPECTED:  1                     │")
print(f"│    ACTUAL:    {result25.residual_grc:<2}  {'✓ MATCH' if result25.residual_grc == 1 else '✗ MISMATCH'}              │")
print(f"└─────────────────────────────────────┘")

status25 = "PASSED ✓" if result25.initial_grc == 3 and result25.residual_grc == 1 else "FAILED ✗"
print(f"\nTEST STATUS: {status25}")

print("\n" + "="*70)
print(f" OVERALL: Both tests {status20.split()[0]} and {status25.split()[0]}")
print("="*70 + "\n")
