// 🧪 TEST 1: SC05 - Residential Solar (SORA 2.0) → Expected: iGRC=5, fGRC=1, SAIL=II

{
  "SoraVersion": "2.0",
  "DroneId": "SKYTECH_SC05",
  "GroundRisk": {
    "MTOM_kg": 3.5,
    "Scenario_V2_0": "VLOS_Populated",
    "Mitigations": [
      { "Type": "M1", "Robustness": "Medium" },
      { "Type": "M2", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Uncontrolled",
    "AdjacentAirspace": false,
    "AtcServiceAvailable": false,
    "TechnicalMitigations": ["TM1", "TM2"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 2: SC15 - Commercial Solar Farm Cleaning (SORA 2.0)
Expected: iGRC=4, fGRC=1, SAIL=II
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.0",
  "DroneId": "SKYTECH_SC15",
  "GroundRisk": {
    "MTOM_kg": 32,
    "Scenario_V2_0": "BVLOS_SparselyPopulated",
    "Mitigations": [
      { "Type": "M1", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" },
      { "Type": "M3", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Uncontrolled",
    "AdjacentAirspace": false,
    "AtcServiceAvailable": false,
    "TechnicalMitigations": ["TM1", "TM2", "TM3"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 3: SC25 - High-Rise Facade Cleaning (SORA 2.0)
Expected: iGRC=6, fGRC=2, SAIL=III
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.0",
  "DroneId": "SKYTECH_SC25",
  "GroundRisk": {
    "MTOM_kg": 50,
    "Scenario_V2_0": "VLOS_Populated",
    "Mitigations": [
      { "Type": "M1", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" },
      { "Type": "M3", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Controlled",
    "AdjacentAirspace": true,
    "AtcServiceAvailable": true,
    "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 4: SC35 - Offshore Wind Turbine Cleaning (SORA 2.0)
Expected: iGRC=4, fGRC=1, SAIL=II
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.0",
  "DroneId": "SKYTECH_SC35",
  "GroundRisk": {
    "MTOM_kg": 120,
    "Scenario_V2_0": "BVLOS_SparselyPopulated",
    "Mitigations": [
      { "Type": "M1", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" },
      { "Type": "M3", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Uncontrolled",
    "AdjacentAirspace": false,
    "AtcServiceAvailable": false,
    "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 5: SC05 - Residential Solar (SORA 2.5)
Expected: iGRC=3, fGRC=1, SAIL=II
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.5",
  "DroneId": "SKYTECH_SC05",
  "GroundRisk": {
    "Dimension_m": 0.8,
    "Speed_m_s": 8.0,
    "PopulationDensity": 5000,
    "Mitigations": [
      { "Type": "M1_A", "Robustness": "Medium" },
      { "Type": "M2", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Uncontrolled",
    "AdjacentAirspace": false,
    "AtcServiceAvailable": false,
    "TechnicalMitigations": ["TM1", "TM2"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 6: SC15 - Mid-Rise Facade Cleaning (SORA 2.5)
Expected: iGRC=6, fGRC=3, SAIL=III
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.5",
  "DroneId": "SKYTECH_SC15",
  "GroundRisk": {
    "Dimension_m": 1.5,
    "Speed_m_s": 10.0,
    "PopulationDensity": 15000,
    "Mitigations": [
      { "Type": "M1_B", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Controlled",
    "AdjacentAirspace": true,
    "AtcServiceAvailable": true,
    "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 7: SC25 - High-Rise Window Cleaning (SORA 2.5)
Expected: iGRC=7, fGRC=3, SAIL=III
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.5",
  "DroneId": "SKYTECH_SC25",
  "GroundRisk": {
    "Dimension_m": 2.0,
    "Speed_m_s": 15.0,
    "PopulationDensity": 20000,
    "Mitigations": [
      { "Type": "M1_B", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Controlled",
    "AdjacentAirspace": true,
    "AtcServiceAvailable": true,
    "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4", "TM5"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 8: SC35 - Offshore Wind Turbine (SORA 2.5)
Expected: iGRC=6, fGRC=2, SAIL=II
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.5",
  "DroneId": "SKYTECH_SC35",
  "GroundRisk": {
    "Dimension_m": 3.0,
    "Speed_m_s": 25.0,
    "PopulationDensity": 100,
    "Mitigations": [
      { "Type": "M1_A", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Uncontrolled",
    "AdjacentAirspace": false,
    "AtcServiceAvailable": false,
    "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 9: SC35 - Stadium Facade (SORA 2.5)
Expected: iGRC=8, fGRC=4, SAIL=III
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.5",
  "DroneId": "SKYTECH_SC35",
  "GroundRisk": {
    "Dimension_m": 3.0,
    "Speed_m_s": 20.0,
    "PopulationDensity": 18000,
    "Mitigations": [
      { "Type": "M1_B", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Controlled",
    "AdjacentAirspace": true,
    "AtcServiceAvailable": true,
    "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4", "TM5", "TM6"]
  }
}

═══════════════════════════════════════════════════════════════════════════
TEST 10: SC25 - Warehouse Roof (SORA 2.0 - Low Risk)
Expected: iGRC=2, fGRC=1, SAIL=I
═══════════════════════════════════════════════════════════════════════════

{
  "SoraVersion": "2.0",
  "DroneId": "SKYTECH_SC25",
  "GroundRisk": {
    "MTOM_kg": 50,
    "Scenario_V2_0": "ControlledGroundArea",
    "Mitigations": [
      { "Type": "M1", "Robustness": "High" },
      { "Type": "M2", "Robustness": "High" },
      { "Type": "M3", "Robustness": "High" }
    ]
  },
  "AirRisk": {
    "AirspaceType": "Uncontrolled",
    "AdjacentAirspace": false,
    "AtcServiceAvailable": false,
    "TechnicalMitigations": ["TM1", "TM2"]
  }
}

═══════════════════════════════════════════════════════════════════════════
✅ Τι να ελέγξεις μετά από κάθε test:
═══════════════════════════════════════════════════════════════════════════

✓ intrinsicGRC > 0 (ΠΟΤΕ μηδέν!)
✓ finalGRC >= 1
✓ SAIL level σωστό (I, II, III ή IV)
✓ isSuccessful: true
✓ Χωρίς errors στο response

═══════════════════════════════════════════════════════════════════════════
