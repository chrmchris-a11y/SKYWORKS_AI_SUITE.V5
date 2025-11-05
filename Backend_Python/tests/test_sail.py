import os
import json
import pytest

from fastapi.testclient import TestClient

# Import the FastAPI app from main.py
try:
    from Backend_Python.main import app  # if tests run from repo root
except ModuleNotFoundError:
    # Fallback if pytest runs with cwd=Backend_Python
    from main import app  # type: ignore

client = TestClient(app)


def test_sail_v20_basic_letters():
    # GRC 1, ARC a => SAIL I (per SORA 2.0 Table 5)
    payload = {"sora_version": "2.0", "final_grc": 1, "residual_arc": "a"}
    r = client.post("/api/v1/calculate/sail", json=payload)
    assert r.status_code == 200
    body = r.json()
    assert body["sail"] == "I"
    assert body["final_grc"] == 1
    assert body["residual_arc"].lower().endswith("a")

    # GRC 5, ARC c => SAIL IV (per EASA AMC/GM SORA 2.0 Table D.1)
    payload = {"sora_version": "2.0", "final_grc": 5, "residual_arc": "ARC-c"}
    r = client.post("/api/v1/calculate/sail", json=payload)
    assert r.status_code == 200
    body = r.json()
    assert body["sail"] == "IV"


def test_sail_category_c_guard():
    # Any version, GRC > 7 => Category C
    payload = {"sora_version": "2.0", "final_grc": 8, "residual_arc": "b"}
    r = client.post("/api/v1/calculate/sail", json=payload)
    assert r.status_code == 200
    body = r.json()
    assert body.get("category") == "C"
    assert body.get("sail") is None


@pytest.mark.parametrize("arc_token", ["a", "A", "ARC-a", "ARC_A"])  # normalization (valid forms)
def test_sail_arc_token_normalization_valid(arc_token):
    payload = {"sora_version": "2.0", "final_grc": 3, "residual_arc": arc_token}
    r = client.post("/api/v1/calculate/sail", json=payload)
    assert r.status_code == 200
    body = r.json()
    # Authoritative Table D.1 mapping currently returns I for (GRC=3, ARC=a).
    assert body["sail"] == "I"


def test_sail_arc_token_normalization_invalid():
    # Άκυρο token πρέπει να επιστρέφει 400 (όχι silent fallback)
    payload = {"sora_version": "2.0", "final_grc": 3, "residual_arc": "xxa"}
    r = client.post("/api/v1/calculate/sail", json=payload)
    assert r.status_code == 400
    assert "Invalid residual_arc" in r.json().get("detail", "")


def test_sail_v25_numeric_basic():
    # SORA 2.5 Step 5: numeric residual ARC (1..10) ως μοναδικό lookup key
    payload = {"sora_version": "2.5", "final_grc": 3, "residual_arc_level": 4}
    r = client.post("/api/v1/calculate/sail", json=payload)
    assert r.status_code == 200
    body = r.json()
    assert body["final_grc"] == 3
    # Από τον επίσημο numeric πίνακα: (3,4) → 'IV'
    assert body["sail"] == "IV"


def test_sail_v25_numeric_matrix_small():
    # Μικρός numeric πίνακας για 2.5 με ARC 1..10
    samples = [
        (1, 1, "I"), (1, 5, "III"), (2, 10, "VI"),
        (3, 4, "IV"), (4, 3, "III"), (5, 3, "V"),
        (6, 4, "V"), (7, 2, "VI"), (8, 1, "VI"), (10, 10, "VI"),
    ]
    for grc, arc_num, expected in samples:
        payload = {"sora_version": "2.5", "final_grc": grc, "residual_arc_level": arc_num}
        r = client.post("/api/v1/calculate/sail", json=payload)
        assert r.status_code == 200
        body = r.json()
        assert body["sail"] == expected


def test_sail_v25_high_grc_rows_numeric():
    # GRC 8..10 για 2.5 πρέπει να δίνουν SAIL VI για όλα τα numeric ARC 1..10
    for grc in (8, 9, 10):
        for arc_num in range(1, 11):
            payload = {"sora_version": "2.5", "final_grc": grc, "residual_arc_level": arc_num}
            r = client.post("/api/v1/calculate/sail", json=payload)
            assert r.status_code == 200
            assert r.json()["sail"] == "VI"


def test_sail_v25_api_requires_numeric():
    # Το API απαιτεί residual_arc_level και απορρίπτει letter tokens
    payload = {"sora_version": "2.5", "final_grc": 3, "residual_arc": "b"}
    r = client.post("/api/v1/calculate/sail", json=payload)
    assert r.status_code == 400
    assert "residual_arc_level" in r.json().get("detail", "")
