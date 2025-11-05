"""SAIL calculator for SORA 2.0 and SORA 2.5 (spec-accurate, deterministic).

Sources of truth:
- SORA 2.0: EASA AMC/GM SORA 2.0 Annex D (Table D.1) — residual ARC a–d vs GRC 1–7.
  GRC > 7 ⇒ Category C (no SAIL).
  Reference: EASA Easy Access Rules for Unmanned Aircraft Systems
  https://www.easa.europa.eu/en/document-library/easy-access-rules/easy-access-rules-unmanned-aircraft-systems

- SORA 2.5: JARUS SORA 2.5 Annex D (Table 7) — numeric residual ARC 1..10 vs GRC 1..10.
  GRC 9–10 ⇒ SAIL VI for any residual ARC.
"""

from typing import Dict, Optional

from ..models.sail_models import (
    SAILLevel,
    ARCLevel,
    SORAVersion,
    SAILCalculationRequest,
    SAILCalculationResponse,
)


class SAILCalculator:
    """Unified SAIL calculator for SORA 2.0 (letter ARC) and SORA 2.5 (numeric residual ARC).

    Implements deterministic lookup from authoritative tables:
    - SORA 2.0: EASA AMC/GM Annex D Table D.1 (GRC 1..7 × ARC a–d → SAIL I–VI)
    - SORA 2.5: JARUS SORA 2.5 Annex D Table 7 (GRC 1..10 × residual ARC 1..10 → SAIL I–VI)

    No OSO counting here; that is owned by OSOMapper.
    """

    def __init__(self) -> None:
        """Initialize SAIL calculator with embedded authoritative tables."""
        # SORA 2.0 Table D.1 (EASA AMC/GM SORA 2.0 Annex D)
        # GRC 1..7 × ARC a–d → SAIL I–VI
        # Source: EASA Easy Access Rules for UAS, Annex D "SAIL determination"
        # https://www.easa.europa.eu/en/document-library/easy-access-rules/easy-access-rules-unmanned-aircraft-systems
        self.TABLE_20: Dict[int, Dict[str, SAILLevel]] = {
            1: {"a": SAILLevel.I, "b": SAILLevel.II, "c": SAILLevel.IV, "d": SAILLevel.VI},
            2: {"a": SAILLevel.I, "b": SAILLevel.II, "c": SAILLevel.IV, "d": SAILLevel.VI},
            3: {"a": SAILLevel.I, "b": SAILLevel.IV, "c": SAILLevel.IV, "d": SAILLevel.VI},
            4: {"a": SAILLevel.II, "b": SAILLevel.IV, "c": SAILLevel.IV, "d": SAILLevel.VI},
            5: {"a": SAILLevel.IV, "b": SAILLevel.IV, "c": SAILLevel.IV, "d": SAILLevel.VI},
            6: {"a": SAILLevel.IV, "b": SAILLevel.IV, "c": SAILLevel.V, "d": SAILLevel.VI},
            7: {"a": SAILLevel.VI, "b": SAILLevel.VI, "c": SAILLevel.VI, "d": SAILLevel.VI},
        }

        # SORA 2.5 Table 7 (JARUS SORA 2.5 Annex D)
        # GRC 1..10 × numeric residual ARC 1..10 → SAIL I–VI
        # Source: JARUS SORA v2.5 Main Body Annex D, Table 7 "SAIL determination"
        # Note: GRC 9–10 ⇒ SAIL VI for any residual ARC (1..10)
        self.TABLE_25: Dict[int, Dict[int, SAILLevel]] = {
            1: {1: SAILLevel.I, 2: SAILLevel.I, 3: SAILLevel.I, 4: SAILLevel.I, 5: SAILLevel.II, 6: SAILLevel.II, 7: SAILLevel.II, 8: SAILLevel.IV, 9: SAILLevel.IV, 10: SAILLevel.VI},
            2: {1: SAILLevel.I, 2: SAILLevel.I, 3: SAILLevel.I, 4: SAILLevel.II, 5: SAILLevel.II, 6: SAILLevel.II, 7: SAILLevel.IV, 8: SAILLevel.IV, 9: SAILLevel.IV, 10: SAILLevel.VI},
            3: {1: SAILLevel.I, 2: SAILLevel.I, 3: SAILLevel.II, 4: SAILLevel.II, 5: SAILLevel.II, 6: SAILLevel.IV, 7: SAILLevel.IV, 8: SAILLevel.IV, 9: SAILLevel.IV, 10: SAILLevel.VI},
            4: {1: SAILLevel.I, 2: SAILLevel.II, 3: SAILLevel.II, 4: SAILLevel.II, 5: SAILLevel.IV, 6: SAILLevel.IV, 7: SAILLevel.IV, 8: SAILLevel.IV, 9: SAILLevel.V, 10: SAILLevel.VI},
            5: {1: SAILLevel.II, 2: SAILLevel.II, 3: SAILLevel.II, 4: SAILLevel.IV, 5: SAILLevel.IV, 6: SAILLevel.IV, 7: SAILLevel.IV, 8: SAILLevel.V, 9: SAILLevel.V, 10: SAILLevel.VI},
            6: {1: SAILLevel.II, 2: SAILLevel.II, 3: SAILLevel.IV, 4: SAILLevel.IV, 5: SAILLevel.IV, 6: SAILLevel.IV, 7: SAILLevel.V, 8: SAILLevel.V, 9: SAILLevel.V, 10: SAILLevel.VI},
            7: {1: SAILLevel.II, 2: SAILLevel.IV, 3: SAILLevel.IV, 4: SAILLevel.IV, 5: SAILLevel.IV, 6: SAILLevel.V, 7: SAILLevel.V, 8: SAILLevel.V, 9: SAILLevel.V, 10: SAILLevel.VI},
            8: {1: SAILLevel.IV, 2: SAILLevel.IV, 3: SAILLevel.IV, 4: SAILLevel.IV, 5: SAILLevel.V, 6: SAILLevel.V, 7: SAILLevel.V, 8: SAILLevel.V, 9: SAILLevel.VI, 10: SAILLevel.VI},
            9: {1: SAILLevel.VI, 2: SAILLevel.VI, 3: SAILLevel.VI, 4: SAILLevel.VI, 5: SAILLevel.VI, 6: SAILLevel.VI, 7: SAILLevel.VI, 8: SAILLevel.VI, 9: SAILLevel.VI, 10: SAILLevel.VI},
            10: {1: SAILLevel.VI, 2: SAILLevel.VI, 3: SAILLevel.VI, 4: SAILLevel.VI, 5: SAILLevel.VI, 6: SAILLevel.VI, 7: SAILLevel.VI, 8: SAILLevel.VI, 9: SAILLevel.VI, 10: SAILLevel.VI},
        }

    def calculate_sail(self, request: SAILCalculationRequest) -> SAILCalculationResponse:
        """Calculate SAIL level from Final GRC and Residual ARC per SORA version.

        Args:
            request: SAILCalculationRequest with grc_level, arc_level (2.0) or
                     residual_arc_level (2.5), and sora_version.

        Returns:
            SAILCalculationResponse with sail_level (I–VI or None for Category C),
            echoed inputs, and reference string.

        Raises:
            ValueError: If required fields are missing or inputs are invalid.
        """
        grc = request.grc_level
        version = request.sora_version

        if version == SORAVersion.SORA_2_0:
            return self._calculate_sail_20(grc, request.arc_level)
        elif version == SORAVersion.SORA_2_5:
            return self._calculate_sail_25(grc, request.residual_arc_level)
        else:
            raise ValueError(f"Unsupported SORA version: {version}")

    def _calculate_sail_20(
        self, grc: int, arc_level: Optional[ARCLevel]
    ) -> SAILCalculationResponse:
        """SORA 2.0 SAIL determination (EASA AMC/GM Annex D Table D.1).

        Rules:
        - GRC 1..7: lookup from Table D.1 (letter ARC a–d)
        - GRC > 7: Category C (no SAIL)

        Args:
            grc: Ground Risk Class (int)
            arc_level: Letter ARC (a–d)

        Returns:
            SAILCalculationResponse with sail_level or None (Category C).

        Raises:
            ValueError: If arc_level is missing or GRC/ARC combination is invalid.
        """
        if arc_level is None:
            raise ValueError("arc_level (a–d) is required for SORA 2.0")

        # Category C: GRC > 7 ⇒ no SAIL
        if grc > 7:
            return SAILCalculationResponse(
                sail_level=None,
                sail=None,
                sora_version=SORAVersion.SORA_2_0,
                grc_level=grc,
                arc_level=arc_level,
                category="C",
                oso_count=None,
                reference="EASA AMC/GM SORA 2.0 Annex D – Category C for GRC>7",
            )

        # Validate GRC range for table lookup
        if grc < 1:
            raise ValueError(f"GRC must be >= 1 for SORA 2.0, got {grc}")

        # Normalize ARC to lowercase string
        arc_str = arc_level.value if hasattr(arc_level, "value") else str(arc_level)
        arc_str = arc_str.lower()

        # Lookup SAIL from Table D.1
        if grc not in self.TABLE_20:
            raise ValueError(f"No SAIL mapping for GRC {grc} in SORA 2.0 table (valid: 1..7)")
        if arc_str not in self.TABLE_20[grc]:
            raise ValueError(f"Invalid ARC '{arc_str}' for SORA 2.0 (valid: a, b, c, d)")

        sail_level = self.TABLE_20[grc][arc_str]

        return SAILCalculationResponse(
            sail_level=sail_level,
            sail=sail_level,
            sora_version=SORAVersion.SORA_2_0,
            grc_level=grc,
            arc_level=arc_level,
            category=None,
            oso_count=None,  # OSO counting is owned by OSOMapper
            reference="EASA AMC/GM SORA 2.0 Annex D (Table D.1)",
        )

    def _calculate_sail_25(
        self, grc: int, residual_arc_level: Optional[int]
    ) -> SAILCalculationResponse:
        """SORA 2.5 SAIL determination (JARUS SORA 2.5 Annex D Table 7).

        Rules:
        - GRC 1..8: lookup from Table 7 (numeric residual ARC 1..10)
        - GRC 9–10: SAIL VI for any residual ARC

        Args:
            grc: Ground Risk Class (1..10)
            residual_arc_level: Numeric residual ARC (1..10)

        Returns:
            SAILCalculationResponse with sail_level (I–VI).

        Raises:
            ValueError: If residual_arc_level is missing or out of range.
        """
        if residual_arc_level is None:
            raise ValueError("residual_arc_level (1..10) is required for SORA 2.5")

        # Validate residual ARC range
        if not (1 <= residual_arc_level <= 10):
            raise ValueError(
                f"residual_arc_level must be 1..10 for SORA 2.5, got {residual_arc_level}"
            )

        # Validate GRC range
        if not (1 <= grc <= 10):
            raise ValueError(f"GRC must be 1..10 for SORA 2.5, got {grc}")

        # GRC 9–10 ⇒ SAIL VI for any residual ARC
        if grc >= 9:
            sail_level = SAILLevel.VI
        else:
            # Lookup from Table 7
            if grc not in self.TABLE_25:
                raise ValueError(f"No SAIL mapping for GRC {grc} in SORA 2.5 table")
            if residual_arc_level not in self.TABLE_25[grc]:
                raise ValueError(
                    f"Invalid residual_arc_level {residual_arc_level} for GRC {grc}"
                )
            sail_level = self.TABLE_25[grc][residual_arc_level]

        return SAILCalculationResponse(
            sail_level=sail_level,
            sail=sail_level,
            sora_version=SORAVersion.SORA_2_5,
            grc_level=grc,
            arc_level=None,
            residual_arc_level=residual_arc_level,
            category=None,
            oso_count=None,  # OSO counting is owned by OSOMapper
            reference="JARUS SORA 2.5 Annex D (Table 7)",
        )
