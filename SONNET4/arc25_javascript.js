/**
 * SORA 2.5 ARC Fields - Data Collection and Validation
 * Add this code to your mission.html JavaScript section
 * Insert near line ~2789 where you collect form data
 */

/**
 * Collect and validate SORA 2.5 Enhanced ARC inputs
 * Returns null if validation fails or if not SORA 2.5
 */
function collectArc25Inputs(soraCategory) {
  // Only collect these fields for SORA 2.5
  if (soraCategory !== 'SORA-2.5') {
    console.log('ℹ️ SORA 2.0 selected - Skipping ARC 2.5 fields');
    return null;
  }
  
  console.log('📊 Collecting SORA 2.5 Enhanced ARC inputs...');
  
  // Field 1: U-space Services Available (boolean)
  const uSpaceServicesEl = document.getElementById('uSpaceServices');
  const uSpaceServices = uSpaceServicesEl?.value === 'true';
  
  // Field 2: Traffic Density Data Source (enum)
  const trafficDensitySourceEl = document.getElementById('trafficDensitySource');
  const trafficDensitySource = trafficDensitySourceEl?.value;
  
  // Field 3: Airspace Containment (enum)
  const airspaceContainment25El = document.getElementById('airspaceContainment25');
  const airspaceContainment25 = airspaceContainment25El?.value;
  
  // Field 4: Temporal Segregation (boolean checkbox)
  const temporalSegregationEl = document.getElementById('temporalSegregation');
  const temporalSegregation = temporalSegregationEl?.checked || false;
  
  // Field 5: Spatial Segregation (boolean checkbox)
  const spatialSegregationEl = document.getElementById('spatialSegregation');
  const spatialSegregation = spatialSegregationEl?.checked || false;
  
  // Validation
  const errors = [];
  
  if (!trafficDensitySource || trafficDensitySource === '') {
    errors.push('Traffic Density Data Source is required for SORA 2.5');
  }
  
  if (!airspaceContainment25 || airspaceContainment25 === '') {
    errors.push('Airspace Containment level is required for SORA 2.5');
  }
  
  // Specific validation: Expert judgment only valid for low density
  if (trafficDensitySource === 'Expert') {
    console.warn('⚠️ Expert judgment selected - Only valid for low traffic density areas');
    // You could add additional validation here if you have traffic density field
  }
  
  // Log validation warnings for segregation
  if (!temporalSegregation && !spatialSegregation) {
    console.warn('⚠️ No segregation mitigations selected - This may limit ARC reduction');
  }
  
  if (temporalSegregation) {
    console.log('✅ Temporal Segregation enabled - Requires time-based operational restrictions in ConOps');
  }
  
  if (spatialSegregation) {
    console.log('✅ Spatial Segregation enabled - Requires boundary-defined operational volume in ConOps');
  }
  
  // Display errors if any
  if (errors.length > 0) {
    const errorMessage = 'SORA 2.5 ARC Validation Errors:\n\n' + errors.join('\n');
    alert(errorMessage);
    console.error('❌ ARC 2.5 validation failed:', errors);
    return null;
  }
  
  // Build the data object matching backend model
  const arc25Data = {
    u_space_services_available: uSpaceServices,
    traffic_density_data_source: trafficDensitySource,
    airspace_containment: airspaceContainment25,
    temporal_segregation: temporalSegregation,
    spatial_segregation: spatialSegregation
  };
  
  console.log('✅ SORA 2.5 ARC inputs collected:', arc25Data);
  
  return arc25Data;
}

/**
 * Enhanced version switching function with validation
 * Replace or update your existing onCategoryChanged function
 */
function onCategoryChanged(category) {
  console.log(`🔄 Category changed to: ${category}`);
  
  const grc20 = document.getElementById('grc20');
  const grc25 = document.getElementById('grc25');
  const populationDensityField = document.getElementById('populationDensityField');
  const arc25Fields = document.getElementById('arc25Fields');
  
  if (category === 'SORA-2.0') {
    // Show SORA 2.0 fields
    if (grc20) grc20.style.display = 'block';
    if (grc25) grc25.style.display = 'none';
    if (populationDensityField) populationDensityField.style.display = 'none';
    if (arc25Fields) arc25Fields.style.display = 'none';
    
    console.log('✅ SORA 2.0 Mode: Basic fields visible, ARC 2.5 fields hidden');
    console.log('   → Using SORA 2.0 AMC (JAR_doc_06)');
  }
  
  if (category === 'SORA-2.5') {
    // Show SORA 2.5 fields
    if (grc20) grc20.style.display = 'none';
    if (grc25) grc25.style.display = 'block';
    if (populationDensityField) populationDensityField.style.display = 'block';
    if (arc25Fields) arc25Fields.style.display = 'block';
    
    console.log('✅ SORA 2.5 Mode: Enhanced fields visible');
    console.log('   → Using SORA 2.5 Main Body (JAR_doc_25) + Annexes');
    console.log('   → ARC 2.5 Strategic Mitigations available');
  }
}

/**
 * Example: Integration into your executeSora() function
 * This shows how to integrate the ARC 2.5 data collection
 */
function executeSora() {
  // Get selected category
  const category = document.getElementById('category')?.value;
  
  if (!category) {
    alert('Please select a SORA category (2.0 or 2.5)');
    return;
  }
  
  // ... collect other form data (GRC inputs, etc.) ...
  
  // Collect ARC 2.5 inputs if SORA 2.5 is selected
  const arc25Inputs = collectArc25Inputs(category);
  
  // If SORA 2.5 and validation failed, stop execution
  if (category === 'SORA-2.5' && arc25Inputs === null) {
    console.error('❌ SORA 2.5 execution stopped due to validation errors');
    return;
  }
  
  // Build request payload
  const requestPayload = {
    category: category,
    // ... other fields ...
  };
  
  // Add ARC 2.5 inputs only if present
  if (arc25Inputs !== null) {
    requestPayload.arc_inputs_25 = arc25Inputs;
  }
  
  console.log('📤 Sending SORA evaluation request:', requestPayload);
  
  // Make API call
  fetch('http://localhost:5210/api/sora/complete', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(requestPayload)
  })
  .then(response => {
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    return response.json();
  })
  .then(data => {
    console.log('✅ SORA evaluation successful:', data);
    // Display results...
  })
  .catch(error => {
    console.error('❌ SORA evaluation failed:', error);
    alert(`Error: ${error.message}`);
  });
}

/**
 * Initialize event listeners when page loads
 */
document.addEventListener('DOMContentLoaded', function() {
  // Category change listener
  const categorySelect = document.getElementById('category');
  if (categorySelect) {
    categorySelect.addEventListener('change', function() {
      onCategoryChanged(this.value);
    });
    
    // Trigger initial state
    if (categorySelect.value) {
      onCategoryChanged(categorySelect.value);
    }
  }
  
  // Add visual feedback for checkbox changes
  const temporalCheckbox = document.getElementById('temporalSegregation');
  const spatialCheckbox = document.getElementById('spatialSegregation');
  
  if (temporalCheckbox) {
    temporalCheckbox.addEventListener('change', function() {
      if (this.checked) {
        console.log('✅ Temporal Segregation selected');
        console.log('   → Remember: Requires documented time-based restrictions in ConOps');
      } else {
        console.log('ℹ️ Temporal Segregation deselected');
      }
    });
  }
  
  if (spatialCheckbox) {
    spatialCheckbox.addEventListener('change', function() {
      if (this.checked) {
        console.log('✅ Spatial Segregation selected');
        console.log('   → Remember: Requires documented boundary restrictions in ConOps');
      } else {
        console.log('ℹ️ Spatial Segregation deselected');
      }
    });
  }
  
  // Add warning for Expert traffic density source
  const trafficDensitySelect = document.getElementById('trafficDensitySource');
  if (trafficDensitySelect) {
    trafficDensitySelect.addEventListener('change', function() {
      if (this.value === 'Expert') {
        console.warn('⚠️ Expert judgment selected for traffic density');
        console.warn('   → Only valid for LOW traffic density areas');
        console.warn('   → Requires expert substantiation in your application');
        
        // Optional: Show a warning tooltip or modal
        alert(
          'Warning: Expert Judgment Selected\n\n' +
          'Expert judgment for traffic density is ONLY valid for LOW traffic density areas.\n\n' +
          'You must provide:\n' +
          '• Qualified expert assessment\n' +
          '• Substantiation of low traffic density\n' +
          '• Documentation in your SORA application\n\n' +
          'Reference: JAR_doc_25 Step #4'
        );
      }
    });
  }
  
  console.log('✅ SORA 2.5 ARC fields initialized');
});

/**
 * Utility function: Get current ARC 2.5 inputs (for debugging)
 */
function debugGetArc25Inputs() {
  const category = document.getElementById('category')?.value;
  const arc25 = collectArc25Inputs(category);
  console.table(arc25);
  return arc25;
}

// Make debug function available in console
window.debugGetArc25Inputs = debugGetArc25Inputs;
console.log('💡 Debug function available: debugGetArc25Inputs()');
