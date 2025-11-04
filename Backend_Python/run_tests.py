#!/usr/bin/env python3
"""
Run comprehensive tests with service in subprocess
"""
import subprocess
import time
import sys
import os

# Start service in background
print("Starting FastAPI service...")
proc = subprocess.Popen(
    [os.path.join('venv', 'Scripts', 'python.exe'), '-m', 'uvicorn', 'main:app', '--host', '127.0.0.1', '--port', '8001'],
    stdout=subprocess.DEVNULL,
    stderr=subprocess.DEVNULL
)

# Wait for startup
time.sleep(5)

# Run comprehensive tests
try:
    print("\nRunning comprehensive tests...\n")
    result = subprocess.run(
        [os.path.join('venv', 'Scripts', 'python.exe'), 'test_comprehensive_report.py'],
        cwd=os.getcwd()
    )
    sys.exit(result.returncode)
finally:
    print("\n\nStopping service...")
    proc.terminate()
    proc.wait()
