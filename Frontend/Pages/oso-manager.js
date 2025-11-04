/**
 * OSO Manager - Comprehensive OSO Framework (Phase 5 - Step 41)
 * Handles OSO selection, validation, compliance tracking, and reporting
 * Based on JARUS SORA 2.0 (24 OSOs) and SORA 2.5 (17 OSOs)
 */

const OSOManager = (function() {
  'use strict';

  // OSO Definitions with detailed metadata
  const OSO_METADATA = {
    // SORA 2.5 OSOs (17 total)
    '1': {
      id: 1,
      name: 'Operator competent/proven',
      category: 'Organizational',
      description: 'UAS operator is competent and has been approved by the competent authority',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Training records', 'Certification documentation', 'Previous operation history'],
      applies_to: ['2.5']
    },
    '2': {
      id: 2,
      name: 'Operational procedures designed',
      category: 'Organizational',
      description: 'Operational procedures are designed by competent person(s)',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Operations manual', 'SOP documentation', 'Design review records'],
      applies_to: ['2.0', '2.5']
    },
    '3': {
      id: 3,
      name: 'UAS maintained by competent entity',
      category: 'Technical',
      description: 'UAS maintained by competent entity/person',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Maintenance logs', 'Qualified technician records', 'Maintenance schedule'],
      applies_to: ['2.0', '2.5']
    },
    '4': {
      id: 4,
      name: 'Manufacturer competent/proven',
      category: 'Technical',
      description: 'UAS designed/manufactured by competent entity',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Manufacturer certification', 'Quality management system', 'Design documentation'],
      applies_to: ['2.0', '2.5']
    },
    '5': {
      id: 5,
      name: 'UAS designed/made for safe recovery',
      category: 'Technical',
      description: 'UAS designed/manufactured such that flight/technical issue can be managed',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Return-to-home capability', 'Redundant systems', 'Failure mode analysis'],
      applies_to: ['2.5']
    },
    '6': {
      id: 6,
      name: 'Operational procedures validated',
      category: 'Organizational',
      description: 'Operational procedures are validated',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Flight tests', 'Simulation results', 'Validation reports'],
      applies_to: ['2.0', '2.5']
    },
    '7': {
      id: 7,
      name: 'Conformity check of configuration',
      category: 'Technical',
      description: 'Conformity check of configuration and software before each flight',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Pre-flight checklist', 'Configuration verification', 'Software version control'],
      applies_to: ['2.5']
    },
    '8': {
      id: 8,
      name: 'Operational procedures defined',
      category: 'Organizational',
      description: 'Operational procedures/limitation/emergency procedures defined',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Operations manual', 'Emergency procedures', 'Operational limitations document'],
      applies_to: ['2.0', '2.5']
    },
    '9': {
      id: 9,
      name: 'Remote crew trained and current',
      category: 'Human Factors',
      description: 'Remote crew trained/current and able to control anomalies',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Training certificates', 'Recency requirements', 'Competency assessments'],
      applies_to: ['2.0', '2.5']
    },
    '10': {
      id: 10,
      name: 'Safe recovery from technical issue',
      category: 'Technical',
      description: 'Safe recovery from technical issue (SORA 2.0 only)',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Redundancy analysis', 'Recovery procedures', 'System health monitoring'],
      applies_to: ['2.0']
    },
    '11': {
      id: 11,
      name: 'Handling deterioration of external systems',
      category: 'Technical',
      description: 'Procedures for handling deterioration of external systems',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Degraded mode operations', 'GPS loss procedures', 'Communication backup'],
      applies_to: ['2.0', '2.5']
    },
    '12': {
      id: 12,
      name: 'UAS designed for deterioration',
      category: 'Technical',
      description: 'UAS designed to manage deterioration of external systems',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Redundant navigation', 'Sensor fusion', 'Fail-safe mechanisms'],
      applies_to: ['2.0', '2.5']
    },
    '13': {
      id: 13,
      name: 'External services supporting UAS adequate',
      category: 'Organizational',
      description: 'External services supporting UAS operation are adequate',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['ATC coordination', 'Weather services', 'NOTAM monitoring'],
      applies_to: ['2.0', '2.5']
    },
    '14': {
      id: 14,
      name: 'Operational procedures validated',
      category: 'Organizational',
      description: 'Operational procedures validated (comprehensive)',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Full-scale testing', 'Validation report', 'Authority approval'],
      applies_to: ['2.0']
    },
    '15': {
      id: 15,
      name: 'Remote crew trained/current',
      category: 'Human Factors',
      description: 'Remote crew trained, current and proficient',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Advanced training', 'Proficiency checks', 'Type rating'],
      applies_to: ['2.0']
    },
    '16': {
      id: 16,
      name: 'Multi crew coordination',
      category: 'Human Factors',
      description: 'Multi-crew coordination processes',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['CRM training', 'Communication protocols', 'Role definitions'],
      applies_to: ['2.0', '2.5']
    },
    '17': {
      id: 17,
      name: 'Remote crew fit to operate',
      category: 'Human Factors',
      description: 'Remote crew is fit to operate',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Medical certification', 'Fitness declarations', 'Fatigue management'],
      applies_to: ['2.0', '2.5']
    },
    '18': {
      id: 18,
      name: 'Automatic protection of flight envelope',
      category: 'Technical',
      description: 'Automatic protection of flight envelope (SORA 2.0 only)',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Geo-fencing', 'Altitude limits', 'Speed limits'],
      applies_to: ['2.0']
    },
    '19': {
      id: 19,
      name: 'Safe recovery from human error',
      category: 'Technical',
      description: 'Safe recovery from human error (SORA 2.0 only)',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Error detection', 'Automated intervention', 'Undo functions'],
      applies_to: ['2.0']
    },
    '20': {
      id: 20,
      name: 'Simplified HMI',
      category: 'Human Factors',
      description: 'Human-machine interface prevents human error (SORA 2.0 only)',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Intuitive interface', 'Error prevention design', 'Usability testing'],
      applies_to: ['2.0']
    },
    '21': {
      id: 21,
      name: 'Operational procedures for adverse conditions',
      category: 'Organizational',
      description: 'Operational procedures for adverse operating conditions',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Weather limits', 'Night operations procedures', 'Emergency protocols'],
      applies_to: ['2.0']
    },
    '22': {
      id: 22,
      name: 'Crew trained for environmental conditions',
      category: 'Human Factors',
      description: 'Remote crew trained for environmental conditions',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Weather training', 'Night flying qualification', 'Special conditions training'],
      applies_to: ['2.0']
    },
    '23': {
      id: 23,
      name: 'Environmental conditions defined',
      category: 'Organizational',
      description: 'Operational and environmental conditions defined',
      jarus_ref: 'SORA 2.5 Table 14',
      ami_examples: ['Weather limitations', 'Operational envelope', 'Environmental impact assessment'],
      applies_to: ['2.0', '2.5']
    },
    '24': {
      id: 24,
      name: 'UAS components proven reliable',
      category: 'Technical',
      description: 'Critical UAS components proven reliable (SORA 2.0 only)',
      jarus_ref: 'SORA 2.0 Table 6',
      ami_examples: ['Reliability testing', 'Mean time between failures', 'Quality records'],
      applies_to: ['2.0']
    }
  };

  // OSO Categories for grouping
  const OSO_CATEGORIES = {
    'Organizational': {
      name: 'Organizational Mitigations',
      icon: '🏢',
      color: '#ff9800'
    },
    'Technical': {
      name: 'Technical Mitigations',
      icon: '⚙️',
      color: '#2196f3'
    },
    'Human Factors': {
      name: 'Human Factors Mitigations',
      icon: '👥',
      color: '#4caf50'
    }
  };

  // Track selected OSOs and their robustness
  let selectedOSOs = {};

  /**
   * Initialize OSO Manager
   */
  function initialize() {
    console.log('[OSO Manager] Initializing OSO Framework v1.0');
    console.log('[OSO Manager] Loaded metadata for 24 unique OSOs');
    selectedOSOs = {};
  }

  /**
   * Get OSO metadata by ID
   */
  function getOSOMetadata(osoId) {
    return OSO_METADATA[String(osoId)] || null;
  }

  /**
   * Get all OSOs for a specific SORA version
   */
  function getOSOsForVersion(version) {
    const versionKey = version === 'SORA-2.0' ? '2.0' : '2.5';
    return Object.values(OSO_METADATA).filter(oso => 
      oso.applies_to.includes(versionKey)
    );
  }

  /**
   * Select/Deselect an OSO with robustness level
   */
  function selectOSO(osoId, robustness) {
    const metadata = getOSOMetadata(osoId);
    if (!metadata) {
      console.warn(`[OSO Manager] Unknown OSO ID: ${osoId}`);
      return false;
    }

    selectedOSOs[osoId] = {
      id: osoId,
      robustness: robustness,
      metadata: metadata,
      timestamp: new Date().toISOString()
    };

    console.log(`[OSO Manager] Selected OSO#${osoId} with robustness ${robustness}`);
    return true;
  }

  /**
   * Deselect an OSO
   */
  function deselectOSO(osoId) {
    if (selectedOSOs[osoId]) {
      delete selectedOSOs[osoId];
      console.log(`[OSO Manager] Deselected OSO#${osoId}`);
      return true;
    }
    return false;
  }

  /**
   * Get all selected OSOs
   */
  function getSelectedOSOs() {
    return { ...selectedOSOs };
  }

  /**
   * Clear all selected OSOs
   */
  function clearSelectedOSOs() {
    selectedOSOs = {};
    console.log('[OSO Manager] Cleared all selected OSOs');
  }

  /**
   * Generate OSO compliance summary
   */
  function generateComplianceSummary(sail, version) {
    const required = getRequiredOSOs(sail, version);
    const selected = Object.keys(selectedOSOs).map(Number);
    
    const missing = required.filter(req => !selected.includes(req.id));
    const extra = selected.filter(sel => !required.find(req => req.id === sel));
    
    return {
      sail,
      version,
      required: required.length,
      selected: selected.length,
      missing: missing.length,
      extra: extra.length,
      compliant: missing.length === 0,
      details: {
        required,
        missing,
        extra
      }
    };
  }

  /**
   * Get required OSOs for a SAIL level (stub - will be enhanced in Step 42)
   */
  function getRequiredOSOs(sail, version) {
    // This will be implemented in Step 42 with actual SAIL-based logic
    console.log(`[OSO Manager] Getting required OSOs for SAIL ${sail}, ${version}`);
    return [];
  }

  /**
   * Export OSO selection as JSON
   */
  function exportOSOSelection() {
    return {
      timestamp: new Date().toISOString(),
      version: '1.0',
      osos: selectedOSOs
    };
  }

  /**
   * Import OSO selection from JSON
   */
  function importOSOSelection(data) {
    if (!data || !data.osos) {
      console.error('[OSO Manager] Invalid import data');
      return false;
    }

    selectedOSOs = { ...data.osos };
    console.log(`[OSO Manager] Imported ${Object.keys(selectedOSOs).length} OSOs`);
    return true;
  }

  // Public API
  return {
    initialize,
    getOSOMetadata,
    getOSOsForVersion,
    selectOSO,
    deselectOSO,
    getSelectedOSOs,
    clearSelectedOSOs,
    generateComplianceSummary,
    getRequiredOSOs,
    exportOSOSelection,
    importOSOSelection,
    CATEGORIES: OSO_CATEGORIES
  };
})();

// Make globally available
window.OSOManager = OSOManager;

// Auto-initialize
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => OSOManager.initialize());
} else {
  OSOManager.initialize();
}
