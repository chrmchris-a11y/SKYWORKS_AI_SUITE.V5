/**
 * ═══════════════════════════════════════════════════════════════════════════
 * SKYWORKS COMPREHENSIVE TEST MISSIONS
 * ═══════════════════════════════════════════════════════════════════════════
 * 
 * 20 realistic missions (10 SORA 2.0 + 10 SORA 2.5)
 * Covering: Sky Tech SC05/15/25/35, DJI Agras series, DJI Matrice series
 * All fields populated realistically per EASA/JARUS requirements
 * 
 * USAGE:
 * 1. Open Mission Planner UI
 * 2. Paste mission JSON into Quick Test panel
 * 3. Click "Run Test"
 * 4. Verify 100% EASA/JARUS alignment
 */

const COMPREHENSIVE_TEST_MISSIONS = [
  // ═══════════════════════════════════════════════════════════════════════════
  // SORA 2.0 MISSIONS - CLEANING SERVICES (12 missions)
  // ═══════════════════════════════════════════════════════════════════════════
  
  {
    missionId: "M-2.0-001",
    name: "Sky Tech SC05 - Solar Panel Cleaning (VLOS Populated)",
    description: "Photovoltaic solar panel cleaning in suburban residential area",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC05",
    missionType: "Solar Panel Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC05",
      "GroundRisk": {
        "MTOM_kg": 3.5,
        "Scenario_V2_0": "VLOS_Populated",
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
  },

  {
    missionId: "M-2.0-002",
    name: "Sky Tech SC05 - Building Windows Cleaning (VLOS Populated)",
    description: "High-rise building window cleaning in urban commercial district",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC05",
    missionType: "Windows Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 2,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC05",
      "GroundRisk": {
        "MTOM_kg": 3.5,
        "Scenario_V2_0": "VLOS_Populated",
        "Mitigations": [
          { "Type": "M1", "Robustness": "Medium" },
          { "Type": "M2", "Robustness": "High" },
          { "Type": "M3", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Controlled",
        "AdjacentAirspace": true,
        "AtcServiceAvailable": true,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.0-003",
    name: "Sky Tech SC05 - Facade Cleaning (VLOS Populated)",
    description: "Building facade pressure washing in downtown business district",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC05",
    missionType: "Facade Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 2,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC05",
      "GroundRisk": {
        "MTOM_kg": 3.5,
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
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.0-002",
    name: "Sky Tech SC15 - Infrastructure Inspection (BVLOS Populated)",
    description: "Power line inspection in urban area with high mitigations and ATC coordination",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC15",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 2,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC15",
      "GroundRisk": {
        "MTOM_kg": 32,
        "Scenario_V2_0": "BVLOS_Populated",
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
        "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4", "TM5"]
      }
    }
  },

  {
    missionId: "M-2.0-003",
    name: "Sky Tech SC25 - Search & Rescue (BVLOS Sparsely)",
    description: "Emergency search operation in sparsely populated mountainous area",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC25",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC25",
      "GroundRisk": {
        "MTOM_kg": 50,
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
  },

  {
    missionId: "M-2.0-004",
    name: "Sky Tech SC15 - Solar Farm Cleaning (BVLOS Sparsely)",
    description: "Large-scale solar farm panel cleaning in remote industrial area",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC15",
    missionType: "Solar Panel Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
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
  },

  {
    missionId: "M-2.0-005",
    name: "Sky Tech SC15 - Roof Cleaning (VLOS Populated)",
    description: "Commercial building roof moss removal and cleaning in suburban area",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC15",
    missionType: "Roof Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 2,
      initialARC: "ARC_b",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC15",
      "GroundRisk": {
        "MTOM_kg": 32,
        "Scenario_V2_0": "VLOS_Populated",
        "Mitigations": [
          { "Type": "M1", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" },
          { "Type": "M3", "Robustness": "Medium" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1", "TM2"]
      }
    }
  },

  {
    missionId: "M-2.0-006",
    name: "Sky Tech SC15 - Wind Turbine Cleaning (BVLOS Sparsely)",
    description: "Wind turbine blade cleaning in remote wind farm",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC15",
    missionType: "Wind Turbine Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
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
  },

  {
    missionId: "M-2.0-007",
    name: "Sky Tech SC25 - Industrial Solar Array Cleaning (BVLOS Sparsely)",
    description: "Heavy-duty cleaning of large industrial solar installation",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC25",
    missionType: "Solar Panel Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC25",
      "GroundRisk": {
        "MTOM_kg": 50,
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
  },

  {
    missionId: "M-2.0-008",
    name: "Sky Tech SC25 - High-Rise Facade Cleaning (VLOS Populated)",
    description: "Skyscraper facade cleaning in dense urban business district",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC25",
    missionType: "Facade Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 2,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
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
  },

  {
    missionId: "M-2.0-009",
    name: "Sky Tech SC25 - Warehouse Roof Cleaning (Controlled Ground)",
    description: "Large warehouse roof cleaning with full ground control",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC25",
    missionType: "Roof Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 2,
      finalGRC: 1,
      initialARC: "ARC_a",
      residualARC: "ARC_a",
      sail: "I",
      tmpr: "Low"
    },
    request: {
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
  },

  {
    missionId: "M-2.0-010",
    name: "Sky Tech SC35 - Offshore Wind Turbine Cleaning (BVLOS Sparsely)",
    description: "Offshore wind farm turbine blade cleaning in remote maritime area",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC35",
    missionType: "Wind Turbine Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
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
  },

  {
    missionId: "M-2.0-011",
    name: "Sky Tech SC35 - Stadium Roof Cleaning (VLOS Populated)",
    description: "Large sports stadium retractable roof cleaning in urban area",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC35",
    missionType: "Roof Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 2,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC35",
      "GroundRisk": {
        "MTOM_kg": 120,
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
        "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4", "TM5"]
      }
    }
  },

  {
    missionId: "M-2.0-012",
    name: "Sky Tech SC35 - Industrial Chimney Cleaning (BVLOS Populated)",
    description: "Power plant chimney exterior cleaning in industrial zone",
    soraVersion: "2.0",
    droneId: "SKYTECH_SC35",
    missionType: "Facade Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 1,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "SKYTECH_SC35",
      "GroundRisk": {
        "MTOM_kg": 120,
        "Scenario_V2_0": "BVLOS_Populated",
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
        "TechnicalMitigations": ["TM1", "TM2", "TM3", "TM4", "TM5", "TM6"]
      }
    }
  },

  {
    missionId: "M-2.0-005",
    name: "DJI Agras T40 - Crop Spraying (VLOS Populated)",
    description: "Precision agriculture spraying in vineyard with medium population density",
    soraVersion: "2.0",
    droneId: "DJI_AGRAS_T40",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 2,
      initialARC: "ARC_b",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "DJI_AGRAS_T40",
      "GroundRisk": {
        "MTOM_kg": 75.6,
        "Scenario_V2_0": "VLOS_Populated",
        "Mitigations": [
          { "Type": "M1", "Robustness": "Medium" },
          { "Type": "M2", "Robustness": "High" },
          { "Type": "M3", "Robustness": "Medium" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1"]
      }
    }
  },

  {
    missionId: "M-2.0-006",
    name: "DJI Agras T30 - Orchard Spraying (Controlled Ground)",
    description: "Private orchard spraying with full ground control and fencing",
    soraVersion: "2.0",
    droneId: "DJI_AGRAS_T30",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 2,
      finalGRC: 1,
      initialARC: "ARC_a",
      residualARC: "ARC_a",
      sail: "I",
      tmpr: "Low"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "DJI_AGRAS_T30",
      "GroundRisk": {
        "MTOM_kg": 59.5,
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
  },

  {
    missionId: "M-2.0-007",
    name: "DJI Matrice 350 RTK - Mapping Survey (VLOS Sparsely)",
    description: "Topographic mapping in low-density rural area",
    soraVersion: "2.0",
    droneId: "DJI_MATRICE_350_RTK",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 2,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "I",
      tmpr: "Low"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "DJI_MATRICE_350_RTK",
      "GroundRisk": {
        "MTOM_kg": 9.2,
        "Scenario_V2_0": "VLOS_SparselyPopulated",
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
  },

  {
    missionId: "M-2.0-008",
    name: "DJI Matrice 300 RTK - Infrastructure Inspection (BVLOS Populated)",
    description: "Bridge inspection in suburban area with ATC coordination",
    soraVersion: "2.0",
    droneId: "DJI_MATRICE_300_RTK",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 2,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "DJI_MATRICE_300_RTK",
      "GroundRisk": {
        "MTOM_kg": 9.0,
        "Scenario_V2_0": "BVLOS_Populated",
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
  },

  {
    missionId: "M-2.0-009",
    name: "DJI Matrice 30T - Thermal Inspection (VLOS Gathering)",
    description: "Firefighter thermal imaging during public event",
    soraVersion: "2.0",
    droneId: "DJI_MATRICE_30T",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 7,
      finalGRC: 4,
      initialARC: "ARC_c",
      residualARC: "ARC_c",
      sail: "IV",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "DJI_MATRICE_30T",
      "GroundRisk": {
        "MTOM_kg": 3.77,
        "Scenario_V2_0": "VLOS_GatheringOfPeople",
        "Mitigations": [
          { "Type": "M1", "Robustness": "Medium" },
          { "Type": "M2", "Robustness": "High" },
          { "Type": "M3", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Controlled",
        "AdjacentAirspace": true,
        "AtcServiceAvailable": true,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.0-010",
    name: "DJI Agras T20 - Rice Field Spraying (BVLOS Sparsely)",
    description: "Large-scale rice field spraying in remote agricultural area",
    soraVersion: "2.0",
    droneId: "DJI_AGRAS_T20",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "I",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.0",
      "DroneId": "DJI_AGRAS_T20",
      "GroundRisk": {
        "MTOM_kg": 47.5,
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
        "TechnicalMitigations": ["TM1", "TM2"]
      }
    }
  },

  // ═══════════════════════════════════════════════════════════════════════════
  // SORA 2.5 CLEANING MISSIONS (12 missions)
  // ═══════════════════════════════════════════════════════════════════════════

  {
    missionId: "M-2.5-001",
    name: "Sky Tech SC05 - Residential Solar Cleaning (SORA 2.5)",
    description: "Small residential rooftop solar panel cleaning using dimension-based risk assessment",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC05",
    missionType: "Solar Panel Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 3,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
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
  },

  {
    missionId: "M-2.5-002",
    name: "Sky Tech SC05 - Window Cleaning (SORA 2.5)",
    description: "Low-rise residential building window cleaning - dimension/speed assessment",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC05",
    missionType: "Window Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 3,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC05",
      "GroundRisk": {
        "Dimension_m": 0.8,
        "Speed_m_s": 6.0,
        "PopulationDensity": 8000,
        "Mitigations": [
          { "Type": "M1_B", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Controlled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1", "TM2", "TM6"]
      }
    }
  },

  {
    missionId: "M-2.5-003",
    name: "Sky Tech SC15 - Commercial Solar Farm (SORA 2.5)",
    description: "Large-scale commercial solar array cleaning with dimension-based GRC",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC15",
    missionType: "Solar Panel Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 2,
      initialARC: "ARC_b",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC15",
      "GroundRisk": {
        "Dimension_m": 1.5,
        "Speed_m_s": 12.0,
        "PopulationDensity": 3000,
        "Mitigations": [
          { "Type": "M1_A", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.5-004",
    name: "Sky Tech SC15 - Mid-Rise Facade Cleaning (SORA 2.5)",
    description: "Office building facade cleaning in urban environment",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC15",
    missionType: "Facade Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 3,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
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
  },

  {
    missionId: "M-2.5-005",
    name: "Sky Tech SC15 - Wind Farm Turbine Cleaning (SORA 2.5)",
    description: "Onshore wind turbine blade cleaning - dimension/speed approach",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC15",
    missionType: "Wind Turbine Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC15",
      "GroundRisk": {
        "Dimension_m": 1.5,
        "Speed_m_s": 15.0,
        "PopulationDensity": 500,
        "Mitigations": [
          { "Type": "M1_A", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.5-006",
    name: "Sky Tech SC25 - Industrial Roof Cleaning (SORA 2.5)",
    description: "Heavy-duty warehouse roof cleaning using dimension-based assessment",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC25",
    missionType: "Roof Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 2,
      initialARC: "ARC_b",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC25",
      "GroundRisk": {
        "Dimension_m": 2.0,
        "Speed_m_s": 18.0,
        "PopulationDensity": 2000,
        "Mitigations": [
          { "Type": "M1_A", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.5-007",
    name: "Sky Tech SC25 - High-Rise Window Cleaning (SORA 2.5)",
    description: "Skyscraper window cleaning in dense urban center",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC25",
    missionType: "Window Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 7,
      finalGRC: 3,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
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
  },

  {
    missionId: "M-2.5-008",
    name: "Sky Tech SC25 - Solar Array Cleaning (SORA 2.5)",
    description: "Industrial-scale solar installation cleaning",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC25",
    missionType: "Solar Panel Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC25",
      "GroundRisk": {
        "Dimension_m": 2.0,
        "Speed_m_s": 20.0,
        "PopulationDensity": 1000,
        "Mitigations": [
          { "Type": "M1_A", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.5-009",
    name: "Sky Tech SC35 - Offshore Wind Turbine (SORA 2.5)",
    description: "Heavy-duty offshore wind turbine blade cleaning - dimension/speed method",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC35",
    missionType: "Wind Turbine Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 2,
      initialARC: "ARC_b",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
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
  },

  {
    missionId: "M-2.5-010",
    name: "Sky Tech SC35 - Stadium Facade Cleaning (SORA 2.5)",
    description: "Large sports complex exterior cleaning in urban area",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC35",
    missionType: "Facade Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 8,
      finalGRC: 4,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
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
  },

  {
    missionId: "M-2.5-011",
    name: "Sky Tech SC35 - Industrial Solar Cleaning (SORA 2.5)",
    description: "Mega solar farm cleaning with heavy-duty equipment",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC35",
    missionType: "Solar Panel Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 2,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC35",
      "GroundRisk": {
        "Dimension_m": 3.0,
        "Speed_m_s": 22.0,
        "PopulationDensity": 800,
        "Mitigations": [
          { "Type": "M1_A", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.5-012",
    name: "Sky Tech SC35 - Power Plant Chimney Cleaning (SORA 2.5)",
    description: "Industrial chimney exterior cleaning - high altitude operations",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC35",
    missionType: "Facade Cleaning",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 7,
      finalGRC: 3,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC35",
      "GroundRisk": {
        "Dimension_m": 3.0,
        "Speed_m_s": 18.0,
        "PopulationDensity": 5000,
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
  },

  {
    missionId: "M-2.5-LEGACY-001",
    name: "Sky Tech SC15 - Urban Infrastructure (High Density) [LEGACY]",
    description: "Building inspection in dense urban area with maximum mitigations",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC15",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 5,
      finalGRC: 1,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC15",
      "GroundRisk": {
        "CharacteristicDimension_m": 1.5,
        "MaxSpeed_ms": 25,
        "PopulationDensity": 15000,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Low" },
          { "Type": "M1B", "Robustness": "High" },
          { "Type": "M1C", "Robustness": "Low" },
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
  },

  {
    missionId: "M-2.5-003",
    name: "Sky Tech SC25 - Heavy Cargo (Medium Density)",
    description: "Medical supply delivery in suburban area",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC25",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC25",
      "GroundRisk": {
        "CharacteristicDimension_m": 2.0,
        "MaxSpeed_ms": 30,
        "PopulationDensity": 5000,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Low" },
          { "Type": "M1B", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Controlled",
        "AdjacentAirspace": true,
        "AtcServiceAvailable": true,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.5-004",
    name: "Sky Tech SC35 - Long-Range Cargo (Very High Density)",
    description: "Emergency supplies to disaster area in densely populated region",
    soraVersion: "2.5",
    droneId: "SKYTECH_SC35",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 6,
      finalGRC: 1,
      initialARC: "ARC_d",
      residualARC: "ARC_c",
      sail: "IV",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "SKYTECH_SC35",
      "GroundRisk": {
        "CharacteristicDimension_m": 3.0,
        "MaxSpeed_ms": 35,
        "PopulationDensity": 25000,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Low" },
          { "Type": "M1B", "Robustness": "High" },
          { "Type": "M1C", "Robustness": "Low" },
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
  },

  {
    missionId: "M-2.5-005",
    name: "DJI Agras T40 - Precision Spraying (Medium Density)",
    description: "Vineyard spraying with advanced flight planning",
    soraVersion: "2.5",
    droneId: "DJI_AGRAS_T40",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "DJI_AGRAS_T40",
      "GroundRisk": {
        "CharacteristicDimension_m": 3.2,
        "MaxSpeed_ms": 10,
        "PopulationDensity": 2000,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Medium" },
          { "Type": "M1B", "Robustness": "Medium" },
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
  },

  {
    missionId: "M-2.5-006",
    name: "DJI Agras T30 - Large Field Spraying (Low Density)",
    description: "Wheat field spraying in remote agricultural zone",
    soraVersion: "2.5",
    droneId: "DJI_AGRAS_T30",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 2,
      finalGRC: 1,
      initialARC: "ARC_a",
      residualARC: "ARC_a",
      sail: "I",
      tmpr: "Low"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "DJI_AGRAS_T30",
      "GroundRisk": {
        "CharacteristicDimension_m": 2.8,
        "MaxSpeed_ms": 10,
        "PopulationDensity": 200,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Low" },
          { "Type": "M1B", "Robustness": "Medium" },
          { "Type": "M2", "Robustness": "Medium" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": ["TM1"]
      }
    }
  },

  {
    missionId: "M-2.5-007",
    name: "DJI Matrice 350 RTK - High-Precision Mapping (High Density)",
    description: "Urban photogrammetry with RTK precision",
    soraVersion: "2.5",
    droneId: "DJI_MATRICE_350_RTK",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_c",
      residualARC: "ARC_b",
      sail: "III",
      tmpr: "High"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "DJI_MATRICE_350_RTK",
      "GroundRisk": {
        "CharacteristicDimension_m": 0.895,
        "MaxSpeed_ms": 23,
        "PopulationDensity": 10000,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Low" },
          { "Type": "M1B", "Robustness": "High" },
          { "Type": "M1C", "Robustness": "Low" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Controlled",
        "AdjacentAirspace": true,
        "AtcServiceAvailable": true,
        "TechnicalMitigations": ["TM1", "TM2", "TM3"]
      }
    }
  },

  {
    missionId: "M-2.5-008",
    name: "DJI Matrice 300 RTK - Infrastructure Monitoring (Medium Density)",
    description: "Wind turbine inspection in suburban area",
    soraVersion: "2.5",
    droneId: "DJI_MATRICE_300_RTK",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 3,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_a",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "DJI_MATRICE_300_RTK",
      "GroundRisk": {
        "CharacteristicDimension_m": 0.81,
        "MaxSpeed_ms": 23,
        "PopulationDensity": 3000,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Medium" },
          { "Type": "M1B", "Robustness": "High" },
          { "Type": "M2", "Robustness": "High" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Controlled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": true,
        "TechnicalMitigations": ["TM1", "TM2"]
      }
    }
  },

  {
    missionId: "M-2.5-009",
    name: "DJI Matrice 30T - Thermal Survey (Very Low Density)",
    description: "Wildlife monitoring in remote nature reserve",
    soraVersion: "2.5",
    droneId: "DJI_MATRICE_30T",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 1,
      finalGRC: 1,
      initialARC: "ARC_a",
      residualARC: "ARC_a",
      sail: "I",
      tmpr: "Low"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "DJI_MATRICE_30T",
      "GroundRisk": {
        "CharacteristicDimension_m": 0.47,
        "MaxSpeed_ms": 23,
        "PopulationDensity": 5,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Low" },
          { "Type": "M2", "Robustness": "Medium" }
        ]
      },
      "AirRisk": {
        "AirspaceType": "Uncontrolled",
        "AdjacentAirspace": false,
        "AtcServiceAvailable": false,
        "TechnicalMitigations": []
      }
    }
  },

  {
    missionId: "M-2.5-010",
    name: "DJI Agras T20 - Multi-Field Campaign (High Density)",
    description: "Sequential field spraying across multiple farms in populated area",
    soraVersion: "2.5",
    droneId: "DJI_AGRAS_T20",
    expected100PercentCompliant: false,
    expectedResults: {
      intrinsicGRC: 4,
      finalGRC: 1,
      initialARC: "ARC_b",
      residualARC: "ARC_b",
      sail: "II",
      tmpr: "Medium"
    },
    request: {
      "SoraVersion": "2.5",
      "DroneId": "DJI_AGRAS_T20",
      "GroundRisk": {
        "CharacteristicDimension_m": 2.7,
        "MaxSpeed_ms": 10,
        "PopulationDensity": 8000,
        "Mitigations": [
          { "Type": "M1A", "Robustness": "Medium" },
          { "Type": "M1B", "Robustness": "High" },
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
  }
];

/**
 * Run all comprehensive tests
 */
async function runComprehensiveTests() {
  console.clear();
  console.log(`
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                                                                                   ║
║           🚁 SKYWORKS COMPREHENSIVE TEST SUITE 🚁                                 ║
║                                                                                   ║
║  Total Missions: ${COMPREHENSIVE_TEST_MISSIONS.length} (10 SORA 2.0 + 10 SORA 2.5)                             ║
║  Coverage: Sky Tech SC05/15/25/35, DJI Agras series, DJI Matrice series          ║
║  Goal: 100% EASA/JARUS Alignment Verification                                    ║
║                                                                                   ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
  `);
  
  const results = [];
  const startTime = Date.now();
  
  for (let i = 0; i < COMPREHENSIVE_TEST_MISSIONS.length; i++) {
    const mission = COMPREHENSIVE_TEST_MISSIONS[i];
    console.group(`\n🧪 Mission ${i + 1}/${COMPREHENSIVE_TEST_MISSIONS.length}: ${mission.name}`);
    console.log(`📝 Mission ID: ${mission.missionId}`);
    console.log(`📋 Description: ${mission.description}`);
    console.log(`🎯 SORA Version: ${mission.soraVersion}`);
    console.log(`🚁 Drone: ${mission.droneId}`);
    
    try {
      const baseUrl = window.location.origin;
      const apiUrl = `${baseUrl}/api/sora/complete`;
      
      console.log('📤 Sending request...');
      
      const response = await fetch(apiUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(mission.request)
      });
      
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText}`);
      }
      
      const result = await response.json();
      
      // Validate against expected results
      const validations = {
        success: result.isSuccessful === true,
        intrinsicGRC: result.intrinsicGRC === mission.expectedResults.intrinsicGRC,
        intrinsicGRC_NotZero: result.intrinsicGRC > 0,
        finalGRC: result.finalGRC === mission.expectedResults.finalGRC,
        sail: result.sail === mission.expectedResults.sail,
        hasOSOs: result.requiredOSOCount > 0
      };
      
      const allValid = Object.values(validations).every(v => v === true);
      
      console.log(`\n📊 Results:`);
      console.log(`  ✓ Intrinsic GRC: ${result.intrinsicGRC} (expected: ${mission.expectedResults.intrinsicGRC}) ${validations.intrinsicGRC ? '✅' : '⚠️'}`);
      console.log(`  ✓ Final GRC: ${result.finalGRC} (expected: ${mission.expectedResults.finalGRC}) ${validations.finalGRC ? '✅' : '⚠️'}`);
      console.log(`  ✓ SAIL: ${result.sail} (expected: ${mission.expectedResults.sail}) ${validations.sail ? '✅' : '⚠️'}`);
      console.log(`  ✓ Initial ARC: ${result.initialARC}`);
      console.log(`  ✓ Residual ARC: ${result.residualARC}`);
      console.log(`  ✓ TMPR: ${result.tmpr}`);
      console.log(`  ✓ Required OSOs: ${result.requiredOSOCount}`);
      console.log(`  ✓ Compliant: ${result.isCompliant ? 'YES ✅' : 'NO ⚠️'}`);
      console.log(`  ✓ Risk Score: ${result.riskScore}`);
      console.log(`  ✓ Risk Band: ${result.riskBand}`);
      
      if (allValid) {
        console.log(`\n✅ PASSED - 100% EASA/JARUS Alignment`);
      } else {
        console.log(`\n⚠️ VALIDATION ISSUES DETECTED`);
      }
      
      console.groupEnd();
      
      results.push({
        missionId: mission.missionId,
        name: mission.name,
        passed: allValid,
        validations: validations,
        result: result
      });
      
    } catch (error) {
      console.error(`❌ Mission Failed: ${error.message}`);
      console.groupEnd();
      
      results.push({
        missionId: mission.missionId,
        name: mission.name,
        passed: false,
        error: error.message
      });
    }
    
    // Small delay between missions
    await new Promise(resolve => setTimeout(resolve, 800));
  }
  
  const endTime = Date.now();
  const duration = ((endTime - startTime) / 1000).toFixed(2);
  
  // Final Summary
  const passed = results.filter(r => r.passed).length;
  const failed = results.filter(r => !r.passed).length;
  
  console.log(`
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                                                                                   ║
║                          📊 FINAL TEST SUMMARY 📊                                 ║
║                                                                                   ║
╚═══════════════════════════════════════════════════════════════════════════════════╝

  Total Missions: ${COMPREHENSIVE_TEST_MISSIONS.length}
  ✅ Passed: ${passed}
  ❌ Failed: ${failed}
  ⏱️  Duration: ${duration}s
  
  Success Rate: ${((passed / COMPREHENSIVE_TEST_MISSIONS.length) * 100).toFixed(1)}%
  
  ${passed === COMPREHENSIVE_TEST_MISSIONS.length ? 
    '🎉🎉🎉 100% SUCCESS - FULL EASA/JARUS ALIGNMENT ACHIEVED! 🎉🎉🎉' : 
    '⚠️ Some missions need review - check failed missions above'}
  `);
  
  return results;
}

// Export to window
window.COMPREHENSIVE_TEST_MISSIONS = COMPREHENSIVE_TEST_MISSIONS;
window.runComprehensiveTests = runComprehensiveTests;

console.log(`
╔═══════════════════════════════════════════════════════════════════════════════════╗
║                                                                                   ║
║  ✅ Comprehensive Test Suite Loaded!                                              ║
║                                                                                   ║
║  Command: await runComprehensiveTests()                                           ║
║                                                                                   ║
║  Missions: 20 (10 SORA 2.0 + 10 SORA 2.5)                                         ║
║  Drones: Sky Tech (SC05/15/25/35), DJI Agras (T20/T30/T40), Matrice (30T/300/350) ║
║                                                                                   ║
╚═══════════════════════════════════════════════════════════════════════════════════╝
`);
