#!/usr/bin/env node

/**
 * SKYWORKS SORA MCP Server
 * 
 * Provides instant expert knowledge on:
 * - JARUS SORA 2.0 & 2.5 methodology
 * - Ground Risk (GRC) & Air Risk (ARC) calculations
 * - SAIL determination & OSO requirements
 * - M1/M2/M3 mitigations & floor rules
 * - Operations Manual structure (PDRA/STS)
 * - All 23 EASA/JARUS regulatory documents
 * 
 * Usage in VS Code settings.json:
 * {
 *   "mcp.servers": {
 *     "skyworks-sora": {
 *       "command": "node",
 *       "args": ["C:/Users/chrmc/Desktop/SKYWORKS_AI_SUITE.V5/skyworks-sora-mcp-server/build/index.js"]
 *     }
 *   }
 * }
 */

import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
  ListResourcesRequestSchema,
  ReadResourceRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";
import { readFileSync, writeFileSync, existsSync } from "fs";
import { fileURLToPath } from "url";
import { dirname, join } from "path";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

// Path to knowledge base (relative to build directory)
const KB_PATH = join(__dirname, "../../KnowledgeBase/EASA DOCS SPLIT CHUNKS");
const DOCS_PATH = join(__dirname, "../../Docs/Knowledge");
const STATUS_JSON = join(DOCS_PATH, "PROJECT_STATUS.json");
const PHASES_MD = join(DOCS_PATH, "PROJECT_PHASES_12.md");
const CORPUS_INDEX = join(DOCS_PATH, "CORPUS_INDEX.md");
const EVIDENCE_POLICY = join(DOCS_PATH, "EVIDENCE_POLICY.md");

// Pre-loaded SORA tables and formulas
const SORA_KNOWLEDGE = {
  // SORA 2.0 GRC Table (Table 2)
  grcTable20: {
    vlos_controlled: [1, 2, 3, 4],
    vlos_sparsely: [2, 3, 4, 5],
    bvlos_sparsely: [3, 4, 5, 6],
    vlos_populated: [4, 5, 6, 8],
    bvlos_populated: [5, 6, 8, 10],
    vlos_gathering: [7, null, null, null],
    bvlos_gathering: [8, null, null, null],
  },

  // SORA 2.5 iGRC Table (Table 2 - New 7×5 Matrix)
  grcTable25: {
    controlled: [1, 1, 2, 3, 4],
    remote: [1, 2, 3, 4, 5],
    lightly_populated: [2, 3, 4, 5, 6],
    sparsely_populated: [3, 4, 5, 6, 7],
    suburban: [4, 5, 6, 7, 8],
    high_density: [5, 6, 7, 8, 9],
    assemblies: [6, 7, 8, 9, 10],
  },

  // M1/M2/M3 Mitigation Rules (SORA 2.0)
  mitigations20: {
    M1: { none: 0, low: -1, medium: -2, high: -4 },
    M2: { none: 0, medium: -1, high: -2 },
    M3: { low: 1, medium: 0, high: -1 },
  },

  // M1A/M1B/M1C/M2 Rules (SORA 2.5)
  mitigations25: {
    M1A: { none: 0, low: -2 }, // Sheltering
    M1B: { none: 0, medium: -2, high: -4 }, // Operational restrictions
    M1C: { none: 0, low: -2 }, // Ground observation
    M2: { none: 0, medium: -1, high: -2 },
  },

  // SAIL Matrix (Table 5 - SORA 2.0, Table 7 - SORA 2.5)
  sailMatrix: {
    // [GRC][ARC] where ARC: a=0, b=1, c=2, d=3
    "0-2": [1, 2, 4, 6],
    "3": [2, 2, 4, 6],
    "4": [3, 3, 4, 6],
    "5": [4, 4, 4, 6],
    "6": [5, 5, 5, 6],
    "7": [6, 6, 6, 6],
  },

  // OSO Requirements (17 OSOs for SORA 2.5)
  oso: {
    1: "Operator competency & proven track record",
    2: "UAS manufacturer competency",
    3: "UAS maintenance by competent entity",
    4: "UAS design to ADS (Airworthiness Design Standard)",
    5: "System safety & reliability design",
    6: "C3 link performance appropriate",
    7: "UAS configuration inspection (product inspection)",
    8: "Operational procedures defined, validated, adhered",
    9: "Remote crew trained & current",
    10: "Safe recovery from technical issues",
    11: "Deterioration of external systems - procedures",
    12: "Deterioration of external systems - UAS design",
    13: "External services adequate",
    14: "Operational procedures (human error mitigation)",
    15: "Remote crew training (human error mitigation)",
    16: "Multi-crew coordination",
    17: "Remote crew fitness",
    18: "Automatic flight envelope protection",
    19: "Safe recovery from human error",
    20: "Human factors evaluation & HMI",
    21: "Operational procedures (adverse conditions)",
    22: "Remote crew trained for adverse conditions",
    23: "Environmental conditions defined & measurable",
    24: "UAS designed for adverse environmental conditions",
  },
};

const server = new Server(
  {
    name: "skyworks-sora-mcp-server",
    version: "1.0.0",
  },
  {
    capabilities: {
      tools: {},
      resources: {},
    },
  }
);

// TOOLS
server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [
      {
        name: "get_grc_table",
        description:
          "Get the Ground Risk Class (GRC) table for SORA 2.0 or 2.5. Returns the intrinsic GRC based on operational scenario and UA size.",
        inputSchema: {
          type: "object",
          properties: {
            version: {
              type: "string",
              enum: ["2.0", "2.5"],
              description: "SORA version (2.0 or 2.5)",
            },
            scenario: {
              type: "string",
              description:
                "Operational scenario (e.g., 'vlos_controlled', 'bvlos_populated', 'sparsely_populated')",
            },
            ua_size_column: {
              type: "number",
              description:
                "UA size column index (0=≤1m/<700J, 1=≤3m/<34kJ, 2=≤8m/<1084kJ, 3=>8m/>1084kJ)",
            },
          },
          required: ["version"],
        },
      },
      {
        name: "get_project_status",
        description:
          "Return the current project phase/step and the immediate next step. Reads Docs/Knowledge/PROJECT_STATUS.json.",
        inputSchema: {
          type: "object",
          properties: {},
        },
      },
      {
        name: "update_project_status",
        description:
          "Update current project status (phase/step/notes). Persists to Docs/Knowledge/PROJECT_STATUS.json.",
        inputSchema: {
          type: "object",
          properties: {
            currentPhase: { type: "number" },
            currentStep: { type: "number" },
            nextStep: { type: "number" },
            phaseTitle: { type: "string" },
            notes: { type: "string" },
            updatedBy: { type: "string" }
          },
          required: ["currentPhase", "currentStep"],
        },
      },
      {
        name: "calculate_sail",
        description:
          "Calculate the Specific Assurance Integrity Level (SAIL) based on final GRC and residual ARC.",
        inputSchema: {
          type: "object",
          properties: {
            final_grc: {
              type: "number",
              description: "Final Ground Risk Class (after mitigations)",
            },
            residual_arc: {
              type: "string",
              enum: ["a", "b", "c", "d"],
              description: "Residual Air Risk Class",
            },
          },
          required: ["final_grc", "residual_arc"],
        },
      },
      {
        name: "apply_mitigation",
        description:
          "Apply M1/M2/M3 mitigations to intrinsic GRC and return final GRC with floor rule validation.",
        inputSchema: {
          type: "object",
          properties: {
            version: {
              type: "string",
              enum: ["2.0", "2.5"],
              description: "SORA version",
            },
            intrinsic_grc: {
              type: "number",
              description: "Intrinsic GRC before mitigations",
            },
            m1_level: {
              type: "string",
              description:
                "M1 robustness level (SORA 2.0: none/low/medium/high, SORA 2.5 M1B: none/medium/high)",
            },
            m1a_level: {
              type: "string",
              description: "M1A (sheltering) level for SORA 2.5 (none/low)",
            },
            m1c_level: {
              type: "string",
              description:
                "M1C (ground observation) level for SORA 2.5 (none/low)",
            },
            m2_level: {
              type: "string",
              description: "M2 robustness level (none/medium/high)",
            },
            m3_level: {
              type: "string",
              description: "M3 (ERP) robustness level (low/medium/high)",
            },
            scenario_column: {
              type: "string",
              description:
                "Operational scenario to determine floor (e.g., 'vlos_controlled')",
            },
            ua_size_column: {
              type: "number",
              description: "UA size column index (0-3)",
            },
          },
          required: ["version", "intrinsic_grc"],
        },
      },
      {
        name: "get_oso_requirements",
        description:
          "Get the required Operational Safety Objectives (OSOs) and their robustness levels for a given SAIL.",
        inputSchema: {
          type: "object",
          properties: {
            sail: {
              type: "number",
              description: "SAIL level (I=1, II=2, III=3, IV=4, V=5, VI=6)",
            },
          },
          required: ["sail"],
        },
      },
      {
        name: "search_sora_docs",
        description:
          "Search all 23 EASA/JARUS SORA documents for specific topics (e.g., containment, TMPR, DAA, OSO details).",
        inputSchema: {
          type: "object",
          properties: {
            query: {
              type: "string",
              description:
                "Search query (e.g., 'M1 mitigation sheltering', 'containment requirements', 'TMPR robustness')",
            },
            document: {
              type: "string",
              description:
                "Optional: specific document to search (e.g., 'SORA-v2.5-Main-Body', 'Annex-B', 'Annex-E')",
            },
          },
          required: ["query"],
        },
      },
      {
        name: "get_operations_manual_structure",
        description:
          "Get the recommended Operations Manual structure (Part A-T) with compliance matrix guidance.",
        inputSchema: {
          type: "object",
          properties: {
            part: {
              type: "string",
              description:
                "Optional: specific part to retrieve (A=General, B=Procedures, C=Flight Areas, D=Training, E=ERP, M=Maintenance, T=Technical)",
            },
          },
        },
      },
      {
        name: "validate_floor_rule",
        description:
          "Validate that the floor rule is correctly applied: Final GRC cannot be lower than the column minimum from Table 2.",
        inputSchema: {
          type: "object",
          properties: {
            version: {
              type: "string",
              enum: ["2.0", "2.5"],
              description: "SORA version",
            },
            final_grc: {
              type: "number",
              description: "Calculated final GRC after mitigations",
            },
            scenario: {
              type: "string",
              description: "Operational scenario",
            },
            ua_size_column: {
              type: "number",
              description: "UA size column index (0-3)",
            },
          },
          required: ["version", "final_grc", "scenario", "ua_size_column"],
        },
      },
      {
        name: "knowledge_check",
        description:
          "Verify that the SKYWORKS knowledge corpus is present (all 23 EASA/JARUS items) and key quick-reference resources are available. Returns a READY status and guidance.",
        inputSchema: { type: "object", properties: {} },
      },
    ],
  };
});

server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  switch (name) {
    case "get_grc_table": {
      const { version, scenario, ua_size_column } = args as any;
      const table =
        version === "2.5" ? SORA_KNOWLEDGE.grcTable25 : SORA_KNOWLEDGE.grcTable20;

      if (scenario && ua_size_column !== undefined) {
        const grc = table[scenario as keyof typeof table]?.[ua_size_column];
        return {
          content: [
            {
              type: "text",
              text: `SORA ${version} - ${scenario} - Size Column ${ua_size_column}: GRC = ${grc ?? "N/A"}`,
            },
          ],
        };
      }

      return {
        content: [
          {
            type: "text",
            text: `SORA ${version} GRC Table:\n${JSON.stringify(table, null, 2)}`,
          },
        ],
      };
    }

    case "calculate_sail": {
      const { final_grc, residual_arc } = args as any;
      const arcMap: Record<string, number> = { a: 0, b: 1, c: 2, d: 3 };
      const arcIndex = arcMap[residual_arc as string];

      let grcKey = final_grc <= 2 ? "0-2" : final_grc.toString();
      if (final_grc > 7) grcKey = ">7";

      const sailRow = SORA_KNOWLEDGE.sailMatrix[grcKey as keyof typeof SORA_KNOWLEDGE.sailMatrix];
      const sail = sailRow?.[arcIndex] ?? "Category C";

      return {
        content: [
          {
            type: "text",
            text: `GRC ${final_grc} + ARC-${residual_arc} → SAIL ${sail}\n\nSAIL Matrix Reference:\nGRC ≤2: [I, II, IV, VI]\nGRC 3: [II, II, IV, VI]\nGRC 4: [III, III, IV, VI]\nGRC 5: [IV, IV, IV, VI]\nGRC 6: [V, V, V, VI]\nGRC 7: [VI, VI, VI, VI]\nGRC >7: Category C operation`,
          },
        ],
      };
    }

    case "apply_mitigation": {
      const {
        version,
        intrinsic_grc,
        m1_level = "none",
        m1a_level = "none",
        m1c_level = "none",
        m2_level = "none",
        m3_level = "medium",
        scenario_column,
        ua_size_column,
      } = args as any;

      let reduction = 0;
      let details: string[] = [];

      if (version === "2.5") {
        // SORA 2.5: M1A + M1B + M1C
        const m1a = SORA_KNOWLEDGE.mitigations25.M1A[m1a_level as keyof typeof SORA_KNOWLEDGE.mitigations25.M1A] ?? 0;
        const m1b = SORA_KNOWLEDGE.mitigations25.M1B[m1_level as keyof typeof SORA_KNOWLEDGE.mitigations25.M1B] ?? 0;
        const m1c = SORA_KNOWLEDGE.mitigations25.M1C[m1c_level as keyof typeof SORA_KNOWLEDGE.mitigations25.M1C] ?? 0;
        const m2 = SORA_KNOWLEDGE.mitigations25.M2[m2_level as keyof typeof SORA_KNOWLEDGE.mitigations25.M2] ?? 0;

        reduction = m1a + m1b + m1c + m2;
        details.push(`M1A (${m1a_level}): ${m1a}`);
        details.push(`M1B (${m1_level}): ${m1b}`);
        details.push(`M1C (${m1c_level}): ${m1c}`);
        details.push(`M2 (${m2_level}): ${m2}`);
      } else {
        // SORA 2.0: M1 + M2 + M3
        const m1 = SORA_KNOWLEDGE.mitigations20.M1[m1_level as keyof typeof SORA_KNOWLEDGE.mitigations20.M1] ?? 0;
        const m2 = SORA_KNOWLEDGE.mitigations20.M2[m2_level as keyof typeof SORA_KNOWLEDGE.mitigations20.M2] ?? 0;
        const m3 = SORA_KNOWLEDGE.mitigations20.M3[m3_level as keyof typeof SORA_KNOWLEDGE.mitigations20.M3] ?? 0;

        reduction = m1 + m2 + m3;
        details.push(`M1 (${m1_level}): ${m1}`);
        details.push(`M2 (${m2_level}): ${m2}`);
        details.push(`M3 (${m3_level}): ${m3}`);
      }

      let final_grc = intrinsic_grc + reduction;

      // Floor rule validation
      if (scenario_column && ua_size_column !== undefined) {
        const table =
          version === "2.5" ? SORA_KNOWLEDGE.grcTable25 : SORA_KNOWLEDGE.grcTable20;
        const column_values = Object.values(table).map((row: any) => row[ua_size_column]).filter((v: any) => v !== null);
        const floor = Math.min(...column_values);

        if (final_grc < floor) {
          details.push(`⚠️ FLOOR RULE: Final GRC ${final_grc} < Column Floor ${floor} → Adjusted to ${floor}`);
          final_grc = floor;
        }
      }

      return {
        content: [
          {
            type: "text",
            text: `SORA ${version} Mitigation Calculation:\n\nIntrinsic GRC: ${intrinsic_grc}\n${details.join("\n")}\n\nTotal Reduction: ${reduction}\nFinal GRC: ${final_grc}`,
          },
        ],
      };
    }

    case "get_oso_requirements": {
      const { sail } = args as any;

      const osoMatrix = {
        1: { required: [1, 3, 7, 8, 9, 17], levels: "Low (L)" },
        2: { required: [1, 3, 7, 8, 9, 13, 16, 17, 20, 23], levels: "Low-Medium (L-M)" },
        3: { required: [1, 3, 4, 5, 6, 7, 8, 9, 13, 16, 17, 18, 19, 20, 23], levels: "Low-Medium-High" },
        4: { required: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 13, 16, 17, 18, 19, 20, 23, 24], levels: "Medium-High" },
        5: { required: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 16, 17, 18, 19, 20, 22, 23, 24], levels: "High" },
        6: { required: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 16, 17, 18, 19, 20, 21, 22, 23, 24], levels: "High" },
      };

      const requirements = osoMatrix[sail as keyof typeof osoMatrix];

      if (!requirements) {
        return {
          content: [{ type: "text", text: `Invalid SAIL: ${sail}. Must be 1-6.` }],
        };
      }

      const osoList = requirements.required
        .map((num) => `OSO #${num}: ${SORA_KNOWLEDGE.oso[num as keyof typeof SORA_KNOWLEDGE.oso]}`)
        .join("\n");

      return {
        content: [
          {
            type: "text",
            text: `SAIL ${sail} Requirements:\n\nRequired OSOs:\n${osoList}\n\nTypical Robustness Levels: ${requirements.levels}`,
          },
        ],
      };
    }

    case "search_sora_docs": {
      const { query } = args as any;
      return {
        content: [
          {
            type: "text",
            text: `Searching SORA documentation for: "${query}"\n\n[Note: Full-text search implementation requires file system access to ${KB_PATH}]\n\nRecommended documents:\n- SORA v2.5 Main Body\n- Annex B (M1 mitigations)\n- Annex E (OSO criteria)\n- Operations Manual Structure (Annex A)`,
          },
        ],
      };
    }

    case "get_operations_manual_structure": {
      const structure = `Operations Manual Structure (SORA 2.5 Annex A):

Part A - General (Organization, Change Management, Personnel)
Part B - Procedures (Multi-crew, Flight Planning, Weather, TMPR, Contingency)
Part C - Flight Areas (CV/GRB calculations, ERP local info)
Part D - Training
Part E - Emergency Response Plan (ERP)
Part M - Maintenance
Part T - Technical (UAS description, C3, M2 parachute, TMPR, Containment, HMI)

Annex 8.1 - Evidence
  8.1.1 - Organizational (Operating Certificate, Maintenance Cert)
  8.1.2 - Operational (ATC agreements, M1 proof, Flight tests)
  8.1.3 - Technical (DVR/TC, M2 parachute cert, Manufacturer competence)

Annex 8.2 - Printed Forms (Logbooks, Personnel lists, Checklists)
Annex 8.3 - Check Lists (ERP, Pre-flight, Post-flight)
Annex 8.4 - Manuals (Maintenance manuals for each UAS)`;

      return {
        content: [{ type: "text", text: structure }],
      };
    }

    case "validate_floor_rule": {
      const { version, final_grc, scenario, ua_size_column } = args as any;
      const table =
        version === "2.5" ? SORA_KNOWLEDGE.grcTable25 : SORA_KNOWLEDGE.grcTable20;
      const column_values = Object.values(table)
        .map((row: any) => row[ua_size_column])
        .filter((v: any) => v !== null);
      const floor = Math.min(...column_values);

      const isValid = final_grc >= floor;

      return {
        content: [
          {
            type: "text",
            text: `Floor Rule Validation (SORA ${version}):\n\nScenario: ${scenario}\nUA Size Column: ${ua_size_column}\nColumn Floor: ${floor}\nFinal GRC: ${final_grc}\n\nResult: ${isValid ? "✅ VALID" : `❌ INVALID - Must be ≥ ${floor}`}`,
          },
        ],
      };
    }

    case "knowledge_check": {
      try {
        let corpusCount = 0;
        let corpusSample = "";
        try {
          const idx = readFileSync(CORPUS_INDEX, "utf-8");
          const lines = idx.split(/\r?\n/).filter((l) => l.trim().startsWith("- "));
          corpusCount = lines.length;
          corpusSample = lines.slice(0, 10).join("\n");
        } catch {}

        const requiredFiles = [
          join(DOCS_PATH, "SORA_2_0_TABLES_REFERENCE.md"),
          join(DOCS_PATH, "SORA_2_5_TABLES_REFERENCE.md"),
          join(DOCS_PATH, "OPERATIONS_MANUAL_STRUCTURE.md"),
          join(DOCS_PATH, "AIR_RISK_ARC_TMPR_REFERENCE.md"),
        ];
        const missing = requiredFiles.filter((p) => {
          try { readFileSync(p, "utf-8"); return false; } catch { return true; }
        });

        const ready = corpusCount >= 23 && missing.length === 0;
        const token = ready ? `READY-${corpusCount}-DOCS` : `PARTIAL-${corpusCount}-DOCS`;

        const guidance = `Policy: Always cite EASA/JARUS. For each assertion, include document name and section/page. If uncertain, call search_sora_docs and do not proceed without a citation. Optional resource: skyworks://policy/evidence-policy`;

        return {
          content: [
            {
              type: "text",
              text: `Knowledge Check → ${ready ? "✅ READY" : "⚠️ PARTIAL"}\nDocs counted (from CORPUS_INDEX.md): ${corpusCount}${corpusSample ? "\n\nSample:\n" + corpusSample : ""}\nMissing quick refs: ${missing.length ? missing.join(", ") : "none"}\nToken: ${token}\n\n${guidance}`,
            },
          ],
        };
      } catch (e) {
        return { content: [{ type: "text", text: `Error in knowledge_check: ${e}` }], isError: true };
      }
    }

    case "get_project_status": {
      try {
        if (existsSync(STATUS_JSON)) {
          const raw = readFileSync(STATUS_JSON, "utf-8");
          const data = JSON.parse(raw);
          const { currentPhase, phaseTitle, currentStep, nextStep, updatedAt, notes, pendingJobs } = data;
          
          let statusText = `Τρέχουσα κατάσταση έργου:\nΦάση: ${currentPhase} — ${phaseTitle ?? ""}\nΒήμα: ${currentStep}\nΕπόμενο: ${nextStep ?? currentStep + 1}\nΤελευταία ενημέρωση: ${updatedAt ?? "n/a"}\n\nΣημειώσεις: ${notes ?? "-"}`;
          
          // Add pending jobs if they exist
          if (pendingJobs && Object.keys(pendingJobs).length > 0) {
            statusText += "\n\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n📋 PENDING JOBS:\n";
            for (const [jobKey, job] of Object.entries(pendingJobs as any)) {
              const jobData = job as any;
              statusText += `\n🔸 ${jobKey.toUpperCase()}\n`;
              statusText += `   Priority: ${jobData.priority ?? 'N/A'} | Status: ${jobData.status ?? 'N/A'}\n`;
              statusText += `   Progress: ${jobData.completedItems ?? 0}/${jobData.totalItems ?? 0} (${jobData.completionPercent ?? 0}%)\n`;
              statusText += `   ${jobData.description ?? 'No description'}\n`;
              if (jobData.remainingTasks && Array.isArray(jobData.remainingTasks) && jobData.remainingTasks.length > 0) {
                statusText += `   Remaining:\n`;
                jobData.remainingTasks.forEach((task: any, idx: number) => {
                  statusText += `   ${idx + 1}. ${task.task ?? 'Task'} (${task.estimatedTime ?? 'N/A'})\n`;
                  statusText += `      ${task.detail ?? ''}\n`;
                });
              }
            }
          }
          
          return {
            content: [
              {
                type: "text",
                text: statusText,
              },
            ],
          };
        }
        // Fallback summary using phases markdown
        const md = readFileSync(PHASES_MD, "utf-8");
        return { content: [{ type: "text", text: md.slice(0, 1000) + "\n..." }] };
      } catch (e) {
        return { content: [{ type: "text", text: `Error reading status: ${e}` }], isError: true };
      }
    }

    case "update_project_status": {
      try {
        const { currentPhase, currentStep, nextStep, phaseTitle, notes, updatedBy } = (args as any) ?? {};
        const payload = {
          currentPhase,
          phaseTitle: phaseTitle ?? "",
          currentStep,
          nextStep: nextStep ?? (currentStep + 1),
          updatedAt: new Date().toISOString(),
          updatedBy: updatedBy ?? "mcp",
          notes: notes ?? ""
        };
        writeFileSync(STATUS_JSON, JSON.stringify(payload, null, 2));
        return { content: [{ type: "text", text: `✅ Project status updated: Phase ${payload.currentPhase}, Step ${payload.currentStep} → Next ${payload.nextStep}` }] };
      } catch (e) {
        return { content: [{ type: "text", text: `Error updating status: ${e}` }], isError: true };
      }
    }

    default:
      return {
        content: [{ type: "text", text: `Unknown tool: ${name}` }],
        isError: true,
      };
  }
});

// RESOURCES
server.setRequestHandler(ListResourcesRequestSchema, async () => {
  return {
    resources: [
      {
        uri: "skyworks://knowledge/sora-2.0-tables",
        name: "SORA 2.0 Tables Reference",
        mimeType: "text/markdown",
        description: "Quick reference for SORA 2.0 tables, formulas, and methodology",
      },
      {
        uri: "skyworks://knowledge/sora-2.5-tables",
        name: "SORA 2.5 Tables Reference",
        mimeType: "text/markdown",
        description: "Quick reference for SORA 2.5 tables, new iGRC matrix, M1A/B/C split",
      },
      {
        uri: "skyworks://knowledge/operations-manual",
        name: "Operations Manual Structure",
        mimeType: "text/markdown",
        description: "SORA 2.5 Annex A operations manual structure (Part A-T)",
      },
      {
        uri: "skyworks://knowledge/air-risk-arc-tmpr",
        name: "Air Risk (ARC/TMPR) Reference",
        mimeType: "text/markdown",
        description: "ARC determination, TMPR requirements, DAA systems",
      },
      {
        uri: "skyworks://knowledge/project-status",
        name: "Project Status (Phase/Step)",
        mimeType: "application/json",
        description: "Current phase/step and next action for SKYWORKS project",
      },
      {
        uri: "skyworks://knowledge/project-phases",
        name: "12 ΦΑΣΕΙΣ ΠΛΗΡΟΥΣ ΥΛΟΠΟΙΗΣΗΣ",
        mimeType: "text/markdown",
        description: "Full plan with phases and steps",
      },
      {
        uri: "skyworks://policy/evidence-policy",
        name: "Evidence and Citations Policy",
        mimeType: "text/markdown",
        description: "Rules: Always cite EASA/JARUS sources; checklist and examples",
      },
    ],
  };
});

server.setRequestHandler(ReadResourceRequestSchema, async (request) => {
  const uri = request.params.uri;

  const resourceMap: Record<string, string> = {
    "skyworks://knowledge/sora-2.0-tables": join(DOCS_PATH, "SORA_2_0_TABLES_REFERENCE.md"),
    "skyworks://knowledge/sora-2.5-tables": join(DOCS_PATH, "SORA_2_5_TABLES_REFERENCE.md"),
    "skyworks://knowledge/operations-manual": join(DOCS_PATH, "OPERATIONS_MANUAL_STRUCTURE.md"),
    "skyworks://knowledge/air-risk-arc-tmpr": join(DOCS_PATH, "AIR_RISK_ARC_TMPR_REFERENCE.md"),
    "skyworks://knowledge/project-status": STATUS_JSON,
    "skyworks://knowledge/project-phases": PHASES_MD,
    "skyworks://policy/evidence-policy": EVIDENCE_POLICY,
  };

  const filePath = resourceMap[uri];

  if (!filePath) {
    return {
      contents: [
        {
          uri,
          mimeType: "text/plain",
          text: `Resource not found: ${uri}`,
        },
      ],
    };
  }

  try {
    const isJson = filePath.endsWith(".json");
    const content = readFileSync(filePath, "utf-8");
    return {
      contents: [
        {
          uri,
          mimeType: isJson ? "application/json" : "text/markdown",
          text: content,
        },
      ],
    };
  } catch (error) {
    return {
      contents: [
        {
          uri,
          mimeType: "text/plain",
          text: `Error reading resource: ${error}`,
        },
      ],
    };
  }
});

// Start server
async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("SKYWORKS SORA MCP Server running on stdio");
}

main().catch((error) => {
  console.error("Fatal error in main():", error);
  process.exit(1);
});
