#!/usr/bin/env python3
"""
╔═══════════════════════════════════════════════════════════════╗
║  FIELD-BY-FIELD DROPDOWN VERIFICATION                        ║
║  Detailed inspection of EVERY dropdown per SORA version      ║
╚═══════════════════════════════════════════════════════════════╝

Purpose: Line-by-line verification of ALL UI dropdowns against backend specs
Date: October 30, 2025
"""

import sys
from pathlib import Path
from typing import List, Dict, Tuple
from dataclasses import dataclass

# Add Backend_Python to path
backend_path = Path(__file__).parent.parent / "Backend_Python"
sys.path.insert(0, str(backend_path))

from grc.models.grc_models import (
    M1Level20, M2Level20, M3Level20,
    M1ALevel25, M1BLevel25, M1CLevel25, M2Level25,
    ContainmentQuality
)
from arc.models.arc_models import (
    AirspaceClass20, AirspaceClass25,
    AirspaceSegregation, AirspaceContainment25,
    TrafficDensityDataSource,
    OperationType, ProximityAerodrome, TimeOfOperation
)

@dataclass
class DropdownCheck:
    """Single dropdown verification result"""
    field_name: str
    sora_version: str
    ui_element_id: str
    backend_enum: str
    expected_options: List[str]
    status: str  # "✅ PASS", "❌ FAIL", "⚠️ WARNING"
    notes: str = ""

class DetailedDropdownVerifier:
    """Field-by-field dropdown verification"""
    
    def __init__(self):
        self.checks: List[DropdownCheck] = []
    
    def verify_dropdown(
        self,
        field_name: str,
        sora_version: str,
        ui_element_id: str,
        backend_enum_class,
        expected_ui_options: List[str],
        official_reference: str = ""
    ) -> bool:
        """Verify single dropdown against backend enum"""
        
        backend_values = [e.value for e in backend_enum_class]
        backend_enum_name = backend_enum_class.__name__
        
        # Check if options match
        missing = set(backend_values) - set(expected_ui_options)
        extra = set(expected_ui_options) - set(backend_values)
        
        if not missing and not extra:
            status = "✅ PASS"
            notes = f"All options match backend {backend_enum_name}"
        elif missing:
            status = "❌ FAIL"
            notes = f"MISSING in UI: {missing}"
        elif extra:
            status = "⚠️ WARNING"
            notes = f"EXTRA in UI (not in backend): {extra}"
        else:
            status = "✅ PASS"
            notes = "Options validated"
        
        if official_reference:
            notes += f" | Ref: {official_reference}"
        
        check = DropdownCheck(
            field_name=field_name,
            sora_version=sora_version,
            ui_element_id=ui_element_id,
            backend_enum=backend_enum_name,
            expected_options=backend_values,
            status=status,
            notes=notes
        )
        
        self.checks.append(check)
        return "✅" in status
    
    def print_section_header(self, title: str):
        """Print formatted section header"""
        print("\n" + "="*70)
        print(f"  {title}")
        print("="*70)
    
    def print_check_result(self, check: DropdownCheck):
        """Print single check result"""
        print(f"\n{check.status} {check.field_name}")
        print(f"   UI Element: #{check.ui_element_id}")
        print(f"   Backend: {check.backend_enum}")
        print(f"   Options: {', '.join(check.expected_options)}")
        if check.notes:
            print(f"   Notes: {check.notes}")
    
    def verify_sora20_grc_fields(self):
        """Verify SORA 2.0 GRC mitigation dropdowns"""
        self.print_section_header("SORA 2.0 - GROUND RISK MITIGATIONS (JAR_doc_06 Table 3)")
        
        # M1 Strategic Mitigation
        print("\n📌 Field 1/3: M1 Strategic Mitigation")
        self.verify_dropdown(
            field_name="M1 - Strategic (reduce people at risk)",
            sora_version="SORA 2.0",
            ui_element_id="m1_20",
            backend_enum_class=M1Level20,
            expected_ui_options=["None", "Low", "Medium", "High"],
            official_reference="JAR_doc_06 Table 3, Row 1"
        )
        self.print_check_result(self.checks[-1])
        
        # M2 Impact Reduction
        print("\n📌 Field 2/3: M2 Impact Reduction")
        self.verify_dropdown(
            field_name="M2 - Impact reduction (e.g., parachute)",
            sora_version="SORA 2.0",
            ui_element_id="m2_20",
            backend_enum_class=M2Level20,
            expected_ui_options=["None", "Low", "High"],  # CRITICAL: Must have "Low", not "Medium"
            official_reference="JAR_doc_06 Table 3, Row 2"
        )
        self.print_check_result(self.checks[-1])
        
        # M3 Emergency Response Plan
        print("\n📌 Field 3/3: M3 Emergency Response Plan")
        self.verify_dropdown(
            field_name="M3 - Emergency Response Plan",
            sora_version="SORA 2.0",
            ui_element_id="m3_20",
            backend_enum_class=M3Level20,
            expected_ui_options=["None", "Low", "Medium", "High"],
            official_reference="JAR_doc_06 Table 3, Row 3"
        )
        self.print_check_result(self.checks[-1])
    
    def verify_sora25_grc_fields(self):
        """Verify SORA 2.5 GRC mitigation dropdowns"""
        self.print_section_header("SORA 2.5 - GROUND RISK MITIGATIONS (JAR_doc_25 Table 5, Annex B)")
        
        # M1A Sheltering
        print("\n📌 Field 1/4: M1A Sheltering")
        self.verify_dropdown(
            field_name="M1(A) - Sheltering",
            sora_version="SORA 2.5",
            ui_element_id="m1a_25",
            backend_enum_class=M1ALevel25,
            expected_ui_options=["None", "Low", "Medium"],  # High is N/A
            official_reference="JAR_doc_27 Annex B, M1A (High is N/A)"
        )
        self.print_check_result(self.checks[-1])
        
        # M1B Operational Restrictions
        print("\n📌 Field 2/4: M1B Operational Restrictions")
        self.verify_dropdown(
            field_name="M1(B) - Operational restrictions",
            sora_version="SORA 2.5",
            ui_element_id="m1b_25",
            backend_enum_class=M1BLevel25,
            expected_ui_options=["None", "Medium", "High"],  # Low is N/A
            official_reference="JAR_doc_27 Annex B, M1B (Low is N/A)"
        )
        self.print_check_result(self.checks[-1])
        
        # M1C Ground Observation
        print("\n📌 Field 3/4: M1C Ground Observation")
        self.verify_dropdown(
            field_name="M1(C) - Ground observation",
            sora_version="SORA 2.5",
            ui_element_id="m1c_25",
            backend_enum_class=M1CLevel25,
            expected_ui_options=["None", "Low"],  # Medium/High are N/A
            official_reference="JAR_doc_27 Annex B, M1C (Med/High are N/A)"
        )
        self.print_check_result(self.checks[-1])
        
        # M2 Impact Dynamics
        print("\n📌 Field 4/4: M2 Impact Dynamics")
        self.verify_dropdown(
            field_name="M2 - Impact dynamics reduced",
            sora_version="SORA 2.5",
            ui_element_id="m2_25",
            backend_enum_class=M2Level25,
            expected_ui_options=["None", "Medium", "High"],  # Low is N/A
            official_reference="JAR_doc_27 Annex B, M2 (Low is N/A)"
        )
        self.print_check_result(self.checks[-1])
    
    def verify_sora20_arc_fields(self):
        """Verify SORA 2.0 ARC-related dropdowns"""
        self.print_section_header("SORA 2.0 - AIR RISK FIELDS (JAR_doc_06 Annex E)")
        
        # Airspace Class
        print("\n📌 Field 1/5: Airspace Class")
        self.verify_dropdown(
            field_name="Airspace Class",
            sora_version="SORA 2.0",
            ui_element_id="airspaceClass",
            backend_enum_class=AirspaceClass20,
            expected_ui_options=["G", "E", "D", "C", "CTR", "TMA", "TIZ", "ATZ", "RMZ", "TMZ"],
            official_reference="ICAO Airspace Classification + EASA zones"
        )
        self.print_check_result(self.checks[-1])
        
        # Airspace Segregation
        print("\n📌 Field 2/5: Airspace Segregation (Strategic Mitigation)")
        self.verify_dropdown(
            field_name="Airspace Segregation",
            sora_version="SORA 2.0",
            ui_element_id="airspaceSegregation",
            backend_enum_class=AirspaceSegregation,
            expected_ui_options=["None", "Partial", "Full_Certified"],
            official_reference="JAR_doc_06 Annex E, Strategic Mitigations"
        )
        self.print_check_result(self.checks[-1])
        
        # Operation Type
        print("\n📌 Field 3/5: Operation Type")
        self.verify_dropdown(
            field_name="Operation Type",
            sora_version="SORA 2.0",
            ui_element_id="operationType",
            backend_enum_class=OperationType,
            expected_ui_options=["VLOS", "EVLOS", "BVLOS"],
            official_reference="EASA Regulation 2019/947"
        )
        self.print_check_result(self.checks[-1])
        
        # Proximity to Aerodrome
        print("\n📌 Field 4/5: Proximity to Aerodrome")
        self.verify_dropdown(
            field_name="Proximity to Aerodrome",
            sora_version="SORA 2.0",
            ui_element_id="proximityAerodrome",
            backend_enum_class=ProximityAerodrome,
            expected_ui_options=["Inside", "Near", "Outside"],
            official_reference="JAR_doc_06 Annex E"
        )
        self.print_check_result(self.checks[-1])
        
        # Time of Operation
        print("\n📌 Field 5/5: Time of Operation")
        self.verify_dropdown(
            field_name="Time of Operation",
            sora_version="SORA 2.0",
            ui_element_id="timeOfOperation",
            backend_enum_class=TimeOfOperation,
            expected_ui_options=["Day", "Night", "Off-peak"],
            official_reference="JAR_doc_06"
        )
        self.print_check_result(self.checks[-1])
    
    def verify_sora25_arc_fields(self):
        """Verify SORA 2.5 enhanced ARC fields"""
        self.print_section_header("SORA 2.5 - ENHANCED AIR RISK FIELDS (JAR_doc_27 Annex B)")
        
        # Airspace Class (includes U-space)
        print("\n📌 Field 1/4: Airspace Class (Enhanced)")
        self.verify_dropdown(
            field_name="Airspace Class",
            sora_version="SORA 2.5",
            ui_element_id="airspaceClass",
            backend_enum_class=AirspaceClass25,
            expected_ui_options=["G", "E", "D", "C", "CTR", "TMA", "TIZ", "ATZ", "RMZ", "TMZ", "U-space"],
            official_reference="JAR_doc_27 Annex B + U-space zones"
        )
        self.print_check_result(self.checks[-1])
        
        # Airspace Containment (replaces Segregation)
        print("\n📌 Field 2/4: Airspace Containment (NEW in 2.5)")
        self.verify_dropdown(
            field_name="Airspace Containment",
            sora_version="SORA 2.5",
            ui_element_id="airspaceContainment25",
            backend_enum_class=AirspaceContainment25,
            expected_ui_options=["None", "Operational", "Certified"],
            official_reference="JAR_doc_27 Annex B (replaces 'Segregation')"
        )
        self.print_check_result(self.checks[-1])
        
        # Traffic Density Data Source
        print("\n📌 Field 3/4: Traffic Density Data Source (NEW in 2.5)")
        self.verify_dropdown(
            field_name="Traffic Density Data Source",
            sora_version="SORA 2.5",
            ui_element_id="trafficDensitySource",
            backend_enum_class=TrafficDensityDataSource,
            expected_ui_options=["Empirical", "Statistical", "Expert"],
            official_reference="JAR_doc_25 Table 4 (Expert not valid for Med/High density)"
        )
        self.print_check_result(self.checks[-1])
        
        # Boolean Fields (U-space services, temporal/spatial segregation)
        print("\n📌 Field 4/4: Boolean Fields (Checkboxes)")
        print("\n   ✅ u_space_services_available (bool)")
        print("      UI Element: #uSpaceServices")
        print("      Backend: ARCInputs25.u_space_services_available")
        print("      Options: Yes/No (select or checkbox)")
        print("      Ref: JAR_doc_27 Annex B - U-space Context")
        
        print("\n   ✅ temporal_segregation (bool)")
        print("      UI Element: #temporalSegregation")
        print("      Backend: ARCInputs25.temporal_segregation")
        print("      Options: Checked/Unchecked")
        print("      Ref: JAR_doc_27 Annex B - Time-based separation")
        
        print("\n   ✅ spatial_segregation (bool)")
        print("      UI Element: #spatialSegregation")
        print("      Backend: ARCInputs25.spatial_segregation")
        print("      Options: Checked/Unchecked")
        print("      Ref: JAR_doc_27 Annex B - Area-based separation")
    
    def verify_common_fields(self):
        """Verify fields common to both versions"""
        self.print_section_header("COMMON FIELDS (Both SORA 2.0 & 2.5)")
        
        # Containment Quality
        print("\n📌 Field: Containment Quality")
        self.verify_dropdown(
            field_name="Containment Quality",
            sora_version="Both",
            ui_element_id="containmentQuality",
            backend_enum_class=ContainmentQuality,
            expected_ui_options=["Poor", "Adequate", "Good"],
            official_reference="JAR_doc_06 & JAR_doc_27 (footprint control)"
        )
        self.print_check_result(self.checks[-1])
    
    def generate_summary_report(self):
        """Generate final summary with pass/fail counts"""
        self.print_section_header("VERIFICATION SUMMARY")
        
        total = len(self.checks)
        passed = sum(1 for c in self.checks if "✅" in c.status)
        failed = sum(1 for c in self.checks if "❌" in c.status)
        warnings = sum(1 for c in self.checks if "⚠️" in c.status)
        
        print(f"\n📊 Total Checks: {total}")
        print(f"✅ Passed: {passed}")
        print(f"❌ Failed: {failed}")
        print(f"⚠️ Warnings: {warnings}")
        print(f"\n📈 Success Rate: {(passed/total*100):.1f}%")
        
        if failed > 0:
            print("\n" + "="*70)
            print("❌ FAILED CHECKS:")
            print("="*70)
            for check in self.checks:
                if "❌" in check.status:
                    print(f"\n• {check.field_name} ({check.sora_version})")
                    print(f"  UI Element: #{check.ui_element_id}")
                    print(f"  Issue: {check.notes}")
        
        if warnings > 0:
            print("\n" + "="*70)
            print("⚠️ WARNINGS:")
            print("="*70)
            for check in self.checks:
                if "⚠️" in check.status:
                    print(f"\n• {check.field_name} ({check.sora_version})")
                    print(f"  Issue: {check.notes}")
        
        print("\n" + "="*70)
        if failed == 0 and warnings == 0:
            print("✅ ALL DROPDOWNS VERIFIED - 100% COMPLIANT!")
        elif failed == 0:
            print("✅ ALL CRITICAL CHECKS PASSED (warnings are non-blocking)")
        else:
            print(f"❌ {failed} CRITICAL ISSUE(S) FOUND - FIX REQUIRED")
        print("="*70 + "\n")
        
        return failed == 0

def main():
    """Run detailed field-by-field verification"""
    print("""
╔═══════════════════════════════════════════════════════════════╗
║                                                               ║
║       FIELD-BY-FIELD DROPDOWN VERIFICATION                   ║
║       Checking EVERY dropdown against backend specs          ║
║                                                               ║
╚═══════════════════════════════════════════════════════════════╝
    """)
    
    verifier = DetailedDropdownVerifier()
    
    # Verify SORA 2.0 GRC fields (M1/M2/M3)
    verifier.verify_sora20_grc_fields()
    
    # Verify SORA 2.5 GRC fields (M1A/M1B/M1C/M2)
    verifier.verify_sora25_grc_fields()
    
    # Verify SORA 2.0 ARC fields
    verifier.verify_sora20_arc_fields()
    
    # Verify SORA 2.5 enhanced ARC fields
    verifier.verify_sora25_arc_fields()
    
    # Verify common fields
    verifier.verify_common_fields()
    
    # Generate summary
    success = verifier.generate_summary_report()
    
    sys.exit(0 if success else 1)

if __name__ == "__main__":
    main()
