/**
 * SORA 2.5 ARC Fields - Data Collection and Validation
 * Integrated from Sonnet 4 - PROMPT 2
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
 * Initialize event listeners when page loads
 */
document.addEventListener('DOMContentLoaded', function() {
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
