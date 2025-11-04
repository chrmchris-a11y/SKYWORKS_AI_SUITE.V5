#!/usr/bin/env python3
"""
Developer quick probe script for manual testing.

Important: Wrapped under __main__ so pytest import/collection won't execute it.
"""
import subprocess
import time
import requests
import sys


def main():
    # Start service
    proc = subprocess.Popen(
        ['venv/Scripts/uvicorn.exe', 'main:app', '--host', '127.0.0.1', '--port', '8001'],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE
    )

    print("Starting service...")
    time.sleep(3)

    try:
        # Note: The current /api/v1/calculate/grc/2.0 expects mtom_kg and population_density.
        # This probe still uses older payload; update if you want a live check.
        r = requests.post(
            'http://localhost:8001/api/v1/calculate/grc/2.0',
            json={'max_dimension_m': 0.8, 'operational_scenario': 'VLOS_Controlled'},
            timeout=5
        )
        print(f'GRC 2.0 Test: {r.status_code}')
        if r.status_code == 200:
            result = r.json()
            print(f'Final GRC: {result.get("final_grc")}')
            print(f'Intrinsic GRC: {result.get("intrinsic_grc")}')
            print('✓ Service working!')
        else:
            print('✗ Service error:', r.json())
            sys.exit(1)
    except Exception as e:
        print(f'✗ Error: {e}')
        sys.exit(1)
    finally:
        proc.terminate()


if __name__ == "__main__":
    main()
