/**
 * SKYWORKS Mission Planner - Core JavaScript
 * Handles form submission, validation, API calls, and UI updates
 */

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

// ═══════════════════════════════════════════════════════════════════
// SORA VERSION TOGGLE
// ═══════════════════════════════════════════════════════════════════

function toggleSoraVersion(version) {
  currentSoraVersion = version;
  
  // Update toggle button states
  document.querySelectorAll('.toggle-btn').forEach(btn => {
    btn.classList.remove('active');
  });
  document.querySelector(`[data-version="${version}"]`).classList.add('active');
  
  // Show/hide appropriate fieldsets
  const sora25Fields = document.getElementById('sora25-fields');
  const sora20Fields = document.getElementById('sora20-fields');
  
  if (version === "2.5") {
    sora25Fields.classList.remove('hidden');
    sora20Fields.classList.add('hidden');
  } else {
    sora25Fields.classList.add('hidden');
    sora20Fields.classList.remove('hidden');
  }
  
  logToConsole(`Switched to SORA ${version}`, 'success');
}

// ═══════════════════════════════════════════════════════════════════
// FORM SUBMISSION & PAYLOAD COMPOSITION
// ═══════════════════════════════════════════════════════════════════

async function handleFormSubmit(event) {
  event.preventDefault();
  
  try {
    clearConsole();
    logToConsole('Building payload...', 'success');
    
    // Build payload
    const payload = buildPayload();
    
    // Validate payload
    logToConsole('Validating payload...', 'success');
    validatePayload(payload);
    
    // Log request
    logToConsole('REQUEST:', 'success');
    logToConsole(JSON.stringify(payload, null, 2), 'success');
    
    // Call API
    logToConsole('Calling POST /api/v1/sora/calculate...', 'success');
    const response = await callSoraAPI(payload);
    
    // Log response
    logToConsole('RESPONSE:', 'success');
    logToConsole(JSON.stringify(response, null, 2), 'success');
    
    // Update Live Breakdown
    updateLiveBreakdown(response);
    
    lastResponse = response;
    
  } catch (error) {
    logToConsole(`ERROR: ${error.message}`, 'error');
  }
}

function buildPayload() {
  const common = {
    operationType: document.getElementById('operation-type').value,
    airspaceClass: document.getElementById('airspace-class').value,
    typicality: document.getElementById('typicality').value,
    maxHeightAGL_m: parseInt(document.getElementById('max-height').value),
    specialZones: getSelectedSpecialZones(),
    aec: document.getElementById('aec').value,
    uSpace: document.getElementById('uspace').value === "Yes",
    trafficDensitySource: document.getElementById('traffic-density').value,
    airspaceContainment: document.getElementById('airspace-containment').value
  };
  
  let grc;
  if (currentSoraVersion === "2.5") {
    grc = {
      m1a: document.getElementById('m1a').value,
      m1b: document.getElementById('m1b').value,
      m1c: document.getElementById('m1c').value,
      m2: document.getElementById('m2-25').value,
      smallUARuleApplies: document.getElementById('small-ua-rule').checked
    };
  } else {
    grc = {
      m1: document.getElementById('m1-20').value,
      m2: document.getElementById('m2-20').value,
      m3: document.getElementById('m3-20').value
    };
  }
  
  return {
    soraVersion: currentSoraVersion,
    common,
    grc
  };
}

function getSelectedSpecialZones() {
  const checkboxes = document.querySelectorAll('input[name="special-zones"]:checked');
  return Array.from(checkboxes).map(cb => cb.value);
}

function validatePayload(payload) {
  const errors = [];
  
  try {
    if (payload.soraVersion === "2.5") {
      SORA_25_SCHEMAS.m1a.validate(payload.grc.m1a);
      SORA_25_SCHEMAS.m1b.validate(payload.grc.m1b);
      SORA_25_SCHEMAS.m1c.validate(payload.grc.m1c);
      SORA_25_SCHEMAS.m2.validate(payload.grc.m2);
    } else {
      SORA_20_SCHEMAS.m1.validate(payload.grc.m1);
      SORA_20_SCHEMAS.m2.validate(payload.grc.m2);
      SORA_20_SCHEMAS.m3.validate(payload.grc.m3);
    }
  } catch (err) {
    errors.push(err.message);
  }
  
  if (errors.length > 0) {
    throw new Error(errors.join('\n'));
  }
}

async function callSoraAPI(payload) {
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

// ═══════════════════════════════════════════════════════════════════
// LIVE BREAKDOWN UPDATE
// ═══════════════════════════════════════════════════════════════════

function updateLiveBreakdown(response) {
  document.getElementById('kpi-igrc').textContent = response.initialGrc;
  document.getElementById('kpi-fgrc').textContent = response.finalGrc;
  document.getElementById('kpi-iarc').textContent = response.arc.initial;
  document.getElementById('kpi-rarc').textContent = response.arc.residual;
  document.getElementById('kpi-sail').textContent = response.sail;
  
  // Update warnings/errors
  if (response.warnings.length > 0) {
    response.warnings.forEach(w => logToConsole(`⚠️ ${w}`, 'warning'));
  }
  if (response.errors.length > 0) {
    response.errors.forEach(e => logToConsole(`❌ ${e}`, 'error'));
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
  // Attach toggle listeners
  document.querySelectorAll('.toggle-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      toggleSoraVersion(btn.dataset.version);
    });
  });
  
  // Attach form submit
  const form = document.getElementById('mission-form');
  if (form) {
    form.addEventListener('submit', handleFormSubmit);
  }
  
  // Initialize to 2.5
  toggleSoraVersion('2.5');
  
  logToConsole('✅ SKYWORKS Mission Planner Ready', 'success');
  logToConsole('SORA 2.5 Annex B + SORA 2.0 AMC1 Art.11 Compliant', 'success');
});
