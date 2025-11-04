/**
 * PDRA/STS Category Handler for Skyworks Mission Planner
 * Handles dynamic form rendering based on selected operation category
 * Based on EU Regulation 2019/947 and JARUS SORA
 */

const CategoryHandler = (function() {
  'use strict';

  // Category definitions with required fields
  const CATEGORIES = {
    'SORA-2.0': {
      name: 'SORA 2.0',
      endpoint: '/api/sora/complete',
      fields: ['scenario', 'maxDimension', 'mtom', 'sheltering', 'controlledArea', 
               'isAtypicalSegregated', 'altitudeAGL_ft', 'controlledAirspace', 'urbanAir']
    },
    'SORA-2.5': {
      name: 'SORA 2.5',
      endpoint: '/api/sora/complete',
      fields: ['populationDensity', 'isControlledGroundArea', 'maxDimension', 'maxSpeed',
               'sheltering', 'isAtypicalSegregated', 'altitudeAGL_ft', 'controlledAirspace', 'urbanAir']
    },
    'STS-01': {
      name: 'STS-01 - VLOS over controlled ground',
      endpoint: '/api/pdra-sts/evaluate',
      requiredClass: 'C5',
      fields: [
        { name: 'uasClass', type: 'select', options: ['C5'], required: true, readonly: true },
        { name: 'initialGRC', type: 'info', value: 'Not specified by STS doc' },
        { name: 'finalGRC', type: 'select', options: ['GRC_3'], readonly: true },
        { name: 'initialARC', type: 'info', value: 'Not specified by STS doc' },
        { name: 'finalARC', type: 'select', options: ['ARC_b'], readonly: true },
        { name: 'sail', type: 'info', value: 'SAIL_II' }
      ]
    },
    'STS-02': {
      name: 'STS-02 - BVLOS with airspace observers',
      endpoint: '/api/pdra-sts/evaluate',
      requiredClass: 'C6',
      fields: [
        { name: 'uasClass', type: 'select', options: ['C6'], required: true, readonly: true },
        { name: 'initialGRC', type: 'info', value: 'Initial GRC = Final GRC = 3' },
        { name: 'finalGRC', type: 'select', options: ['GRC_3'], readonly: true },
        { name: 'initialARC', type: 'info', value: 'Not specified by STS doc' },
        { name: 'finalARC', type: 'select', options: ['ARC_b'], readonly: true },
        { name: 'sail', type: 'info', value: 'SAIL_II' }
      ]
    },
    'PDRA-S01': {
      name: 'PDRA-S01 - VLOS (custom UAS)',
      endpoint: '/api/pdra-sts/evaluate',
      fields: [
        { name: 'initialGRC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalGRC', type: 'select', options: ['GRC_3'], readonly: true },
        { name: 'initialARC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalARC', type: 'select', options: ['ARC_b'], readonly: true },
        { name: 'sail', type: 'info', value: 'SAIL_II' }
      ]
    },
    'PDRA-S02': {
      name: 'PDRA-S02 - BVLOS (custom UAS)',
      endpoint: '/api/pdra-sts/evaluate',
      fields: [
        { name: 'initialGRC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalGRC', type: 'select', options: ['GRC_3'], readonly: true },
        { name: 'initialARC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalARC', type: 'select', options: ['ARC_b'], readonly: true },
        { name: 'sail', type: 'info', value: 'SAIL_II' }
      ]
    },
    'PDRA-G01': {
      name: 'PDRA-G01 - Aerial survey BVLOS',
      endpoint: '/api/pdra-sts/evaluate',
      fields: [
        { name: 'initialGRC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalGRC', type: 'select', options: ['GRC_3'], readonly: true },
        { name: 'initialARC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalARC', type: 'select', options: ['ARC_b'], readonly: true },
        { name: 'sail', type: 'info', value: 'SAIL_II' }
      ]
    },
    'PDRA-G02': {
      name: 'PDRA-G02 - Aerial survey BVLOS extended',
      endpoint: '/api/pdra-sts/evaluate',
      fields: [
        { name: 'initialGRC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalGRC', type: 'select', options: ['GRC_3'], readonly: true },
        { name: 'initialARC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalARC', type: 'select', options: ['ARC_a'], readonly: true },
        { name: 'sail', type: 'info', value: 'SAIL_II' }
      ]
    },
    'PDRA-G03': {
      name: 'PDRA-G03 - BVLOS low altitude',
      endpoint: '/api/pdra-sts/evaluate',
      fields: [
        { name: 'initialGRC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalGRC', type: 'select', options: ['GRC_3'], readonly: true },
        { name: 'initialARC', type: 'info', value: 'Not specified by PDRA table' },
        { name: 'finalARC', type: 'select', options: ['ARC_a'], readonly: true },
        { name: 'sail', type: 'info', value: 'SAIL_II' }
      ]
    }
  };

  // Ground Risk Buffer calculation for STS-01
  function calculateGroundRiskBuffer(height, mtom) {
    // Table from UAS.STS-01.020(1)(c)(i)(C)
    const table = [
      { height: 30, light: 10, heavy: 20 },
      { height: 60, light: 15, heavy: 30 },
      { height: 90, light: 20, heavy: 45 },
      { height: 120, light: 25, heavy: 60 }
    ];

    const isHeavy = mtom > 10;
    
    // Find appropriate row or interpolate
    if (height <= 30) return isHeavy ? 20 : 10;
    if (height >= 120) return isHeavy ? 60 : 25;
    
    // Linear interpolation
    for (let i = 0; i < table.length - 1; i++) {
      if (height >= table[i].height && height < table[i+1].height) {
        const ratio = (height - table[i].height) / (table[i+1].height - table[i].height);
        const val1 = isHeavy ? table[i].heavy : table[i].light;
        const val2 = isHeavy ? table[i+1].heavy : table[i+1].light;
        return Math.round(val1 + ratio * (val2 - val1));
      }
    }
    
    return isHeavy ? 60 : 25;
  }

  // Render form fields based on category
  function renderFields(category, container) {
    const categoryDef = CATEGORIES[category];
    if (!categoryDef) return;

    container.innerHTML = '<p><em>Loading fields for ' + categoryDef.name + '...</em></p>';
    
    let html = '';
    
    if (typeof categoryDef.fields[0] === 'string') {
      // Simple field list (SORA 2.0/2.5) - keep existing rendering
      html += '<p><em>Use existing SORA form fields</em></p>';
    } else {
      // Complex field definitions (PDRA/STS)
      categoryDef.fields.forEach(field => {
        const fieldId = 'field_' + field.name;
  let fieldHtml = '<div class="field-group" data-field="' + field.name + '" style="margin-bottom:16px;">';
  fieldHtml += '<label for="' + fieldId + '" style="display:block;margin-bottom:6px;">';
  const labelKey = 'missionPlanner.categoryFields.' + field.name;
  const labelText = (window.i18n && window.i18n.t) ? window.i18n.t(labelKey) : null;
  const finalLabel = (labelText && labelText !== labelKey) ? labelText : formatFieldName(field.name);
  fieldHtml += '<strong style="color:#424242;font-size:14px;">' + finalLabel + '</strong>';
        if (field.required) fieldHtml += ' <span style="color:red">*</span>';
        if (field.unit) fieldHtml += ' <span style="color:#666">(' + field.unit + ')</span>';
        if (field.helpText) fieldHtml += '<br><small style="color:#666">' + field.helpText + '</small>';
        fieldHtml += '</label>';
        
        if (field.type === 'checkbox') {
          fieldHtml += '<input type="checkbox" id="' + fieldId + '" ';
          if (field.defaultValue) fieldHtml += 'checked ';
          if (field.readonly) fieldHtml += 'disabled ';
          fieldHtml += '/>';
        } else if (field.type === 'select') {
          fieldHtml += '<select id="' + fieldId + '" ';
          if (field.readonly) fieldHtml += 'disabled style="background:#e9ecef;cursor:not-allowed;color:#495057" ';
          fieldHtml += '>';
          (field.options || []).forEach(opt => {
            fieldHtml += '<option value="' + opt + '">' + opt + '</option>';
          });
          fieldHtml += '</select>';
          if (field.readonly) {
            fieldHtml += '<br><small style="color:#6c757d"><em>🔒 Fixed value per JARUS specification</em></small>';
          }
        } else if (field.type === 'number') {
          fieldHtml += '<input type="number" id="' + fieldId + '" ';
          if (field.min !== undefined) fieldHtml += 'min="' + field.min + '" ';
          if (field.max !== undefined) fieldHtml += 'max="' + field.max + '" ';
          if (field.readonly) fieldHtml += 'readonly ';
          if (field.calculated) fieldHtml += 'readonly style="background:#f0f0f0" ';
          fieldHtml += '/>';
        } else if (field.type === 'info') {
          const value = field.value || '';
          // Enhanced styling for different field types
          let bgColor = '#f8f9fa';
          let borderColor = '#0d6efd';
          let icon = '📋';
          let fontWeight = '500';
          
          // Special styling for GRC/ARC/SAIL results
          if (field.name.includes('GRC')) {
            bgColor = 'linear-gradient(135deg, #fff3e0 0%, #ffe0b2 100%)';
            borderColor = '#ff9800';
            icon = '🛡️';
            fontWeight = '700';
          } else if (field.name.includes('ARC')) {
            bgColor = 'linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%)';
            borderColor = '#2196f3';
            icon = '✈️';
            fontWeight = '700';
          } else if (field.name.toLowerCase().includes('sail')) {
            bgColor = 'linear-gradient(135deg, #e8f5e9 0%, #c8e6c9 100%)';
            borderColor = '#4caf50';
            icon = '⚓';
            fontWeight = '700';
          }
          
          fieldHtml += '<div id="' + fieldId + '" class="info" style="' +
            'padding:14px;' +
            'background:' + bgColor + ';' +
            'border-left:5px solid ' + borderColor + ';' +
            'border-radius:8px;' +
            'color:#212529;' +
            'font-weight:' + fontWeight + ';' +
            'font-size:15px;' +
            'box-shadow:0 2px 4px rgba(0,0,0,0.1);' +
            'transition:transform 0.2s,box-shadow 0.2s;' +
            '">';
          fieldHtml += icon + ' ' + value;
          fieldHtml += '</div>';
        } else {
          fieldHtml += '<input type="text" id="' + fieldId + '" ';
          if (field.placeholder) fieldHtml += 'placeholder="' + field.placeholder + '" ';
          if (field.readonly) fieldHtml += 'readonly ';
          fieldHtml += '/>';
        }
        
        fieldHtml += '</div>';
        html += fieldHtml;
      });
    }
    
    container.innerHTML = html;
    
    // Attach conditional logic handlers
    attachConditionalLogic(category);
  }

  function formatFieldName(name) {
    return name
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase())
      .replace(/Uas/g, 'UAS')
      .replace(/Aos/g, 'AOs')
      .replace(/Vlos/g, 'VLOS')
      .replace(/Bvlos/g, 'BVLOS')
      .replace(/Mtom/g, 'MTOM')
      .replace(/Atc/g, 'ATC')
      .replace(/Agl/g, 'AGL');
  }

  function attachConditionalLogic(category) {
    const categoryDef = CATEGORIES[category];
    if (!categoryDef || typeof categoryDef.fields[0] === 'string') return;

    // STS-01 buffer auto-calculation
    if (category === 'STS-01') {
      const heightInput = document.getElementById('field_maxHeight');
      const mtomInput = document.getElementById('field_mtom');
      const bufferInput = document.getElementById('field_groundRiskBuffer');
      
      const updateBuffer = () => {
        if (heightInput && mtomInput && bufferInput) {
          const h = parseFloat(heightInput.value) || 0;
          const m = parseFloat(mtomInput.value) || 0;
          bufferInput.value = calculateGroundRiskBuffer(h, m);
        }
      };
      
      if (heightInput) heightInput.addEventListener('input', updateBuffer);
      if (mtomInput) mtomInput.addEventListener('input', updateBuffer);
      setTimeout(updateBuffer, 100);
    }

    // STS-02 range validation based on AOs
    if (category === 'STS-02') {
      const aoInput = document.getElementById('field_airspaceObservers');
      const rangeInput = document.getElementById('field_flightRange');
      
      if (aoInput && rangeInput) {
        aoInput.addEventListener('input', () => {
          const aos = parseInt(aoInput.value) || 0;
          const maxRange = aos === 0 ? 1000 : 2000;
          rangeInput.max = maxRange;
          rangeInput.placeholder = aos === 0 ? 'Max 1000m without AOs' : 'Max 2000m with AOs';
        });
      }
    }

    // Conditional field visibility
    categoryDef.fields.forEach(field => {
      if (field.showIf) {
        const parts = field.showIf.split('==');
        const triggerField = parts[0];
        const triggerValue = parts[1];
        const triggerElement = document.getElementById('field_' + triggerField);
        const targetElement = document.querySelector('[data-field="' + field.name + '"]');
        
        if (triggerElement && targetElement) {
          const updateVisibility = () => {
            if (triggerElement.type === 'checkbox') {
              targetElement.style.display = triggerElement.checked ? 'block' : 'none';
            } else {
              const currentValue = triggerElement.value;
              targetElement.style.display = (currentValue == triggerValue) ? 'block' : 'none';
            }
          };
          
          triggerElement.addEventListener('change', updateVisibility);
          setTimeout(updateVisibility, 100);
        }
      }
    });
  }

  // Validate PDRA/STS fields before submission
  function validateFields(category) {
    const categoryDef = CATEGORIES[category];
    if (!categoryDef || typeof categoryDef.fields[0] === 'string') return { valid: true };

    const errors = [];
    const warnings = [];

    categoryDef.fields.forEach(field => {
      const fieldId = 'field_' + field.name;
      const element = document.getElementById(fieldId);
      
      if (!element) return;

      // Check required fields
      if (field.required) {
        const value = element.type === 'checkbox' ? element.checked : element.value;
        if (!value || value === '') {
          errors.push(`${formatFieldName(field.name)} is required`);
        }
      }

      // Validate readonly fields haven't been tampered
      if (field.readonly && field.options && field.options.length === 1) {
        const expectedValue = field.options[0];
        if (element.value !== expectedValue) {
          errors.push(`${formatFieldName(field.name)} must be ${expectedValue} (readonly)`);
        }
      }

      // Number range validation
      if (field.type === 'number' && element.value !== '') {
        const numVal = parseFloat(element.value);
        if (field.min !== undefined && numVal < field.min) {
          errors.push(`${formatFieldName(field.name)} must be >= ${field.min}`);
        }
        if (field.max !== undefined && numVal > field.max) {
          errors.push(`${formatFieldName(field.name)} must be <= ${field.max}`);
        }
      }

      // UAS Class validation for STS categories
      if (category === 'STS-01' && field.name === 'uasClass') {
        if (element.value !== 'C5') {
          errors.push('STS-01 requires UAS Class C5 (EU Regulation 2019/945)');
        }
      }
      if (category === 'STS-02' && field.name === 'uasClass') {
        if (element.value !== 'C6') {
          errors.push('STS-02 requires UAS Class C6 (EU Regulation 2019/945)');
        }
      }
    });

    return {
      valid: errors.length === 0,
      errors,
      warnings
    };
  }

  // Get field values for API submission
  function getFieldValues(category) {
    const categoryDef = CATEGORIES[category];
    if (!categoryDef || typeof categoryDef.fields[0] === 'string') return {};

    const values = { operationCategory: category };

    categoryDef.fields.forEach(field => {
      const fieldId = 'field_' + field.name;
      const element = document.getElementById(fieldId);
      
      if (!element) return;

      if (element.type === 'checkbox') {
        values[field.name] = element.checked;
      } else if (element.type === 'number') {
        values[field.name] = parseFloat(element.value) || 0;
      } else if (field.type === 'info') {
        // Info fields are read-only displays, extract from categoryDef
        if (field.value && typeof field.value === 'string') {
          const match = field.value.match(/(\d+|[A-Z]+_[a-z]+|SAIL_[IVX]+)/);
          if (match) values[field.name] = match[1];
        }
      } else {
        values[field.name] = element.value;
      }
    });

    console.log(`[CategoryHandler] Collected values for ${category}:`, values);
    return values;
  }

  // Public API
  return {
    CATEGORIES,
    renderFields,
    calculateGroundRiskBuffer,
    validateFields,
    getFieldValues
  };
})();

// Make globally available
window.CategoryHandler = CategoryHandler;
