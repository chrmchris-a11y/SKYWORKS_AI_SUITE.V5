# Backend_Python testing notes

This test suite validates SORA calculations (GRC/ARC/SAIL) against EASA/JARUS tables and decision trees.

Out-of-scope handling semantics used by tests:
- SORA 2.0: If intrinsic GRC (iGRC) is beyond the valid table range (> 7), calculators return a numeric out-of-scope marker (final_grc >= 8). Tests accept this as valid and do not force a mapping back into [1..7].
- SORA 2.5: Grey cells in the iGRC matrix (e.g., assemblies of people with larger UA categories) are treated as out-of-scope. Calculators raise ValueError for these inputs. Tests assert the exception for such cases.

Rationale: This mirrors the official tables, where grey cells represent “not applicable / out-of-scope” combinations rather than a computable risk level.

Running tests locally:
- Disable third-party pytest plugin autoloading to avoid environment issues.
  PowerShell example:
  - Set the variable for the current process and run pytest from the Backend_Python directory.

Notes:
- The tests include a conftest.py that adds the project root to sys.path, ensuring imports like `from calculations.grc_calculator import GRCCalculator` work reliably.