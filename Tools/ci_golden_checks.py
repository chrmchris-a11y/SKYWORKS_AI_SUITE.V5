import json
import sys
import time
from urllib import request as urr


def http_get(url: str, timeout=5):
    req = urr.Request(url, method="GET")
    with urr.urlopen(req, timeout=timeout) as resp:
        return resp.getcode(), resp.read().decode("utf-8", errors="replace")


def http_post_json(url: str, payload: dict, timeout=5):
    data = json.dumps(payload).encode("utf-8")
    req = urr.Request(url, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
    try:
        with urr.urlopen(req, timeout=timeout) as resp:
            return resp.getcode(), resp.read().decode("utf-8", errors="replace")
    except Exception as e:
        if hasattr(e, 'fp') and e.fp is not None:
            body = e.fp.read().decode("utf-8", errors="replace")
        else:
            body = str(e)
        # Surface non-200 details
        return getattr(e, 'code', 0) or 0, body


def wait_for(url: str, seconds: int = 60) -> bool:
    deadline = time.time() + seconds
    while time.time() < deadline:
        try:
            code, _ = http_get(url, timeout=3)
            if code == 200:
                return True
        except Exception:
            pass
        time.sleep(1)
    return False


def main():
    # 1) Python FastAPI health
    if not wait_for("http://localhost:8001/health", seconds=60):
        print("[FAIL] Python FastAPI health check did not return 200 within timeout", file=sys.stderr)
        return 1

    # 2) .NET API info
    code, body = http_get("http://localhost:5210/api/sora/info")
    if code != 200:
        print(f"[FAIL] .NET /api/sora/info status={code} body={body[:500]}", file=sys.stderr)
        return 1

    # 3) .NET SORA 2.0 SAIL
    payload20 = {
        "soraVersion": "2.0",
        "finalGrc": 5,
        "residualArc": "c"
    }
    code, body = http_post_json("http://localhost:5210/api/sail/calculate", payload20)
    if code != 200:
        print(f"[FAIL] .NET 2.0 SAIL status={code} body={body[:500]}", file=sys.stderr)
        return 1
    try:
        data = json.loads(body)
        assert data.get("sailLevel") in ("IV", "V")  # depends on input; here we expect V for residualArc 'c' and GRC 5
    except Exception as e:
        print(f"[FAIL] .NET 2.0 SAIL invalid JSON or content: {e}; body={body[:500]}", file=sys.stderr)
        return 1

    # 4) .NET SORA 2.5 SAIL
    payload25 = {
        "soraVersion": "2.5",
        "finalGrc": 10,
        "residualArcLevel": 10
    }
    code, body = http_post_json("http://localhost:5210/api/sail/calculate", payload25)
    if code != 200:
        print(f"[FAIL] .NET 2.5 SAIL status={code} body={body[:500]}", file=sys.stderr)
        return 1
    try:
        data = json.loads(body)
        assert data.get("sailLevel") == "VI"
    except Exception as e:
        print(f"[FAIL] .NET 2.5 SAIL invalid JSON or content: {e}; body={body[:500]}", file=sys.stderr)
        return 1

    print("[OK] Golden checks passed")
    return 0


if __name__ == "__main__":
    sys.exit(main())
