"""
Ensure test session can import local packages (calculations, models) by adding
the project root (Backend_Python) to sys.path.
"""
import os
import sys

# tests/ -> project root
PROJECT_ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir))
if PROJECT_ROOT not in sys.path:
    sys.path.insert(0, PROJECT_ROOT)
