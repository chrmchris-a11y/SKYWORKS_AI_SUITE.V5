"""Full-stack tests for SAIL endpoints and calculators (items 1–5).

Covers:
- main.py /api/v1/calculate/sail (2.0 letters, 2.5 numeric, Category C)
- sail/api/sail_api.py POST/GET variants
- Cross-checks with safe, authoritative cells common across implementations
"""

from fastapi.testclient import TestClient

# Import the FastAPI app (includes /api/v1/* and mounts /sail router)
from main import app  # type: ignore


client = TestClient(app)


class TestMainEndpoint:
    def test_v20_letter_arc_returns_expected_sail(self):
        """SORA 2.0: final_grc=5, residual_arc='ARC-c' → SAIL IV (EASA Table D.1)."""
        resp = client.post(
            "/api/v1/calculate/sail",
            json={"sora_version": "2.0", "final_grc": 5, "residual_arc": "ARC-c"},
        )
        assert resp.status_code == 200
        data = resp.json()
        assert data.get("sail_level") == "IV"
        assert data.get("sail") == "IV"
        assert data.get("final_grc") == 5
        assert data.get("residual_arc") == "ARC-c"

    def test_v20_category_c_when_grc_gt_7(self):
        """SORA 2.0: GRC > 7 ⇒ Category C (no SAIL)."""
        resp = client.post(
            "/api/v1/calculate/sail",
            json={"sora_version": "2.0", "final_grc": 8, "residual_arc": "b"},
        )
        assert resp.status_code == 200
        data = resp.json()
        assert data.get("category") == "C"
        assert data.get("sail_level") is None
        assert data.get("sail") is None

    def test_v25_numeric_arc_returns_expected_sail(self):
        """SORA 2.5: final_grc=9, residual_arc_level=5 → SAIL VI (authoritative high rows)."""
        resp = client.post(
            "/api/v1/calculate/sail",
            json={"sora_version": "2.5", "final_grc": 9, "residual_arc_level": 5},
        )
        assert resp.status_code == 200
        data = resp.json()
        assert data.get("sail_level") == "VI"
        assert data.get("sail") == "VI"
        assert data.get("final_grc") == 9
        assert data.get("residual_arc") == "ARC-5"

    def test_v25_missing_numeric_arc_returns_400(self):
        resp = client.post(
            "/api/v1/calculate/sail",
            json={"sora_version": "2.5", "final_grc": 4},
        )
        assert resp.status_code == 400


class TestRouterEndpoints:
    def test_router_post_sora20(self):
        """/sail/calculate (POST) with SORA 2.0 returns expected SAIL."""
        body = {
            "grc_level": 5,
            "arc_level": "c",
            "sora_version": "SORA_2.0",
        }
        resp = client.post("/sail/calculate", json=body)
        assert resp.status_code == 200
        data = resp.json()
        assert data.get("sail_level") == "IV"
        assert data.get("arc_level") == "c"
        assert data.get("grc_level") == 5

    def test_router_post_sora25(self):
        """/sail/calculate (POST) with SORA 2.5 returns expected SAIL for GRC=8 (VI in table)."""
        body = {
            "grc_level": 8,
            "residual_arc_level": 1,
            "sora_version": "SORA_2.5",
        }
        resp = client.post("/sail/calculate", json=body)
        assert resp.status_code == 200
        data = resp.json()
        assert data.get("sail_level") == "VI"
        assert data.get("residual_arc_level") == 1

    def test_router_get_sora20(self):
        """/sail/calculate (GET) supported for 2.0 only."""
        resp = client.get("/sail/calculate", params={"grc_level": 5, "arc_level": "c", "sora_version": "2.0"})
        assert resp.status_code == 200
        data = resp.json()
        assert data.get("sail_level") == "IV"

    def test_router_get_sora25_returns_400(self):
        resp = client.get("/sail/calculate", params={"grc_level": 5, "arc_level": "c", "sora_version": "2.5"})
        assert resp.status_code == 400
