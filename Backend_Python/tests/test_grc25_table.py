import pytest
from calculations.grc_calculator import GRCCalculator
from models.sora_models import GRCRequest_2_5

# Rows: population bands (0..6), Cols: dim/speed categories (0..4)
EXPECTED = [
    [1, 1, 2, 3, 3],
    [2, 3, 4, 5, 6],
    [3, 4, 5, 6, 7],
    [4, 5, 6, 7, 8],
    [5, 6, 7, 8, 9],
    [6, 7, 8, 9, 10],
    [7, 8, -1, -1, -1],
]

# Representative inputs that map to each column category
# 0: <1m & <25m/s, 1: <3m & <35m/s, 2: <8m & <75m/s, 3: <20m & <120m/s, 4: >20m or >120m/s
DIM_SPEED = [
    (0.9, 20.0),
    (2.5, 30.0),
    (5.0, 60.0),
    (10.0, 100.0),
    (25.0, 150.0),
]

# Population bands densities and control flag
POP = [
    (0.0, True),     # controlled
    (1.0, False),    # <5
    (10.0, False),   # <50
    (200.0, False),  # <500
    (2000.0, False), # <5000
    (20000.0, False),# <50000
    (80000.0, False) # >50000
]

@pytest.mark.parametrize("pop_idx,params", list(enumerate(POP)))
@pytest.mark.parametrize("col,ds", list(enumerate(DIM_SPEED)))
def test_sora25_igrc_table(pop_idx, params, col, ds):
    density, is_controlled = params
    dim, speed = ds
    calc = GRCCalculator()
    req = GRCRequest_2_5(
        max_dimension_m=dim,
        max_speed_ms=speed,
        population_density=density,
        is_controlled_ground=is_controlled,
    )
    expected = EXPECTED[pop_idx][col]
    if expected < 0:
        with pytest.raises(ValueError):
            calc.calculate_grc_2_5(req)
    else:
        res = calc.calculate_grc_2_5(req)
        assert res.intrinsic_grc == expected
