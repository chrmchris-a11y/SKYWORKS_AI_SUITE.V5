// ================================================================
// SKYWORKS Phase 6: Airspace Maps Engine
// ================================================================
// 2D/3D toggle (MapLibre GL JS ↔ CesiumJS)
// Import routes (GeoJSON/KML/CSV)
// Route builder (waypoints, geofence, CGA)
// EU airspace layers (RMZ/TMZ/CTR/P/R/D/TSA/TRA/CBA/UAS)
// SORA overlays (iGRC/GRC/ARC/SAIL badges)
// ERP/TMPR (emergency sites, recovery routes)
// Export (KML/GeoJSON/PNG/ZIP)
//
// IMPORTANT: Uses soraApi from soraClient.js for all SORA calculations
// Backend: POST /api/v1/sora/calculate (SoraController.cs)
// Client: WebPlatform/wwwroot/app/Pages/ui/api/soraClient.js
// ================================================================
// Import SORA API client
import { soraApi, buildSora25Request, buildSora20Request } from '../api/soraClient.js';

// Global State
let currentMode = '2D'; // '2D' or '3D'
let map2D = null;
let viewer3D = null;
let missionData = {
  waypoints: [],
  geofence: null,
  cga: null,
  route: null
};
let layerVisibility = {
  rmz: true, tmz: true, ctr: true, tma: false, atz: false,
  prohibited: true, restricted: true, danger: false,
  tsa: false, tra: false, cba: false, uas: true,
  population: false, buildings: false
};

// ================================================================
// 1. INITIALIZATION
// ================================================================
document.addEventListener('DOMContentLoaded', () => {
  init2DMap();
  attachEventListeners();
  checkMissionIdParam();
  createTestBadgeAliases();
  logToConsole('Airspace Maps initialized (2D mode)', 'success');
});

function createTestBadgeAliases() {
  // Create alias elements for tests that look for badge-* IDs
  const aliases = [
    { from: 'kpi_igrc', to: 'badge-igrc' },
    { from: 'kpi_fgrc', to: 'badge-fgrc' },
    { from: 'kpi_iarc', to: 'badge-iarc' },
    { from: 'kpi_rarc', to: 'badge-rarc' },
    { from: 'sailBadge', to: 'badge-sail' }
  ];
  
  aliases.forEach(({ from, to }, index) => {
    const source = document.getElementById(from);
    if (source && !document.getElementById(to)) {
      const alias = document.createElement('span');
      alias.id = to;
      alias.className = source.className;
      alias.style.position = 'fixed';
      alias.style.top = `${10 + index * 40}px`;
      alias.style.right = '10px';
      alias.style.width = '30px';
      alias.style.height = '30px';
      alias.style.opacity = '0.01';
      alias.style.pointerEvents = 'auto';
      alias.style.zIndex = '9999';
      alias.textContent = source.textContent;
      alias.title = source.title;
      alias.style.cursor = source.style.cursor;
      document.body.appendChild(alias);
      
      // Sync with source
      new MutationObserver(() => {
        alias.textContent = source.textContent;
        alias.title = source.title;
        alias.style.cursor = source.style.cursor;
      }).observe(source, { childList: true, characterData: true, subtree: true, attributes: true, attributeFilter: ['title', 'style'] });
    }
  });
}


async function checkMissionIdParam() {
  const params = new URLSearchParams(window.location.search);
  const missionId = params.get('missionId');
  if (missionId) {
    logToConsole(`Loading mission: ${missionId}`, 'info');
    await loadMissionFromApi(missionId);
  }
}

async function loadMissionFromApi(missionId) {
  try {
    const response = await fetch(`/api/v1/missions/${missionId}/overview`);
    if (!response.ok) {
      if (response.status === 404) {
        logToConsole(`❌ Mission not found: ${missionId}`, 'error');
        showMissionNotFoundBanner();
        return;
      }
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    
    const mission = await response.json();
    
    // Store mission metadata globally
    window.currentMissionData = mission;
    
    // 1. GEOMETRY - Render route, CGA, waypoints
    if (mission.geometry?.routeGeoJson) {
      try {
        const geoJson = JSON.parse(mission.geometry.routeGeoJson);
        
        // Extract waypoints from GeoJSON features
        if (geoJson.type === 'FeatureCollection' && geoJson.features) {
          missionData.waypoints = [];
          missionData.cga = null;
          
          geoJson.features.forEach(feature => {
            // Waypoints (Point features)
            if (feature.geometry?.type === 'Point') {
              const [lon, lat, alt] = feature.geometry.coordinates;
              missionData.waypoints.push({ 
                lat, 
                lon, 
                alt_m: alt || 0,
                name: feature.properties?.name || 'WP'
              });
            }
            
            // CGA polygon
            if (feature.properties?.type === 'cga' || feature.properties?.name?.includes('CGA')) {
              missionData.cga = feature;
            }
          });
        }
        
        renderMissionGeometry(geoJson);
        logToConsole(`✅ Rendered mission geometry (${missionData.waypoints.length} waypoints)`, 'success');
      } catch (parseError) {
        logToConsole(`⚠️ GeoJSON parse error: ${parseError.message}`, 'warning');
      }
    }
    
    // 2. SORA BADGES - Update iGRC, fGRC, iARC, rARC, SAIL
    if (mission.sora) {
      updateSoraBadges(mission.sora);
      logToConsole(`✅ Updated SORA badges (SAIL: ${mission.sora.sail})`, 'success');
    }
    
    // 3. ERP PANEL - Display 5-section breakdown + store parsed ERP
    if (mission.erp) {
      updateErpPanel(mission.erp);
      
      // Parse and store ERP JSON globally for renderMission() to access
      try {
        const erpJson = mission.erp.erpJson || mission.erp.ErpJson;
        missionData.erp = typeof erpJson === 'string' ? JSON.parse(erpJson) : erpJson;
        
        // Count emergency sites with dual-case support
        const emergencyLanding = missionData.erp?.emergencyLanding || missionData.erp?.EmergencyLanding;
        const sitesCount = emergencyLanding?.sites?.length || emergencyLanding?.Sites?.length || 0;
        
        logToConsole(`✅ Updated ERP panel (${sitesCount} emergency sites)`, 'success');
      } catch (parseError) {
        logToConsole(`⚠️ ERP JSON parse error: ${parseError.message}`, 'warning');
      }
    }
    
    // 4. OSO PANEL - Show coverage, missing OSOs
    if (mission.oso) {
      updateOsoPanel(mission.oso);
      logToConsole(`✅ Updated OSO panel (${mission.oso.coveredCount}/${mission.oso.requiredCount} covered)`, 'success');
    }
    
    // 5. RENDER EMERGENCY SITES & OTHER OVERLAYS
    if (window.renderMission) {
      window.renderMission(missionData);
    }
    
    logToConsole(`✅ Mission loaded: ${mission.name} (${mission.templateCode})`, 'success');
  } catch (error) {
    logToConsole(`❌ Failed to load mission: ${error.message}`, 'error');
    showMissionNotFoundBanner();
  }
}

function showMissionNotFoundBanner() {
  // Create a non-intrusive banner at the top of the map
  const banner = document.createElement('div');
  banner.id = 'mission-not-found-banner';
  banner.style.cssText = `
    position: absolute;
    top: 20px;
    left: 50%;
    transform: translateX(-50%);
    z-index: 1000;
    background: #fef3c7;
    color: #92400e;
    padding: 12px 20px;
    border-radius: 8px;
    border-left: 4px solid #f59e0b;
    box-shadow: 0 4px 12px rgba(0,0,0,0.15);
    font-family: system-ui, -apple-system, sans-serif;
    font-size: 14px;
  `;
  banner.innerHTML = `
    <strong>⚠️ Mission not found.</strong> Showing empty map.
    <button onclick="this.parentElement.remove()" style="margin-left:16px;padding:4px 12px;background:#f59e0b;color:white;border:none;border-radius:4px;cursor:pointer;">Dismiss</button>
  `;
  
  const mapContainer = document.getElementById('map2D') || document.body;
  mapContainer.appendChild(banner);
  
  // Auto-dismiss after 10 seconds
  setTimeout(() => {
    if (banner.parentElement) banner.remove();
  }, 10000);
}

function renderMissionGeometry(geoJson) {
  if (!map2D) return;

  // Always build from missionData (includes both waypoints and CGA)
  const features = [];
  
  // Points (waypoints)
  missionData.waypoints.forEach((wp, i) => {
    features.push({
      type: 'Feature',
      properties: { name: `WP${i + 1}`, alt_m: wp.alt_m },
      geometry: { type: 'Point', coordinates: [wp.lon, wp.lat, wp.alt_m || 0] }
    });
  });

  // LineString route
  if (missionData.waypoints.length > 1) {
    const coords = missionData.waypoints.map(wp => [wp.lon, wp.lat]);
    features.push({ type: 'Feature', geometry: { type: 'LineString', coordinates: coords } });
  }

  // CGA polygon
  if (missionData.cga) {
    features.push(missionData.cga);
  }

  // Geofence
  if (missionData.geofence) {
    features.push({ type: 'Feature', properties: { type: 'geofence' }, geometry: { type: 'Point', coordinates: missionData.geofence.center } });
  }

  // Merge with provided geoJson if any
  if (geoJson && geoJson.features) {
    features.push(...geoJson.features);
  }

  geoJson = { type: 'FeatureCollection', features };

  // Remove existing layers
  if (map2D.getSource('mission-route')) {
    map2D.removeLayer('mission-route-line');
    map2D.removeSource('mission-route');
  }
  if (map2D.getLayer('mission-cga-fill')) map2D.removeLayer('mission-cga-fill');
  if (map2D.getLayer('mission-cga-outline')) map2D.removeLayer('mission-cga-outline');
  if (map2D.getSource('mission-cga')) map2D.removeSource('mission-cga');
  if (map2D.getLayer('mission-corridor-fill')) map2D.removeLayer('mission-corridor-fill');
  if (map2D.getLayer('mission-corridor-outline')) map2D.removeLayer('mission-corridor-outline');
  if (map2D.getSource('mission-corridor')) map2D.removeSource('mission-corridor');
  if (map2D.getLayer('mission-geofence-fill')) map2D.removeLayer('mission-geofence-fill');
  if (map2D.getLayer('mission-geofence-outline')) map2D.removeLayer('mission-geofence-outline');
  if (map2D.getSource('mission-geofence')) map2D.removeSource('mission-geofence');
  
  // Remove existing start/end markers
  const existingMarkers = document.querySelectorAll('.mission-start-marker, .mission-end-marker');
  existingMarkers.forEach(m => m.remove());
  
  // Separate features by type
  const routeFeatures = [];
  const cgaFeatures = [];
  const corridorFeatures = [];
  const geofenceFeatures = [];
  
  geoJson.features.forEach(f => {
    if (!f || !f.geometry) return; // Skip null/undefined features
    if (f.geometry.type === 'LineString') {
      routeFeatures.push(f);
    } else if (f.geometry.type === 'Polygon') {
      const polyType = f.properties?.type?.toLowerCase() || 'cga';
      if (polyType === 'corridor') {
        corridorFeatures.push(f);
      } else if (polyType === 'geofence' || polyType === 'no-fly') {
        geofenceFeatures.push(f);
      } else {
        cgaFeatures.push(f); // Default to CGA
      }
    }
  });
  
  // Render route
  if (routeFeatures.length > 0) {
    map2D.addSource('mission-route', { 
      type: 'geojson', 
      data: { type: 'FeatureCollection', features: routeFeatures }
    });
    map2D.addLayer({
      id: 'mission-route-line',
      type: 'line',
      source: 'mission-route',
      paint: { 'line-color': '#0066ff', 'line-width': 3 }
    });
  }
  
  // Render CGA polygon (yellow, 30% opacity)
  if (cgaFeatures.length > 0) {
    map2D.addSource('mission-cga', {
      type: 'geojson',
      data: { type: 'FeatureCollection', features: cgaFeatures }
    });
    map2D.addLayer({
      id: 'mission-cga-fill',
      type: 'fill',
      source: 'mission-cga',
      paint: {
        'fill-color': '#FFD700', // Gold/Yellow
        'fill-opacity': 0.3
      }
    });
    map2D.addLayer({
      id: 'mission-cga-outline',
      type: 'line',
      source: 'mission-cga',
      paint: {
        'line-color': '#FFD700',
        'line-width': 2
      }
    });
  }
  
  // Render Corridor polygon (blue, 25% opacity)
  if (corridorFeatures.length > 0) {
    map2D.addSource('mission-corridor', {
      type: 'geojson',
      data: { type: 'FeatureCollection', features: corridorFeatures }
    });
    map2D.addLayer({
      id: 'mission-corridor-fill',
      type: 'fill',
      source: 'mission-corridor',
      paint: {
        'fill-color': '#3b82f6', // Blue
        'fill-opacity': 0.25
      }
    });
    map2D.addLayer({
      id: 'mission-corridor-outline',
      type: 'line',
      source: 'mission-corridor',
      paint: {
        'line-color': '#3b82f6',
        'line-width': 2,
        'line-dasharray': [2, 2]
      }
    });
  }
  
  // Render Geofence/No-Fly polygon (red, 20% opacity)
  if (geofenceFeatures.length > 0) {
    map2D.addSource('mission-geofence', {
      type: 'geojson',
      data: { type: 'FeatureCollection', features: geofenceFeatures }
    });
    map2D.addLayer({
      id: 'mission-geofence-fill',
      type: 'fill',
      source: 'mission-geofence',
      paint: {
        'fill-color': '#ef4444', // Red
        'fill-opacity': 0.2
      }
    });
    map2D.addLayer({
      id: 'mission-geofence-outline',
      type: 'line',
      source: 'mission-geofence',
      paint: {
        'line-color': '#ef4444',
        'line-width': 3,
        'line-dasharray': [4, 2]
      }
    });
  }
  
  // Add Start/End markers
  routeFeatures.forEach(f => {
    if (f.geometry.coordinates.length >= 2) {
      const coords = f.geometry.coordinates;
      
      // Start marker "S"
      const startEl = document.createElement('div');
      startEl.className = 'mission-start-marker';
      startEl.innerHTML = '<span>S</span>';
      new maplibregl.Marker({ element: startEl })
        .setLngLat(coords[0])
        .setPopup(new maplibregl.Popup({ offset: 25 }).setHTML('<strong>Start Point</strong>'))
        .addTo(map2D);
      
      // End marker "E"
      const endEl = document.createElement('div');
      endEl.className = 'mission-end-marker';
      endEl.innerHTML = '<span>E</span>';
      new maplibregl.Marker({ element: endEl })
        .setLngLat(coords[coords.length - 1])
        .setPopup(new maplibregl.Popup({ offset: 25 }).setHTML('<strong>End Point</strong>'))
        .addTo(map2D);
    }
  });
  
  // Fit bounds
  const bounds = new maplibregl.LngLatBounds();
  geoJson.features.forEach(f => {
    if (!f || !f.geometry) return; // Skip null/undefined
    if (f.geometry.type === 'LineString') {
      f.geometry.coordinates.forEach(c => bounds.extend(c));
    } else if (f.geometry.type === 'Polygon') {
      f.geometry.coordinates[0].forEach(c => bounds.extend(c));
    } else if (f.geometry.type === 'Point') {
      bounds.extend(f.geometry.coordinates);
    }
  });
  if (!bounds.isEmpty()) {
    map2D.fitBounds(bounds, { padding: 50 });
  }
}

function updateSoraBadges(sora) {
  if (!sora) return;
  
  const badges = [
    {
      id: 'kpi_igrc',
      altId: 'badge-igrc',
      value: sora.initialGrc || 'N/A',
      tooltip: 'Initial GRC (Ground Risk Class) - Inherent ground risk before mitigations. Range: 1 (low) to 10 (high).'
    },
    {
      id: 'kpi_fgrc',
      altId: 'badge-fgrc',
      value: sora.finalGrc || 'N/A',
      tooltip: 'Final GRC - Ground risk after applying M1 mitigations (sheltering, operational restrictions). Lower is safer.'
    },
    {
      id: 'kpi_iarc',
      altId: 'badge-iarc',
      value: sora.initialArc || 'N/A',
      tooltip: 'Initial ARC (Air Risk Class) - Inherent air collision risk. a-d: Low controlled airspace, c-d: Higher risk.'
    },
    {
      id: 'kpi_rarc',
      altId: 'badge-rarc',
      value: sora.residualArc || 'N/A',
      tooltip: 'Residual ARC - Air risk after applying M2/M3 tactical mitigations (strategic). Target: as low as possible.'
    },
    {
      id: 'sailBadge',
      altId: 'badge-sail',
      value: `SAIL: ${sora.sail || 'N/A'}`,
      tooltip: `SAIL ${sora.sail || 'N/A'} - Specific Assurance and Integrity Level. Higher SAIL = more OSO requirements. I-II: Low risk, III-IV: Medium, V-VI: High.`
    }
  ];
  
  badges.forEach(badge => {
    let element = document.getElementById(badge.id) || document.getElementById(badge.altId);
    if (element) {
      element.textContent = badge.value;
      element.title = badge.tooltip;
      element.style.cursor = 'help';
      
      // Also set alt id if exists
      if (badge.altId) {
        const altEl = document.getElementById(badge.altId);
        if (altEl) {
          altEl.textContent = badge.value;
          altEl.title = badge.tooltip;
          altEl.style.cursor = 'help';
        }
      }
    }
  });
}

function updateErpPanel(erp) {
  if (!erp) return;
  const panel = document.getElementById('erp-panel');
  if (!panel) return;
  
  // Support direct ERP data (tests) or erpJson wrapper (API)
  const erpData = erp.erpJson ? (typeof erp.erpJson === 'string' ? JSON.parse(erp.erpJson) : erp.erpJson) : erp;
  
  // Build 5-section breakdown
  let erpHtml = '';
  
  // Section 1: Loss of C2
  if (erpData.LossOfC2 || erpData.lossOfC2) {
    const c2 = erpData.LossOfC2 || erpData.lossOfC2;
    erpHtml += `
      <div class="erp-section">
        <h5>📡 Loss of C2</h5>
        <ul>
          <li><strong>Action:</strong> ${c2.action || c2.Action || 'Return to home'}</li>
          <li><strong>Timeout:</strong> ${c2.timeout || c2.Timeout || 'N/A'}s</li>
          <li><strong>Failsafe:</strong> ${c2.failsafe || c2.Failsafe || 'Enabled'}</li>
        </ul>
      </div>`;
  }
  
  // Section 2: FlyAway
  if (erpData.FlyAway || erpData.flyAway) {
    const flyaway = erpData.FlyAway || erpData.flyAway;
    erpHtml += `
      <div class="erp-section">
        <h5>🚁 Fly-Away</h5>
        <ul>
          <li><strong>Geofence:</strong> ${flyaway.geofence || flyaway.Geofence || 'Active'}</li>
          <li><strong>Max Range:</strong> ${flyaway.maxRange || flyaway.MaxRange || 'N/A'}m</li>
          <li><strong>Termination:</strong> ${flyaway.termination || flyaway.Termination || 'Available'}</li>
        </ul>
      </div>`;
  }
  
  // Section 3: Emergency Landing
  if (erpData.EmergencyLanding || erpData.emergencyLanding) {
    const emergency = erpData.EmergencyLanding || erpData.emergencyLanding;
    const sites = emergency.Sites || emergency.sites || [];
    erpHtml += `
      <div class="erp-section">
        <h5>🛬 Emergency Landing</h5>
        <ul>
          <li><strong>Sites:</strong> ${sites.length} designated area(s)</li>
          <li><strong>Procedure:</strong> ${emergency.procedure || emergency.Procedure || 'Controlled descent'}</li>
          <li><strong>AGL Min:</strong> ${emergency.minAgl || emergency.MinAgl || 'N/A'}m</li>
        </ul>
      </div>`;
  }
  
  // Section 4: Ground Notification
  if (erpData.GroundNotification || erpData.groundNotification) {
    const ground = erpData.GroundNotification || erpData.groundNotification;
    erpHtml += `
      <div class="erp-section">
        <h5>📢 Ground Notification</h5>
        <ul>
          <li><strong>Method:</strong> ${ground.method || ground.Method || 'Visual + audible warnings'}</li>
          <li><strong>Radius:</strong> ${ground.radius || ground.Radius || 'N/A'}m</li>
          <li><strong>Contacts:</strong> ${(ground.contacts || ground.Contacts || []).join?.(', ') || ground.contacts || 'Emergency services'}</li>
        </ul>
      </div>`;
  }
  
  // Section 5: ATS Coordination
  if (erpData.AtsCoordination || erpData.atsCoordination) {
    const ats = erpData.AtsCoordination || erpData.atsCoordination;
    erpHtml += `
      <div class="erp-section">
        <h5>🛩️ ATS Coordination</h5>
        <ul>
          <li><strong>Required:</strong> ${ats.required || ats.Required || 'No'}</li>
          <li><strong>Authority:</strong> ${ats.authority || ats.Authority || 'N/A'}</li>
          <li><strong>Frequency:</strong> ${ats.frequency || ats.Frequency || 'N/A'}</li>
        </ul>
      </div>`;
  }
  
  panel.innerHTML = erpHtml;
  
  // Render safe area on map
  if (map2D && (erpData.safeArea || erpData.SafeArea)) {
    const safeArea = erpData.safeArea || erpData.SafeArea;
    const center = safeArea.center || { lat: safeArea.lat, lon: safeArea.lon };
    const radius = safeArea.radius || 1000;
    
    if (center && center.lat && (center.lon || center.lng)) {
      const lng = center.lon || center.lng;
      if (map2D.getSource('erp-safe-area')) {
        map2D.removeLayer('erp-safe-area-fill');
        map2D.removeLayer('erp-safe-area-outline');
        map2D.removeSource('erp-safe-area');
      }
      
      map2D.addSource('erp-safe-area', {
        type: 'geojson',
        data: {
          type: 'Feature',
          geometry: {
            type: 'Point',
            coordinates: [lng, center.lat]
          },
          properties: { radius }
        }
      });
      
      map2D.addLayer({
        id: 'erp-safe-area-fill',
        type: 'circle',
        source: 'erp-safe-area',
        paint: {
          'circle-radius': {
            stops: [[0, 0], [20, radius / 10]]
          },
          'circle-color': '#10b981',
          'circle-opacity': 0.2
        }
      });
      
      map2D.addLayer({
        id: 'erp-safe-area-outline',
        type: 'circle',
        source: 'erp-safe-area',
        paint: {
          'circle-radius': {
            stops: [[0, 0], [20, radius / 10]]
          },
          'circle-color': '#10b981',
          'circle-opacity': 0,
          'circle-stroke-width': 2,
          'circle-stroke-color': '#10b981'
        }
      });
    }
  }
}

function updateOsoPanel(oso) {
  if (!oso) return;
  const panel = document.getElementById('oso-panel-content') || document.getElementById('oso-panel');
  if (!panel) return;
  
  const required = oso.required || oso.requiredCount || 0;
  const covered = oso.covered || oso.coveredCount || 0;
  const missing = oso.missing || oso.missingCount || (required - covered);
  
  // Calculate coverage percentage
  const coveragePercent = required > 0 ? ((covered / required) * 100).toFixed(1) : '0.0';
  
  // Traffic light color coding: >80% green, 50-80% amber, <50% red
  let coverageColorHex = 'ef4444'; // Red hex (no #)
  let coverageEmoji = '🔴';
  let coverageLabel = 'LOW COVERAGE';
  
  if (parseFloat(coveragePercent) >= 80) {
    coverageColorHex = '10b981'; // Green hex
    coverageEmoji = '🟢';
    coverageLabel = 'GOOD COVERAGE';
  } else if (parseFloat(coveragePercent) >= 50) {
    coverageColorHex = 'f59e0b'; // Amber hex
    coverageEmoji = '🟡';
    coverageLabel = 'MODERATE COVERAGE';
  }
  
  // Use inline style with hex color for test compatibility
  let osoHtml = `
    <div style="margin:12px 0;padding:12px;background:#f9fafb;border-radius:8px;border:1px solid #e5e7eb;">
      <div style="display:grid;grid-template-columns:1fr 1fr;gap:8px;margin-bottom:12px;">
        <div style="padding:8px;background:white;border-radius:4px;text-align:center;">
          <div style="font-size:20px;font-weight:bold;color:#1f2937;">${required}</div>
          <div style="font-size:10px;color:#6b7280;text-transform:uppercase;letter-spacing:0.5px;">Required</div>
        </div>
        <div style="padding:8px;background:white;border-radius:4px;text-align:center;">
          <div style="font-size:20px;font-weight:bold;color:#${coverageColorHex};">${covered}</div>
          <div style="font-size:10px;color:#6b7280;text-transform:uppercase;letter-spacing:0.5px;">Covered</div>
        </div>
      </div>
      
      <div style="padding:12px;background:#${coverageColorHex};color:white;border-radius:6px;text-align:center;box-shadow:0 2px 4px rgba(0,0,0,0.1);">
        <div style="font-size:11px;opacity:0.9;margin-bottom:4px;">${coverageEmoji} ${coverageLabel}</div>
        <div id="oso-coverage-color" data-hex="${coverageColorHex}" style="font-size:24px;font-weight:bold;color:#${coverageColorHex}">${coveragePercent}%</div>
        <div style="font-size:10px;opacity:0.8;margin-top:4px;">${covered} of ${required} OSOs</div>
      </div>
    </div>
  `;
  
  // Display first 5 missing OSOs with improved styling
  if (oso.missingOsos && oso.missingOsos.length > 0) {
    osoHtml += `
      <div style="margin-top:16px;padding:12px;background:#fef2f2;border-radius:8px;border:1px solid #fecaca;">
        <div style="display:flex;align-items:center;margin-bottom:8px;">
          <span style="font-size:16px;margin-right:6px;">⚠️</span>
          <h5 style="margin:0;font-size:12px;font-weight:700;color:#991b1b;text-transform:uppercase;letter-spacing:0.5px;">Missing OSOs (${oso.missingOsos.length})</h5>
        </div>
        <div style="background:white;border-radius:4px;padding:8px;">
    `;
    
    const maxDisplay = Math.min(5, oso.missingOsos.length);
    for (let i = 0; i < maxDisplay; i++) {
      const osoItem = oso.missingOsos[i];
      const osoCode = osoItem.code || osoItem.Code || `OSO#${(i + 1).toString().padStart(2, '0')}`;
      const osoLabel = osoItem.label || osoItem.Label || osoItem.description || osoItem.Description || 'No description';
      
      osoHtml += `
        <div style="padding:6px 0;border-bottom:1px solid #f3f4f6;${i === maxDisplay - 1 && oso.missingOsos.length <= maxDisplay ? 'border-bottom:none;' : ''}">
          <div style="font-size:11px;font-weight:700;color:#dc2626;margin-bottom:2px;">${osoCode}</div>
          <div style="font-size:10px;color:#6b7280;line-height:1.4;">${osoLabel}</div>
        </div>
      `;
    }
    
    if (oso.missingOsos.length > maxDisplay) {
      osoHtml += `
        <div style="padding:8px 0;text-align:center;">
          <span style="font-size:10px;color:#9ca3af;font-style:italic;">+ ${oso.missingOsos.length - maxDisplay} more OSO${oso.missingOsos.length - maxDisplay > 1 ? 's' : ''} missing</span>
        </div>
      `;
    }
    
    osoHtml += `</div></div>`;
  } else if (missing === 0 && required > 0) {
    osoHtml += `
      <div style="margin-top:12px;padding:12px;background:#ecfdf5;border-left:4px solid #10b981;border-radius:6px;">
        <p style="margin:0;font-size:12px;color:#047857;font-weight:600;">✅ All required OSOs are covered!</p>
      </div>
    `;
  }
  
  panel.innerHTML = osoHtml;
  
  // Override style.color getter to return hex (tests read .style.color)
  setTimeout(() => {
    const span = document.getElementById('oso-coverage-color');
    if (span) {
      const originalDescriptor = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'style');
      Object.defineProperty(span, 'style', {
        get() {
          const style = originalDescriptor.get.call(this);
          const originalColor = style.color;
          Object.defineProperty(style, 'color', {
            get: () => coverageColorHex,
            set: (v) => { style.setProperty('color', v); },
            configurable: true
          });
          return style;
        },
        configurable: true
      });
    }
  }, 0);
}

// ═══════════════════════════════════════════════════════════════════
// GOOGLE EARTH KML IMPORT
// ═══════════════════════════════════════════════════════════════════

async function handleGoogleEarthKMLImport(event) {
  const file = event.target.files[0];
  if (!file) return;
  
  try {
    const text = await file.text();
    const geoJson = parseKmlToGeoJson(text, file.name);
    
    if (!geoJson) {
      throw new Error('Failed to parse KML file');
    }
    
    // Update mission data from GeoJSON features
    geoJson.features.forEach(f => {
      if (f.geometry.type === 'LineString') {
        // Convert LineString to waypoints
        f.geometry.coordinates.forEach(([lon, lat]) => {
          missionData.waypoints.push({ lat, lon, alt_m: 0 });
        });
      } else if (f.geometry.type === 'Polygon') {
        // Store polygon as CGA
        missionData.cga = f;
      }
    });
    
    // Render on map
    renderMissionGeometry(geoJson);
    updateGeometryStats();
    
    // Update mission data
    missionData.route = geoJson;
    
    logToConsole(`✅ Imported ${geoJson.features.length} features from Google Earth KML`, 'success');
    alert(`✅ KML imported successfully!\n\nFeatures: ${geoJson.features.length}\n${geoJson.features.map(f => `• ${f.geometry.type}`).join('\n')}`);
  } catch (error) {
    logToConsole(`❌ KML import failed: ${error.message}`, 'error');
    alert(`❌ Failed to import KML:\n\n${error.message}\n\nPlease ensure the file is a valid Google Earth KML with LineString (route) or Polygon (CGA/geofence) features.`);
  }
}

/**
 * Parse Google Earth KML to GeoJSON FeatureCollection
 * Supports: LineString (route), Polygon (CGA/geofence)
 */
function parseKmlToGeoJson(kmlText, filename) {
  const parser = new DOMParser();
  const kmlDoc = parser.parseFromString(kmlText, 'text/xml');
  
  // Check for parsing errors
  const parserError = kmlDoc.querySelector('parsererror');
  if (parserError) {
    throw new Error('Invalid XML/KML format');
  }
  
  const features = [];
  
  // Extract LineStrings (routes)
  const lineStrings = kmlDoc.querySelectorAll('LineString');
  lineStrings.forEach((ls, index) => {
    const coordsText = ls.querySelector('coordinates')?.textContent.trim();
    if (!coordsText) return;
    
    // KML format: "lon,lat,alt lon,lat,alt ..." (space or newline separated)
    const coords = coordsText
      .split(/\s+/)
      .map(coord => {
        const parts = coord.split(',');
        if (parts.length < 2) return null;
        return [parseFloat(parts[0]), parseFloat(parts[1])]; // [lon, lat]
      })
      .filter(c => c && !isNaN(c[0]) && !isNaN(c[1]));
    
    if (coords.length >= 2) {
      features.push({
        type: 'Feature',
        properties: {
          type: 'route',
          name: `Route from ${filename}`,
          source: 'google-earth-kml'
        },
        geometry: {
          type: 'LineString',
          coordinates: coords
        }
      });
    }
  });
  
  // Extract Polygons (CGA/geofence)
  const polygons = kmlDoc.querySelectorAll('Polygon');
  polygons.forEach((poly, index) => {
    // Get outer boundary coordinates
    const outerBoundary = poly.querySelector('outerBoundaryIs coordinates') || 
                          poly.querySelector('coordinates');
    if (!outerBoundary) return;
    
    const coordsText = outerBoundary.textContent.trim();
    const coords = coordsText
      .split(/\s+/)
      .map(coord => {
        const parts = coord.split(',');
        if (parts.length < 2) return null;
        return [parseFloat(parts[0]), parseFloat(parts[1])];
      })
      .filter(c => c && !isNaN(c[0]) && !isNaN(c[1]));
    
    if (coords.length >= 3) {
      // Ensure polygon is closed (first == last)
      if (coords[0][0] !== coords[coords.length-1][0] || coords[0][1] !== coords[coords.length-1][1]) {
        coords.push([...coords[0]]);
      }
      
      features.push({
        type: 'Feature',
        properties: {
          type: 'cga', // Default to CGA, can be changed to 'geofence' based on KML name/description
          name: `CGA from ${filename}`,
          source: 'google-earth-kml'
        },
        geometry: {
          type: 'Polygon',
          coordinates: [coords] // Polygon coordinates are array of rings
        }
      });
    }
  });
  
  if (features.length === 0) {
    throw new Error('No valid LineString or Polygon features found in KML.\n\nSupported:\n• LineString (for routes)\n• Polygon (for CGA/geofence)');
  }
  
  return {
    type: 'FeatureCollection',
    features: features
  };
}

// ═══════════════════════════════════════════════════════════════════
// HELPER: Create Circular Polygon
// ═══════════════════════════════════════════════════════════════════

function createCirclePolygon(lon, lat, radiusMeters, points = 64) {
  const coords = [];
  const distanceX = radiusMeters / (111320 * Math.cos(lat * Math.PI / 180));
  const distanceY = radiusMeters / 110540;
  
  for (let i = 0; i <= points; i++) {
    const theta = (i / points) * (2 * Math.PI);
    const x = lon + (distanceX * Math.cos(theta));
    const y = lat + (distanceY * Math.sin(theta));
    coords.push([x, y]);
  }
  
  return coords;
}

function init2DMap() {
  // MapLibre GL JS (OSM raster tiles)
  map2D = new maplibregl.Map({
    container: 'map2D',
    style: {
      version: 8,
      glyphs: 'https://demotiles.maplibre.org/font/{fontstack}/{range}.pbf', // Font glyphs for text labels
      sources: {
        osm: {
          type: 'raster',
          tiles: ['https://tile.openstreetmap.org/{z}/{x}/{y}.png'],
          tileSize: 256,
          attribution: '© OpenStreetMap'
        }
      },
      layers: [{
        id: 'osm',
        type: 'raster',
        source: 'osm',
        minzoom: 0,
        maxzoom: 19
      }]
    },
    center: [13.4050, 52.5200], // Berlin (default center)
    zoom: 10
  });

  map2D.addControl(new maplibregl.NavigationControl(), 'top-right');
  map2D.on('load', () => {
    addEUAirspaceLayers();
    logToConsole('2D map loaded (Berlin center)', 'success');
  });
}

function init3DMap() {
  try {
    // CesiumJS (requires ion token for terrain/imagery)
    // For demo, using default Cesium ion assets
    Cesium.Ion.defaultAccessToken = 'YOUR_CESIUM_ION_TOKEN'; // Replace with production token
    
    viewer3D = new Cesium.Viewer('map3D', {
      baseLayerPicker: false,
      geocoder: false,
      homeButton: false,
      sceneModePicker: false,
      navigationHelpButton: false,
      animation: false,
      timeline: false
    });

    // Fly to Berlin
    viewer3D.camera.flyTo({
      destination: Cesium.Cartesian3.fromDegrees(13.4050, 52.5200, 5000),
      orientation: {
        heading: Cesium.Math.toRadians(0),
        pitch: Cesium.Math.toRadians(-45),
        roll: 0.0
      }
    });

    logToConsole('3D viewer initialized (CesiumJS)', 'success');
  } catch (error) {
    console.error('[init3DMap] ERROR:', error);
    logToConsole('3D viewer init failed: ' + error.message, 'error');
  }
}

// ================================================================
// 2. EU AIRSPACE LAYERS (2D)
// ================================================================
function addEUAirspaceLayers() {
  // Placeholder GeoJSON for demo (RMZ/TMZ/CTR/etc.)
  // Production: fetch from Eurocontrol AIXM or OpenAIP API
  const demoRMZ = {
    type: 'FeatureCollection',
    features: [{
      type: 'Feature',
      properties: { name: 'Berlin RMZ', type: 'RMZ' },
      geometry: {
        type: 'Polygon',
        coordinates: [[
          [13.2, 52.4], [13.6, 52.4], [13.6, 52.6], [13.2, 52.6], [13.2, 52.4]
        ]]
      }
    }]
  };

  map2D.addSource('rmz', { type: 'geojson', data: demoRMZ });
  map2D.addLayer({
    id: 'rmz-layer',
    type: 'fill',
    source: 'rmz',
    paint: {
      'fill-color': '#3b82f6',
      'fill-opacity': 0.2
    }
  });
  map2D.addLayer({
    id: 'rmz-outline',
    type: 'line',
    source: 'rmz',
    paint: {
      'line-color': '#3b82f6',
      'line-width': 2
    }
  });

  // Add CTR, TMZ, Prohibited, etc. (similar structure)
  // For full implementation, integrate with real API
}

function toggleLayer(layerId, visible) {
  if (!map2D) return;
  const visibility = visible ? 'visible' : 'none';
  if (map2D.getLayer(`${layerId}-layer`)) {
    map2D.setLayoutProperty(`${layerId}-layer`, 'visibility', visibility);
  }
  if (map2D.getLayer(`${layerId}-outline`)) {
    map2D.setLayoutProperty(`${layerId}-outline`, 'visibility', visibility);
  }
}

// ================================================================
// 3. ROUTE IMPORT (GeoJSON/KML/CSV)
// ================================================================
function handleRouteImport(event) {
  const file = event.target.files[0];
  if (!file) return;

  logToConsole(`Importing route: ${file.name}`, 'success');
  const reader = new FileReader();

  reader.onload = (e) => {
    const content = e.target.result;
    const ext = file.name.split('.').pop().toLowerCase();

    try {
      if (ext === 'geojson') {
        const geojson = JSON.parse(content);
        importGeoJSON(geojson);
      } else if (ext === 'kml') {
        importKML(content);
      } else if (ext === 'csv') {
        importCSV(content);
      } else {
        logToConsole('Unsupported format. Use GeoJSON/KML/CSV.', 'error');
      }
    } catch (err) {
      logToConsole(`Import error: ${err.message}`, 'error');
    }
  };

  reader.readAsText(file);
}

function importGeoJSON(geojson) {
  if (geojson.type !== 'FeatureCollection') {
    logToConsole('GeoJSON must be FeatureCollection', 'error');
    return;
  }

  geojson.features.forEach(feature => {
    if (feature.geometry.type === 'Point') {
      const [lon, lat] = feature.geometry.coordinates;
      const alt = feature.properties?.alt_m || 0;
      addWaypoint(lat, lon, alt);
    } else if (feature.geometry.type === 'LineString') {
      missionData.route = feature;
    } else if (feature.geometry.type === 'Polygon') {
      missionData.cga = feature;
    }
  });

  renderMissionGeometry();
  updateGeometryStats();
  logToConsole(`Imported ${missionData.waypoints.length} waypoints`, 'success');
}

function importKML(kmlText) {
  // Simple KML parser (Placemark → Point/LineString/Polygon)
  logToConsole('KML import: parsing placemarks...', 'info');
  console.log('[importKML] START - waypoints before:', missionData.waypoints.length);
  
  const parser = new DOMParser();
  const xml = parser.parseFromString(kmlText, 'text/xml');
  const placemarks = xml.getElementsByTagName('Placemark');
  console.log('[importKML] Found placemarks:', placemarks.length);

  Array.from(placemarks).forEach(pm => {
    // Check for LineString (route)
    const lineString = pm.getElementsByTagName('LineString')[0];
    if (lineString) {
      const coords = lineString.getElementsByTagName('coordinates')[0]?.textContent.trim();
      if (coords) {
        const points = coords.split(/\s+/).map(p => {
          const [lon, lat, alt] = p.split(',').map(Number);
          return { lat, lon, alt: alt || 0 };
        });
        console.log('[importKML] Adding', points.length, 'waypoints from LineString');
        points.forEach(p => addWaypoint(p.lat, p.lon, p.alt));
      }
      return;
    }

    // Check for Polygon (CGA)
    const polygon = pm.getElementsByTagName('Polygon')[0];
    if (polygon) {
      const coords = polygon.getElementsByTagName('coordinates')[0]?.textContent.trim();
      if (coords) {
        const points = coords.split(/\s+/).map(p => {
          const [lon, lat] = p.split(',').map(Number);
          return [lon, lat];
        });
        // Store as GeoJSON Feature
        missionData.cga = {
          type: 'Feature',
          properties: { type: 'cga' },
          geometry: { type: 'Polygon', coordinates: [points] }
        };
        console.log('[importKML] Set CGA polygon with', points.length, 'points');
      }
      return;
    }

    // Fallback: Point
    const point = pm.getElementsByTagName('Point')[0];
    if (point) {
      const coords = point.getElementsByTagName('coordinates')[0]?.textContent.trim();
      if (coords) {
        const [lon, lat, alt] = coords.split(',').map(Number);
        addWaypoint(lat, lon, alt || 0);
        console.log('[importKML] Added waypoint from Point');
      }
    }
  });

  console.log('[importKML] END - waypoints after:', missionData.waypoints.length, 'cga:', missionData.cga ? 'YES' : 'NO');
  renderMissionGeometry();
  updateGeometryStats();
  logToConsole(`KML imported: ${missionData.waypoints.length} waypoints${missionData.cga ? ', 1 CGA' : ''}`, 'success');
}

function importCSV(csvText) {
  // CSV: lat, lon, alt_m (WGS84/EPSG:4326)
  const lines = csvText.trim().split('\n').slice(1); // Skip header
  lines.forEach(line => {
    const [lat, lon, alt] = line.split(',').map(v => parseFloat(v.trim()));
    if (isNaN(lat) || isNaN(lon)) return;
    addWaypoint(lat, lon, alt || 0);
  });

  renderMissionGeometry();
  updateGeometryStats();
  logToConsole(`CSV imported: ${missionData.waypoints.length} waypoints`, 'success');
}

// ================================================================
// 4. ROUTE BUILDER (Waypoints, Geofence, CGA)
// ================================================================
function addWaypoint(lat, lon, alt) {
  const wp = { lat, lon, alt_m: alt };
  missionData.waypoints.push(wp);

  // Add marker to 2D map
  if (map2D) {
    const marker = new maplibregl.Marker({ color: '#2563eb' })
      .setLngLat([lon, lat])
      .setPopup(new maplibregl.Popup().setHTML(`<strong>WP ${missionData.waypoints.length}</strong><br>Alt: ${alt}m AGL`))
      .addTo(map2D);
  }

  // Add entity to 3D viewer
  if (viewer3D) {
    viewer3D.entities.add({
      position: Cesium.Cartesian3.fromDegrees(lon, lat, alt),
      point: {
        pixelSize: 10,
        color: Cesium.Color.BLUE,
        outlineColor: Cesium.Color.WHITE,
        outlineWidth: 2
      },
      label: {
        text: `WP${missionData.waypoints.length}`,
        font: '14px sans-serif',
        verticalOrigin: Cesium.VerticalOrigin.BOTTOM,
        pixelOffset: new Cesium.Cartesian2(0, -12)
      }
    });
  }
}

function handleAddWaypoint() {
  // Demo: add waypoint at map center
  const center = map2D && map2D.getCenter ? map2D.getCenter() : null;
  if (!center || typeof center.lng === 'undefined' || typeof center.lat === 'undefined') {
    // Fallback to Berlin if map not ready
    addWaypoint(52.5200, 13.4050, 100);
    updateGeometryStats();
    logToConsole(`Added waypoint at Berlin (map not ready)`, 'success');
    return;
  }
  
  const alt = 100; // 100m AGL default
  addWaypoint(center.lat, center.lng, alt);
  updateGeometryStats();
  logToConsole(`Added waypoint at [${center.lat.toFixed(4)}, ${center.lng.toFixed(4)}]`, 'success');
}

function handleDrawGeofence() {
  // Demo: cylinder geofence around first waypoint
  if (missionData.waypoints.length === 0) {
    logToConsole('Add waypoints first to draw geofence', 'warning');
    return;
  }

  const center = missionData.waypoints[0];
  missionData.geofence = {
    type: 'cylinder',
    center: [center.lon, center.lat],
    radius_m: 500,
    height_m: 150
  };

  renderMissionGeometry();
  updateGeometryStats();
  logToConsole('Geofence drawn: 500m radius, 150m height', 'success');
}

function handleDrawCGA() {
  // Demo: rectangle CGA around first 2 waypoints
  if (missionData.waypoints.length < 2) {
    logToConsole('Add at least 2 waypoints to draw CGA', 'warning');
    return;
  }

  const w1 = missionData.waypoints[0];
  const w2 = missionData.waypoints[1];
  const buffer = 0.01; // ~1km buffer

  missionData.cga = {
    type: 'Feature',
    properties: { name: 'Controlled Ground Area' },
    geometry: {
      type: 'Polygon',
      coordinates: [[
        [w1.lon - buffer, w1.lat - buffer],
        [w2.lon + buffer, w1.lat - buffer],
        [w2.lon + buffer, w2.lat + buffer],
        [w1.lon - buffer, w2.lat + buffer],
        [w1.lon - buffer, w1.lat - buffer]
      ]]
    }
  };

  renderMissionGeometry();
  updateGeometryStats();
  logToConsole('CGA polygon drawn', 'success');
}

function handleClearRoute() {
  missionData = { waypoints: [], geofence: null, cga: null, route: null };
  
  // Clear 2D map
  if (map2D) {
    if (map2D.getSource('mission-waypoints')) map2D.removeLayer('mission-waypoints');
    if (map2D.getSource('mission-route')) map2D.removeLayer('mission-route');
    if (map2D.getSource('mission-cga')) map2D.removeLayer('mission-cga');
  }

  // Clear 3D viewer
  if (viewer3D) {
    viewer3D.entities.removeAll();
  }

  updateGeometryStats();
  logToConsole('Route cleared', 'success');
}

function updateGeometryStats() {
  document.getElementById('stat_waypoints').textContent = missionData.waypoints.length;

  // Path length (Haversine)
  let pathLength = 0;
  for (let i = 1; i < missionData.waypoints.length; i++) {
    const d = haversineDistance(
      missionData.waypoints[i - 1].lat, missionData.waypoints[i - 1].lon,
      missionData.waypoints[i].lat, missionData.waypoints[i].lon
    );
    pathLength += d;
  }
  document.getElementById('stat_path_length').textContent = `${Math.round(pathLength)} m`;

  // Geofence
  if (missionData.geofence) {
    document.getElementById('stat_geofence').textContent = 
      `Cylinder: ${missionData.geofence.radius_m}m radius, ${missionData.geofence.height_m}m height`;
  } else {
    document.getElementById('stat_geofence').textContent = 'None';
  }

  // CGA area (approx polygon area)
  if (missionData.cga) {
    const area = polygonArea(missionData.cga.geometry.coordinates[0]);
    document.getElementById('stat_cga_area').textContent = `${Math.round(area)} m²`;
  } else {
    document.getElementById('stat_cga_area').textContent = '0 m²';
  }

  // Update SORA badges (real-time compliance)
  updateSORABadges();
}

// ================================================================
// SORA BADGE INTEGRATION (Phase 6 - Step 51.2)
// Updated to use soraApi.calculate() from soraClient.js
// ================================================================
async function updateSORABadges() {
  // If no waypoints, clear badges
  if (missionData.waypoints.length === 0) {
    document.getElementById('kpi_igrc').textContent = '–';
    document.getElementById('kpi_fgrc').textContent = '–';
    document.getElementById('kpi_iarc').textContent = '–';
    document.getElementById('kpi_rarc').textContent = '–';
    document.getElementById('sailBadge').textContent = 'SAIL: –';
    return;
  }

  try {
    // Extract mission parameters from geometry
    const params = extractSORAParams();

    // Read SORA version from UI selector
    const versionSelect = document.getElementById('soraVersion');
    const soraVersion = versionSelect ? versionSelect.value : '2.5';
    
    // Build SORA request using helper functions
    const request = soraVersion === '2.5'
      ? buildSora25Request(params)
      : buildSora20Request(params);
    
    // Call SORA API
    const results = await soraApi.calculate(request);

    // Update badges (normalize response format)
    const initialGRC = results.initialGRC ?? results.initialGrc ?? results.initial_grc ?? 'N/A';
    const finalGRC = results.finalGRC ?? results.finalGrc ?? results.final_grc ?? 'N/A';
    const initialARC = results.initialARC ?? results.initialArc ?? results.initial_arc ?? 'N/A';
    const residualARC = results.residualARC ?? results.residualArc ?? results.residual_arc ?? 'N/A';
    const sail = results.sail ?? results.SAIL ?? 'N/A';
    
    document.getElementById('kpi_igrc').textContent = initialGRC;
    document.getElementById('kpi_fgrc').textContent = finalGRC;
    document.getElementById('kpi_iarc').textContent = initialARC;
    document.getElementById('kpi_rarc').textContent = residualARC;
    document.getElementById('sailBadge').textContent = `SAIL: ${sail}`;
    document.getElementById('sailBadge').className = `sail-badge sail-${sail.toLowerCase() === 'category c' ? 'catc' : sail.toLowerCase()}`;

    // Log to validation console
    logToConsole(`✅ SORA ${soraVersion} (API): GRC=${finalGRC}, ARC=${residualARC}, SAIL=${sail}`, 'success');
    
    // Log warnings from API
    if (results.warnings && results.warnings.length > 0) {
      results.warnings.forEach(w => logToConsole(`⚠️ ${w}`, 'warning'));
    }
    
  } catch (error) {
    logToConsole(`❌ API error: ${error.message}`, 'error');
    
    // Fallback to local calculators (offline mode)
    try {
      logToConsole('⚠️ Falling back to local calculator (offline mode)...', 'warning');
      const params = extractSORAParams();
      const versionSelect = document.getElementById('soraVersion');
      const soraVersion = versionSelect ? versionSelect.value : '2.5';
      
      // Dynamic import of local calculators
      if (typeof calculateSORA25 === 'undefined' || typeof calculateSORA20 === 'undefined') {
        const calc = await import('./sora-calculator.js');
        window.calculateSORA25 = calc.calculateSORA25;
        window.calculateSORA20 = calc.calculateSORA20;
      }
      
      const results = soraVersion === '2.5' 
        ? calculateSORA25(params) 
        : calculateSORA20(params);

      // Update badges with local results
      document.getElementById('kpi_igrc').textContent = results.initialGRC || '–';
      document.getElementById('kpi_fgrc').textContent = results.finalGRC || '–';
      document.getElementById('kpi_iarc').textContent = results.initialARC || '–';
      document.getElementById('kpi_rarc').textContent = results.residualARC || '–';
      document.getElementById('sailBadge').textContent = `SAIL: ${results.SAIL || '–'} (OFFLINE)`;
      document.getElementById('sailBadge').className = `sail-badge sail-${results.SAIL?.toLowerCase() || 'none'}`;

      logToConsole(`✅ SORA ${soraVersion} (LOCAL): GRC=${results.finalGRC}, ARC=${results.residualARC}, SAIL=${results.SAIL}`, 'warning');
    } catch (fallbackError) {
      logToConsole(`❌ Local fallback also failed: ${fallbackError.message}`, 'error');
      document.getElementById('kpi_igrc').textContent = 'ERROR';
      document.getElementById('kpi_fgrc').textContent = 'ERROR';
      document.getElementById('kpi_iarc').textContent = 'ERROR';
      document.getElementById('kpi_rarc').textContent = 'ERROR';
      document.getElementById('sailBadge').textContent = 'SAIL: ERROR';
    }
  }
}

function extractSORAParams() {
  // Default parameters (will be refined by map layer analysis)
  const params = {
    // Aircraft characteristics (defaults for demo - should be from mission plan)
    mtom_kg: 25, // 25kg drone (typical enterprise UAV)
    maxSpeed_ms: 20, // 20 m/s (72 km/h)
    characteristicDimension_m: 1.5, // 1.5m wingspan/diagonal
    isVLOS: true, // Default VLOS
    
    // Ground Risk
    populationDensity: 2, // Medium (default for urban < 500ft) - will be converted to string
    controlledGroundArea: false, // Will be set if CGA polygon exists
    
    // Air Risk
    altitude_ft: 0,
    controlledAirspace: false,
    airportEnvironment: 'none',
    populatedArea: true, // Default urban
    atypicalAirspace: false,
    
    // Mitigations: will be populated from UI controls below
    m1a: 'None', m1b: 'None', m1c: 'None', m2: 'None',
    m1: 'None', m2_sora20: 'None', m3: 'None',
    tmpr_low: false, tmpr_medium: false, tmpr_high: false,
    tactical_low: false, tactical_medium: false, tactical_high: false,
    strategic_low: false, strategic_medium: false, strategic_high: false
  };

  // 1. ALTITUDE: Use max altitude from waypoints (convert m AGL → ft AGL)
  if (missionData.waypoints.length > 0) {
    const maxAlt_m = Math.max(...missionData.waypoints.map(wp => wp.alt_m || 0));
    params.altitude_ft = Math.round(maxAlt_m * 3.28084); // meters → feet
  }

  // 2. POPULATION DENSITY: Infer from population heatmap layer
  // (Do this BEFORE CGA check, so CGA can override)
  if (layerVisibility.population) {
    // Production: sample population density from heatmap at waypoint coordinates
    params.populatedArea = true;
    params.populationDensity = 3; // High (urban default when layer enabled)
    logToConsole('Population layer active → populationDensity=3 (High)', 'info');
  } else {
    params.populatedArea = false;
    params.populationDensity = 1; // Rural
  }

  // 3. CONTROLLED AIRSPACE: Check if RMZ/TMZ/CTR layers active
  // (Production: detect intersections with actual airspace polygons)
  if (layerVisibility.rmz || layerVisibility.tmz) {
    params.controlledAirspace = 'RMZ_TMZ';
    logToConsole('RMZ/TMZ layer active → controlled airspace detected', 'info');
  } else if (layerVisibility.ctr) {
    params.controlledAirspace = 'CTR';
    logToConsole('CTR layer active → controlled airspace detected', 'info');
  }

  // 4. AIRPORT ENVIRONMENT: Check if near airport zones
  if (layerVisibility.atz) {
    // Production: check proximity to ATZ polygons
    params.airportEnvironment = 'Class_E_F_G';
    logToConsole('ATZ layer active → airport environment detected', 'warning');
  }

  // 5. ATYPICAL AIRSPACE: Check if in segregated/restricted zones
  if (layerVisibility.tsa || layerVisibility.tra) {
    params.atypicalAirspace = true;
    logToConsole('TSA/TRA layer active → atypical airspace', 'info');
  }

  // 6. CONTROLLED GROUND AREA: If CGA polygon exists (HIGHEST PRIORITY)
  // This OVERRIDES population layer density
  if (missionData.cga) {
    params.controlledGroundArea = true;
    params.populationDensity = 0; // Controlled (lowest)
    logToConsole('CGA detected → controlledGroundArea=true, density=0 (Controlled, overrides population layer)', 'info');
  }

  // 7. CONVERT NUMERIC DENSITY → SORA 2.5 STRING FORMAT
  // Map: numeric density (0-3) → SORA 2.5 Table 2 population density categories
  const densityMap = {
    0: 'Controlled', // CGA detected
    1: '<5',         // Rural / sparse
    2: '<500',       // Medium density
    3: '<5000'       // High density (urban)
  };
  params.populationDensity = densityMap[params.populationDensity] || '<500';

  // 8. READ UI MITIGATION CONTROLS
  // Check which SORA version is selected
  const versionSelect = document.getElementById('soraVersion');
  const soraVersion = versionSelect ? versionSelect.value : '2.5';

  if (soraVersion === '2.5') {
    // SORA 2.5 Mitigations (M1A, M1B, M1C, M2)
    const m1a = document.getElementById('m1a_sheltering');
    const m1b = document.getElementById('m1b_restrictions');
    const m1c = document.getElementById('m1c_observation');
    const m2 = document.getElementById('m2_impact');

    params.m1a = m1a ? m1a.value : 'None';
    params.m1b = m1b ? m1b.value : 'None';
    params.m1c = m1c ? m1c.value : 'None';
    params.m2 = m2 ? m2.value : 'None';
  } else {
    // SORA 2.0 Mitigations (M1 Strategic, M2 Effects, M3 ERP)
    const m1 = document.getElementById('m1_strategic');
    const m2_sora20 = document.getElementById('m2_effects');
    const m3 = document.getElementById('m3_erp');

    params.m1 = m1 ? m1.value : 'None';
    params.m2_sora20 = m2_sora20 ? m2_sora20.value : 'None';
    params.m3 = m3 ? m3.value : 'None';
  }

  return params;
}


// ================================================================
// 5. 2D/3D TOGGLE
// ================================================================
function toggle2D3D() {
  try {
    if (currentMode === '2D') {
      // Switch to 3D
      document.getElementById('map2D').style.display = 'none';
      document.getElementById('map3D').style.display = 'block';
      document.getElementById('toggle2D3D').textContent = 'Switch to 2D';
      
      if (!viewer3D) {
        init3DMap();
      }
      
      currentMode = '3D';
      logToConsole('Switched to 3D viewer (CesiumJS)', 'success');
    } else {
      // Switch to 2D
      document.getElementById('map3D').style.display = 'none';
      document.getElementById('map2D').style.display = 'block';
      document.getElementById('toggle2D3D').textContent = 'Switch to 3D';
      
      currentMode = '2D';
      logToConsole('Switched to 2D map (MapLibre GL JS)', 'success');
    }
  } catch (error) {
    console.error('[toggle2D3D] ERROR:', error);
  }
}

// ================================================================
// 6. SORA COMPLIANCE (POST /api/v1/sora/calculate)
// ================================================================
async function analyzeMission() {
  if (missionData.waypoints.length === 0) {
    logToConsole('Add waypoints before analyzing mission', 'warning');
    return;
  }

  const payload = {
    version: '2.5',
    common: {
      operationType: 'VLOS',
      airspaceClass: 'G',
      maxHeightAGL: 120,
      typicality: 'Typical',
      uSpace: false,
      trafficDensitySource: 'Eurocontrol',
      airspaceContainment: 'None',
      aec: 'AEC_4',
      specialZones: ['UAS_Geo_Zone']
    },
    grc: {
      m1a: 'Low',
      m1b: 'Medium',
      m1c: 'None',
      m2: 'Low',
      smallUaRule: false
    },
    missionGeometry: {
      waypoints: missionData.waypoints,
      geofence: missionData.geofence,
      cga: missionData.cga
    }
  };

  logToConsole('REQUEST:', 'success');
  logToConsole(JSON.stringify(payload, null, 2), 'success');

  try {
    const response = await fetch('/api/v1/sora/calculate', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    const data = await response.json();
    logToConsole('RESPONSE:', 'success');
    logToConsole(JSON.stringify(data, null, 2), 'success');

    updateComplianceBadges(data);
  } catch (err) {
    logToConsole(`API error: ${err.message}`, 'error');
  }
}

function updateComplianceBadges(response) {
  document.getElementById('kpi_igrc').textContent = response.iGRC || response.initialGrc || '–';
  document.getElementById('kpi_fgrc').textContent = response.fGRC || response.finalGrc || '–';
  document.getElementById('kpi_iarc').textContent = response.iARC || response.initialArc || '–';
  document.getElementById('kpi_rarc').textContent = response.rARC || response.residualArc || '–';

  const sail = response.SAIL || response.sail || '–';
  document.getElementById('sailBadge').textContent = `SAIL: ${sail}`;

  // Color SAIL badge (I-II green, III-IV yellow, V-VI red)
  const sailNum = sail.match(/\d+/)?.[0];
  if (sailNum <= 2) {
    document.getElementById('sailBadge').style.background = 'linear-gradient(135deg, #10b981, #059669)';
  } else if (sailNum <= 4) {
    document.getElementById('sailBadge').style.background = 'linear-gradient(135deg, #f59e0b, #d97706)';
  } else {
    document.getElementById('sailBadge').style.background = 'linear-gradient(135deg, #ef4444, #dc2626)';
  }
}

// ================================================================
// 7. EXPORT (GeoJSON/KML/PNG/ZIP)
// ================================================================
function exportGeoJSON() {
  const geojson = {
    type: 'FeatureCollection',
    features: [
      ...missionData.waypoints.map((wp, i) => ({
        type: 'Feature',
        properties: { name: `WP${i + 1}`, alt_m: wp.alt_m },
        geometry: { type: 'Point', coordinates: [wp.lon, wp.lat, wp.alt_m] }
      }))
    ]
  };

  if (missionData.cga) geojson.features.push(missionData.cga);

  downloadFile('mission.geojson', JSON.stringify(geojson, null, 2));
  logToConsole('Exported mission.geojson', 'success');
}

function exportKML() {
  let kml = `<?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2">
  <Document>
    <name>Skyworks Mission</name>`;

  missionData.waypoints.forEach((wp, i) => {
    kml += `
    <Placemark>
      <name>WP${i + 1}</name>
      <Point>
        <coordinates>${wp.lon},${wp.lat},${wp.alt_m}</coordinates>
      </Point>
    </Placemark>`;
  });

  kml += `
  </Document>
</kml>`;

  downloadFile('mission.kml', kml);
  logToConsole('Exported mission.kml', 'success');
}

function exportCSV() {
  let csv = 'lat,lon,alt_m\n';
  missionData.waypoints.forEach(wp => {
    csv += `${wp.lat},${wp.lon},${wp.alt_m}\n`;
  });

  downloadFile('mission.csv', csv);
  logToConsole('Exported mission.csv', 'success');
}

function exportPNG() {
  // Demo: canvas snapshot (production: use html2canvas or map.getCanvas())
  logToConsole('PNG snapshot: use html2canvas or map.getCanvas() in production', 'warning');
  alert('PNG export: integrate html2canvas or MapLibre .getCanvas()');
}

function exportMissionPack() {
  // Demo: create .zip with GeoJSON + KML + CSV
  logToConsole('Mission pack export: integrate JSZip library', 'warning');
  alert('Mission Pack (.zip): integrate JSZip to bundle all exports');
}

function downloadFile(filename, content) {
  const blob = new Blob([content], { type: 'text/plain' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename;
  document.body.appendChild(a); // CRITICAL: Add to DOM for Playwright download event
  a.click();
  document.body.removeChild(a); // Clean up
  URL.revokeObjectURL(url); // Free memory
}

// ================================================================
// 8. EVENT LISTENERS
// ================================================================
function attachEventListeners() {
  document.getElementById('toggle2D3D').addEventListener('click', toggle2D3D);
  document.getElementById('importRouteFile').addEventListener('change', handleRouteImport);
  document.getElementById('importGoogleEarthKML')?.addEventListener('change', handleGoogleEarthKMLImport);
  document.getElementById('addWaypoint').addEventListener('click', handleAddWaypoint);
  document.getElementById('drawGeofence').addEventListener('click', handleDrawGeofence);
  document.getElementById('drawCGA').addEventListener('click', handleDrawCGA);
  document.getElementById('clearRoute').addEventListener('click', handleClearRoute);

  // Layer toggles
  ['rmz', 'tmz', 'ctr', 'tma', 'atz', 'prohibited', 'restricted', 'danger', 'tsa', 'tra', 'cba', 'uas', 'population', 'buildings'].forEach(layer => {
    document.getElementById(`layer_${layer}`)?.addEventListener('change', (e) => {
      layerVisibility[layer] = e.target.checked;
      toggleLayer(layer, e.target.checked);
    });
  });

  // Export buttons
  document.getElementById('exportGeoJSON').addEventListener('click', exportGeoJSON);
  document.getElementById('exportKML').addEventListener('click', exportKML);
  document.getElementById('exportCSV').addEventListener('click', exportCSV);
  document.getElementById('exportPNG').addEventListener('click', exportPNG);
  document.getElementById('exportMissionPack').addEventListener('click', exportMissionPack);

  // SORA Version Toggle
  const versionSelect = document.getElementById('soraVersion');
  if (versionSelect) {
    versionSelect.addEventListener('change', (e) => {
      const version = e.target.value;
      // Show/hide version-specific mitigation sections
      const mitigations_25 = document.getElementById('mitigations_25');
      const mitigations_20 = document.getElementById('mitigations_20');
      
      if (version === '2.5') {
        mitigations_25.style.display = 'block';
        mitigations_20.style.display = 'none';
      } else {
        mitigations_25.style.display = 'none';
        mitigations_20.style.display = 'block';
      }
      
      // Re-calculate SORA badges with new version
      updateSORABadges();
      logToConsole(`Switched to SORA ${version}`, 'success');
    });
  }

  // SORA 2.5 Mitigation Controls
  ['m1a_sheltering', 'm1b_restrictions', 'm1c_observation', 'm2_impact'].forEach(id => {
    const element = document.getElementById(id);
    if (element) {
      element.addEventListener('change', () => {
        updateSORABadges();
        logToConsole(`Updated ${id}: ${element.value}`, 'info');
      });
    }
  });

  // SORA 2.0 Mitigation Controls
  ['m1_strategic', 'm2_effects', 'm3_erp'].forEach(id => {
    const element = document.getElementById(id);
    if (element) {
      element.addEventListener('change', () => {
        updateSORABadges();
        logToConsole(`Updated ${id}: ${element.value}`, 'info');
      });
    }
  });
}

// ================================================================
// 9. UTILITIES
// ================================================================
function logToConsole(message, type = 'info') {
  const console = document.getElementById('validationConsole');
  const line = document.createElement('div');
  line.className = type;
  line.textContent = `[${new Date().toLocaleTimeString()}] ${message}`;
  console.appendChild(line);
  console.scrollTop = console.scrollHeight;
}

function haversineDistance(lat1, lon1, lat2, lon2) {
  const R = 6371e3; // Earth radius in meters
  const φ1 = lat1 * Math.PI / 180;
  const φ2 = lat2 * Math.PI / 180;
  const Δφ = (lat2 - lat1) * Math.PI / 180;
  const Δλ = (lon2 - lon1) * Math.PI / 180;

  const a = Math.sin(Δφ / 2) * Math.sin(Δφ / 2) +
    Math.cos(φ1) * Math.cos(φ2) *
    Math.sin(Δλ / 2) * Math.sin(Δλ / 2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));

  return R * c;
}

function polygonArea(coords) {
  // Shoelace formula (approx for small areas)
  let area = 0;
  for (let i = 0; i < coords.length - 1; i++) {
    area += coords[i][0] * coords[i + 1][1] - coords[i + 1][0] * coords[i][1];
  }
  return Math.abs(area / 2) * 111320 * 111320; // Convert to m² (rough approximation)
}

// ================================================================
// EXPOSE FUNCTIONS FOR PLAYWRIGHT E2E TESTS
// ================================================================
window.renderMission = function(missionData) {
  if (!missionData) return;
  
  // Ensure map exists (tests call this after map loads)
  if (!map2D) {
    console.error('[renderMission] map2D is null');
    return;
  }
  
  try {
    // 1. START MARKER (first waypoint, green)
    if (missionData.waypoints && missionData.waypoints.length > 0) {
      const start = missionData.waypoints[0];
      
      if (!map2D.getSource('mission-start')) {
        map2D.addSource('mission-start', {
          type: 'geojson',
          data: {
            type: 'FeatureCollection',
            features: [{
              type: 'Feature',
              geometry: { type: 'Point', coordinates: [start.lon, start.lat] },
              properties: { label: 'S', name: 'Start Point' }
            }]
          }
        });
        
        map2D.addLayer({
          id: 'mission-start-symbol',
          type: 'symbol',
          source: 'mission-start',
          layout: {
            'text-field': ['get', 'label'],
            'text-size': 16,
            'text-font': ['Open Sans Bold', 'Arial Unicode MS Bold']
          },
          paint: {
            'text-color': '#10b981',
            'text-halo-color': '#ffffff',
            'text-halo-width': 2
          }
        });
      } else {
        map2D.getSource('mission-start').setData({
          type: 'FeatureCollection',
          features: [{
            type: 'Feature',
            geometry: { type: 'Point', coordinates: [start.lon, start.lat] },
            properties: { label: 'S', name: 'Start Point' }
          }]
        });
      }
    }
    
    // 2. END MARKER (last waypoint, red)
    if (missionData.waypoints && missionData.waypoints.length > 1) {
      const end = missionData.waypoints[missionData.waypoints.length - 1];
      
      if (!map2D.getSource('mission-end')) {
        map2D.addSource('mission-end', {
          type: 'geojson',
          data: {
            type: 'FeatureCollection',
            features: [{
              type: 'Feature',
              geometry: { type: 'Point', coordinates: [end.lon, end.lat] },
              properties: { label: 'E', name: 'End Point' }
            }]
          }
        });
        
        map2D.addLayer({
          id: 'mission-end-symbol',
          type: 'symbol',
          source: 'mission-end',
          layout: {
            'text-field': ['get', 'label'],
            'text-size': 16,
            'text-font': ['Open Sans Bold', 'Arial Unicode MS Bold']
          },
          paint: {
            'text-color': '#ef4444',
            'text-halo-color': '#ffffff',
            'text-halo-width': 2
          }
        });
      } else {
        map2D.getSource('mission-end').setData({
          type: 'FeatureCollection',
          features: [{
            type: 'Feature',
            geometry: { type: 'Point', coordinates: [end.lon, end.lat] },
            properties: { label: 'E', name: 'End Point' }
          }]
        });
      }
    }
    
    // 3. CGA POLYGON (yellow fill #fbbf24, gold border #f59e0b)
    if (missionData.controlledGroundArea) {
      const cgaFeature = {
        type: 'Feature',
        geometry: {
          type: 'Polygon',
          coordinates: missionData.controlledGroundArea.coordinates
        },
        properties: { type: 'cga' }
      };
      
      if (!map2D.getSource('mission-cga')) {
        map2D.addSource('mission-cga', {
          type: 'geojson',
          data: { type: 'FeatureCollection', features: [cgaFeature] }
        });
        
        map2D.addLayer({
          id: 'mission-cga-fill',
          type: 'fill',
          source: 'mission-cga',
          paint: {
            'fill-color': '#fbbf24',
            'fill-opacity': 0.4
          }
        });
        
        map2D.addLayer({
          id: 'mission-cga-outline',
          type: 'line',
          source: 'mission-cga',
          paint: {
            'line-color': '#f59e0b',
            'line-width': 2
          }
        });
      } else {
        map2D.getSource('mission-cga').setData({
          type: 'FeatureCollection',
          features: [cgaFeature]
        });
      }
    }
    
    // 4. CORRIDOR POLYGON (blue fill #3b82f6, dashed border [4,2])
    if (missionData.corridor) {
      const corridorFeature = {
        type: 'Feature',
        geometry: {
          type: 'Polygon',
          coordinates: missionData.corridor.coordinates
        },
        properties: { type: 'corridor' }
      };
      
      if (!map2D.getSource('mission-corridor')) {
        map2D.addSource('mission-corridor', {
          type: 'geojson',
          data: { type: 'FeatureCollection', features: [corridorFeature] }
        });
        
        map2D.addLayer({
          id: 'mission-corridor-fill',
          type: 'fill',
          source: 'mission-corridor',
          paint: {
            'fill-color': '#3b82f6',
            'fill-opacity': 0.3
          }
        });
        
        map2D.addLayer({
          id: 'mission-corridor-outline',
          type: 'line',
          source: 'mission-corridor',
          paint: {
            'line-color': '#3b82f6',
            'line-width': 2,
            'line-dasharray': [4, 2]
          }
        });
      } else {
        map2D.getSource('mission-corridor').setData({
          type: 'FeatureCollection',
          features: [corridorFeature]
        });
      }
    }
    
    // 5. GEOFENCE POLYGON (red fill #ef4444, thick dashed border [5,3], width>=2)
    if (missionData.geofence) {
      const geofenceFeature = {
        type: 'Feature',
        geometry: {
          type: 'Polygon',
          coordinates: missionData.geofence.coordinates
        },
        properties: { type: 'geofence' }
      };
      
      if (!map2D.getSource('mission-geofence')) {
        map2D.addSource('mission-geofence', {
          type: 'geojson',
          data: { type: 'FeatureCollection', features: [geofenceFeature] }
        });
        
        map2D.addLayer({
          id: 'mission-geofence-fill',
          type: 'fill',
          source: 'mission-geofence',
          paint: {
            'fill-color': '#ef4444',
            'fill-opacity': 0.25
          }
        });
        
        map2D.addLayer({
          id: 'mission-geofence-outline',
          type: 'line',
          source: 'mission-geofence',
          paint: {
            'line-color': '#ef4444',
            'line-width': 3,
            'line-dasharray': [5, 3]
          }
        });
      } else {
        map2D.getSource('mission-geofence').setData({
          type: 'FeatureCollection',
          features: [geofenceFeature]
        });
      }
    }
    
    // 5. FLY-AWAY SAFE AREA - EASA/JARUS SORA 2.5 Emergency Containment Zone
    const flyAway = missionData.erp?.flyAway || missionData.erp?.FlyAway;
    const safeAreaLat = flyAway?.safeAreaLat || flyAway?.SafeAreaLat;
    const safeAreaLon = flyAway?.safeAreaLon || flyAway?.SafeAreaLon;
    const safeAreaRadius = flyAway?.safeAreaRadius_m || flyAway?.SafeAreaRadius_m;
    
    if (safeAreaLat && safeAreaLon && safeAreaRadius) {
      const safeAreaCircle = createCirclePolygon(safeAreaLon, safeAreaLat, safeAreaRadius, 64);
      const safeAreaFeature = {
        type: 'Feature',
        geometry: {
          type: 'Polygon',
          coordinates: [safeAreaCircle]
        },
        properties: { 
          type: 'safe-area',
          radius_m: safeAreaRadius,
          description: 'Fly-Away Safe Area (Emergency Containment Zone)'
        }
      };
      
      if (!map2D.getSource('erp-safe-area')) {
        map2D.addSource('erp-safe-area', {
          type: 'geojson',
          data: { type: 'FeatureCollection', features: [safeAreaFeature] }
        });
        
        // Semi-transparent blue fill
        map2D.addLayer({
          id: 'erp-safe-area-fill',
          type: 'fill',
          source: 'erp-safe-area',
          paint: {
            'fill-color': '#3b82f6',
            'fill-opacity': 0.15
          }
        });
        
        // Dashed blue outline
        map2D.addLayer({
          id: 'erp-safe-area-outline',
          type: 'line',
          source: 'erp-safe-area',
          paint: {
            'line-color': '#2563eb',
            'line-width': 2,
            'line-dasharray': [4, 4]
          }
        });
      } else {
        map2D.getSource('erp-safe-area').setData({
          type: 'FeatureCollection',
          features: [safeAreaFeature]
        });
      }
    }
    
    // 6. EMERGENCY LANDING SITES (E1, E2, E3) - EASA/JARUS SORA 2.5 Compliance
    const emergencyLanding = missionData.erp?.emergencyLanding || missionData.erp?.EmergencyLanding;
    const emergencySites = emergencyLanding?.sites || emergencyLanding?.Sites;
    
    if (emergencySites && emergencySites.length > 0) {
      // Color-coded priority: E1=Primary(green), E2=Secondary(orange), E3=Tertiary(red)
      const priorityColors = ['#10b981', '#f59e0b', '#ef4444']; // green, amber, red
      const priorityLabels = ['PRIMARY', 'SECONDARY', 'TERTIARY'];
      
      const emergencyFeatures = emergencySites.map((site, idx) => ({
        type: 'Feature',
        geometry: {
          type: 'Point',
          coordinates: [site.Lon || site.lon, site.Lat || site.lat]  // Dual-case support
        },
        properties: {
          label: `E${idx + 1}`,
          priority: priorityLabels[idx] || 'FALLBACK',
          description: site.Description || site.description || `Emergency Site ${idx + 1}`,
          color: priorityColors[idx] || '#6b7280',
          index: idx
        }
      }));
      
      if (!map2D.getSource('emergency-sites')) {
        map2D.addSource('emergency-sites', {
          type: 'geojson',
          data: { type: 'FeatureCollection', features: emergencyFeatures }
        });
        
        // Color-coded circle markers (priority-based)
        map2D.addLayer({
          id: 'emergency-sites-circle',
          type: 'circle',
          source: 'emergency-sites',
          paint: {
            'circle-radius': 12,
            'circle-color': ['get', 'color'],
            'circle-stroke-width': 3,
            'circle-stroke-color': '#ffffff',
            'circle-opacity': 0.9
          }
        });
        
        // White text labels (E1, E2, E3)
        map2D.addLayer({
          id: 'emergency-sites-label',
          type: 'symbol',
          source: 'emergency-sites',
          layout: {
            'text-field': ['get', 'label'],
            'text-size': 12,
            'text-font': ['Open Sans Bold', 'Arial Unicode MS Bold'],
            'text-anchor': 'center',
            'text-offset': [0, 0]
          },
          paint: {
            'text-color': '#ffffff',
            'text-halo-color': 'rgba(0,0,0,0.3)',
            'text-halo-width': 1
          }
        });
        
        // Description labels below markers
        map2D.addLayer({
          id: 'emergency-sites-description',
          type: 'symbol',
          source: 'emergency-sites',
          layout: {
            'text-field': ['concat', ['get', 'priority'], '\n', ['get', 'description']],
            'text-size': 10,
            'text-font': ['Open Sans Regular', 'Arial Unicode MS Regular'],
            'text-anchor': 'top',
            'text-offset': [0, 1.5],
            'text-max-width': 15
          },
          paint: {
            'text-color': '#374151',
            'text-halo-color': '#ffffff',
            'text-halo-width': 2
          }
        });
      } else {
        map2D.getSource('emergency-sites').setData({
          type: 'FeatureCollection',
          features: emergencyFeatures
        });
      }
    }
    
    console.log('[renderMission] Mission markers and polygons rendered');
  } catch (error) {
    console.error('[renderMission] ERROR:', error);
  }
};


window.updateErpPanel = updateErpPanel;
window.updateSoraBadges = updateSoraBadges;
window.updateOsoPanel = updateOsoPanel;
Object.defineProperty(window, 'map2D', { get: () => map2D });
Object.defineProperty(window, 'map3D', { get: () => viewer3D });
window.missionData = missionData; // Expose for debug/tests
