/**
 * SKYWORKS Mission Planner - Core JavaScript
 * Handles form submission, validation, API calls, and UI updates
 * 
 * IMPORTANT: Uses soraApi from soraClient.js for all SORA calculations
 * Backend: POST /api/v1/sora/calculate (SoraController.cs)
 * Client: WebPlatform/wwwroot/app/Pages/ui/api/soraClient.js
 */

// Import SORA API client
import { soraApi, buildSora25Request, buildSora20Request } from './api/soraClient.js';

// ═══════════════════════════════════════════════════════════════════
// VALIDATION SCHEMAS (inline from schemas.ts)
// ═══════════════════════════════════════════════════════════════════

const SORA_25_SCHEMAS = {
  m1a: {
    allowed: ["None", "Low"],
    validate: (val) => {
      if (!["None", "Low"].includes(val)) {
        throw new Error(`Invalid M1A: ${val}. Allowed: None, Low (NO MEDIUM per Annex B Tables 2-3)`);
      }
      return val;
    }
  },
  m1b: {
    allowed: ["None", "Medium", "High"],
    validate: (val) => {
      if (!["None", "Medium", "High"].includes(val)) {
        throw new Error(`Invalid M1B: ${val}. Allowed: None, Medium, High`);
      }
      return val;
    }
  },
  m1c: {
    allowed: ["None", "Low"],
    validate: (val) => {
      if (!["None", "Low"].includes(val)) {
        throw new Error(`Invalid M1C: ${val}. Allowed: None, Low`);
      }
      return val;
    }
  },
  m2: {
    allowed: ["None", "Low", "Medium", "High"],
    validate: (val) => {
      if (!["None", "Low", "Medium", "High"].includes(val)) {
        throw new Error(`Invalid M2: ${val}. Allowed: None, Low, Medium, High`);
      }
      return val;
    }
  }
};

const SORA_20_SCHEMAS = {
  m1: {
    allowed: ["None", "Low", "Medium", "High"],
    validate: (val) => {
      if (!["None", "Low", "Medium", "High"].includes(val)) {
        throw new Error(`Invalid M1: ${val}`);
      }
      return val;
    }
  },
  m2: {
    allowed: ["None", "Low", "High"],
    validate: (val) => {
      if (!["None", "Low", "High"].includes(val)) {
        throw new Error(`Invalid M2: ${val}. NO MEDIUM in SORA 2.0 (AMC1 Art.11)`);
      }
      return val;
    }
  },
  m3: {
    allowed: ["None", "Adequate", "Validated"],
    validate: (val) => {
      if (!["None", "Adequate", "Validated"].includes(val)) {
        throw new Error(`Invalid M3: ${val}. Use: None, Adequate, Validated`);
      }
      return val;
    }
  }
};

// ═══════════════════════════════════════════════════════════════════
// GLOBAL STATE
// ═══════════════════════════════════════════════════════════════════

let currentSoraVersion = "2.5";
let lastResponse = null;
let specificationsCache = {
  "2.5": null,
  "2.0": null
};

// ═══════════════════════════════════════════════════════════════════
// SPECIFICATIONS API (populate dropdowns from backend)
// ═══════════════════════════════════════════════════════════════════

/**
 * Load SORA specifications from API and populate dropdowns
 * Uses in-memory cache to avoid redundant API calls
 * 
 * @param {string} version - "2.5" or "2.0"
 */
async function loadSpecifications(version = "2.5") {
  try {
    // Check cache first
    if (specificationsCache[version]) {
      logToConsole(`✅ Using cached specifications for SORA ${version}`, 'info');
      populateDropdowns(specificationsCache[version], version);
      return;
    }
    
    logToConsole(`Loading SORA ${version} specifications from API...`, 'info');
    
    // Call API
    const specs = await soraApi.getSpecifications(version);
    
    // Cache result
    specificationsCache[version] = specs;
    
    // Populate dropdowns
    populateDropdowns(specs, version);
    
    logToConsole(`✅ Loaded SORA ${version} specifications: ${specs.constraints.length} constraints`, 'success');
    
    // Log constraints for validation
    if (specs.constraints && specs.constraints.length > 0) {
      specs.constraints.forEach(c => logToConsole(`  • ${c}`, 'info'));
    }
    
  } catch (error) {
    logToConsole(`❌ Failed to load specifications: ${error.message}`, 'error');
    logToConsole(`Using hardcoded dropdown values as fallback`, 'warning');
  }
}

/**
 * Populate dropdown options from specifications
 * 
 * @param {Object} specs - Specifications response from API
 * @param {string} version - "2.5" or "2.0"
 */
function populateDropdowns(specs, version) {
  if (version === "2.5") {
    // SORA 2.5 dropdowns
    if (specs.m1aOptions) {
      populateSelect('m1a', specs.m1aOptions);
    }
    if (specs.m1bOptions) {
      populateSelect('m1b', specs.m1bOptions);
    }
    if (specs.m1cOptions) {
      populateSelect('m1c', specs.m1cOptions);
    }
    if (specs.m2Options) {
      populateSelect('m2-25', specs.m2Options);
    }
    if (specs.populationDensityOptions) {
      // Population density is not a direct dropdown in mission.html
      // but could be added in future (for now inferred from operation type)
      logToConsole(`  Population density options: ${specs.populationDensityOptions.join(', ')}`, 'info');
    }
  } else if (version === "2.0") {
    // SORA 2.0 dropdowns
    if (specs.m1Options) {
      populateSelect('m1-20', specs.m1Options);
    }
    if (specs.m2Options) {
      populateSelect('m2-20', specs.m2Options);
    }
    if (specs.m3Options) {
      populateSelect('m3-20', specs.m3Options);
    }
    if (specs.operationScenarioOptions) {
      // Operation scenario is not a direct dropdown in mission.html
      // but could be added in future (for now inferred from operation type)
      logToConsole(`  Operation scenarios: ${specs.operationScenarioOptions.join(', ')}`, 'info');
    }
  }
}

/**
 * Populate a single select element with options
 * 
 * @param {string} selectId - ID of select element
 * @param {string[]} options - Array of option values
 */
function populateSelect(selectId, options) {
  const select = document.getElementById(selectId);
  if (!select) {
    logToConsole(`⚠️ Select element #${selectId} not found`, 'warning');
    return;
  }
  
  // Clear existing options
  select.innerHTML = '';
  
  // Add new options
  options.forEach(opt => {
    const option = document.createElement('option');
    option.value = opt;
    option.textContent = opt;
    select.appendChild(option);
  });
  
  logToConsole(`  Populated #${selectId} with ${options.length} options`, 'info');
}

// ═══════════════════════════════════════════════════════════════════
// SORA VERSION TOGGLE (DEPRECATED - Use toggleFramework instead)
// ═══════════════════════════════════════════════════════════════════

function toggleSoraVersion(version) {
  // Legacy function - redirect to new toggleFramework
  if (version === "2.5") {
    toggleFramework("sora25");
  } else if (version === "2.0") {
    toggleFramework("sora20");
  }
}

// ═══════════════════════════════════════════════════════════════════
// FORM SUBMISSION & PAYLOAD COMPOSITION
// ═══════════════════════════════════════════════════════════════════

async function handleFormSubmit(event) {
  event.preventDefault();
  
  try {
    clearConsole();
    logToConsole('Building SORA request...', 'success');
    
    // Build SORA request using correct DTO structure (matches backend)
    const request = buildSoraRequest();
    
    // Validate request
    logToConsole('Validating request...', 'success');
    validateSoraRequest(request);
    
    // Log request
    logToConsole('REQUEST:', 'success');
    logToConsole(JSON.stringify(request, null, 2), 'success');
    
    // Call SORA API via soraClient
    logToConsole('Calling POST /api/v1/sora/calculate via soraApi...', 'success');
    const response = await soraApi.calculate(request);
    
    // Log response
    logToConsole('RESPONSE:', 'success');
    logToConsole(JSON.stringify(response, null, 2), 'success');
    
    // Update UI badges from API response
    updateLiveBreakdown(response);
    
    lastResponse = response;
    
  } catch (error) {
    logToConsole(`ERROR: ${error.message}`, 'error');
    
    // Fallback to local calculators if API fails (network/5xx errors)
    if (error.message.includes('fetch') || error.message.includes('API error')) {
      logToConsole('⚠️ API failed - attempting local calculator fallback...', 'warning');
      try {
        const fallbackResult = await calculateLocalFallback(buildSoraRequest());
        logToConsole('✅ Local calculator result (offline mode):', 'warning');
        logToConsole(JSON.stringify(fallbackResult, null, 2), 'warning');
        updateLiveBreakdown(fallbackResult);
        lastResponse = fallbackResult;
      } catch (fallbackError) {
        logToConsole(`❌ Local fallback also failed: ${fallbackError.message}`, 'error');
      }
    }
  }
}

/**
 * Build SORA request matching backend DTO structure
 * Backend expects: { soraVersion, drone: { mtom_kg, maxSpeed_ms, characteristicDimension_m }, m1a, m2, ... }
 */
function buildSoraRequest() {
  // Extract drone specs from form (add drone fields to mission.html if missing)
  const droneSpecs = {
    mtom_kg: parseFloat(document.getElementById('drone-mtom')?.value || 0.249), // Default: DJI Mini 4 Pro
    maxSpeed_ms: parseFloat(document.getElementById('drone-speed')?.value || 16),
    characteristicDimension_m: parseFloat(document.getElementById('drone-dimension')?.value || 0.213)
  };
  
  // Common airspace parameters
  const commonParams = {
    altitude_ft: parseInt(document.getElementById('max-height')?.value || 400),
    controlledAirspace: document.getElementById('airspace-class')?.value?.includes('C') || 
                       document.getElementById('airspace-class')?.value?.includes('D') || false,
    airportEnvironment: getSelectedSpecialZones().includes('Airport Environment'),
    populatedArea: document.getElementById('operation-type')?.value?.includes('Populated') || 
                   document.getElementById('operation-type')?.value?.includes('Urban') || false,
    atypicalAirspace: document.getElementById('typicality')?.value === 'Atypical' || false,
    isVLOS: document.getElementById('operation-type')?.value?.includes('VLOS') || true
  };
  
  // Build SORA 2.5 or 2.0 request
  if (currentSoraVersion === "2.5") {
    return buildSora25Request({
      drone: droneSpecs,
      populationDensity: extractPopulationDensity(),
      m1a: document.getElementById('m1a')?.value || 'None',
      m1b: document.getElementById('m1b')?.value || 'None',
      m1c: document.getElementById('m1c')?.value || 'None',
      m2: document.getElementById('m2-25')?.value || 'None',
      ...commonParams
    });
  } else {
    return buildSora20Request({
      drone: droneSpecs,
      operationScenario: extractOperationScenario(),
      m1: document.getElementById('m1-20')?.value || 'None',
      m2_20: document.getElementById('m2-20')?.value || 'None',
      m3: document.getElementById('m3-20')?.value || 'None',
      ...commonParams
    });
  }
}

/**
 * Extract population density from form (SORA 2.5)
 * Map "Urban", "Suburban", "Rural" to official categories
 */
function extractPopulationDensity() {
  const operationType = document.getElementById('operation-type')?.value || '';
  const typicality = document.getElementById('typicality')?.value || '';
  
  // If explicit population density field exists, use it
  const densityField = document.getElementById('population-density');
  if (densityField && densityField.value) {
    return densityField.value;
  }
  
  // Otherwise infer from operation type
  if (typicality === 'Controlled') return 'Controlled';
  if (operationType.includes('Urban')) return '<5000';
  if (operationType.includes('Suburban')) return '<500';
  if (operationType.includes('Rural')) return '<50';
  if (operationType.includes('Sparsely')) return '<5';
  
  return '<500'; // Default: Suburban/Sparsely populated
}

/**
 * Extract operation scenario (SORA 2.0)
 * Map form fields to official scenarios
 */
function extractOperationScenario() {
  const operationType = document.getElementById('operation-type')?.value || '';
  const typicality = document.getElementById('typicality')?.value || '';
  const isVLOS = operationType.includes('VLOS') || !operationType.includes('BVLOS');
  
  // If explicit scenario field exists, use it
  const scenarioField = document.getElementById('operation-scenario');
  if (scenarioField && scenarioField.value) {
    return scenarioField.value;
  }
  
  // Otherwise construct from operation type
  const prefix = isVLOS ? 'VLOS_' : 'BVLOS_';
  
  if (typicality === 'Controlled') return prefix + 'Controlled';
  if (operationType.includes('Urban')) return prefix + 'Populated';
  if (operationType.includes('Suburban')) return prefix + 'Populated';
  if (operationType.includes('Gathering')) return prefix + 'Gathering';
  if (operationType.includes('Rural') || operationType.includes('Sparsely')) return prefix + 'Sparsely';
  
  return prefix + 'Sparsely'; // Default
}

function buildPayload() {
  // DEPRECATED - Use buildSoraRequest() instead
  // Kept for backwards compatibility during migration
  console.warn('buildPayload() is deprecated - use buildSoraRequest() instead');
  return buildSoraRequest();
}

function getSelectedSpecialZones() {
  const checkboxes = document.querySelectorAll('input[name="special-zones"]:checked');
  return Array.from(checkboxes).map(cb => cb.value);
}

function validateSoraRequest(request) {
  const errors = [];
  
  try {
    // Validate drone specs
    if (!request.drone || !request.drone.mtom_kg || !request.drone.maxSpeed_ms) {
      errors.push('Drone specifications missing (mtom_kg, maxSpeed_ms, characteristicDimension_m)');
    }
    
    // Validate SORA version-specific fields
    if (request.soraVersion === "2.5") {
      if (!request.populationDensity) {
        errors.push('Population density required for SORA 2.5');
      }
      SORA_25_SCHEMAS.m1a.validate(request.m1a || 'None');
      SORA_25_SCHEMAS.m1b.validate(request.m1b || 'None');
      SORA_25_SCHEMAS.m1c.validate(request.m1c || 'None');
      SORA_25_SCHEMAS.m2.validate(request.m2 || 'None');
    } else if (request.soraVersion === "2.0") {
      if (!request.operationScenario) {
        errors.push('Operation scenario required for SORA 2.0');
      }
      SORA_20_SCHEMAS.m1.validate(request.m1 || 'None');
      SORA_20_SCHEMAS.m2.validate(request.m2_20 || 'None');
      SORA_20_SCHEMAS.m3.validate(request.m3 || 'None');
    } else {
      errors.push(`Invalid SORA version: ${request.soraVersion}. Must be "2.0" or "2.5"`);
    }
  } catch (err) {
    errors.push(err.message);
  }
  
  if (errors.length > 0) {
    throw new Error(errors.join('\n'));
  }
}

function validatePayload(payload) {
  // DEPRECATED - Use validateSoraRequest() instead
  console.warn('validatePayload() is deprecated - use validateSoraRequest() instead');
  return validateSoraRequest(payload);
}

async function callSoraAPI(payload) {
  // DEPRECATED - Use soraApi.calculate() instead
  console.warn('callSoraAPI() is deprecated - use soraApi.calculate() from soraClient.js');
  
  const response = await fetch('/api/v1/sora/calculate', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(payload)
  });
  
  if (!response.ok) {
    throw new Error(`API Error: ${response.status} ${response.statusText}`);
  }
  
  const data = await response.json();
  
  // Best-effort mapping (in case backend keys differ slightly)
  return {
    initialGrc: data.initialGrc ?? data.initial_grc ?? data.iGRC,
    finalGrc: data.finalGrc ?? data.final_grc ?? data.fGRC,
    arc: {
      initial: data.arc?.initial ?? data.initial_arc ?? data.iARC ?? 'N/A',
      residual: data.arc?.residual ?? data.residual_arc ?? data.rARC ?? 'N/A'
    },
    sail: data.sail ?? data.SAIL ?? 'N/A',
    warnings: data.warnings ?? [],
    errors: data.errors ?? []
  };
}

/**
 * Local calculator fallback (offline mode)
 * Uses validated TS calculators (sora-calculator.js) when API is unavailable
 * 
 * @param {Object} request - SoraCalculationRequest
 * @returns {Promise<Object>} SORA calculation result
 */
async function calculateLocalFallback(request) {
  // Import calculateSORA25/20 if not already available
  if (typeof calculateSORA25 === 'undefined' || typeof calculateSORA20 === 'undefined') {
    // Dynamic import (if module system available)
    try {
      const calc = await import('./sora-calculator.js');
      window.calculateSORA25 = calc.calculateSORA25;
      window.calculateSORA20 = calc.calculateSORA20;
    } catch (err) {
      throw new Error('Local calculator unavailable (sora-calculator.js not loaded)');
    }
  }
  
  // Call local calculator
  let result;
  if (request.soraVersion === "2.5") {
    result = calculateSORA25(request);
  } else if (request.soraVersion === "2.0") {
    result = calculateSORA20(request);
  } else {
    throw new Error(`Invalid SORA version: ${request.soraVersion}`);
  }
  
  // Normalize result to match API response format
  return {
    initialGRC: result.initialGRC || result.initial_grc || result.iGRC,
    finalGRC: result.finalGRC || result.final_grc || result.fGRC,
    initialARC: result.initialARC || result.initial_arc || result.iARC,
    residualARC: result.residualARC || result.residual_arc || result.rARC,
    aec: result.aec || result.AEC,
    sail: result.sail || result.SAIL,
    tmpr: result.tmpr || result.TMPR,
    warnings: result.warnings || [],
    errors: result.errors || [],
    calculationSteps: result.steps || result.calculationSteps
  };
}

// ═══════════════════════════════════════════════════════════════════
// LIVE BREAKDOWN UPDATE (from API response)
// ═══════════════════════════════════════════════════════════════════

function updateLiveBreakdown(response) {
  // Normalize response (handles both API format and local calculator format)
  const initialGRC = response.initialGRC ?? response.initialGrc ?? response.initial_grc ?? response.iGRC ?? 'N/A';
  const finalGRC = response.finalGRC ?? response.finalGrc ?? response.final_grc ?? response.fGRC ?? 'N/A';
  const initialARC = response.initialARC ?? response.arc?.initial ?? response.initial_arc ?? response.iARC ?? 'N/A';
  const residualARC = response.residualARC ?? response.arc?.residual ?? response.residual_arc ?? response.rARC ?? 'N/A';
  const sail = response.sail ?? response.SAIL ?? 'N/A';
  
  // Update KPI badges
  document.getElementById('kpi-igrc').textContent = initialGRC;
  document.getElementById('kpi-fgrc').textContent = finalGRC;
  document.getElementById('kpi-iarc').textContent = initialARC;
  document.getElementById('kpi-rarc').textContent = residualARC;
  document.getElementById('kpi-sail').textContent = sail;
  
  // Update warnings/errors
  const warnings = response.warnings ?? [];
  const errors = response.errors ?? [];
  
  if (warnings.length > 0) {
    warnings.forEach(w => logToConsole(`⚠️ ${w}`, 'warning'));
  }
  if (errors.length > 0) {
    errors.forEach(e => logToConsole(`❌ ${e}`, 'error'));
  }
  
  // If AEC is available, show it
  if (response.aec || response.AEC) {
    const aec = response.aec ?? response.AEC;
    logToConsole(`ℹ️ AEC: ${aec}`, 'success');
  }
  
  // If TMPR is available (SORA 2.5 BVLOS), show it
  if (response.tmpr || response.TMPR) {
    const tmpr = response.tmpr ?? response.TMPR;
    logToConsole(`ℹ️ TMPR: ${tmpr}`, 'success');
  }
}

// ═══════════════════════════════════════════════════════════════════
// VALIDATION CONSOLE
// ═══════════════════════════════════════════════════════════════════

function logToConsole(message, type = 'success') {
  const console = document.getElementById('validation-console');
  const line = document.createElement('div');
  line.className = type;
  line.textContent = message;
  console.appendChild(line);
  console.scrollTop = console.scrollHeight;
}

function clearConsole() {
  const console = document.getElementById('validation-console');
  console.innerHTML = '';
}

// ═══════════════════════════════════════════════════════════════════
// INIT
// ═══════════════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', () => {
  // Load job types
  loadJobTypes();
  
  // Attach toggle listeners (NEW: framework toggle instead of version)
  document.querySelectorAll('.toggle-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      toggleFramework(btn.dataset.framework);
    });
  });
  
  // Attach form submit
  const form = document.getElementById('mission-form');
  if (form) {
    form.addEventListener('submit', handleFormSubmit);
  }
  
  // Attach job type change
  const jobTypeSelect = document.getElementById('job-type');
  if (jobTypeSelect) {
    jobTypeSelect.addEventListener('change', handleJobTypeChange);
  }
  
  // Attach action buttons
  document.getElementById('btn-print')?.addEventListener('click', handlePrint);
  document.getElementById('btn-pdf')?.addEventListener('click', handlePdfExport);
  document.getElementById('btn-email')?.addEventListener('click', handleEmail);
  
  // Initialize to SORA 2.5
  toggleFramework('sora25');
  
  logToConsole('✅ SKYWORKS Mission Planner Ready', 'success');
  logToConsole('SORA 2.5 Annex B + SORA 2.0 AMC1 Art.11 Compliant', 'success');
});

// ═══════════════════════════════════════════════════════════════════
// JOB TYPES MANAGEMENT
// ═══════════════════════════════════════════════════════════════════

let jobTypesData = null;

async function loadJobTypes() {
  try {
    const response = await fetch('assets/job-types.json');
    jobTypesData = await response.json();
    
    const select = document.getElementById('job-type');
    if (!select) return;
    
    // Group by category
    jobTypesData.categories.forEach(category => {
      const optgroup = document.createElement('optgroup');
      optgroup.label = `${category.icon} ${category.name}`;
      
      const categoryJobs = jobTypesData.jobTypes.filter(j => j.category === category.id);
      categoryJobs.forEach(job => {
        const option = document.createElement('option');
        option.value = job.id;
        option.textContent = `${job.icon} ${job.name}`;
        optgroup.appendChild(option);
      });
      
      select.appendChild(optgroup);
    });
    
    logToConsole(`✅ Loaded ${jobTypesData.jobTypes.length} job types`, 'success');
  } catch (err) {
    logToConsole(`❌ Failed to load job types: ${err.message}`, 'error');
  }
}

function handleJobTypeChange(event) {
  const jobId = event.target.value;
  if (!jobId || !jobTypesData) return;
  
  const job = jobTypesData.jobTypes.find(j => j.id === jobId);
  if (!job) return;
  
  // Auto-fill form fields
  document.getElementById('operation-type').value = job.operationType;
  document.getElementById('max-height').value = job.defaultHeight;
  document.getElementById('typicality').value = job.typicality;
  
  // Update description
  document.getElementById('job-description').textContent = `${job.icon} ${job.description} (Default: ${job.defaultHeight}m, ${job.defaultDuration}min, ${job.defaultSpeed} m/s)`;
  
  logToConsole(`✅ Applied job template: ${job.name}`, 'success');
}

// ═══════════════════════════════════════════════════════════════════
// FRAMEWORK TOGGLE (SORA 2.0 / 2.5 / PDRA / STS)
// ═══════════════════════════════════════════════════════════════════

async function toggleFramework(framework) {
  // Update currentSoraVersion based on framework
  if (framework === 'sora25') {
    currentSoraVersion = "2.5";
  } else if (framework === 'sora20') {
    currentSoraVersion = "2.0";
  }
  
  // Update toggle button states
  document.querySelectorAll('.toggle-btn').forEach(btn => {
    btn.classList.remove('active');
  });
  document.querySelector(`[data-framework="${framework}"]`)?.classList.add('active');
  
  // Show/hide appropriate fieldsets
  const sora25Fields = document.getElementById('sora25-fields');
  const sora20Fields = document.getElementById('sora20-fields');
  
  if (framework === 'sora25') {
    sora25Fields?.classList.remove('hidden');
    sora20Fields?.classList.add('hidden');
    logToConsole('✅ Switched to SORA 2.5 (Annex B)', 'success');
    
    // Load SORA 2.5 specifications and populate dropdowns
    await loadSpecifications("2.5");
    
  } else if (framework === 'sora20') {
    sora25Fields?.classList.add('hidden');
    sora20Fields?.classList.remove('hidden');
    logToConsole('✅ Switched to SORA 2.0 (AMC1 Art.11)', 'success');
    
    // Load SORA 2.0 specifications and populate dropdowns
    await loadSpecifications("2.0");
    
  } else if (framework === 'pdra') {
    sora25Fields?.classList.add('hidden');
    sora20Fields?.classList.add('hidden');
    logToConsole('🚀 Redirecting to PDRA & STS hub...', 'success');
    setTimeout(() => {
      window.location.href = 'pdrasts.html';
    }, 500);
  } else if (framework === 'sts') {
    sora25Fields?.classList.add('hidden');
    sora20Fields?.classList.add('hidden');
    logToConsole('🚀 Redirecting to PDRA & STS hub...', 'success');
    setTimeout(() => {
      window.location.href = 'pdrasts.html';
    }, 500);
  }
}

// ═══════════════════════════════════════════════════════════════════
// ACTION BUTTONS (Print / PDF / Email)
// ═══════════════════════════════════════════════════════════════════

function handlePrint() {
  logToConsole('🖨️ Printing mission dossier...', 'success');
  window.print();
}

function handlePdfExport() {
  logToConsole('📄 Exporting to PDF (jsPDF integration required)', 'warning');
  alert('PDF export will be implemented with jsPDF library. For now, use Print → Save as PDF.');
}

function handleEmail() {
  const subject = encodeURIComponent('SKYWORKS SORA Mission Dossier');
  const body = encodeURIComponent(`Mission Details:\n\nFramework: ${currentSoraVersion}\nJob Type: ${document.getElementById('job-type').value}\nOperation: ${document.getElementById('operation-type').value}\n\nGenerated by SKYWORKS SORA Suite v0.7.0`);
  
  window.location.href = `mailto:?subject=${subject}&body=${body}`;
  logToConsole('📧 Email client opened', 'success');
}

// ═══════════════════════════════════════════════════════════════════
// MISSION WIZARD
// ═══════════════════════════════════════════════════════════════════

let wizardCurrentStep = 1;
const MISSION_TEMPLATES = [
  {code:'PhotovoltaicParkInspection',name:'Photovoltaic Park Inspection',cat:'EnergyAndUtilities',type:'Solar',env:'rural',sora:'2.5'},
  {code:'BridgeStructuralInspection',name:'Bridge Structural Inspection',cat:'InfrastructureAndTransport',type:'Facade',env:'suburban',sora:'2.5'},
  {code:'SolarPanelCleaning',name:'Solar Panel Cleaning',cat:'EnergyAndUtilities',type:'Roof',env:'suburban',sora:'2.5'},
  {code:'TrainingFlightVLOS',name:'Training Flight VLOS',cat:'TrainingAndTest',type:'Agriculture',env:'rural',sora:'2.5'},
  {code:'FacadeInspection',name:'Facade Inspection',cat:'BuildingsAndFacades',type:'Facade',env:'urban',sora:'2.5'}
];

function initMissionWizard() {
  const wizard = document.getElementById('missionWizard');
  if (!wizard) return;
  
  loadMissionTemplates();
  
  wizard.querySelectorAll('.wizard-next').forEach(btn => {
    btn.addEventListener('click', () => navigateWizard(1));
  });
  
  wizard.querySelectorAll('.wizard-prev').forEach(btn => {
    btn.addEventListener('click', () => navigateWizard(-1));
  });
  
  document.getElementById('wizard-template')?.addEventListener('change', (e) => {
    updateTemplatePreview(e.target.value);
  });
  
  document.getElementById('btn-parse-gmaps')?.addEventListener('click', parseGoogleMapsInput);
  document.getElementById('wizard-create')?.addEventListener('click', createMission);
}

function loadMissionTemplates() {
  const select = document.getElementById('wizard-template');
  if (!select) return;
  
  MISSION_TEMPLATES.forEach(t => {
    const option = document.createElement('option');
    option.value = t.code;
    option.textContent = `${t.cat} - ${t.name}`;
    select.appendChild(option);
  });
}

function updateTemplatePreview(code) {
  const template = MISSION_TEMPLATES.find(t => t.code === code);
  const preview = document.getElementById('template-preview');
  if (!template || !preview) return;
  
  preview.style.display = 'block';
  document.getElementById('preview-category').textContent = template.cat;
  document.getElementById('preview-type').textContent = template.type;
  document.getElementById('preview-environment').textContent = template.env;
  document.getElementById('preview-sora').textContent = `SORA ${template.sora}`;
}

function navigateWizard(direction) {
  const newStep = wizardCurrentStep + direction;
  if (newStep < 1 || newStep > 3) return;
  
  if (direction > 0 && !validateWizardStep(wizardCurrentStep)) return;
  
  document.querySelectorAll('.wizard-step').forEach(step => step.style.display = 'none');
  document.querySelector(`[data-step="${newStep}"]`).style.display = 'block';
  
  document.querySelectorAll('.progress-step').forEach(step => step.classList.remove('active'));
  document.querySelector(`.progress-step[data-step="${newStep}"]`)?.classList.add('active');
  
  wizardCurrentStep = newStep;
  
  if (newStep === 3) updateSummary();
}

function validateWizardStep(step) {
  if (step === 1) {
    const template = document.getElementById('wizard-template').value;
    if (!template) { alert('Please select a template'); return false; }
  }
  if (step === 2) {
    const lat = document.getElementById('wizard-lat').value;
    const lon = document.getElementById('wizard-lon').value;
    const model = document.getElementById('wizard-drone-model').value;
    const mtom = document.getElementById('wizard-drone-mtom').value;
    const droneClass = document.getElementById('wizard-drone-class').value;
    
    if (!lat || !lon || !model || !mtom || !droneClass) {
      alert('Please fill all required fields');
      return false;
    }
  }
  return true;
}

function updateSummary() {
  const template = MISSION_TEMPLATES.find(t => t.code === document.getElementById('wizard-template').value);
  const lat = document.getElementById('wizard-lat').value;
  const lon = document.getElementById('wizard-lon').value;
  const height = document.getElementById('wizard-height').value;
  const areaWidth = document.getElementById('wizard-area-width').value;
  const areaLength = document.getElementById('wizard-area-length').value;
  const model = document.getElementById('wizard-drone-model').value;
  const mtom = document.getElementById('wizard-drone-mtom').value;
  const droneClass = document.getElementById('wizard-drone-class').value;
  
  const summary = `
    <ul style="list-style:none;padding:0;">
      <li><strong>Template:</strong> ${template?.name || 'N/A'}</li>
      <li><strong>Category:</strong> ${template?.cat || 'N/A'}</li>
      <li><strong>Location:</strong> ${lat}, ${lon}</li>
      <li><strong>Max Height:</strong> ${height} m AGL</li>
      <li><strong>Area:</strong> ${areaWidth} × ${areaLength} m</li>
      <li><strong>Drone:</strong> ${model} (${mtom} kg, ${droneClass})</li>
    </ul>
  `;
  
  document.getElementById('wizard-summary').innerHTML = summary;
}

function parseGoogleMapsInput() {
  const input = document.getElementById('gmaps-paste-input').value.trim();
  if (!input) return;
  
  let lat = null, lon = null;
  
  // Try parsing simple "lat, lon" format
  const latLonRegex = /^(-?\d+\.?\d*)[,\s]+(-?\d+\.?\d*)$/;
  const simpleMatch = input.match(latLonRegex);
  
  if (simpleMatch) {
    lat = simpleMatch[1];
    lon = simpleMatch[2];
  } else {
    // Try parsing Google Maps URL format: https://www.google.com/maps/@37.9838,23.7275,15z
    // or https://maps.google.com/?q=37.9838,23.7275
    // or https://www.google.com/maps/place/.../@37.9838,23.7275,15z
    const urlRegexAt = /@(-?\d+\.?\d+),(-?\d+\.?\d+)/;
    const urlRegexQ = /[?&]q=(-?\d+\.?\d+),(-?\d+\.?\d+)/;
    
    const urlMatchAt = input.match(urlRegexAt);
    const urlMatchQ = input.match(urlRegexQ);
    
    if (urlMatchAt) {
      lat = urlMatchAt[1];
      lon = urlMatchAt[2];
    } else if (urlMatchQ) {
      lat = urlMatchQ[1];
      lon = urlMatchQ[2];
    }
  }
  
  if (lat && lon) {
    // Validate ranges
    const latNum = parseFloat(lat);
    const lonNum = parseFloat(lon);
    
    if (latNum < -90 || latNum > 90) {
      alert(`❌ Invalid latitude: ${lat}. Must be between -90 and 90.`);
      return;
    }
    if (lonNum < -180 || lonNum > 180) {
      alert(`❌ Invalid longitude: ${lon}. Must be between -180 and 180.`);
      return;
    }
    
    document.getElementById('wizard-lat').value = lat;
    document.getElementById('wizard-lon').value = lon;
    logToConsole(`✅ Parsed coordinates: ${lat}, ${lon}`, 'success');
    alert(`✅ Coordinates extracted: ${lat}, ${lon}`);
  } else {
    alert(`❌ Could not parse coordinates.\n\nSupported formats:\n• "lat, lon" (e.g., "37.9838, 23.7275")\n• Google Maps URL (e.g., "https://www.google.com/maps/@37.9838,23.7275,15z")\n• Google Maps share link with ?q= parameter`);
  }
}

async function createMission() {
  const statusEl = document.getElementById('wizard-status');
  const resultEl = document.getElementById('wizard-result');
  
  statusEl.innerHTML = '<div class="loading">Creating mission...</div>';
  resultEl.style.display = 'none';
  
  try {
    const request = {
      templateCode: document.getElementById('wizard-template').value,
      centerLat: parseFloat(document.getElementById('wizard-lat').value),
      centerLon: parseFloat(document.getElementById('wizard-lon').value),
      maxHeightAGL_m: parseFloat(document.getElementById('wizard-height').value),
      areaWidth_m: parseFloat(document.getElementById('wizard-area-width').value),
      areaLength_m: parseFloat(document.getElementById('wizard-area-length').value),
      droneModel: document.getElementById('wizard-drone-model').value,
      droneMtom_kg: parseFloat(document.getElementById('wizard-drone-mtom').value),
      droneClass: document.getElementById('wizard-drone-class').value
    };
    
    const response = await fetch('/api/v1/missions/auto-create', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(request)
    });
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || 'Failed to create mission');
    }
    
    const data = await response.json();
    const missionId = data.missionId;
    
    localStorage.setItem('lastMissionId', missionId);
    
    statusEl.innerHTML = '';
    resultEl.style.display = 'block';
    document.getElementById('result-mission-id').textContent = missionId;
    document.getElementById('link-maps').href = `airspace-maps.html?missionId=${missionId}`;
    document.getElementById('link-report').href = `final-report.html?missionId=${missionId}`;
    
    logToConsole(`✅ Mission created: ${missionId}`, 'success');
  } catch (error) {
    statusEl.innerHTML = `<div class="error" style="color:red;padding:10px;background:#fee;">❌ ${error.message}</div>`;
    logToConsole(`❌ Mission creation failed: ${error.message}`, 'error');
  }
}

if (document.getElementById('missionWizard')) {
  initMissionWizard();
}

// ═══════════════════════════════════════════════════════════════════
// GLOBAL EXPORTS (for module compatibility)
// ═══════════════════════════════════════════════════════════════════

// Make functions available globally for HTML onclick handlers
window.toggleFramework = toggleFramework;
window.toggleSoraVersion = toggleSoraVersion;
window.handlePrint = handlePrint;
window.handlePdfExport = handlePdfExport;
window.handleEmail = handleEmail;
