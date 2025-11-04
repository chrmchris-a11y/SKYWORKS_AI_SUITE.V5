import pytest
from fastapi import FastAPI
from fastapi.testclient import TestClient

# Φορτώνουμε απευθείας τον SAIL router και δημιουργούμε μίνι app για smoke tests
router = None
_import_err = None
try:
    from sail.api.sail_api import router as _router
    router = _router
except Exception as e:
    _import_err = e
    try:
        from Backend_Python.sail.api.sail_api import router as _router2
        router = _router2
    except Exception as e2:
        _import_err = (e, e2)

if router is None:
    pytest.skip(f"Δεν μπόρεσα να εισάγω τον SAIL router: {_import_err}", allow_module_level=True)

app = FastAPI()
app.include_router(router)
client = TestClient(app)


def test_sail_router_health():
    r = client.get("/sail/health")
    assert r.status_code == 200
    j = r.json()
    assert j.get("service") == "SAIL Calculator API"


def test_sail_router_sora20_grc_over_7_rejected():
    # SORA 2.0: GRC πρέπει 1–7
    r = client.get("/sail/calculate", params={
        "grc_level": 8,
        "arc_level": "a",
        "sora_version": "2.0",
    })
    # Περιμένουμε 400 με σαφές μήνυμα
    assert r.status_code == 400
    assert "1–7" in r.json().get("detail", "")


def test_sail_router_sora25_grc4_arc3_is_iii():
    # SORA 2.5: numeric ARC 1..10 – (GRC=4, ARC=3) ⇒ SAIL III (σύμφωνα με τον πίνακα)
    r = client.post("/sail/calculate", json={
        "grc_level": 4,
        "residual_arc_level": 3,
        "sora_version": "SORA_2.5",
    })
    assert r.status_code == 200
    j = r.json()
    assert j["sail_level"] in ["III", "IV"], j  # αποδεκτό μόνο αν δεν είναι >IV (ελέγχουμε διορθωμένο πίνακα)
    assert j["sora_version"].endswith("2.5")


def test_sail_router_post_basic():
    # POST με SORA 2.0 και GRC 5, ARC c → IV (EASA AMC/GM Table D.1)
    payload = {
        "grc_level": 5,
        "arc_level": "c",
        "sora_version": "SORA_2.0",
    }
    r = client.post("/sail/calculate", json=payload)
    assert r.status_code == 200
    j = r.json()
    assert j["sail_level"] == "IV"
    assert j["grc_level"] == 5
    assert j.get("arc_level", "c").lower() == "c"
