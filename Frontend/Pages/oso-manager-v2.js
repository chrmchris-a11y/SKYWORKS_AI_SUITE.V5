/**
 * ═══════════════════════════════════════════════════════════════════════════
 * OSO MANAGER v2.0 - Authoritative JARUS SORA Framework
 * ═══════════════════════════════════════════════════════════════════════════
 * Phase 5 - Step 41: OSO Basic Framework
 * 
 * Based on authoritative JARUS documents:
 * - SORA 2.0: JAR-doc-06 (24 OSOs total)
 * - SORA 2.5: JAR-doc-25 Table 14 + Annex E (17 OSOs)
 * 
 * Source Documents:
 * - EXTRACTED_jar_doc_06_jarus_sora_v2.0.txt
 * - EXTRACTED_SORA-v2.5-Main-Body-Release-JAR_doc_25.txt
 * - EXTRACTED_SORA-v2.5-Annex-E-Release.JAR_doc_28pdf.txt
 * ═══════════════════════════════════════════════════════════════════════════
 */

const OSOManager = (function() {
  'use strict';

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * SORA 2.0 OSO DEFINITIONS (24 OSOs) - Table 6 from JAR-doc-06
   * ═══════════════════════════════════════════════════════════════════════════
   */
  const OSO_DEFINITIONS_V20 = {
    1: {
      id: 1,
      number: 'OSO#01',
      name: 'Ensure the Operator is competent and/or proven',
      category: 'Organizational',
      description: 'The operator has appropriate procedures and organization for the safe conduct of the operation.',
      jarusRef: 'SORA 2.0 Table 6, Section 3.2',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic procedures',
        M: 'Medium - Organization with continuous evaluation',
        H: 'High - SMS per ICAO Annex 19'
      },
      evidenceTypes: ['SMS documentation', 'Procedures', 'Training records'],
      sailRequirements: { I: 'NR', II: 'L', III: 'M', IV: 'H', V: 'H', VI: 'H' }
    },
    2: {
      id: 2,
      number: 'OSO#02',
      name: 'UAS manufactured by competent and/or proven entity',
      category: 'Technical',
      description: 'UAS manufactured with quality procedures.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic manufacturing',
        M: 'Medium - Enhanced procedures',
        H: 'High - Full quality management'
      },
      evidenceTypes: ['Manufacturing procedures', 'QMS certification'],
      sailRequirements: { I: 'NR', II: 'NR', III: 'L', IV: 'M', V: 'H', VI: 'H' }
    },
    3: {
      id: 3,
      number: 'OSO#03',
      name: 'UAS maintained by competent entity',
      category: 'Technical',
      description: 'Maintenance performed by qualified personnel.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic maintenance',
        M: 'Medium - Certified maintenance',
        H: 'High - Full maintenance program'
      },
      evidenceTypes: ['Maintenance logs', 'Personnel qualifications'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    4: {
      id: 4,
      number: 'OSO#04',
      name: 'UAS designed to authority recognized design standards',
      category: 'Technical',
      description: 'UAS components designed to recognized standards.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Industry standards',
        M: 'Medium - Certified standards',
        H: 'High - Full ADS compliance'
      },
      evidenceTypes: ['Design documentation', 'Certification'],
      sailRequirements: { I: 'NR', II: 'NR', III: 'NR', IV: 'L', V: 'M', VI: 'H' }
    },
    5: {
      id: 5,
      number: 'OSO#05',
      name: 'UAS designed considering system safety and reliability',
      category: 'Technical',
      description: 'System design with safety and reliability principles.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic safety analysis',
        M: 'Medium - System safety assessment',
        H: 'High - Full FHA/FMEA'
      },
      evidenceTypes: ['Safety analysis', 'FMEA', 'FHA'],
      sailRequirements: { I: 'NR', II: 'NR', III: 'L', IV: 'M', V: 'H', VI: 'H' }
    },
    6: {
      id: 6,
      number: 'OSO#06',
      name: 'C3 link characteristics appropriate for operation',
      category: 'Technical',
      description: 'Command and Control link meets operational requirements.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic link',
        M: 'Medium - Tested link',
        H: 'High - Certified link performance'
      },
      evidenceTypes: ['C3 link specifications', 'Test reports'],
      sailRequirements: { I: 'NR', II: 'L', III: 'L', IV: 'M', V: 'H', VI: 'H' }
    },
    7: {
      id: 7,
      number: 'OSO#07',
      name: 'Inspection of UAS to ensure consistency to ConOps',
      category: 'Technical',
      description: 'Regular inspection to maintain safe state.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic inspection',
        M: 'Medium - Structured inspection',
        H: 'High - Certified inspection program'
      },
      evidenceTypes: ['Inspection procedures', 'Inspection logs'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    8: {
      id: 8,
      number: 'OSO#08',
      name: 'Operational procedures are defined, validated and adhered to',
      category: 'Organizational',
      description: 'Comprehensive operational procedures in place.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic procedures',
        M: 'Medium - Validated procedures',
        H: 'High - Certified procedures'
      },
      evidenceTypes: ['Operations manual', 'Validation records'],
      sailRequirements: { I: 'L', II: 'M', III: 'H', IV: 'H', V: 'H', VI: 'H' }
    },
    9: {
      id: 9,
      number: 'OSO#09',
      name: 'Remote crew trained and current and able to control abnormal situations',
      category: 'Human Factors',
      description: 'Crew training and proficiency maintained.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic training',
        M: 'Medium - Structured training',
        H: 'High - Certified training program'
      },
      evidenceTypes: ['Training records', 'Proficiency checks'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    10: {
      id: 10,
      number: 'OSO#10',
      name: 'Safe recovery from technical issue (MERGED into OSO#05 in SORA 2.5)',
      category: 'Technical',
      description: 'Ability to recover from technical failures.',
      jarusRef: 'SORA 2.0 Table 6 (Removed in SORA 2.5)',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic recovery',
        M: 'Medium - Tested recovery',
        H: 'High - Certified recovery procedures'
      },
      evidenceTypes: ['Recovery procedures', 'Test evidence'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    11: {
      id: 11,
      number: 'OSO#11',
      name: 'Procedures to handle the deterioration of external systems (REMOVED in SORA 2.5)',
      category: 'Organizational',
      description: 'Procedures for external system failures.',
      jarusRef: 'SORA 2.0 Table 6 (Removed in SORA 2.5)',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic procedures',
        M: 'Medium - Validated procedures',
        H: 'High - Certified procedures'
      },
      evidenceTypes: ['Contingency procedures'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'H', V: 'H', VI: 'H' }
    },
    12: {
      id: 12,
      number: 'OSO#12',
      name: 'UAS designed and qualified for deterioration (REMOVED in SORA 2.5)',
      category: 'Technical',
      description: 'UAS can handle deteriorated conditions.',
      jarusRef: 'SORA 2.0 Table 6 (Removed in SORA 2.5)',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic resilience',
        M: 'Medium - Tested resilience',
        H: 'High - Certified resilience'
      },
      evidenceTypes: ['Design documentation', 'Test reports'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'H', V: 'H', VI: 'H' }
    },
    13: {
      id: 13,
      number: 'OSO#13',
      name: 'External services supporting UAS operation are adequate',
      category: 'Organizational',
      description: 'External services (ATC, weather, etc.) are reliable.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic services',
        M: 'Medium - Verified services',
        H: 'High - Certified services'
      },
      evidenceTypes: ['Service agreements', 'Reliability data'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'H', V: 'H', VI: 'H' }
    },
    14: {
      id: 14,
      number: 'OSO#14',
      name: 'Operational procedures validated (REMOVED in SORA 2.5)',
      category: 'Organizational',
      description: 'Procedures comprehensively validated.',
      jarusRef: 'SORA 2.0 Table 6 (Removed in SORA 2.5)',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic validation',
        M: 'Medium - Structured validation',
        H: 'High - Certified validation'
      },
      evidenceTypes: ['Validation records'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    15: {
      id: 15,
      number: 'OSO#15',
      name: 'Remote crew trained for advanced operations (REMOVED in SORA 2.5)',
      category: 'Human Factors',
      description: 'Advanced crew training.',
      jarusRef: 'SORA 2.0 Table 6 (Removed in SORA 2.5)',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic training',
        M: 'Medium - Advanced training',
        H: 'High - Expert training'
      },
      evidenceTypes: ['Training records'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    16: {
      id: 16,
      number: 'OSO#16',
      name: 'Multi-crew coordination',
      category: 'Human Factors',
      description: 'Crew coordination for multi-crew operations.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic coordination',
        M: 'Medium - Structured coordination',
        H: 'High - Certified CRM'
      },
      evidenceTypes: ['CRM training', 'Coordination procedures'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    17: {
      id: 17,
      number: 'OSO#17',
      name: 'Remote crew fit to operate',
      category: 'Human Factors',
      description: 'Crew medical and fitness standards.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Self-declaration',
        M: 'Medium - Medical assessment',
        H: 'High - Aviation medical certificate'
      },
      evidenceTypes: ['Medical certificates', 'Fitness declarations'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    18: {
      id: 18,
      number: 'OSO#18',
      name: 'Automatic protection of flight envelope from human error',
      category: 'Technical',
      description: 'Automated systems protect from pilot error.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic limiters',
        M: 'Medium - Flight envelope protection',
        H: 'High - Certified protection systems'
      },
      evidenceTypes: ['System documentation', 'Test reports'],
      sailRequirements: { I: 'NR', II: 'NR', III: 'L', IV: 'M', V: 'H', VI: 'H' }
    },
    19: {
      id: 19,
      number: 'OSO#19',
      name: 'Safe recovery from human error',
      category: 'Technical',
      description: 'Systems to recover from human mistakes.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic recovery',
        M: 'Medium - Tested recovery',
        H: 'High - Certified recovery systems'
      },
      evidenceTypes: ['Recovery procedures', 'Test evidence'],
      sailRequirements: { I: 'NR', II: 'NR', III: 'L', IV: 'M', V: 'M', VI: 'H' }
    },
    20: {
      id: 20,
      number: 'OSO#20',
      name: 'Human-Machine Interface (HMI) appropriate',
      category: 'Human Factors',
      description: 'HMI suitable for mission complexity.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic HMI',
        M: 'Medium - Ergonomic HMI',
        H: 'High - Certified HMI design'
      },
      evidenceTypes: ['HMI design documentation', 'Usability testing'],
      sailRequirements: { I: 'NR', II: 'L', III: 'L', IV: 'M', V: 'M', VI: 'H' }
    },
    21: {
      id: 21,
      number: 'OSO#21',
      name: 'Operational procedures for adverse environmental conditions (REMOVED in SORA 2.5)',
      category: 'Organizational',
      description: 'Procedures for adverse weather/conditions.',
      jarusRef: 'SORA 2.0 Table 6 (Removed in SORA 2.5)',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic procedures',
        M: 'Medium - Comprehensive procedures',
        H: 'High - Certified procedures'
      },
      evidenceTypes: ['Weather procedures', 'Environmental limits'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'H', V: 'H', VI: 'H' }
    },
    22: {
      id: 22,
      number: 'OSO#22',
      name: 'Crew trained for environmental conditions (REMOVED in SORA 2.5)',
      category: 'Human Factors',
      description: 'Crew training for adverse conditions.',
      jarusRef: 'SORA 2.0 Table 6 (Removed in SORA 2.5)',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic training',
        M: 'Medium - Comprehensive training',
        H: 'High - Specialist training'
      },
      evidenceTypes: ['Training records'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'H', V: 'H', VI: 'H' }
    },
    23: {
      id: 23,
      number: 'OSO#23',
      name: 'Environmental conditions for safe operation defined',
      category: 'Organizational',
      description: 'Operational environmental limits defined.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic limits',
        M: 'Medium - Detailed limits',
        H: 'High - Comprehensive envelope'
      },
      evidenceTypes: ['Operating limitations', 'Environmental envelope'],
      sailRequirements: { I: 'L', II: 'L', III: 'M', IV: 'M', V: 'H', VI: 'H' }
    },
    24: {
      id: 24,
      number: 'OSO#24',
      name: 'UAS designed and qualified for adverse environmental conditions',
      category: 'Technical',
      description: 'UAS capable of operating in defined adverse conditions.',
      jarusRef: 'SORA 2.0 Table 6',
      applies_to: ['SORA-2.0'],
      robustnessLevels: {
        NR: 'Not Required',
        L: 'Low - Basic capability',
        M: 'Medium - Tested capability',
        H: 'High - Certified environmental qualification'
      },
      evidenceTypes: ['Environmental testing', 'Qualification reports'],
      sailRequirements: { I: 'NR', II: 'NR', III: 'M', IV: 'H', V: 'H', VI: 'H' }
    }
  };

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * SORA 2.5 OSO DEFINITIONS (17 OSOs) - Table 14 from JAR-doc-25
   * ═══════════════════════════════════════════════════════════════════════════
   */
  const OSO_DEFINITIONS_V25 = {
    1: {
      id: 1,
      number: 'OSO#01',
      name: 'Ensure the Operator is competent and/or proven',
      category: 'Organizational',
      description: 'The applicant has a safety management system in place in line with ICAO Annex 19 principles.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#01',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I)',
        L: 'Knowledgeable of UAS, basic procedures (checklists, maintenance, training) - SAIL II',
        M: 'Organization with continuous evaluation, occurrence analysis, reporting - SAIL III',
        H: 'Safety Management System per ICAO Annex 19 - SAIL IV-VI'
      },
      evidenceTypes: [
        'Safety Management System documentation',
        'Training records',
        'Operational procedures (checklists, maintenance, training)',
        'Occurrence analysis procedures',
        'Organizational structure documentation'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'L',
        III: 'M',
        IV: 'H',
        V: 'H',
        VI: 'H'
      }
    },

    2: {
      id: 2,
      number: 'OSO#02',
      name: 'UAS manufactured by competent and/or proven entity',
      category: 'Technical',
      description: 'Manufacturing procedures cover material specification, repeatability, configuration control, and quality management.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#02',
      dependencies: {
        operator: false,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I & II)',
        L: 'Basic manufacturing procedures: materials, repeatability, configuration - SAIL III',
        M: 'Enhanced procedures: incoming verification, traceability, inspections, supplier control - SAIL IV',
        H: 'Full procedures: personnel competence, tool calibration, handling, non-conforming control - SAIL V & VI'
      },
      evidenceTypes: [
        'Manufacturing procedures documentation',
        'Material specifications',
        'Quality management system records',
        'Configuration control documentation',
        'Supplier audit reports',
        'Inspection and test records'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'NR',
        III: 'L',
        IV: 'M',
        V: 'H',
        VI: 'H'
      }
    },

    3: {
      id: 3,
      number: 'OSO#03',
      name: 'UAS maintained by competent and/or proven entity',
      category: 'Technical',
      description: 'UAS maintenance follows documented procedures, maintenance logs, and qualified personnel.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#03',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: false
      },
      robustnessLevels: {
        L: 'Maintenance instructions defined and adhered to - SAIL I & II',
        M: 'Documented procedures, maintenance logs, authorized staff list, preventive/scheduled maintenance - SAIL III & IV',
        H: 'Maintenance procedure manual, validated programme, recurrent training for release-to-service staff - SAIL V & VI'
      },
      evidenceTypes: [
        'Maintenance instructions and procedures',
        'Maintenance log system',
        'List of authorized maintenance staff',
        'Training records for maintenance personnel',
        'Maintenance programme documentation',
        'Release-to-service authorization records'
      ],
      sailRequirements: {
        I: 'L',
        II: 'L',
        III: 'M',
        IV: 'M',
        V: 'H',
        VI: 'H'
      },
      criteria: {
        1: 'Procedure requirements',
        2: 'Training requirements'
      }
    },

    4: {
      id: 4,
      number: 'OSO#04',
      name: 'UAS components essential to safe operations are designed to an Airworthiness Design Standard (ADS)',
      category: 'Technical',
      description: 'Critical UAS components meet airworthiness design standards (e.g., EASA SC Light-UAS, NATO STANAG 4671/4703, JARUS LURS/LUAS).',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#04',
      dependencies: {
        operator: false,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I-III)',
        L: 'Components designed to ADS for 10^-4/FH loss of control - SAIL IV (or 30k FTB flight hours)',
        M: 'Components designed to ADS for 10^-5/FH loss of control - SAIL V',
        H: 'Components designed to ADS for 10^-6/FH loss of control - SAIL VI'
      },
      evidenceTypes: [
        'Airworthiness Design Standard compliance documentation',
        'Design analysis and test reports',
        'Certification documentation (EASA, NATO, JARUS)',
        'Functional Test-Based (FTB) flight hours evidence (30k+ hours for SAIL IV)',
        'Third-party validation reports'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'NR',
        III: 'NR',
        IV: 'L',
        V: 'M',
        VI: 'H'
      },
      criteria: {
        1: 'Design to ADS',
        2: 'Functional Test-Based alternative (30k hours for SAIL IV only)'
      },
      applicableStandards: [
        'EASA Special Condition Light-UAS',
        'NATO STANAG 4671 (USAR)',
        'NATO STANAG 4703 (Light UAS)',
        'JARUS CS-LURS (Light Unmanned Rotorcraft Systems)',
        'JARUS CS-LUAS (Light Unmanned Aeroplane Systems)'
      ]
    },

    5: {
      id: 5,
      number: 'OSO#05',
      name: 'UAS is designed considering system safety and reliability',
      category: 'Technical',
      description: 'UAS design ensures failure conditions meet required probabilities: Major ≤ Remote, Hazardous ≤ Extremely Remote, Catastrophic ≤ Extremely Improbable.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#05 (merged with OSO#10 from v2.0)',
      dependencies: {
        operator: false,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I & II - but novel/complex features may require declaration)',
        L: 'Equipment minimizes hazards for probable failures, simple justification - SAIL III',
        M: 'Major ≤ Remote, Hazardous ≤ Extremely Remote, Catastrophic ≤ Extremely Improbable, no single failure → Catastrophic - SAIL IV',
        H: 'Same as Medium + enhanced failure detection/alerting/management strategy - SAIL V & VI'
      },
      evidenceTypes: [
        'System Safety Assessment',
        'Failure Modes and Effects Analysis (FMEA)',
        'Fault Tree Analysis (FTA)',
        'Functional Hazard Assessment (FHA)',
        'Design documentation showing redundancy',
        'Safety case documentation'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'NR', // Note: Novel/complex features may require declaration
        III: 'L',
        IV: 'M',
        V: 'H',
        VI: 'H'
      },
      note: 'SORA 2.5 merged OSO#10 into OSO#05. For novel/complex features in SAIL II, applicant should declare with simple justification (UK CAA CAP 722A Vol 2 §2.4).'
    },

    6: {
      id: 6,
      number: 'OSO#06',
      name: 'C3 link characteristics are appropriate for the operation',
      category: 'Technical',
      description: 'Command and Control (C3) link performance, spectrum use, and integrity are adequate for the operation.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#06',
      dependencies: {
        operator: true,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I)',
        L: 'C3 link characteristics documented and appropriate - SAIL II & III',
        M: 'Enhanced C3 performance requirements, spectrum management - SAIL IV',
        H: 'Full C3 integrity analysis, redundancy, interference mitigation - SAIL V & VI'
      },
      evidenceTypes: [
        'C3 link performance analysis',
        'Spectrum authorization documentation',
        'Link budget analysis',
        'Interference mitigation procedures',
        'C3 loss procedures',
        'Redundancy analysis (if applicable)'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'L',
        III: 'L',
        IV: 'M',
        V: 'H',
        VI: 'H'
      }
    },

    7: {
      id: 7,
      number: 'OSO#07',
      name: 'Conformity check of the UAS configuration',
      category: 'Technical',
      description: 'Pre-flight conformity check of configuration and software ensures aircraft matches approved design.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#07',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: false
      },
      robustnessLevels: {
        L: 'Basic conformity check documented - SAIL I & II',
        M: 'Detailed pre-flight checklist, configuration verification - SAIL III & IV',
        H: 'Comprehensive configuration management, version control, automated checks - SAIL V & VI'
      },
      evidenceTypes: [
        'Pre-flight checklist',
        'Configuration verification procedures',
        'Software version control documentation',
        'Configuration management plan',
        'Automated conformity check tools (if applicable)'
      ],
      sailRequirements: {
        I: 'L',
        II: 'L',
        III: 'M',
        IV: 'M',
        V: 'H',
        VI: 'H'
      },
      criteria: {
        1: 'Configuration check procedures',
        2: 'Software version verification'
      }
    },

    8: {
      id: 8,
      number: 'OSO#08',
      name: 'Operational procedures are defined, validated and adhered to',
      category: 'Organizational',
      description: 'Operational procedures, limitations, and emergency procedures are defined, validated, and followed.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#08',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: true
      },
      robustnessLevels: {
        L: 'Basic operational procedures defined - SAIL I',
        M: 'Comprehensive procedures validated through testing - SAIL II & III',
        H: 'Full operations manual, validated procedures, adherence monitoring - SAIL IV-VI'
      },
      evidenceTypes: [
        'Operations Manual',
        'Standard Operating Procedures (SOPs)',
        'Emergency procedures documentation',
        'Operational limitations document',
        'Procedure validation reports',
        'Adherence monitoring records'
      ],
      sailRequirements: {
        I: 'L',
        II: 'M',
        III: 'H',
        IV: 'H',
        V: 'H',
        VI: 'H'
      },
      criteria: {
        1: 'Procedure definition and validation'
      }
    },

    9: {
      id: 9,
      number: 'OSO#09',
      name: 'Remote crew trained and current',
      category: 'Human Factors',
      description: 'Remote crew is trained, current, and able to control anomalies.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#09',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: true
      },
      robustnessLevels: {
        L: 'Basic training and currency requirements - SAIL I & II',
        M: 'Formal training programme with competency assessments - SAIL III & IV',
        H: 'Advanced training, recurrent checks, type rating - SAIL V & VI'
      },
      evidenceTypes: [
        'Training certificates',
        'Training syllabus',
        'Competency assessment records',
        'Currency requirements documentation',
        'Recurrent training schedule',
        'Type rating (if applicable)'
      ],
      sailRequirements: {
        I: 'L',
        II: 'L',
        III: 'M',
        IV: 'M',
        V: 'H',
        VI: 'H'
      }
    },

    13: {
      id: 13,
      number: 'OSO#13',
      name: 'External services supporting UAS operations are adequate to the operation',
      category: 'Organizational',
      description: 'External services (ATC, weather, NOTAMs, UTM, U-space) supporting the operation are adequate.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#13',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: false
      },
      robustnessLevels: {
        L: 'External services identified and available - SAIL I & II',
        M: 'Service Level Agreements (SLAs), backup procedures - SAIL III',
        H: 'Comprehensive SLAs, service monitoring, degraded mode operations - SAIL IV-VI'
      },
      evidenceTypes: [
        'Service Level Agreements (SLAs)',
        'ATC coordination documentation',
        'Weather service subscriptions',
        'NOTAM monitoring procedures',
        'UTM/U-space integration documentation',
        'Service availability monitoring',
        'Degraded service procedures'
      ],
      sailRequirements: {
        I: 'L',
        II: 'L',
        III: 'M',
        IV: 'H',
        V: 'H',
        VI: 'H'
      }
    },

    16: {
      id: 16,
      number: 'OSO#16',
      name: 'Multi crew coordination',
      category: 'Human Factors',
      description: 'Multi-crew coordination processes and Crew Resource Management (CRM) procedures.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#16',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: true
      },
      robustnessLevels: {
        L: 'Basic role definitions and communication protocols - SAIL I & II',
        M: 'CRM training, documented coordination procedures - SAIL III & IV',
        H: 'Advanced CRM, standardized callouts, error management - SAIL V & VI'
      },
      evidenceTypes: [
        'Crew Resource Management (CRM) training records',
        'Role definitions and responsibilities',
        'Communication protocols',
        'Standardized callouts and procedures',
        'Error management procedures',
        'Multi-crew SOPs'
      ],
      sailRequirements: {
        I: 'L',
        II: 'L',
        III: 'M',
        IV: 'M',
        V: 'H',
        VI: 'H'
      },
      criteria: {
        1: 'CRM training',
        2: 'Communication protocols',
        3: 'Error management'
      }
    },

    17: {
      id: 17,
      number: 'OSO#17',
      name: 'Remote crew is fit to operate',
      category: 'Human Factors',
      description: 'Remote crew fitness (medical, fatigue management, fitness-for-duty).',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#17',
      dependencies: {
        operator: true,
        designer: false,
        trainingOrg: false
      },
      robustnessLevels: {
        L: 'Self-declaration of fitness - SAIL I & II',
        M: 'Fitness-for-duty procedures, fatigue management - SAIL III & IV',
        H: 'Medical certification, comprehensive fatigue risk management - SAIL V & VI'
      },
      evidenceTypes: [
        'Medical certification',
        'Fitness-for-duty declarations',
        'Fatigue Risk Management System (FRMS)',
        'Duty time limitations',
        'Fitness assessment procedures'
      ],
      sailRequirements: {
        I: 'L',
        II: 'L',
        III: 'M',
        IV: 'M',
        V: 'H',
        VI: 'H'
      },
      criteria: {
        1: 'Medical fitness',
        2: 'Fatigue management',
        3: 'Fitness-for-duty procedures'
      }
    },

    18: {
      id: 18,
      number: 'OSO#18',
      name: 'Automatic protection of the flight envelope from human errors',
      category: 'Technical',
      description: 'Automated systems protect flight envelope (geo-fencing, altitude limits, speed limits).',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#18',
      dependencies: {
        operator: false,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I & II)',
        L: 'Basic envelope protection (geo-fence, altitude) - SAIL III',
        M: 'Enhanced protection with alerting and intervention - SAIL IV',
        H: 'Comprehensive protection, redundant systems - SAIL V & VI'
      },
      evidenceTypes: [
        'Geo-fencing implementation documentation',
        'Altitude limitation system',
        'Speed limitation system',
        'Automated intervention logic',
        'Testing and validation reports',
        'Failure mode analysis'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'NR',
        III: 'L',
        IV: 'M',
        V: 'H',
        VI: 'H'
      }
    },

    19: {
      id: 19,
      number: 'OSO#19',
      name: 'Safe recovery from human error',
      category: 'Technical',
      description: 'System provides safe recovery from human errors (error detection, undo functions, automated intervention).',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#19',
      dependencies: {
        operator: false,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I & II)',
        L: 'Basic error recovery (undo, cancel) - SAIL III & IV',
        M: 'Enhanced error detection and recovery - SAIL V',
        H: 'Comprehensive error management with automated intervention - SAIL VI'
      },
      evidenceTypes: [
        'Error detection algorithms',
        'Undo/cancel functionality documentation',
        'Automated intervention procedures',
        'Human error analysis',
        'Recovery testing evidence'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'NR',
        III: 'L',
        IV: 'M',
        V: 'M',
        VI: 'H'
      }
    },

    20: {
      id: 20,
      number: 'OSO#20',
      name: 'A Human Factors evaluation has been performed and the HMI found appropriate for the mission',
      category: 'Human Factors',
      description: 'Human-Machine Interface (HMI) design prevents human error through usability evaluation.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#20',
      dependencies: {
        operator: true,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I)',
        L: 'Basic HMI evaluation, intuitive design - SAIL II & III',
        M: 'Formal Human Factors evaluation, usability testing - SAIL IV & V',
        H: 'Comprehensive Human Factors analysis, iterative design validation - SAIL VI'
      },
      evidenceTypes: [
        'Human Factors evaluation report',
        'Usability testing results',
        'HMI design documentation',
        'Error prevention analysis',
        'User feedback and iteration records'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'L',
        III: 'L',
        IV: 'M',
        V: 'M',
        VI: 'H'
      }
    },

    23: {
      id: 23,
      number: 'OSO#23',
      name: 'Environmental conditions for safe operations defined, measurable and adhered to',
      category: 'Organizational',
      description: 'Operational and environmental conditions (weather, light, etc.) are defined, measurable, and followed.',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#23',
      dependencies: {
        operator: true,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        L: 'Basic environmental limitations defined - SAIL I & II',
        M: 'Detailed weather minima, measurement procedures - SAIL III & IV',
        H: 'Comprehensive environmental envelope, real-time monitoring - SAIL V & VI'
      },
      evidenceTypes: [
        'Operational weather minima',
        'Environmental limitations document',
        'Measurement procedures',
        'Real-time weather monitoring systems',
        'Environmental impact assessment'
      ],
      sailRequirements: {
        I: 'L',
        II: 'L',
        III: 'M',
        IV: 'M',
        V: 'H',
        VI: 'H'
      }
    },

    24: {
      id: 24,
      number: 'OSO#24',
      name: 'UAS designed and qualified for adverse environmental conditions',
      category: 'Technical',
      description: 'UAS is designed and tested for adverse environmental conditions (e.g., DO-160 qualification).',
      jarusRef: 'SORA 2.5 Main Body Table 14, Annex E Section OSO#24',
      dependencies: {
        operator: false,
        designer: true,
        trainingOrg: false
      },
      robustnessLevels: {
        NR: 'Not Required (SAIL I & II)',
        M: 'UAS tested for adverse conditions - SAIL III',
        H: 'Full environmental qualification (e.g., DO-160, MIL-STD) - SAIL IV-VI'
      },
      evidenceTypes: [
        'Environmental testing reports',
        'DO-160 qualification',
        'MIL-STD testing (if applicable)',
        'Weather resistance testing',
        'Sensor adequacy analysis',
        'Adverse condition flight testing'
      ],
      sailRequirements: {
        I: 'NR',
        II: 'NR',
        III: 'M',
        IV: 'H',
        V: 'H',
        VI: 'H'
      }
    }
  };

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * OSO CATEGORIES - For UI Grouping
   * ═══════════════════════════════════════════════════════════════════════════
   */
  const OSO_CATEGORIES = {
    'Organizational': {
      name: 'Organizational Mitigations',
      icon: '🏢',
      color: '#ff9800',
      description: 'OSOs related to operator organization, procedures, and management systems'
    },
    'Technical': {
      name: 'Technical Mitigations',
      icon: '⚙️',
      color: '#2196f3',
      description: 'OSOs related to UAS design, manufacturing, and technical systems'
    },
    'Human Factors': {
      name: 'Human Factors Mitigations',
      icon: '👥',
      color: '#4caf50',
      description: 'OSOs related to crew training, competence, and human-machine interface'
    }
  };

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * OSO COMPLIANCE TRACKER CLASS
   * ═══════════════════════════════════════════════════════════════════════════
   */
  class OSOComplianceTracker {
    constructor(soraVersion, sail) {
      this.soraVersion = soraVersion; // 'SORA-2.0' or 'SORA-2.5'
      this.sail = sail; // 'I', 'II', 'III', 'IV', 'V', 'VI'
      this.selectedOSOs = new Map(); // OSO ID → {robustness, evidence, notes, timestamp}
      this.requiredOSOs = this.getRequiredOSOs();
      this.complianceStatus = 'incomplete'; // 'incomplete', 'partial', 'complete'
      
      console.log(`[OSO Tracker] Initialized for ${soraVersion} SAIL ${sail}`);
      console.log(`[OSO Tracker] ${this.requiredOSOs.length} OSOs required`);
    }

    /**
     * Get required OSOs based on SAIL and SORA version
     */
    getRequiredOSOs() {
      const required = [];
      
      // Select correct OSO definitions based on SORA version
      const osoDefinitions = (this.soraVersion === 'SORA-2.0') 
        ? OSO_DEFINITIONS_V20 
        : OSO_DEFINITIONS_V25;
      
      console.log(`[OSO Tracker] Using ${this.soraVersion} definitions (${Object.keys(osoDefinitions).length} OSOs total)`);
      
      Object.values(osoDefinitions).forEach(oso => {
        const reqLevel = oso.sailRequirements[this.sail];
        if (reqLevel && reqLevel !== 'NR') {
          required.push({
            id: oso.id,
            number: oso.number,
            name: oso.name,
            requiredRobustness: reqLevel,
            category: oso.category
          });
        }
      });
      
      return required;
    }

    /**
     * Select an OSO with robustness level
     */
    selectOSO(osoId, robustness, evidence = '', notes = '') {
      // Get correct OSO definitions based on SORA version
      const osoDefinitions = (this.soraVersion === 'SORA-2.0') 
        ? OSO_DEFINITIONS_V20 
        : OSO_DEFINITIONS_V25;
      
      const osoDetails = osoDefinitions[osoId];
      if (!osoDetails) {
        console.error(`[OSO Tracker] Invalid OSO ID: ${osoId} for ${this.soraVersion}`);
        return false;
      }

      // Validate robustness level
      const validLevels = ['NR', 'L', 'M', 'H'];
      if (!validLevels.includes(robustness)) {
        console.error(`[OSO Tracker] Invalid robustness: ${robustness}`);
        return false;
      }

      this.selectedOSOs.set(osoId, {
        robustness,
        evidence,
        notes,
        timestamp: Date.now(),
        osoNumber: osoDetails.number,
        osoName: osoDetails.name
      });

      console.log(`[OSO Tracker] Selected ${osoDetails.number} with ${robustness} robustness`);
      this.updateComplianceStatus();
      return true;
    }

    /**
     * Remove an OSO
     */
    removeOSO(osoId) {
      const result = this.selectedOSOs.delete(osoId);
      if (result) {
        console.log(`[OSO Tracker] Removed OSO #${osoId}`);
        this.updateComplianceStatus();
      }
      return result;
    }

    /**
     * Validate compliance
     */
    validateCompliance() {
      const missing = [];
      const insufficient = [];
      
      const robustnessOrder = { 'NR': 0, 'L': 1, 'M': 2, 'H': 3 };

      this.requiredOSOs.forEach(required => {
        const selected = this.selectedOSOs.get(required.id);
        
        if (!selected) {
          missing.push({
            id: required.id,
            number: required.number,
            name: required.name,
            requiredRobustness: required.requiredRobustness
          });
          return;
        }

        // Check if robustness is sufficient
        if (robustnessOrder[selected.robustness] < robustnessOrder[required.requiredRobustness]) {
          insufficient.push({
            id: required.id,
            number: required.number,
            name: required.name,
            required: required.requiredRobustness,
            actual: selected.robustness
          });
        }
      });

      const isCompliant = (missing.length === 0 && insufficient.length === 0);

      return {
        isCompliant,
        missingOSOs: missing,
        insufficientOSOs: insufficient,
        selectedCount: this.selectedOSOs.size,
        requiredCount: this.requiredOSOs.length,
        compliancePercentage: this.requiredOSOs.length > 0 
          ? Math.round((this.selectedOSOs.size / this.requiredOSOs.length) * 100) 
          : 0
      };
    }

    /**
     * Update compliance status
     */
    updateComplianceStatus() {
      const validation = this.validateCompliance();
      if (validation.isCompliant) {
        this.complianceStatus = 'complete';
      } else if (validation.selectedCount > 0) {
        this.complianceStatus = 'partial';
      } else {
        this.complianceStatus = 'incomplete';
      }
    }

    /**
     * Export compliance report
     */
    exportComplianceReport() {
      return {
        soraVersion: this.soraVersion,
        sail: this.sail,
        timestamp: new Date().toISOString(),
        compliance: this.validateCompliance(),
        selectedOSOs: Array.from(this.selectedOSOs.entries()).map(([id, data]) => ({
          id,
          number: data.osoNumber,
          name: data.osoName,
          robustness: data.robustness,
          evidence: data.evidence,
          notes: data.notes,
          selectedAt: new Date(data.timestamp).toISOString()
        })),
        requiredOSOs: this.requiredOSOs
      };
    }

    /**
     * Import compliance data
     */
    importComplianceData(data) {
      if (data.soraVersion !== this.soraVersion || data.sail !== this.sail) {
        console.warn('[OSO Tracker] Import data version/SAIL mismatch');
        return false;
      }

      this.selectedOSOs.clear();
      data.selectedOSOs.forEach(oso => {
        this.selectOSO(oso.id, oso.robustness, oso.evidence || '', oso.notes || '');
      });

      console.log(`[OSO Tracker] Imported ${this.selectedOSOs.size} OSOs`);
      return true;
    }
  }

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * STEP 42: COMPLEX OSO ALGORITHMS
   * ═══════════════════════════════════════════════════════════════════════════
   */

  // ─────────────────────────────────────────────────────────────────────────
  // OSO DEPENDENCY DEFINITIONS
  // ─────────────────────────────────────────────────────────────────────────
  
  /**
   * SORA 2.0 Dependencies (24 OSOs)
   * Based on JARUS SORA v2.0 Table 6 + threat groupings
   */
  const SORA_20_DEPENDENCIES = {
    10: {
      dependencies: [5],
      reason: 'OSO#10 (Safe recovery) requires OSO#05 (System Safety) foundation',
      type: 'technical',
      critical: false
    },
    12: {
      dependencies: [5],
      reason: 'OSO#12 (UAS design for external systems) requires OSO#05 (System Safety)',
      type: 'technical',
      critical: false
    },
    13: {
      dependencies: [9],
      reason: 'OSO#13 (External services) requires OSO#09 (Crew training) on service usage',
      type: 'operational',
      critical: false
    },
    11: {
      dependencies: [8],
      reason: 'OSO#11 (External system procedures) requires OSO#08 (Operational procedures)',
      type: 'operational',
      critical: false
    }
  };

  /**
   * SORA 2.5 Dependencies (17 OSOs)
   * Based on JARUS SORA v2.5 Annex E + Table 14
   */
  const SORA_25_DEPENDENCIES = {
    5: {
      dependencies: ['Containment', 'M2'],
      reason: 'OSO#05 must be considered in conjunction with Containment (Step #8) and M2 (Annex B)',
      type: 'technical',
      critical: true,
      source: 'Annex E Section (a)'
    },
    9: {
      dependencies: [13],
      reason: 'OSO#09 (Crew training) must include use of external services (OSO#13)',
      type: 'operational',
      critical: false
    },
    8: {
      dependencies: ['External Systems'],
      reason: 'OSO#08 procedures must address deterioration of external systems (GNSS, ATM, UTM)',
      type: 'operational',
      critical: false
    }
  };

  /**
   * Evidence Provider Dependencies (from Table 14)
   */
  const EVIDENCE_PROVIDERS = {
    Operator: [1, 3, 7, 8, 13, 16, 17, 18, 19, 23],
    Designer: [4, 6, 8, 20, 23],
    Training_Org: [9, 17, 20]
  };

  // ─────────────────────────────────────────────────────────────────────────
  // ALGORITHM 1: DEPENDENCY RESOLUTION
  // ─────────────────────────────────────────────────────────────────────────

  /**
   * Validates OSO dependencies
   * @param {Map} selectedOSOs - Map of selected OSO IDs to their data
   * @param {string} soraVersion - 'SORA-2.0' or 'SORA-2.5'
   * @returns {Object} { missing: [], warnings: [] }
   */
  function validateDependencies(selectedOSOs, soraVersion) {
    const missing = [];
    const warnings = [];
    
    const dependencies = (soraVersion === 'SORA-2.0') 
      ? SORA_20_DEPENDENCIES 
      : SORA_25_DEPENDENCIES;

    // Check each selected OSO for dependencies
    selectedOSOs.forEach((osoData, osoId) => {
      const deps = dependencies[osoId];
      
      if (deps) {
        deps.dependencies.forEach(depId => {
          // Check if dependency is a string (like 'Containment') or an OSO ID
          if (typeof depId === 'string') {
            // Special dependencies (Containment, M2, etc.)
            const issue = {
              osoId: osoId,
              osoNumber: `OSO#${String(osoId).padStart(2, '0')}`,
              dependency: depId,
              reason: deps.reason,
              critical: deps.critical
            };
            
            if (deps.critical) {
              missing.push(issue);
            } else {
              warnings.push(issue);
            }
          } else {
            // OSO dependency
            if (!selectedOSOs.has(depId)) {
              const depOSO = soraVersion === 'SORA-2.0' 
                ? OSO_DEFINITIONS_V20[depId] 
                : OSO_DEFINITIONS_V25[depId];
              
              missing.push({
                osoId: osoId,
                osoNumber: `OSO#${String(osoId).padStart(2, '0')}`,
                dependency: depOSO ? depOSO.number : `OSO#${String(depId).padStart(2, '0')}`,
                dependencyName: depOSO ? depOSO.name : 'Unknown OSO',
                reason: deps.reason,
                critical: deps.critical
              });
            }
          }
        });
      }
    });

    console.log(`[OSO Dependencies] Validated ${selectedOSOs.size} OSOs - Found ${missing.length} missing, ${warnings.length} warnings`);
    
    return { missing, warnings };
  }

  // ─────────────────────────────────────────────────────────────────────────
  // ALGORITHM 2: AUTO-RECOMMENDATION ENGINE
  // ─────────────────────────────────────────────────────────────────────────

  /**
   * Recommends OSOs based on operation characteristics
   * @param {string} operationType - 'VLOS', 'BVLOS', 'EVLOS'
   * @param {string} environment - 'Urban', 'Rural', 'Maritime', 'Controlled'
   * @param {string} sail - 'I', 'II', 'III', 'IV', 'V', 'VI'
   * @param {string} soraVersion - 'SORA-2.0' or 'SORA-2.5'
   * @returns {Array} Recommendation objects
   */
  function recommendOSOs(operationType, environment, sail, soraVersion) {
    const recommendations = [];
    const sailNum = ['I', 'II', 'III', 'IV', 'V', 'VI'].indexOf(sail) + 1;

    // ─── BVLOS Operations ───
    if (operationType === 'BVLOS') {
      // OSO#06: C3 Link (critical for BVLOS)
      recommendations.push({
        id: 6,
        number: 'OSO#06',
        name: 'C3 link characteristics appropriate for operation',
        reason: 'C3 link performance critical for BVLOS operations',
        minRobustness: sailNum >= 4 ? 'M' : 'L',
        priority: 'HIGH',
        category: 'Technical'
      });

      if (soraVersion === 'SORA-2.0') {
        // OSO#10: Safe Recovery (SORA 2.0 only)
        recommendations.push({
          id: 10,
          number: 'OSO#10',
          name: 'Safe recovery from technical issue',
          reason: 'BVLOS requires safe recovery mechanisms',
          minRobustness: sailNum >= 3 ? 'M' : 'L',
          priority: 'HIGH',
          category: 'Technical'
        });

        // OSO#11: External Systems Procedures
        recommendations.push({
          id: 11,
          number: 'OSO#11',
          name: 'Procedures for external systems deterioration',
          reason: 'BVLOS relies on external systems (GNSS, etc.)',
          minRobustness: sailNum >= 2 ? 'M' : 'L',
          priority: 'MEDIUM',
          category: 'Operational'
        });
      } else {
        // SORA 2.5: OSO#10 merged into OSO#05
        recommendations.push({
          id: 5,
          number: 'OSO#05',
          name: 'UAS designed considering system safety and reliability',
          reason: 'System safety (includes recovery) critical for BVLOS',
          minRobustness: sailNum >= 5 ? 'H' : 'M',
          priority: 'HIGH',
          category: 'Technical',
          note: 'Includes safe recovery (formerly OSO#10 in SORA 2.0)'
        });
      }
    }

    // ─── Urban Operations ───
    if (environment === 'Urban') {
      // OSO#05: System Safety + Containment
      recommendations.push({
        id: 5,
        number: 'OSO#05',
        name: 'UAS designed considering system safety and reliability',
        reason: 'System safety essential in populated areas (requires Containment + M2)',
        minRobustness: sailNum >= 5 ? 'H' : 'M',
        priority: 'CRITICAL',
        category: 'Technical',
        dependencies: soraVersion === 'SORA-2.5' ? ['Containment (Step #8)', 'M2 (Ground Risk)'] : []
      });

      // OSO#18: Envelope Protection
      recommendations.push({
        id: 18,
        number: 'OSO#18',
        name: 'Automatic protection of flight envelope from human errors',
        reason: 'Prevents human errors in urban environments',
        minRobustness: sailNum >= 4 ? 'M' : 'L',
        priority: 'HIGH',
        category: 'Human Error'
      });

      // OSO#19: Human Error Recovery
      if (sailNum >= 3) {
        recommendations.push({
          id: 19,
          number: 'OSO#19',
          name: 'Safe recovery from human error',
          reason: 'Recovery mechanisms essential in populated areas',
          minRobustness: sailNum >= 5 ? 'M' : 'L',
          priority: 'HIGH',
          category: 'Human Error'
        });
      }

      // SORA 2.0 specific: OSO#21 Environmental procedures
      if (soraVersion === 'SORA-2.0' && sailNum >= 3) {
        recommendations.push({
          id: 21,
          number: 'OSO#21',
          name: 'Operational procedures for adverse conditions',
          reason: 'Environmental procedures critical for urban operations',
          minRobustness: 'H',
          priority: 'MEDIUM',
          category: 'Environmental'
        });
      }
    }

    // ─── External Services Usage ───
    if (operationType === 'BVLOS' || environment === 'Controlled') {
      // OSO#13: External Services
      recommendations.push({
        id: 13,
        number: 'OSO#13',
        name: 'External services supporting UAS operations',
        reason: 'UTM, GNSS, ATM services require adequacy verification',
        minRobustness: sailNum >= 4 ? 'H' : 'M',
        priority: 'HIGH',
        category: 'External Systems'
      });

      // OSO#09: Crew Training (dependency of OSO#13)
      recommendations.push({
        id: 9,
        number: 'OSO#09',
        name: 'Remote crew trained and current',
        reason: 'Crew must be trained on external service usage',
        minRobustness: sailNum >= 3 ? 'M' : 'L',
        priority: 'MEDIUM',
        category: 'Human Performance',
        dependencies: [13]
      });
    }

    // ─── High SAIL Operations (V-VI) ───
    if (sailNum >= 5) {
      // OSO#02: Manufacturing
      if (!recommendations.find(r => r.id === 2)) {
        recommendations.push({
          id: 2,
          number: 'OSO#02',
          name: 'UAS manufactured by competent entity',
          reason: 'High SAIL requires manufacturing quality assurance',
          minRobustness: 'H',
          priority: 'HIGH',
          category: 'Technical'
        });
      }

      // OSO#04: Airworthiness Design Standard
      if (!recommendations.find(r => r.id === 4)) {
        recommendations.push({
          id: 4,
          number: 'OSO#04',
          name: 'UAS components designed to Airworthiness Design Standard',
          reason: 'SAIL V-VI requires ADS compliance',
          minRobustness: 'H',
          priority: 'HIGH',
          category: 'Technical'
        });
      }
    }

    console.log(`[OSO Recommendations] Generated ${recommendations.length} recommendations for ${operationType}/${environment}/SAIL ${sail}`);
    
    return recommendations;
  }

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * PUBLIC API
   * ═══════════════════════════════════════════════════════════════════════════
   */
  return {
    // OSO Definitions
    OSO_DEFINITIONS_V20,  // SORA 2.0 (24 OSOs)
    OSO_DEFINITIONS_V25,  // SORA 2.5 (17 OSOs)
    OSO_CATEGORIES,

    // Compliance Tracker
    OSOComplianceTracker,

    // Helper Functions
    getOSODetails: function(soraVersion, osoId) {
      if (soraVersion === 'SORA-2.0') {
        return OSO_DEFINITIONS_V20[osoId] || null;
      } else if (soraVersion === 'SORA-2.5') {
        return OSO_DEFINITIONS_V25[osoId] || null;
      }
      return null;
    },

    getAllOSOs: function(soraVersion) {
      if (soraVersion === 'SORA-2.0') {
        return OSO_DEFINITIONS_V20;
      } else if (soraVersion === 'SORA-2.5') {
        return OSO_DEFINITIONS_V25;
      }
      return {};
    },

    getOSOsByCategory: function(soraVersion, category) {
      const allOSOs = this.getAllOSOs(soraVersion);
      const filtered = {};
      
      Object.entries(allOSOs).forEach(([id, oso]) => {
        if (oso.category === category) {
          filtered[id] = oso;
        }
      });
      
      return filtered;
    },

    getCategories: function() {
      return Object.keys(OSO_CATEGORIES);
    },

    createComplianceTracker: function(soraVersion, sail) {
      return new OSOComplianceTracker(soraVersion, sail);
    },

    // Utility: Get OSO count for version
    getOSOCount: function(soraVersion) {
      if (soraVersion === 'SORA-2.5') {
        return Object.keys(OSO_DEFINITIONS_V25).length;
      } else if (soraVersion === 'SORA-2.0') {
        return Object.keys(OSO_DEFINITIONS_V20).length;
      }
      return 0;
    },

    // ═══════════════════════════════════════════════════════════════════════════
    // STEP 42: COMPLEX OSO ALGORITHMS (PUBLIC API)
    // ═══════════════════════════════════════════════════════════════════════════

    /**
     * Algorithm 1: Validate OSO Dependencies
     */
    validateDependencies: validateDependencies,

    /**
     * Algorithm 2: Auto-recommend OSOs based on operation characteristics
     */
    recommendOSOs: recommendOSOs,

    /**
     * Algorithm 3: Get evidence template for OSO + robustness level
     * @param {number} osoId - OSO ID
     * @param {string} robustness - 'L', 'M', 'H'
     * @param {string} soraVersion - 'SORA-2.0' or 'SORA-2.5'
     * @returns {Object} Evidence template
     */
    getEvidenceTemplate: function(osoId, robustness, soraVersion) {
      // Evidence templates based on Annex E
      const templates = {
        // OSO#01 - Operator Competence
        1: {
          'L': {
            requiredDocuments: ['Operational procedures', 'Checklists', 'Maintenance logs', 'Training records'],
            sections: ['UAS knowledge', 'Procedures', 'Maintenance', 'Training', 'Responsibilities'],
            selfDeclared: true,
            thirdParty: false,
            standards: null
          },
          'M': {
            requiredDocuments: ['Organization structure', 'Continuous evaluation method', 'Occurrence reporting procedures'],
            sections: ['Continuous evaluation method', 'Occurrence analysis', 'Design-related event reporting'],
            selfDeclared: false,
            thirdParty: 'Prior to first operation audit',
            auditType: 'Third-party audit',
            standards: null
          },
          'H': {
            requiredDocuments: ['SMS Manual', 'Organizational Operating Certificate', 'Third-party audit reports'],
            sections: ['Safety Policy', 'Risk Management', 'Occurrence Reporting', 'Continuous monitoring'],
            selfDeclared: false,
            thirdParty: 'Recurrent third-party verification',
            standards: ['ICAO Annex 19', 'Recognized flight test organization'],
            icaoReference: 'ICAO Annex 19'
          }
        },
        
        // OSO#02 - Manufacturing
        2: {
          'L': {
            requiredDocuments: ['Manufacturing procedures', 'Material specifications', 'Process documentation'],
            sections: ['Material specifications', 'Suitability and durability', 'Repeatability processes', 'Configuration control'],
            selfDeclared: true,
            thirdParty: false,
            standards: ['Adequate standard per competent authority']
          },
          'M': {
            requiredDocuments: ['Manufacturing procedures', 'Incoming product verification', 'Traceability records'],
            sections: ['Verification of incoming products', 'Identification and traceability', 'In-process inspections', 'Final testing'],
            selfDeclared: false,
            thirdParty: 'Conformity evidence required',
            standards: ['Adequate standard per competent authority']
          },
          'H': {
            requiredDocuments: ['Manufacturing procedures', 'Personnel competence records', 'Supplier control', 'Audit reports'],
            sections: ['Personnel competence', 'Supplier control', 'Tool calibration', 'Handling and storage', 'Non-conforming item control'],
            selfDeclared: false,
            thirdParty: 'Recurrent process/product audit',
            standards: ['Industry standard (e.g., AS9100)']
          }
        },
        
        // OSO#05 - System Safety
        5: {
          'L': {
            requiredDocuments: ['Functional Hazard Assessment (FHA)', 'Design appraisal'],
            sections: ['Hazard minimization', 'Probable failure analysis'],
            selfDeclared: true,
            thirdParty: false,
            standards: ['UK CAA CAP 722A Vol 2 Section 2.4', 'Eurocae ED-280']
          },
          'M': {
            requiredDocuments: ['FHA', 'Safety assessment', 'Pre-flight check strategy', 'Failure detection strategy'],
            sections: ['Safety assessment per standards', 'Detection/alerting strategy', 'Failure management'],
            selfDeclared: false,
            thirdParty: false,
            standards: ['Eurocae ED-280'],
            failureDetection: 'Detection strategy includes pre-flight checks'
          },
          'H': {
            requiredDocuments: ['FHA', 'Safety assessment', 'SW/AEH development documentation', 'Third-party validation'],
            sections: ['Failure rate objectives', 'Single failure analysis', 'SW/AEH development assurance'],
            selfDeclared: false,
            thirdParty: 'Validation required',
            standards: ['JARUS AMC RPAS.1309 Issue 2', 'DO-178 (Software)', 'DO-254 (Hardware)'],
            failureRates: {
              Major: '< Remote (10^-5/FH)',
              Hazardous: '< Extremely Remote (10^-7/FH)',
              Catastrophic: '< Extremely Improbable (10^-9/FH)'
            },
            singleFailure: 'No single failure → Catastrophic'
          }
        },
        
        // OSO#08 - Operational Procedures
        8: {
          'L': {
            requiredDocuments: ['Operational procedures', 'Checklists', 'Contingency procedures'],
            sections: ['Flight planning', 'Pre/post-flight inspections', 'Normal procedures', 'Contingency procedures', 'Emergency procedures'],
            selfDeclared: true,
            thirdParty: false,
            standards: null
          },
          'M': {
            requiredDocuments: ['Operational procedures', 'Task distribution', 'Internal checklists', 'Emergency Response Plan (ERP)'],
            sections: ['Task assignment', 'Internal checklist', 'ERP', 'External system deterioration procedures'],
            selfDeclared: false,
            thirdParty: false,
            standards: ['Adequate standard per competent authority']
          },
          'H': {
            requiredDocuments: ['Operational procedures', 'CRM training', 'Flight tests', 'Validation reports'],
            sections: ['CRM training', 'Flight tests (full envelope)', 'Tabletop ERP exercise', 'Third-party validation'],
            selfDeclared: false,
            thirdParty: 'Validation by competent third party',
            standards: ['Industry standard for procedures'],
            flightTests: 'Cover complete flight envelope or proven conservative'
          }
        },
        
        // OSO#09 - Remote Crew Training
        9: {
          'L': {
            requiredDocuments: ['Training syllabus', 'Self-declaration', 'Training evidence'],
            sections: ['UAS regulation', 'Airspace principles', 'Meteorology', 'Navigation', 'Operational procedures'],
            selfDeclared: true,
            thirdParty: false,
            standards: ['JARUS RPC recommendations']
          },
          'M': {
            requiredDocuments: ['Training syllabus', 'Theoretical training evidence', 'Practical training evidence'],
            sections: ['Theoretical training', 'Practical training', 'Proficiency requirements', 'Recurrence training'],
            selfDeclared: false,
            thirdParty: false,
            standards: ['JARUS RPC recommendations']
          },
          'H': {
            requiredDocuments: ['Validated training syllabus', 'Competency verification', 'Third-party assessment'],
            sections: ['Training syllabus validation', 'Competency verification', 'Third-party assessment'],
            selfDeclared: false,
            thirdParty: 'Validates syllabus + verifies competencies',
            standards: ['JARUS RPC recommendations', 'Annex H Service Levels']
          }
        }
      };

      const template = templates[osoId];
      if (!template) {
        return {
          requiredDocuments: ['Contact competent authority for guidance'],
          sections: [],
          selfDeclared: false,
          thirdParty: false,
          standards: null
        };
      }

      return template[robustness] || template['L'];
    },

    /**
     * Algorithm 4: Validate cross-OSO rules
     * @param {Map} selectedOSOs - Selected OSOs
     * @param {string} sail - SAIL level
     * @param {string} soraVersion - 'SORA-2.0' or 'SORA-2.5'
     * @param {Object} context - Operation context (operationType, environment, etc.)
     * @returns {Object} { errors: [], warnings: [] }
     */
    validateCrossOSO: function(selectedOSOs, sail, soraVersion, context = {}) {
      const errors = [];
      const warnings = [];
      const sailNum = ['I', 'II', 'III', 'IV', 'V', 'VI'].indexOf(sail) + 1;

      // Rule 1: OSO#05 + Containment (SORA 2.5 only)
      if (soraVersion === 'SORA-2.5' && selectedOSOs.has(5)) {
        if (!context.containmentSelected) {
          errors.push({
            rule: 'OSO#05_CONTAINMENT',
            osoId: 5,
            message: 'OSO#05 requires Containment (Step #8) - see Annex E Section (a)',
            severity: 'ERROR',
            reference: 'SORA 2.5 Annex E OSO#05(a)'
          });
        }
        
        if (context.environment === 'Urban' && !context.m2Selected) {
          errors.push({
            rule: 'OSO#05_M2_URBAN',
            osoId: 5,
            message: 'OSO#05 in Urban areas requires M2 mitigation',
            severity: 'ERROR',
            reference: 'SORA 2.5 Annex E OSO#05(a)'
          });
        }
      }

      // Rule 2: BVLOS Minimum Robustness
      if (context.operationType === 'BVLOS') {
        if (soraVersion === 'SORA-2.0') {
          const oso10 = selectedOSOs.get(10);
          if (!oso10) {
            warnings.push({
              rule: 'BVLOS_RECOVERY',
              osoId: 10,
              message: 'BVLOS operations strongly recommend OSO#10 (Safe recovery)',
              severity: 'WARNING'
            });
          } else if (oso10.robustness === 'L' && sailNum >= 3) {
            errors.push({
              rule: 'BVLOS_OSO10_ROBUSTNESS',
              osoId: 10,
              message: 'BVLOS requires OSO#10 (Safe recovery) with robustness >= M for SAIL III+',
              severity: 'ERROR',
              reference: 'SORA 2.0 Table 6'
            });
          }
        } else {
          // SORA 2.5: OSO#05 includes recovery
          const oso5 = selectedOSOs.get(5);
          if (oso5 && oso5.robustness === 'L' && sailNum >= 4) {
            errors.push({
              rule: 'BVLOS_OSO05_ROBUSTNESS',
              osoId: 5,
              message: 'BVLOS requires OSO#05 (System safety) with robustness >= M for SAIL IV+',
              severity: 'ERROR',
              reference: 'SORA 2.5 Table 14'
            });
          }
        }

        // C3 Link check
        const oso6 = selectedOSOs.get(6);
        if (!oso6) {
          errors.push({
            rule: 'BVLOS_C3_REQUIRED',
            osoId: 6,
            message: 'BVLOS operations require OSO#06 (C3 link)',
            severity: 'ERROR'
          });
        } else if (oso6.robustness === 'L' && sailNum >= 4) {
          errors.push({
            rule: 'BVLOS_C3_ROBUSTNESS',
            osoId: 6,
            message: 'BVLOS requires OSO#06 with robustness >= M for SAIL IV+',
            severity: 'ERROR'
          });
        }
      }

      // Rule 3: Training + External Services
      if (selectedOSOs.has(9) && context.usesExternalServices) {
        if (!selectedOSOs.has(13)) {
          warnings.push({
            rule: 'TRAINING_EXTERNAL_SERVICES',
            osoId: 9,
            message: 'OSO#09 (Crew training) selected but OSO#13 (External services) missing - training may be incomplete',
            severity: 'WARNING',
            reference: 'Annex E OSO#09'
          });
        }
      }

      // Rule 4: Evidence Provider Check (from Table 14)
      if (selectedOSOs.has(4) && !context.designerEvidence) {
        warnings.push({
          rule: 'DESIGNER_EVIDENCE_REQUIRED',
          osoId: 4,
          message: 'OSO#04 requires Designer evidence - ensure UAS manufacturer provides ADS documentation',
          severity: 'WARNING',
          reference: 'SORA 2.5 Table 14'
        });
      }

      // Rule 5: High SAIL requirements
      if (sailNum >= 5) {
        [2, 4, 5].forEach(osoId => {
          const oso = selectedOSOs.get(osoId);
          if (oso && oso.robustness !== 'H') {
            errors.push({
              rule: 'HIGH_SAIL_ROBUSTNESS',
              osoId: osoId,
              message: `SAIL ${sail} requires OSO#${String(osoId).padStart(2, '0')} with High robustness`,
              severity: 'ERROR',
              reference: `SORA ${soraVersion} Table ${soraVersion === 'SORA-2.0' ? '6' : '14'}`
            });
          }
        });
      }

      console.log(`[Cross-OSO Validation] Found ${errors.length} errors, ${warnings.length} warnings`);
      
      return { errors, warnings };
    },

    /**
     * Algorithm 5: Filter OSOs by SAIL
     * @param {string} sail - SAIL level
     * @param {string} soraVersion - 'SORA-2.0' or 'SORA-2.5'
     * @returns {Object} { required: [], optional: [], notRequired: [] }
     */
    filterOSOsBySAIL: function(sail, soraVersion) {
      const required = [];
      const optional = [];
      const notRequired = [];
      
      const allOSOs = this.getAllOSOs(soraVersion);
      
      Object.values(allOSOs).forEach(oso => {
        const reqLevel = oso.sailRequirements[sail];
        
        if (!reqLevel || reqLevel === 'NR' || reqLevel === 'O') {
          notRequired.push({
            id: oso.id,
            number: oso.number,
            name: oso.name,
            category: oso.category,
            reason: reqLevel === 'O' ? 'Optional for this SAIL' : 'Not required for this SAIL'
          });
        } else {
          required.push({
            id: oso.id,
            number: oso.number,
            name: oso.name,
            category: oso.category,
            minRobustness: reqLevel,
            required: true
          });
        }
      });

      console.log(`[SAIL Filtering] SAIL ${sail}: ${required.length} required, ${optional.length} optional, ${notRequired.length} not required`);
      
      return { required, optional, notRequired };
    },

    /**
     * Get dependency definitions
     */
    getDependencies: function(soraVersion) {
      return soraVersion === 'SORA-2.0' ? SORA_20_DEPENDENCIES : SORA_25_DEPENDENCIES;
    },

    /**
     * Get evidence provider requirements
     */
    getEvidenceProviders: function() {
      return EVIDENCE_PROVIDERS;
    }
  };
})();

// Make globally available
window.OSOManager = OSOManager;
console.log('[OSO Manager v2.0] ✅ Loaded - Authoritative JARUS SORA 2.0 + 2.5 Framework');
console.log(`[OSO Manager v2.0] 📋 SORA 2.0: ${OSOManager.getOSOCount('SORA-2.0')} OSOs | SORA 2.5: ${OSOManager.getOSOCount('SORA-2.5')} OSOs`);
console.log('[OSO Manager v2.0] 🧠 Step 42 Algorithms: Dependency Resolution, Auto-Recommendations, Evidence Templates, Cross-OSO Validation, SAIL Filtering');
