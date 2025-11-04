/**
 * OSO UI Component - Visual Interface for OSO Selection & Compliance
 * Phase 5 - Step 41: OSO Basic Framework
 */

const OSOUI = (function() {
  'use strict';

  /**
   * Render OSO Selection Panel
   */
  function renderOSOSelectionPanel(containerId, version) {
    const container = document.getElementById(containerId);
    if (!container) {
      console.error(`[OSO UI] Container not found: ${containerId}`);
      return;
    }

    const osos = window.OSOManager.getOSOsForVersion(version);
    if (osos.length === 0) {
      container.innerHTML = '<p>No OSOs available for this version</p>';
      return;
    }

    // Group OSOs by category
    const groupedOSOs = {};
    osos.forEach(oso => {
      if (!groupedOSOs[oso.category]) {
        groupedOSOs[oso.category] = [];
      }
      groupedOSOs[oso.category].push(oso);
    });

    let html = '<div class="oso-selection-panel" style="display:grid;grid-template-columns:repeat(auto-fit,minmax(300px,1fr));gap:16px;">';

    Object.keys(groupedOSOs).forEach(category => {
      const categoryInfo = window.OSOManager.CATEGORIES[category];
      const categoryOSOs = groupedOSOs[category];

      html += `
        <div class="oso-category-group" style="background:#f8f9fa;border:1px solid #dee2e6;border-radius:8px;padding:16px;">
          <h3 style="margin:0 0 12px 0;color:#424242;font-size:16px;">
            ${categoryInfo.icon} ${categoryInfo.name}
          </h3>
          <div class="oso-list">`;

      categoryOSOs.forEach(oso => {
        html += `
          <div class="oso-item" style="background:white;border:1px solid #e0e0e0;border-radius:6px;padding:12px;margin-bottom:8px;cursor:pointer;transition:all 0.2s;" 
               data-oso-id="${oso.id}"
               onclick="OSOUI.toggleOSO(${oso.id})">
            <div style="display:flex;justify-content:space-between;align-items:center;">
              <div>
                <strong style="color:#212529;">OSO#${oso.id}</strong>
                <small style="color:#666;display:block;margin-top:2px;">${oso.name}</small>
              </div>
              <input type="checkbox" id="oso_${oso.id}" style="width:20px;height:20px;cursor:pointer;" 
                     onclick="event.stopPropagation();" 
                     onchange="OSOUI.onOSOCheckChanged(${oso.id}, this.checked)">
            </div>
            <div id="oso_robustness_${oso.id}" style="display:none;margin-top:8px;">
              <select id="oso_robustness_select_${oso.id}" onchange="OSOUI.onRobustnessChanged(${oso.id}, this.value)" style="width:100%;padding:4px;">
                <option value="">Select Robustness...</option>
                <option value="L">Low (L)</option>
                <option value="M">Medium (M)</option>
                <option value="H">High (H)</option>
              </select>
            </div>
          </div>`;
      });

      html += `
          </div>
        </div>`;
    });

    html += '</div>';
    container.innerHTML = html;

    console.log(`[OSO UI] Rendered ${osos.length} OSOs for ${version}`);
  }

  /**
   * Toggle OSO selection
   */
  function toggleOSO(osoId) {
    const checkbox = document.getElementById(`oso_${osoId}`);
    if (checkbox) {
      checkbox.checked = !checkbox.checked;
      onOSOCheckChanged(osoId, checkbox.checked);
    }
  }

  /**
   * Handle OSO checkbox change
   */
  function onOSOCheckChanged(osoId, checked) {
    const robustnessDiv = document.getElementById(`oso_robustness_${osoId}`);
    
    if (checked) {
      robustnessDiv.style.display = 'block';
    } else {
      robustnessDiv.style.display = 'none';
      window.OSOManager.deselectOSO(osoId);
      updateOSOSummary();
    }
  }

  /**
   * Handle robustness level change
   */
  function onRobustnessChanged(osoId, robustness) {
    if (robustness) {
      window.OSOManager.selectOSO(osoId, robustness);
      updateOSOSummary();
    } else {
      window.OSOManager.deselectOSO(osoId);
      updateOSOSummary();
    }
  }

  /**
   * Update OSO summary display
   */
  function updateOSOSummary() {
    const selected = window.OSOManager.getSelectedOSOs();
    const count = Object.keys(selected).length;
    
    const summaryEl = document.getElementById('osoSelectionSummary');
    if (summaryEl) {
      summaryEl.textContent = `${count} OSO${count !== 1 ? 's' : ''} selected`;
    }

    console.log(`[OSO UI] Summary updated: ${count} OSOs selected`);
  }

  /**
   * Render OSO Compliance Matrix
   */
  function renderComplianceMatrix(containerId, sail, version) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const summary = window.OSOManager.generateComplianceSummary(sail, version);
    
    let html = `
      <div class="oso-compliance-matrix" style="background:white;border:2px solid #${summary.compliant ? '4caf50' : 'f44336'};border-radius:8px;padding:16px;">
        <h3 style="margin:0 0 12px 0;color:${summary.compliant ? '#2e7d32' : '#c62828'};">
          ${summary.compliant ? '✅' : '❌'} OSO Compliance Status
        </h3>
        <div class="compliance-stats" style="display:grid;grid-template-columns:repeat(auto-fit,minmax(120px,1fr));gap:12px;">
          <div style="background:#e3f2fd;padding:12px;border-radius:6px;text-align:center;">
            <div style="font-size:24px;font-weight:700;color:#1976d2;">${summary.required}</div>
            <div style="font-size:12px;color:#666;">Required</div>
          </div>
          <div style="background:#e8f5e9;padding:12px;border-radius:6px;text-align:center;">
            <div style="font-size:24px;font-weight:700;color:#4caf50;">${summary.selected}</div>
            <div style="font-size:12px;color:#666;">Selected</div>
          </div>
          <div style="background:#${summary.missing === 0 ? 'e8f5e9' : 'ffebee'};padding:12px;border-radius:6px;text-align:center;">
            <div style="font-size:24px;font-weight:700;color:#${summary.missing === 0 ? '4caf50' : 'f44336'};">${summary.missing}</div>
            <div style="font-size:12px;color:#666;">Missing</div>
          </div>
          <div style="background:#fff3e0;padding:12px;border-radius:6px;text-align:center;">
            <div style="font-size:24px;font-weight:700;color:#ff9800;">${summary.extra}</div>
            <div style="font-size:12px;color:#666;">Extra</div>
          </div>
        </div>
      </div>`;

    container.innerHTML = html;
  }

  /**
   * Show OSO Details Modal
   */
  function showOSODetails(osoId) {
    const metadata = window.OSOManager.getOSOMetadata(osoId);
    if (!metadata) return;

    const modal = document.createElement('div');
    modal.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:10000;display:flex;align-items:center;justify-content:center;';
    
    modal.innerHTML = `
      <div style="background:white;border-radius:12px;padding:24px;max-width:600px;max-height:80vh;overflow-y:auto;box-shadow:0 8px 32px rgba(0,0,0,0.3);">
        <h2 style="margin:0 0 16px 0;color:#0d47a1;">OSO#${metadata.id}: ${metadata.name}</h2>
        
        <div style="background:#f8f9fa;padding:12px;border-radius:6px;margin-bottom:16px;">
          <strong>Category:</strong> ${metadata.category}<br>
          <strong>JARUS Reference:</strong> ${metadata.jarus_ref}<br>
          <strong>Applies to:</strong> SORA ${metadata.applies_to.join(', ')}
        </div>

        <div style="margin-bottom:16px;">
          <strong>Description:</strong>
          <p style="margin:8px 0;color:#666;">${metadata.description}</p>
        </div>

        <div style="margin-bottom:16px;">
          <strong>Acceptable Means of Compliance (AMC) Examples:</strong>
          <ul style="margin:8px 0;padding-left:20px;color:#666;">
            ${metadata.ami_examples.map(ex => `<li>${ex}</li>`).join('')}
          </ul>
        </div>

        <button onclick="this.parentElement.parentElement.remove()" 
                style="background:#2196f3;color:white;border:none;padding:10px 20px;border-radius:6px;cursor:pointer;font-weight:600;">
          Close
        </button>
      </div>`;

    modal.onclick = (e) => {
      if (e.target === modal) modal.remove();
    };

    document.body.appendChild(modal);
  }

  // Public API
  return {
    renderOSOSelectionPanel,
    renderComplianceMatrix,
    showOSODetails,
    toggleOSO,
    onOSOCheckChanged,
    onRobustnessChanged,
    updateOSOSummary
  };
})();

// Make globally available
window.OSOUI = OSOUI;
