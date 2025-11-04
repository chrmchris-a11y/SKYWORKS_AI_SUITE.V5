/**
 * Test Mission Loader for Skyworks Mission Planner
 * Loads pre-configured test missions for quick validation testing
 */

const TestMissionLoader = (function() {
  'use strict';

  let testMissions = [];

  // Load test missions from JSON file
  async function loadTestMissions() {
    try {
      const response = await fetch('test-missions.json');
      const data = await response.json();
      testMissions = data.testMissions || [];
      console.log('Loaded ' + testMissions.length + ' test missions');
      return testMissions;
    } catch (error) {
      console.error('Failed to load test missions:', error);
      return [];
    }
  }

  // Get all test missions
  function getAllMissions() {
    return testMissions;
  }

  // Get missions for specific category
  function getMissionsByCategory(category) {
    return testMissions.filter(m => m.category === category);
  }

  // Load mission into form
  function loadMissionIntoForm(missionId) {
    const mission = testMissions.find(m => m.id === missionId);
    if (!mission) {
      console.error('Mission not found:', missionId);
      return false;
    }

    console.log('Loading mission:', mission.name);

    // Set category first
    const categorySelect = document.getElementById('operationCategory');
    if (categorySelect) {
      categorySelect.value = mission.category;
      // Trigger change event to render correct fields
      const event = new Event('change', { bubbles: true });
      categorySelect.dispatchEvent(event);
      
      // Wait for fields to render, then populate
      setTimeout(() => populateFields(mission.parameters), 300);
    }

    return true;
  }

  // Populate form fields with mission parameters
  function populateFields(parameters) {
    Object.keys(parameters).forEach(key => {
      const fieldId = 'field_' + key;
      const element = document.getElementById(fieldId);
      
      if (element) {
        if (element.type === 'checkbox') {
          element.checked = parameters[key] === true;
        } else if (element.type === 'number') {
          element.value = parameters[key];
        } else {
          element.value = parameters[key];
        }
        
        // Trigger input event for any calculations
        const event = new Event('input', { bubbles: true });
        element.dispatchEvent(event);
      }
    });
  }

  // Render test mission selector UI
  function renderSelector(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;

    let html = '<div class="test-mission-selector">';
    html += '<label for="testMissionSelect"><strong>Load Test Mission:</strong></label>';
    html += '<select id="testMissionSelect" onchange="TestMissionLoader.onMissionSelected(this.value)">';
    html += '<option value="">-- Select Test Mission --</option>';

    testMissions.forEach(mission => {
      html += '<option value="' + mission.id + '">';
      html += mission.name + ' (' + mission.category + ')';
      html += '</option>';
    });

    html += '</select>';
    html += '<button type="button" onclick="TestMissionLoader.clearForm()" style="margin-left:10px">Clear Form</button>';
    html += '</div>';

    container.innerHTML = html;
  }

  // Event handler for mission selection
  function onMissionSelected(missionId) {
    if (missionId) {
      loadMissionIntoForm(missionId);
    }
  }

  // Clear all form fields
  function clearForm() {
    const categorySelect = document.getElementById('operationCategory');
    if (categorySelect) {
      categorySelect.value = 'SORA-2.0'; // Reset to default
      const event = new Event('change', { bubbles: true });
      categorySelect.dispatchEvent(event);
    }

    // Clear all input fields
    document.querySelectorAll('input[id^="field_"]').forEach(input => {
      if (input.type === 'checkbox') {
        input.checked = false;
      } else {
        input.value = '';
      }
    });

    // Clear test mission selector
    const testMissionSelect = document.getElementById('testMissionSelect');
    if (testMissionSelect) {
      testMissionSelect.value = '';
    }
  }

  // Initialize on page load
  async function init() {
    await loadTestMissions();
    renderSelector('testMissionContainer');
  }

  // Public API
  return {
    init,
    loadTestMissions,
    getAllMissions,
    getMissionsByCategory,
    loadMissionIntoForm,
    renderSelector,
    onMissionSelected,
    clearForm
  };
})();

// Make globally available
window.TestMissionLoader = TestMissionLoader;

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', () => TestMissionLoader.init());
} else {
  TestMissionLoader.init();
}
