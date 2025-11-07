from fastapi.testclient import TestClient
from main import app

client = TestClient(app)

cases = [
    {"sora_version": "2.5", "final_grc": 6, "residual_arc_level": 4},
    {"sora_version": "2.5", "final_grc": 10, "residual_arc_level": 10},
    {"sora_version": "2.0", "final_grc": 8, "residual_arc": "ARC-a"},
]

for i, payload in enumerate(cases, 1):
    r = client.post("/api/v1/calculate/sail", json=payload)
    print(f"Case {i} status=", r.status_code)
    try:
        print(r.json())
    except Exception as e:
        print("Non-JSON response:", e)
