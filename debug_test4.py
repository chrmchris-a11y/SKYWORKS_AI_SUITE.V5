import sys
sys.path.insert(0, 'Backend_Python')
from main import JARUSSORATable2

# Test the problematic case
mtom_kg = 0.8
pop_density = 8000

mtom_cat = JARUSSORATable2.get_mtom_category(mtom_kg)
pop_cat = JARUSSORATable2.get_population_category(pop_density)
result = JARUSSORATable2.calculate_initial_grc(mtom_kg, pop_density)

print(f"MTOM: {mtom_kg}kg → Category {mtom_cat}")
print(f"Population: {pop_density} ppl/km² → Category {pop_cat}")
print(f"Matrix lookup: IGRC_MATRIX[{mtom_cat}][{pop_cat}] = {result['initial_grc']}")
print(f"\nJARUS Table 2 Row 1 (0.25-1kg): {JARUSSORATable2.IGRC_MATRIX[1]}")
print(f"According to JARUS: 0.8kg drone in 8000 ppl/km² area = iGRC {result['initial_grc']}")

# Check what iGRC=5 would require
print("\n\nLooking for iGRC=5 in matrix:")
for mtom_idx, row in enumerate(JARUSSORATable2.IGRC_MATRIX):
    for pop_idx, value in enumerate(row):
        if value == 5:
            print(f"  Found at MTOM_Cat={mtom_idx}, Pop_Cat={pop_idx}: {value}")
