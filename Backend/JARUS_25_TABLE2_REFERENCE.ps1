# OFFICIAL JARUS SORA 2.5 TABLE 2 REFERENCE CARD
# ================================================
# Use this as ground truth for all iGRC calculations

## POPULATION DENSITY CATEGORIES (Rows):
#
# Row 0: Controlled ground area (special management)
# Row 1: < 5 people/km² (Remote)
# Row 2: ≥5 and <50 (Lightly populated)
# Row 3: ≥50 and <500 (Sparsely populated)  
# Row 4: ≥500 and <5,000 (Suburban)
# Row 5: ≥5,000 and <50,000 (High density metro)
# Row 6: ≥50,000 (Assemblies of people)

## UA DIMENSION + SPEED CATEGORIES (Columns):
#
# Column 0: ≤1m AND ≤25 m/s (Small)
# Column 1: ≤3m AND ≤35 m/s (Medium)
# Column 2: ≤8m AND ≤75 m/s (Large)
# Column 3: ≤20m AND ≤120 m/s (Very Large)
# Column 4: >20m OR >120 m/s (Extra Large)

## JARUS 2.5 TABLE 2 MATRIX:
#
#             Col0  Col1  Col2  Col3  Col4
#           (1m/  (3m/  (8m/  (20m/ (40m+/
#            25)   35)   75)   120)  200+)
# Row 0 (Controlled)  1     1     2     3     3
# Row 1 (<5)          2     3     4     5     6
# Row 2 (<50)         3     4     5     6     7
# Row 3 (<500)        4     5     6     7     8
# Row 4 (<5k)         5     6     7     8     9
# Row 5 (<50k)        6     7     8     9    10
# Row 6 (≥50k)        7     8    GREY  GREY  GREY

## SPECIAL RULES:
#
# 1. ≤250g MTOM AND ≤25m/s → ALWAYS iGRC=1 (overrides table)
# 2. Grey cells (Row 6, Col 2-4) = Out of SORA 2.5 scope
# 3. BOTH dimension AND speed must satisfy column criteria

## EXAMPLES:
#
# - 0.2m, 20m/s, 50 ppl/km² → Small (0.2≤1 AND 20≤25), Row 2 → iGRC=3
# - 3.0m, 35m/s, 500 ppl/km² → Medium (3≤3 AND 35≤35), Row 4 → iGRC=6 ✅
# - 3.0m, 40m/s, 500 ppl/km² → Large (40>35, fail Med), Row 4 → iGRC=7
# - 8.0m, 75m/s, 2000 ppl/km² → Large (8≤8 AND 75≤75), Row 4 → iGRC=7 ✅
# - 20m, 120m/s, 5 ppl/km² → VeryLarge (20≤20 AND 120≤120), Row 1 → iGRC=5 ✅

## MITIGATION CREDITS (JARUS 2.5):
#
# M1(A) - Sheltering:
#   - Low: -1
#   - Medium: -2
#
# M1(B) - Operational restrictions:
#   - Medium: -1
#   - High: -2
#
# M1(C) - Ground observation:
#   - Low: -1
#
# M2 - Impact dynamics (parachute):
#   - Medium: -1
#   - High: -2
#
# VALIDATION RULES:
# - M1A Medium + M1B ANY = INVALID (Annex B)
# - Final GRC ≥1 (floor)
# - Final GRC ≤7 (SORA 2.5 scope limit)

Write-Host "JARUS SORA 2.5 TABLE 2 REFERENCE LOADED" -ForegroundColor Green
