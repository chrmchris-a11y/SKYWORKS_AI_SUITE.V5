/**
 * ═══════════════════════════════════════════════════════════════════════════
 * OSO UI v2.0 - Visual Interface for OSO Selection & Compliance
 * ═══════════════════════════════════════════════════════════════════════════
 * Phase 5 - Step 41: OSO Basic Framework
 * 
 * Integrates with oso-manager-v2.js to provide:
 * - Interactive OSO grid display
 * - SAIL-based OSO requirement visualization
 * - Compliance status indicators
 * - OSO selection modal with robustness levels
 * - Evidence upload and notes
 * - Compliance report generation
 * ═══════════════════════════════════════════════════════════════════════════
 */

const OSOUI = (function() {
  'use strict';

  let currentTracker = null;
  let currentSoraVersion = 'SORA-2.5';
  let currentSAIL = 'III';

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * MAIN: Render OSO Grid for SAIL Level
   * ═══════════════════════════════════════════════════════════════════════════
   */
  function renderOSOGrid(containerId, soraVersion, sail, onOSOUpdate) {
    const container = document.getElementById(containerId);
    if (!container) {
      console.error('[OSO UI] Container not found:', containerId);
      return;
    }

    // Update current state
    currentSoraVersion = soraVersion;
    currentSAIL = sail;

    // Create or update tracker
    if (!currentTracker || currentTracker.soraVersion !== soraVersion || currentTracker.sail !== sail) {
      currentTracker = OSOManager.createComplianceTracker(soraVersion, sail);
    }

    const requiredOSOs = currentTracker.getRequiredOSOs();

    if (!requiredOSOs || requiredOSOs.length === 0) {
      container.innerHTML = `
        <div style="text-align:center; padding:40px; background:#f5f5f5; border-radius:8px;">
          <div style="font-size:48px; margin-bottom:16px;">🎯</div>
          <strong style="font-size:18px; color:#666;">No OSOs required for SAIL ${sail}</strong>
          <p style="color:#999; margin-top:8px;">This SAIL level has minimal OSO requirements.</p>
        </div>
      `;
      return;
    }

    // Build grid HTML
    let html = `
      <div style="margin-bottom:20px;">
        <div style="display:flex; justify-content:space-between; align-items:center; padding:16px; background:linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius:8px; color:white;">
          <div>
            <strong style="font-size:18px;">📋 ${soraVersion} - SAIL ${sail} Requirements</strong>
            <div style="font-size:14px; opacity:0.9; margin-top:4px;">${requiredOSOs.length} Operational Safety Objectives</div>
          </div>
          <div style="text-align:right;">
            <div id="osoCompliancePercentage" style="font-size:32px; font-weight:700;">0%</div>
            <div style="font-size:12px; opacity:0.9;">Compliance</div>
          </div>
        </div>
      </div>

      <div class="oso-grid" style="display:grid; grid-template-columns:repeat(auto-fill, minmax(280px, 1fr)); gap:16px; margin-bottom:20px;">
    `;

    // Render each required OSO as a badge
    requiredOSOs.forEach(req => {
      const osoDetails = OSOManager.getOSODetails(soraVersion, req.id);
      if (!osoDetails) return;

      const selected = currentTracker.selectedOSOs.get(req.id);
      const isSelected = !!selected;

      // Determine badge color and status
      let bgColor, borderColor, statusIcon, statusText, statusColor;
      if (isSelected) {
        const robustnessOrder = { 'NR': 0, 'L': 1, 'M': 2, 'H': 3 };
        const isSufficient = robustnessOrder[selected.robustness] >= robustnessOrder[req.requiredRobustness];

        if (isSufficient) {
          bgColor = 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)';
          borderColor = '#2e7d32';
          statusIcon = '✅';
          statusText = `Met (${selected.robustness})`;
          statusColor = '#c8e6c9';
        } else {
          bgColor = 'linear-gradient(135deg, #ff9800 0%, #f57c00 100%)';
          borderColor = '#e65100';
          statusIcon = '⚠️';
          statusText = `Partial (${selected.robustness} < ${req.requiredRobustness})`;
          statusColor = '#ffe0b2';
        }
      } else {
        bgColor = 'linear-gradient(135deg, #f44336 0%, #c62828 100%)';
        borderColor = '#b71c1c';
        statusIcon = '❌';
        statusText = `Missing (need ${req.requiredRobustness})`;
        statusColor = '#ffcdd2';
      }

      const categoryInfo = OSOManager.OSO_CATEGORIES[osoDetails.category];

      html += `
        <div class="oso-badge" 
             data-oso-id="${req.id}" 
             data-required="${req.requiredRobustness}"
             style="
               background: ${bgColor};
               border: 3px solid ${borderColor};
               border-radius: 12px;
               color: white;
               cursor: pointer;
               transition: all 0.3s ease;
               box-shadow: 0 4px 8px rgba(0,0,0,0.2);
               overflow: hidden;
             "
             title="${osoDetails.description}">
          
          <!-- Header -->
          <div style="padding:12px; background:rgba(0,0,0,0.1);">
            <div style="display:flex; justify-content:space-between; align-items:center;">
              <strong style="font-size:14px;">${osoDetails.number}</strong>
              <span style="font-size:20px;">${statusIcon}</span>
            </div>
            <div style="font-size:11px; opacity:0.9; margin-top:4px;">
              ${categoryInfo.icon} ${osoDetails.category}
            </div>
          </div>

          <!-- Body -->
          <div style="padding:12px;">
            <div style="font-size:13px; font-weight:600; margin-bottom:8px; line-height:1.3;">
              ${osoDetails.name}
            </div>
            <div style="font-size:11px; opacity:0.9; margin-bottom:8px; max-height:40px; overflow:hidden; text-overflow:ellipsis;">
              ${osoDetails.description.substring(0, 100)}...
            </div>
          </div>

          <!-- Footer Status -->
          <div style="padding:10px 12px; background:${statusColor}; color:#333; font-size:12px; font-weight:600; text-align:center;">
            ${statusText}
          </div>
        </div>
      `;
    });

    html += '</div>'; // Close grid

    // Compliance Summary Section
    const validation = currentTracker.validateCompliance();
    html += `
      <div style="margin-top:20px; padding:20px; background:white; border-radius:12px; border:3px solid ${validation.isCompliant ? '#4caf50' : '#ff9800'}; box-shadow:0 4px 12px rgba(0,0,0,0.1);">
        
        <!-- Compliance Header -->
        <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:16px;">
          <div>
            <strong style="font-size:18px; color:#333;">📊 Compliance Status</strong>
            <div style="font-size:14px; color:#666; margin-top:4px;">
              ${validation.selectedCount} of ${validation.requiredCount} OSOs addressed
            </div>
          </div>
          <div style="text-align:right;">
            <div style="font-size:36px; font-weight:700; color:${validation.isCompliant ? '#4caf50' : '#ff9800'};">
              ${validation.compliancePercentage}%
            </div>
          </div>
        </div>

        <!-- Progress Bar -->
        <div style="background:#e0e0e0; border-radius:8px; height:12px; overflow:hidden; margin-bottom:16px;">
          <div style="width:${validation.compliancePercentage}%; background:${validation.isCompliant ? '#4caf50' : '#ff9800'}; height:100%; transition:width 0.3s;"></div>
        </div>

        <!-- Missing OSOs Warning -->
        ${validation.missingOSOs.length > 0 ? `
          <div style="margin-bottom:12px; padding:12px; background:#ffebee; border-left:4px solid #f44336; border-radius:4px;">
            <strong style="color:#c62828; font-size:13px;">❌ Missing OSOs (${validation.missingOSOs.length}):</strong>
            <div style="font-size:12px; color:#666; margin-top:6px;">
              ${validation.missingOSOs.map(m => `<div style="margin-top:4px;">• ${m.number}: ${m.name} (need ${m.requiredRobustness})</div>`).join('')}
            </div>
          </div>
        ` : ''}

        <!-- Insufficient Robustness Warning -->
        ${validation.insufficientOSOs.length > 0 ? `
          <div style="margin-bottom:12px; padding:12px; background:#fff3e0; border-left:4px solid #ff9800; border-radius:4px;">
            <strong style="color:#e65100; font-size:13px;">⚠️ Insufficient Robustness (${validation.insufficientOSOs.length}):</strong>
            <div style="font-size:12px; color:#666; margin-top:6px;">
              ${validation.insufficientOSOs.map(i => `<div style="margin-top:4px;">• ${i.number}: has ${i.actual}, needs ${i.required}</div>`).join('')}
            </div>
          </div>
        ` : ''}

        <!-- Action Buttons -->
        <div style="display:flex; gap:12px; margin-top:16px;">
          <button id="osoExportBtn" style="flex:1; padding:10px; background:#2196f3; color:white; border:none; border-radius:6px; cursor:pointer; font-weight:600; font-size:14px;">
            📥 Export Compliance Report
          </button>
          <button id="osoClearBtn" style="flex:1; padding:10px; background:#f44336; color:white; border:none; border-radius:6px; cursor:pointer; font-weight:600; font-size:14px;">
            🗑️ Clear All Selections
          </button>
        </div>
      </div>
    `;

    // Update container
    container.innerHTML = html;

    // Update compliance percentage in header
    document.getElementById('osoCompliancePercentage').textContent = validation.compliancePercentage + '%';

    // Attach event listeners to OSO badges
    container.querySelectorAll('.oso-badge').forEach(badge => {
      badge.addEventListener('click', () => {
        const osoId = parseInt(badge.dataset.osoId);
        const requiredRobustness = badge.dataset.required;
        showOSOSelectionModal(osoId, requiredRobustness, () => {
          renderOSOGrid(containerId, soraVersion, sail, onOSOUpdate);
          if (onOSOUpdate) onOSOUpdate();
        });
      });

      // Hover effect
      badge.addEventListener('mouseenter', function() {
        this.style.transform = 'translateY(-4px)';
        this.style.boxShadow = '0 8px 16px rgba(0,0,0,0.3)';
      });
      badge.addEventListener('mouseleave', function() {
        this.style.transform = 'translateY(0)';
        this.style.boxShadow = '0 4px 8px rgba(0,0,0,0.2)';
      });
    });

    // Export button handler
    document.getElementById('osoExportBtn')?.addEventListener('click', () => {
      exportComplianceReport();
    });

    // Clear button handler
    document.getElementById('osoClearBtn')?.addEventListener('click', () => {
      if (confirm('⚠️ Clear all OSO selections? This cannot be undone.')) {
        currentTracker.selectedOSOs.clear();
        renderOSOGrid(containerId, soraVersion, sail, onOSOUpdate);
        if (onOSOUpdate) onOSOUpdate();
      }
    });

    console.log(`[OSO UI] ✅ Rendered ${requiredOSOs.length} OSOs for SAIL ${sail} (${validation.compliancePercentage}% complete)`);
  }

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * MODAL: OSO Selection with Robustness Level
   * ═══════════════════════════════════════════════════════════════════════════
   */
  function showOSOSelectionModal(osoId, requiredRobustness, onUpdate) {
    const osoDetails = OSOManager.getOSODetails(currentSoraVersion, osoId);
    if (!osoDetails) return;

    const currentSelection = currentTracker.selectedOSOs.get(osoId);
    const categoryInfo = OSOManager.OSO_CATEGORIES[osoDetails.category];

    // Create modal overlay
    const overlay = document.createElement('div');
    overlay.style.cssText = `
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0,0,0,0.7);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
      animation: fadeIn 0.2s;
    `;

    // Create modal
    const modal = document.createElement('div');
    modal.style.cssText = `
      background: white;
      border-radius: 16px;
      padding: 0;
      max-width: 700px;
      width: 90%;
      max-height: 85vh;
      overflow-y: auto;
      box-shadow: 0 20px 60px rgba(0,0,0,0.3);
      animation: slideUp 0.3s;
    `;

    modal.innerHTML = `
      <style>
        @keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
        @keyframes slideUp { from { transform: translateY(30px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
      </style>
      
      <!-- Header -->
      <div style="background:linear-gradient(135deg, ${categoryInfo.color} 0%, ${categoryInfo.color}dd 100%); color:white; padding:24px; border-radius:16px 16px 0 0;">
        <div style="display:flex; justify-content:space-between; align-items:flex-start;">
          <div style="flex:1;">
            <div style="font-size:12px; opacity:0.9; margin-bottom:4px;">${categoryInfo.icon} ${osoDetails.category}</div>
            <h3 style="margin:0; font-size:22px; line-height:1.3;">${osoDetails.number}: ${osoDetails.name}</h3>
          </div>
          <button id="closeModal" style="background:rgba(255,255,255,0.2); border:none; color:white; font-size:28px; cursor:pointer; width:40px; height:40px; border-radius:50%; transition:all 0.2s;">&times;</button>
        </div>
      </div>

      <!-- Body -->
      <div style="padding:24px;">
        
        <!-- Description -->
        <div style="margin-bottom:20px; padding:16px; background:#f5f5f5; border-radius:8px; border-left:4px solid ${categoryInfo.color};">
          <strong style="color:#333; font-size:14px;">📄 Description:</strong>
          <p style="margin:8px 0 0 0; color:#666; font-size:14px; line-height:1.5;">${osoDetails.description}</p>
        </div>

        <!-- JARUS Reference -->
        <div style="margin-bottom:20px; padding:12px; background:#e3f2fd; border-radius:6px;">
          <strong style="color:#1976d2; font-size:13px;">📚 JARUS Reference:</strong>
          <div style="color:#555; font-size:13px; margin-top:4px;">${osoDetails.jarusRef}</div>
        </div>

        <!-- Required Robustness -->
        <div style="margin-bottom:20px; padding:12px; background:#fff3e0; border-radius:6px;">
          <strong style="color:#e65100; font-size:13px;">🎯 Required for SAIL ${currentSAIL}:</strong>
          <span style="padding:4px 12px; background:#ff9800; color:white; border-radius:16px; font-weight:600; margin-left:8px; font-size:14px;">${requiredRobustness}</span>
        </div>

        <!-- Robustness Level Selection -->
        <div style="margin-bottom:20px;">
          <label style="display:block; margin-bottom:10px; font-weight:600; color:#333; font-size:14px;">
            ⚡ Select Robustness Level:
          </label>
          <select id="robustnessSelect" style="width:100%; padding:12px; border:2px solid ${categoryInfo.color}; border-radius:8px; font-size:14px; background:white;">
            <option value="">-- Select Robustness --</option>
            ${Object.entries(osoDetails.robustnessLevels).map(([level, desc]) => `
              <option value="${level}" ${currentSelection?.robustness === level ? 'selected' : ''}>
                ${level} - ${desc}
              </option>
            `).join('')}
          </select>
        </div>

        <!-- Evidence Types -->
        <div style="margin-bottom:20px;">
          <strong style="display:block; margin-bottom:8px; color:#333; font-size:14px;">📎 Evidence Types:</strong>
          <div style="display:flex; flex-wrap:wrap; gap:8px;">
            ${osoDetails.evidenceTypes.map(type => `
              <span style="padding:6px 12px; background:#e0e0e0; border-radius:16px; font-size:12px; color:#555;">
                ${type}
              </span>
            `).join('')}
          </div>
        </div>

        <!-- Evidence Reference Input -->
        <div style="margin-bottom:20px;">
          <label style="display:block; margin-bottom:8px; font-weight:600; color:#333; font-size:14px;">
            📋 Evidence Reference:
          </label>
          <input type="text" id="evidenceRef" 
                 style="width:100%; padding:12px; border:2px solid #ddd; border-radius:8px; font-size:14px;" 
                 placeholder="Document name, file path, or URL..."
                 value="${currentSelection?.evidence || ''}">
        </div>

        <!-- Notes Textarea -->
        <div style="margin-bottom:24px;">
          <label style="display:block; margin-bottom:8px; font-weight:600; color:#333; font-size:14px;">
            📝 Implementation Notes:
          </label>
          <textarea id="osoNotes" rows="4" 
                    style="width:100%; padding:12px; border:2px solid #ddd; border-radius:8px; font-size:14px; font-family:inherit; resize:vertical;" 
                    placeholder="Describe how this OSO is implemented, any limitations, or additional context...">${currentSelection?.notes || ''}</textarea>
        </div>

        <!-- Action Buttons -->
        <div style="display:flex; gap:12px; justify-content:flex-end;">
          ${currentSelection ? `
            <button id="removeOSO" style="padding:12px 24px; background:#f44336; color:white; border:none; border-radius:8px; cursor:pointer; font-weight:600; font-size:14px; transition:all 0.2s;">
              🗑️ Remove OSO
            </button>
          ` : ''}
          <button id="cancelBtn" style="padding:12px 24px; background:#9e9e9e; color:white; border:none; border-radius:8px; cursor:pointer; font-weight:600; font-size:14px; transition:all 0.2s;">
            Cancel
          </button>
          <button id="saveOSO" style="padding:12px 32px; background:#4caf50; color:white; border:none; border-radius:8px; cursor:pointer; font-weight:600; font-size:14px; transition:all 0.2s; box-shadow:0 4px 8px rgba(76,175,80,0.3);">
            ✅ Save OSO
          </button>
        </div>

      </div>
    `;

    overlay.appendChild(modal);
    document.body.appendChild(overlay);

    // Event handlers
    document.getElementById('closeModal').onclick = () => overlay.remove();
    document.getElementById('cancelBtn').onclick = () => overlay.remove();

    // Save button
    document.getElementById('saveOSO').onclick = () => {
      const robustness = document.getElementById('robustnessSelect').value;
      const evidence = document.getElementById('evidenceRef').value.trim();
      const notes = document.getElementById('osoNotes').value.trim();

      if (!robustness) {
        alert('⚠️ Please select a robustness level');
        return;
      }

      currentTracker.selectOSO(osoId, robustness, evidence, notes);
      console.log(`[OSO UI] ✅ Saved ${osoDetails.number} with ${robustness} robustness`);
      
      overlay.remove();
      if (onUpdate) onUpdate();
    };

    // Remove button (if OSO already selected)
    if (currentSelection) {
      document.getElementById('removeOSO').onclick = () => {
        if (confirm(`🗑️ Remove ${osoDetails.number} from selection?`)) {
          currentTracker.removeOSO(osoId);
          console.log(`[OSO UI] ❌ Removed ${osoDetails.number}`);
          
          overlay.remove();
          if (onUpdate) onUpdate();
        }
      };
    }

    // Hover effects for buttons
    ['saveOSO', 'cancelBtn', 'removeOSO', 'closeModal'].forEach(id => {
      const btn = document.getElementById(id);
      if (btn) {
        btn.addEventListener('mouseenter', function() {
          this.style.transform = 'translateY(-2px)';
          this.style.filter = 'brightness(1.1)';
        });
        btn.addEventListener('mouseleave', function() {
          this.style.transform = 'translateY(0)';
          this.style.filter = 'brightness(1)';
        });
      }
    });

    // Close on overlay click
    overlay.addEventListener('click', (e) => {
      if (e.target === overlay) {
        overlay.remove();
      }
    });
  }

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * EXPORT: Compliance Report as JSON
   * ═══════════════════════════════════════════════════════════════════════════
   */
  function exportComplianceReport() {
    if (!currentTracker) {
      alert('⚠️ No compliance data to export');
      return;
    }

    const report = currentTracker.exportComplianceReport();

    // Create download
    const blob = new Blob([JSON.stringify(report, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `OSO_Compliance_${report.soraVersion}_SAIL${report.sail}_${new Date().toISOString().split('T')[0]}.json`;
    a.click();
    URL.revokeObjectURL(url);

    console.log('[OSO UI] 📥 Exported compliance report:', a.download);
    alert(`✅ Compliance report exported:\n${a.download}`);
  }

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * IMPORT: Compliance Report from JSON
   * ═══════════════════════════════════════════════════════════════════════════
   */
  function importComplianceReport(jsonData) {
    if (!currentTracker) {
      console.error('[OSO UI] No tracker initialized');
      return false;
    }

    const success = currentTracker.importComplianceData(jsonData);

    if (success) {
      console.log('[OSO UI] 📤 Imported compliance data successfully');
      return true;
    }

    return false;
  }

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * STEP 42: UI COMPONENTS FOR COMPLEX ALGORITHMS
   * ═══════════════════════════════════════════════════════════════════════════
   */

  /**
   * Show dependency warnings panel
   */
  function showDependencyWarnings(containerId, selectedOSOs, soraVersion) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const result = OSOManager.validateDependencies(selectedOSOs, soraVersion);
    
    if (result.missing.length === 0 && result.warnings.length === 0) {
      container.innerHTML = `
        <div style="padding: 15px; background: #e8f5e9; border-left: 4px solid #4caf50; border-radius: 4px;">
          <strong style="color: #2e7d32;">✅ No dependency issues found</strong>
        </div>
      `;
      return;
    }

    let html = '<div style="margin-bottom: 20px;">';
    
    if (result.missing.length > 0) {
      html += `
        <div style="padding: 15px; background: #ffebee; border-left: 4px solid #f44336; border-radius: 4px; margin-bottom: 15px;">
          <strong style="color: #c62828; display: block; margin-bottom: 10px;">
            🔴 Missing Dependencies (${result.missing.length})
          </strong>
          <ul style="margin: 0; padding-left: 20px;">
      `;
      
      result.missing.forEach(dep => {
        html += `
          <li style="margin-bottom: 8px;">
            <strong>${dep.osoNumber}</strong> requires: <code>${dep.dependency}</code>
            <div style="font-size: 0.9em; color: #666; margin-top: 4px;">
              ${dep.reason}
            </div>
          </li>
        `;
      });
      
      html += '</ul></div>';
    }
    
    if (result.warnings.length > 0) {
      html += `
        <div style="padding: 15px; background: #fff3e0; border-left: 4px solid #ff9800; border-radius: 4px;">
          <strong style="color: #e65100; display: block; margin-bottom: 10px;">
            ⚠️ Warnings (${result.warnings.length})
          </strong>
          <ul style="margin: 0; padding-left: 20px;">
      `;
      
      result.warnings.forEach(warn => {
        html += `
          <li style="margin-bottom: 8px;">
            <strong>${warn.osoNumber}</strong>: ${warn.dependency}
          </li>
        `;
      });
      
      html += '</ul></div>';
    }
    
    html += '</div>';
    container.innerHTML = html;
  }

  /**
   * Show auto-recommendations panel
   */
  function showRecommendations(containerId, operationType, environment, sail, soraVersion) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const recommendations = OSOManager.recommendOSOs(operationType, environment, sail, soraVersion);
    
    if (recommendations.length === 0) {
      container.innerHTML = `
        <div style="padding: 15px; background: #f5f5f5; border-radius: 4px; text-align: center;">
          No specific recommendations for this operation type
        </div>
      `;
      return;
    }

    let html = `
      <div style="background: #e3f2fd; padding: 20px; border-radius: 8px; border-left: 4px solid #2196f3;">
        <h4 style="margin: 0 0 15px 0; color: #1565c0; display: flex; align-items: center; gap: 8px;">
          <span style="font-size: 1.3em;">💡</span>
          Recommended OSOs for ${operationType} / ${environment} / SAIL ${sail}
        </h4>
        <div style="display: grid; gap: 12px;">
    `;

    const priorityColors = {
      'CRITICAL': '#f44336',
      'HIGH': '#ff9800',
      'MEDIUM': '#2196f3',
      'LOW': '#4caf50'
    };

    recommendations.forEach(rec => {
      const priorityColor = priorityColors[rec.priority] || '#757575';
      
      html += `
        <div style="background: white; padding: 15px; border-radius: 6px; border-left: 4px solid ${priorityColor};">
          <div style="display: flex; justify-content: space-between; align-items: start; margin-bottom: 8px;">
            <strong style="color: #333;">${rec.number} - ${rec.name}</strong>
            <span style="background: ${priorityColor}; color: white; padding: 3px 8px; border-radius: 12px; font-size: 0.8em; font-weight: 600;">
              ${rec.priority}
            </span>
          </div>
          <div style="font-size: 0.9em; color: #666; margin-bottom: 8px;">
            ${rec.reason}
          </div>
          <div style="display: flex; gap: 15px; font-size: 0.85em; color: #888;">
            <span>Min Robustness: <strong style="color: #333;">${rec.minRobustness}</strong></span>
            <span>Category: <strong style="color: #333;">${rec.category}</strong></span>
          </div>
          ${rec.note ? `<div style="margin-top: 8px; padding: 8px; background: #fff3e0; border-radius: 4px; font-size: 0.85em; color: #e65100;">${rec.note}</div>` : ''}
        </div>
      `;
    });

    html += '</div></div>';
    container.innerHTML = html;
  }

  /**
   * Show evidence checklist for OSO
   */
  function showEvidenceChecklist(containerId, osoId, robustness, soraVersion) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const template = OSOManager.getEvidenceTemplate(osoId, robustness, soraVersion);
    
    let html = `
      <div style="background: #f5f5f5; padding: 20px; border-radius: 8px;">
        <h4 style="margin: 0 0 15px 0; color: #333;">
          📋 Evidence Requirements - OSO#${String(osoId).padStart(2, '0')} (${robustness} Robustness)
        </h4>
        
        <div style="margin-bottom: 15px;">
          <strong style="display: block; margin-bottom: 8px; color: #1976d2;">Required Documents:</strong>
          <ul style="margin: 0; padding-left: 20px;">
    `;

    template.requiredDocuments.forEach(doc => {
      html += `
        <li style="margin-bottom: 6px;">
          <label style="cursor: pointer; display: flex; align-items: center; gap: 8px;">
            <input type="checkbox" />
            <span>${doc}</span>
          </label>
        </li>
      `;
    });

    html += '</ul></div>';

    if (template.sections && template.sections.length > 0) {
      html += `
        <div style="margin-bottom: 15px;">
          <strong style="display: block; margin-bottom: 8px; color: #1976d2;">Required Sections:</strong>
          <ul style="margin: 0; padding-left: 20px; font-size: 0.9em; color: #666;">
      `;
      
      template.sections.forEach(section => {
        html += `<li style="margin-bottom: 4px;">${section}</li>`;
      });
      
      html += '</ul></div>';
    }

    if (template.standards) {
      html += `
        <div style="margin-bottom: 15px;">
          <strong style="display: block; margin-bottom: 8px; color: #1976d2;">Applicable Standards:</strong>
          <ul style="margin: 0; padding-left: 20px; font-size: 0.9em; color: #666;">
      `;
      
      template.standards.forEach(std => {
        html += `<li style="margin-bottom: 4px;">${std}</li>`;
      });
      
      html += '</ul></div>';
    }

    if (template.thirdParty) {
      html += `
        <div style="padding: 12px; background: #fff3e0; border-left: 3px solid #ff9800; border-radius: 4px; margin-bottom: 15px;">
          <strong style="color: #e65100;">🔍 Third-Party Requirement:</strong>
          <div style="font-size: 0.9em; color: #666; margin-top: 4px;">${template.thirdParty}</div>
        </div>
      `;
    }

    if (template.failureRates) {
      html += `
        <div style="padding: 12px; background: #e8f5e9; border-left: 3px solid #4caf50; border-radius: 4px;">
          <strong style="color: #2e7d32;">📊 Failure Rate Objectives:</strong>
          <ul style="margin: 8px 0 0 0; padding-left: 20px; font-size: 0.9em;">
      `;
      
      Object.entries(template.failureRates).forEach(([key, value]) => {
        html += `<li>${key}: ${value}</li>`;
      });
      
      html += '</ul></div>';
    }

    html += '</div>';
    container.innerHTML = html;
  }

  /**
   * Show validation errors panel
   */
  function showValidationErrors(containerId, selectedOSOs, sail, soraVersion, context) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const result = OSOManager.validateCrossOSO(selectedOSOs, sail, soraVersion, context);
    
    if (result.errors.length === 0 && result.warnings.length === 0) {
      container.innerHTML = `
        <div style="padding: 15px; background: #e8f5e9; border-left: 4px solid #4caf50; border-radius: 4px;">
          <strong style="color: #2e7d32;">✅ All validation checks passed</strong>
        </div>
      `;
      return;
    }

    let html = '<div style="margin-bottom: 20px;">';
    
    if (result.errors.length > 0) {
      html += `
        <div style="padding: 15px; background: #ffebee; border-left: 4px solid #f44336; border-radius: 4px; margin-bottom: 15px;">
          <strong style="color: #c62828; display: block; margin-bottom: 10px;">
            🔴 Validation Errors (${result.errors.length})
          </strong>
          <ul style="margin: 0; padding-left: 20px;">
      `;
      
      result.errors.forEach(err => {
        html += `
          <li style="margin-bottom: 10px;">
            <div style="font-weight: 600; color: #333;">${err.message}</div>
            ${err.reference ? `<div style="font-size: 0.85em; color: #666; margin-top: 4px;">Reference: ${err.reference}</div>` : ''}
          </li>
        `;
      });
      
      html += '</ul></div>';
    }
    
    if (result.warnings.length > 0) {
      html += `
        <div style="padding: 15px; background: #fff3e0; border-left: 4px solid #ff9800; border-radius: 4px;">
          <strong style="color: #e65100; display: block; margin-bottom: 10px;">
            ⚠️ Warnings (${result.warnings.length})
          </strong>
          <ul style="margin: 0; padding-left: 20px;">
      `;
      
      result.warnings.forEach(warn => {
        html += `
          <li style="margin-bottom: 8px;">${warn.message}</li>
        `;
      });
      
      html += '</ul></div>';
    }
    
    html += '</div>';
    container.innerHTML = html;
  }

  /**
   * Show SAIL filter panel
   */
  function showSAILFilter(containerId, sail, soraVersion) {
    const container = document.getElementById(containerId);
    if (!container) return;

    const filtered = OSOManager.filterOSOsBySAIL(sail, soraVersion);
    
    let html = `
      <div style="background: #fafafa; padding: 20px; border-radius: 8px; border: 2px solid #e0e0e0;">
        <h4 style="margin: 0 0 15px 0; color: #333;">
          🎯 SAIL ${sail} OSO Filter (${soraVersion})
        </h4>
        
        <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; margin-bottom: 15px;">
          <div style="text-align: center; padding: 15px; background: #e8f5e9; border-radius: 6px;">
            <div style="font-size: 2em; font-weight: 700; color: #2e7d32;">${filtered.required.length}</div>
            <div style="font-size: 0.9em; color: #666;">Required</div>
          </div>
          <div style="text-align: center; padding: 15px; background: #e3f2fd; border-radius: 6px;">
            <div style="font-size: 2em; font-weight: 700; color: #1565c0;">${filtered.optional.length}</div>
            <div style="font-size: 0.9em; color: #666;">Optional</div>
          </div>
          <div style="text-align: center; padding: 15px; background: #fafafa; border-radius: 6px;">
            <div style="font-size: 2em; font-weight: 700; color: #757575;">${filtered.notRequired.length}</div>
            <div style="font-size: 0.9em; color: #666;">Not Required</div>
          </div>
        </div>

        <details style="cursor: pointer;">
          <summary style="font-weight: 600; padding: 10px; background: #e8f5e9; border-radius: 4px; margin-bottom: 10px;">
            ✅ Required OSOs (${filtered.required.length})
          </summary>
          <div style="padding: 10px; font-size: 0.9em;">
    `;

    filtered.required.forEach(oso => {
      html += `
        <div style="padding: 8px; margin-bottom: 6px; background: white; border-left: 3px solid #4caf50; border-radius: 4px;">
          <strong>${oso.number}</strong> [${oso.minRobustness}] - ${oso.name}
        </div>
      `;
    });

    html += '</div></details>';

    if (filtered.notRequired.length > 0) {
      html += `
        <details style="cursor: pointer;">
          <summary style="font-weight: 600; padding: 10px; background: #fafafa; border-radius: 4px;">
            ❌ Not Required OSOs (${filtered.notRequired.length})
          </summary>
          <div style="padding: 10px; font-size: 0.9em;">
      `;

      filtered.notRequired.forEach(oso => {
        html += `
          <div style="padding: 8px; margin-bottom: 6px; background: white; border-left: 3px solid #ccc; border-radius: 4px;">
            <strong>${oso.number}</strong> - ${oso.name}
            <div style="font-size: 0.85em; color: #666; margin-top: 4px;">${oso.reason}</div>
          </div>
        `;
      });

      html += '</div></details>';
    }

    html += '</div>';
    container.innerHTML = html;
  }

  /**
   * ═══════════════════════════════════════════════════════════════════════════
   * PUBLIC API
   * ═══════════════════════════════════════════════════════════════════════════
   */
  return {
    // Step 41: Basic OSO Framework
    renderOSOGrid,
    showOSOSelectionModal,
    exportComplianceReport,
    importComplianceReport,
    getCurrentTracker: () => currentTracker,
    
    // Step 42: Complex Algorithms UI
    showDependencyWarnings,
    showRecommendations,
    showEvidenceChecklist,
    showValidationErrors,
    showSAILFilter
  };
})();

// Make globally available
window.OSOUI = OSOUI;
console.log('[OSO UI v2.0] ✅ Loaded - Interactive OSO Compliance Interface');
console.log('[OSO UI v2.0] 🧠 Step 42 UI Components: Dependency Warnings, Recommendations, Evidence Checklists, Validation Errors, SAIL Filtering');
