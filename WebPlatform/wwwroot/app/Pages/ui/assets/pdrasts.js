/**
 * SKYWORKS PDRA & STS Scenarios Manager
 * Static scenario packs - no SORA computation
 */

let scenariosData = null;
let currentScenario = "STS-01";

// ═══════════════════════════════════════════════════════════════════
// INIT
// ═══════════════════════════════════════════════════════════════════

document.addEventListener('DOMContentLoaded', async () => {
  await loadScenarios();
  
  // Attach tab listeners
  document.querySelectorAll('[data-scenario]').forEach(btn => {
    btn.addEventListener('click', () => {
      switchScenario(btn.dataset.scenario);
    });
  });
  
  // Attach action buttons
  document.getElementById('check-eligibility')?.addEventListener('click', checkEligibility);
  document.getElementById('btn-print')?.addEventListener('click', handlePrint);
  document.getElementById('btn-pdf')?.addEventListener('click', handlePdfExport);
  document.getElementById('btn-om')?.addEventListener('click', handleOmGeneration);
  document.getElementById('btn-back')?.addEventListener('click', () => {
    window.location.href = 'mission.html';
  });
  
  // Load default scenario
  switchScenario('STS-01');
});

// ═══════════════════════════════════════════════════════════════════
// LOAD SCENARIOS JSON
// ═══════════════════════════════════════════════════════════════════

async function loadScenarios() {
  try {
    const response = await fetch('assets/scenarios.json');
    scenariosData = await response.json();
    console.log('✅ Loaded PDRA/STS scenarios:', Object.keys(scenariosData));
  } catch (err) {
    console.error('❌ Failed to load scenarios.json:', err);
    alert('Failed to load scenarios. Please refresh the page.');
  }
}

// ═══════════════════════════════════════════════════════════════════
// SWITCH SCENARIO
// ═══════════════════════════════════════════════════════════════════

function switchScenario(scenarioCode) {
  if (!scenariosData || !scenariosData[scenarioCode]) {
    console.error('Scenario not found:', scenarioCode);
    return;
  }
  
  currentScenario = scenarioCode;
  const data = scenariosData[scenarioCode];
  
  // Update active tab
  document.querySelectorAll('[data-scenario]').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.scenario === scenarioCode);
  });
  
  // Update hero summary
  document.getElementById('scenario-title').textContent = `${scenarioCode}: ${data.title}`;
  document.getElementById('scenario-code').textContent = scenarioCode;
  document.getElementById('scenario-mode').textContent = data.operationMode;
  document.getElementById('scenario-env').textContent = data.environment;
  document.getElementById('scenario-ground').textContent = data.groundArea;
  document.getElementById('scenario-class').textContent = data.uasClass;
  document.getElementById('scenario-height').textContent = data.maxAGL_m ? `${data.maxAGL_m}m AGL` : '—';
  document.getElementById('scenario-approval').textContent = data.approval;
  document.getElementById('scenario-range').textContent = data.maxRange_km ? `${data.maxRange_km} km` : 'N/A';
  document.getElementById('scenario-scope').textContent = data.scope || 'No description available';
  
  // Update status card
  document.getElementById('scenario-result').textContent = data.result;
  document.getElementById('approval-type').textContent = data.approval;
  document.getElementById('uas-class').textContent = data.uasClass;
  
  // Render eligibility checklist
  renderEligibility(data.eligibility || []);
  
  // Render documents
  renderDocuments(data.documents || []);
  
  // Render references
  renderReferences(data.references || []);
  
  // Render download links
  renderDownloads(data.downloadLinks || []);
  
  // Clear eligibility console
  document.getElementById('eligibility-console').innerHTML = '<pre>Check eligibility to see results...</pre>';
  
  console.log(`✅ Switched to ${scenarioCode}`);
}

// ═══════════════════════════════════════════════════════════════════
// RENDER ELIGIBILITY CHECKLIST
// ═══════════════════════════════════════════════════════════════════

function renderEligibility(items) {
  const container = document.getElementById('eligibility-items');
  container.innerHTML = '';
  
  if (items.length === 0) {
    container.innerHTML = '<p class="text-muted">No eligibility checks defined for this scenario.</p>';
    return;
  }
  
  items.forEach((item, index) => {
    const div = document.createElement('div');
    div.className = 'form-group';
    div.innerHTML = `
      <label>
        <input type="checkbox" class="form-checkbox eligibility-check" id="check-${index}">
        ${item}
      </label>
    `;
    container.appendChild(div);
  });
}

// ═══════════════════════════════════════════════════════════════════
// RENDER DOCUMENTS
// ═══════════════════════════════════════════════════════════════════

function renderDocuments(documents) {
  const container = document.getElementById('documents-list');
  container.innerHTML = '';
  
  if (documents.length === 0) {
    container.innerHTML = '<li class="text-muted">No documents specified</li>';
    return;
  }
  
  documents.forEach(doc => {
    const li = document.createElement('li');
    li.style.marginBottom = '8px';
    li.innerHTML = `📄 ${doc}`;
    container.appendChild(li);
  });
}

// ═══════════════════════════════════════════════════════════════════
// RENDER REFERENCES
// ═══════════════════════════════════════════════════════════════════

function renderReferences(references) {
  const container = document.getElementById('references-list');
  container.innerHTML = '';
  
  if (references.length === 0) {
    container.innerHTML = '<p class="text-muted">No references available</p>';
    return;
  }
  
  references.forEach(ref => {
    const link = document.createElement('a');
    link.href = ref.url;
    link.target = '_blank';
    link.className = 'btn btn-secondary';
    link.style.display = 'block';
    link.style.marginBottom = '8px';
    link.textContent = `🔗 ${ref.label}`;
    container.appendChild(link);
  });
}

// ═══════════════════════════════════════════════════════════════════
// RENDER DOWNLOAD LINKS
// ═══════════════════════════════════════════════════════════════════

function renderDownloads(links) {
  const container = document.getElementById('download-links');
  container.innerHTML = '';
  
  if (links.length === 0) {
    container.innerHTML = '<p class="text-muted">No downloads available</p>';
    return;
  }
  
  links.forEach(link => {
    const a = document.createElement('a');
    a.href = link.href;
    a.target = '_blank';
    a.className = 'btn btn-primary';
    a.textContent = link.label;
    container.appendChild(a);
  });
}

// ═══════════════════════════════════════════════════════════════════
// CHECK ELIGIBILITY
// ═══════════════════════════════════════════════════════════════════

function checkEligibility() {
  const console = document.getElementById('eligibility-console');
  console.innerHTML = '';
  
  const checks = document.querySelectorAll('.eligibility-check');
  const total = checks.length;
  let passed = 0;
  
  if (total === 0) {
    console.innerHTML = '<div style="color: var(--slate-500);">No eligibility criteria defined for this scenario.</div>';
    return;
  }
  
  const results = [];
  checks.forEach((check, index) => {
    const label = check.parentElement.textContent.trim();
    if (check.checked) {
      passed++;
      results.push(`<div style="color: var(--green-700);">✅ ${label}</div>`);
    } else {
      results.push(`<div style="color: var(--red-700);">❌ ${label}</div>`);
    }
  });
  
  console.innerHTML = results.join('');
  
  if (passed === total) {
    console.innerHTML += `
      <div style="margin-top: 16px; padding: 12px; background: var(--green-50); border-left: 4px solid var(--green-500); border-radius: 4px;">
        <strong style="color: var(--green-700);">✅ ELIGIBLE</strong><br>
        All ${total} requirements satisfied. You may proceed with ${currentScenario}.
      </div>
    `;
  } else {
    console.innerHTML += `
      <div style="margin-top: 16px; padding: 12px; background: var(--red-50); border-left: 4px solid var(--red-500); border-radius: 4px;">
        <strong style="color: var(--red-700);">❌ NOT ELIGIBLE</strong><br>
        ${passed}/${total} requirements met. Missing ${total - passed} requirement(s).<br>
        Consider using SORA instead if requirements cannot be met.
      </div>
    `;
  }
}

// ═══════════════════════════════════════════════════════════════════
// ACTION BUTTONS
// ═══════════════════════════════════════════════════════════════════

function handlePrint() {
  console.log('🖨️ Printing scenario summary...');
  window.print();
}

function handlePdfExport() {
  console.log('📄 Exporting to PDF...');
  alert('PDF export: Use Print → Save as PDF for now. jsPDF integration coming soon.');
}

function handleOmGeneration() {
  console.log('📋 Generating OM extract...');
  const data = scenariosData[currentScenario];
  const omText = `
OPERATIONS MANUAL EXTRACT
=========================

Scenario: ${currentScenario}
Title: ${data.title}

Operation Mode: ${data.operationMode}
Environment: ${data.environment}
Ground Area: ${data.groundArea}
UAS Class: ${data.uasClass}
Max Height: ${data.maxAGL_m}m AGL
Approval: ${data.approval}

Required Documents:
${data.documents.map(d => `- ${d}`).join('\n')}

Scope & Limitations:
${data.scope}

---
Generated by SKYWORKS SORA Suite v0.7.0
${new Date().toISOString()}
  `;
  
  // Create downloadable text file
  const blob = new Blob([omText], { type: 'text/plain' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `${currentScenario}_OM_Extract.txt`;
  a.click();
  URL.revokeObjectURL(url);
  
  alert(`✅ OM extract downloaded: ${currentScenario}_OM_Extract.txt`);
}
