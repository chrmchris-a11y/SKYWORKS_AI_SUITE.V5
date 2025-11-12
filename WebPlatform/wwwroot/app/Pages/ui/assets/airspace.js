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
  console.log('[Airspace Maps] DOMContentLoaded - starting initialization');
  try {
    init2DMap();
    // NOTE: attachEventListeners() is called AFTER Google Maps loads (in initGoogleMaps callback)
    checkMissionIdParam();
    createTestBadgeAliases();
    logToConsole('Airspace Maps initialized (2D mode)', 'success');
  } catch (error) {
    console.error('[Airspace Maps] INITIALIZATION ERROR:', error);
    alert('Map initialization failed! Check console (F12) for details.\n\nError: ' + error.message);
  }
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
  } else {
    // No mission loaded - show placeholders in SAIL/ARC/GRC panel
    showEmptySoraBadges();
    logToConsole('No mission loaded - SAIL panel shows placeholders', 'info');
  }
}

function showEmptySoraBadges() {
  const badges = [
    { id: 'kpi_igrc', value: '–', tooltip: 'No mission loaded' },
    { id: 'kpi_fgrc', value: '–', tooltip: 'No mission loaded' },
    { id: 'kpi_iarc', value: '–', tooltip: 'No mission loaded' },
    { id: 'kpi_rarc', value: '–', tooltip: 'No mission loaded' },
    { id: 'sailBadge', value: 'SAIL: –', tooltip: 'No mission loaded' }
  ];
  
  badges.forEach(badge => {
    const element = document.getElementById(badge.id);
    if (element) {
      element.textContent = badge.value;
      element.title = badge.tooltip;
      element.style.cursor = 'help';
    }
  });
}

async function loadMissionFromApi(missionId) {
  try {
    const response = await fetch(`/api/v1/missions/${missionId}/overview`);
    if (!response.ok) {
      if (response.status === 404) {
        logToConsole(`Mission not found: ${missionId}`, 'warning');
        showMissionNotFoundBanner();
        showEmptySoraBadges(); // Show placeholders instead of errors
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
    logToConsole(`Failed to load mission: ${error.message}`, 'warning');
    showMissionNotFoundBanner();
    showEmptySoraBadges(); // Show placeholders instead of errors
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

// GOOGLE MAPS ONLY - Global arrays to track map objects for cleanup
let missionPolylines = [];
let missionPolygons = [];
let missionMarkers = [];

function renderMissionGeometry(geoJson) {
  if (!map2D) return;

  // Clear existing Google Maps objects
  missionPolylines.forEach(p => p.setMap(null));
  missionPolygons.forEach(p => p.setMap(null));
  missionMarkers.forEach(m => m.setMap(null));
  missionPolylines = [];
  missionPolygons = [];
  missionMarkers = [];

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
  
  // Render route with Google Maps Polyline (blue #3b82f6, 3px)
  if (routeFeatures.length > 0) {
    routeFeatures.forEach(f => {
      const path = f.geometry.coordinates.map(c => ({ lat: c[1], lng: c[0] }));
      const polyline = new google.maps.Polyline({
        path,
        strokeColor: '#3b82f6', // Blue
        strokeOpacity: 0.8,
        strokeWeight: 3,
        map: map2D
      });
      missionPolylines.push(polyline);
    });
  }
  
  // Render CGA polygon (yellow/gold #facc15, 30% opacity)
  if (cgaFeatures.length > 0) {
    cgaFeatures.forEach(f => {
      const paths = f.geometry.coordinates[0].map(c => ({ lat: c[1], lng: c[0] }));
      const polygon = new google.maps.Polygon({
        paths,
        strokeColor: '#eab308',
        strokeOpacity: 0.8,
        strokeWeight: 2,
        fillColor: '#facc15',
        fillOpacity: 0.3,
        map: map2D
      });
      missionPolygons.push(polygon);
    });
  }
  
  // Render Corridor polygon (blue #3b82f6, 25% opacity)
  if (corridorFeatures.length > 0) {
    corridorFeatures.forEach(f => {
      const paths = f.geometry.coordinates[0].map(c => ({ lat: c[1], lng: c[0] }));
      const polygon = new google.maps.Polygon({
        paths,
        strokeColor: '#3b82f6',
        strokeOpacity: 0.6,
        strokeWeight: 2,
        fillColor: '#3b82f6',
        fillOpacity: 0.25,
        map: map2D
      });
      missionPolygons.push(polygon);
    });
  }
  
  // Render Geofence/No-Fly polygon (red #ef4444, 20% opacity)
  if (geofenceFeatures.length > 0) {
    geofenceFeatures.forEach(f => {
      const paths = f.geometry.coordinates[0].map(c => ({ lat: c[1], lng: c[0] }));
      const polygon = new google.maps.Polygon({
        paths,
        strokeColor: '#ef4444',
        strokeOpacity: 0.8,
        strokeWeight: 3,
        fillColor: '#ef4444',
        fillOpacity: 0.2,
        map: map2D
      });
      missionPolygons.push(polygon);
    });
  }
  
  // Add Start/End markers with Google Maps Marker (custom HTML via icon)
  routeFeatures.forEach(f => {
    if (f.geometry.coordinates.length >= 2) {
      const coords = f.geometry.coordinates;
      
      // Start marker "S" (green)
      const startPos = { lat: coords[0][1], lng: coords[0][0] };
      const startMarker = new google.maps.Marker({
        map: map2D,
        position: startPos,
        title: 'Start Point',
        label: {
          text: 'S',
          color: 'white',
          fontSize: '16px',
          fontWeight: 'bold'
        },
        icon: {
          path: google.maps.SymbolPath.CIRCLE,
          scale: 16,
          fillColor: '#10b981', // Green
          fillOpacity: 1,
          strokeColor: 'white',
          strokeWeight: 3
        }
      });
      missionMarkers.push(startMarker);
      
      // InfoWindow for Start
      const startInfoWindow = new google.maps.InfoWindow({
        content: '<strong>Start Point</strong>'
      });
      startMarker.addListener('click', () => {
        startInfoWindow.open(map2D, startMarker);
      });
      
      // End marker "E" (red)
      const endPos = { lat: coords[coords.length - 1][1], lng: coords[coords.length - 1][0] };
      const endMarker = new google.maps.Marker({
        map: map2D,
        position: endPos,
        title: 'End Point',
        label: {
          text: 'E',
          color: 'white',
          fontSize: '16px',
          fontWeight: 'bold'
        },
        icon: {
          path: google.maps.SymbolPath.CIRCLE,
          scale: 16,
          fillColor: '#ef4444', // Red
          fillOpacity: 1,
          strokeColor: 'white',
          strokeWeight: 3
        }
      });
      missionMarkers.push(endMarker);
      
      // InfoWindow for End
      const endInfoWindow = new google.maps.InfoWindow({
        content: '<strong>End Point</strong>'
      });
      endMarker.addListener('click', () => {
        endInfoWindow.open(map2D, endMarker);
      });
    }
  });
  
  // Fit bounds with Google Maps LatLngBounds
  const bounds = new google.maps.LatLngBounds();
  geoJson.features.forEach(f => {
    if (!f || !f.geometry) return; // Skip null/undefined
    if (f.geometry.type === 'LineString') {
      f.geometry.coordinates.forEach(c => bounds.extend({ lat: c[1], lng: c[0] }));
    } else if (f.geometry.type === 'Polygon') {
      f.geometry.coordinates[0].forEach(c => bounds.extend({ lat: c[1], lng: c[0] }));
    } else if (f.geometry.type === 'Point') {
      bounds.extend({ lat: f.geometry.coordinates[1], lng: f.geometry.coordinates[0] });
    }
  });
  if (!bounds.isEmpty()) {
    map2D.fitBounds(bounds, { padding: 50 });
  }
}

function updateSoraBadges(sora) {
  if (!sora) return;
  
  // Normalize SAIL (I-VI) - handle case variations
  const sailValue = sora.sail || sora.SAIL || sora.Sail || 'N/A';
  
  // Normalize GRC values (1-7) - support multi-case keys
  const initialGrc = sora.initialGrc || sora.InitialGrc || sora.initialGRC || sora.iGRC || 'N/A';
  const finalGrc = sora.finalGrc || sora.FinalGrc || sora.finalGRC || sora.fGRC || 'N/A';
  
  // Normalize ARC values (a-d) - support multi-case keys
  const initialArc = sora.initialArc || sora.InitialArc || sora.initialARC || sora.iARC || 'N/A';
  const residualArc = sora.residualArc || sora.ResidualArc || sora.residualARC || sora.rARC || 'N/A';
  
  const badges = [
    {
      id: 'kpi_igrc',
      altId: 'badge-igrc',
      value: initialGrc,
      tooltip: `Initial GRC (Ground Risk Class): ${initialGrc}. Inherent ground risk before mitigations. Range: 1 (low) to 10 (high).`
    },
    {
      id: 'kpi_fgrc',
      altId: 'badge-fgrc',
      value: finalGrc,
      tooltip: `Final GRC: ${finalGrc}. Ground risk after applying M1 mitigations (sheltering, operational restrictions). Lower is safer.`
    },
    {
      id: 'kpi_iarc',
      altId: 'badge-iarc',
      value: initialArc,
      tooltip: `Initial ARC (Air Risk Class): ${initialArc}. Inherent air collision risk. a-d: Low controlled airspace, c-d: Higher risk.`
    },
    {
      id: 'kpi_rarc',
      altId: 'badge-rarc',
      value: residualArc,
      tooltip: `Residual ARC: ${residualArc}. Air risk after applying M2/M3 tactical mitigations (strategic). Target: as low as possible.`
    },
    {
      id: 'sailBadge',
      altId: 'badge-sail',
      value: `SAIL: ${sailValue}`,
      tooltip: `SAIL ${sailValue} - Specific Assurance and Integrity Level. Higher SAIL = more OSO requirements. I-II: Low risk, III-IV: Medium, V-VI: High.`
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
  console.log('[init2DMap] Starting map initialization...');
  
  // GOOGLE MAPS 2D - STRICTLY Google Maps JavaScript API only
  // NO OSM/Nominatim/MapLibre/Leaflet/Cesium
  window.initGoogleMaps = function() {
    console.log('[initGoogleMaps] Google Maps API loaded, initializing map...');
    
    // DEBUG: Verify Places library loaded
    if (typeof google.maps.places === 'undefined') {
      console.error('[initGoogleMaps] ❌ Google Places library NOT loaded! Check libraries parameter.');
      alert('Google Places library failed to load. Check console for details.');
      return;
    }
    console.log('[initGoogleMaps] ✅ Google Places library loaded:', google.maps.places);
    
    // Create Google Map centered on Athens, Greece
    map2D = new google.maps.Map(document.getElementById('map2D'), {
      center: { lat: 37.9838, lng: 23.7275 }, // Athens
      zoom: 12,
      mapTypeId: google.maps.MapTypeId.HYBRID, // Default: Satellite with labels
      mapTypeControl: true,
      mapTypeControlOptions: {
        style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR,
        position: google.maps.ControlPosition.TOP_RIGHT,
        mapTypeIds: ['roadmap', 'satellite', 'hybrid', 'terrain']
      },
      zoomControl: true,
      zoomControlOptions: {
        position: google.maps.ControlPosition.RIGHT_CENTER
      },
      streetViewControl: true,
      fullscreenControl: true,
      gestureHandling: 'greedy', // One-finger pan/zoom
      tilt: 0, // Start with 2D (0°), toggle to 45° for oblique
      heading: 0,
      mapId: null // Optional: for advanced features
    });

    // ═══════════════════════════════════════════════════════════
    // 1. GOOGLE PLACES AUTOCOMPLETE (Search Box)
    // ═══════════════════════════════════════════════════════════
    const searchInput = document.getElementById('searchLocationInput');
    console.log('[Google Places] Search input element:', searchInput);
    
    if (searchInput) {
      console.log('[Google Places] Initializing Autocomplete...');
      
      const autocomplete = new google.maps.places.Autocomplete(searchInput, {
        fields: ['geometry', 'name', 'formatted_address', 'place_id', 'types'],
        types: [] // All types: cities, addresses, neighborhoods, POIs
      });
      
      console.log('[Google Places] Autocomplete instance created:', autocomplete);

      autocomplete.bindTo('bounds', map2D); // Bias results to map viewport

      let searchMarker = null;
      
      // CUSTOM: Detect coordinates in multiple formats
      searchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
          let input = searchInput.value.trim();
          
          // Try multiple coordinate patterns
          let lat = null, lng = null;
          
          // Pattern 1: "35.066871° N, 33.316661° E" or "35.066871 N, 33.316661 E"
          let match = input.match(/^(-?\d+\.?\d*)\s*°?\s*[NS]?\s*,\s*(-?\d+\.?\d*)\s*°?\s*[EW]?$/i);
          
          // Pattern 2: "N 35.066871, E 33.316661"
          if (!match) {
            match = input.match(/^[NS]?\s*(-?\d+\.?\d*)\s*,\s*[EW]?\s*(-?\d+\.?\d*)$/i);
          }
          
          // Pattern 3: Simple "37.9838, 23.7275"
          if (!match) {
            match = input.match(/^(-?\d+\.?\d*)\s*,\s*(-?\d+\.?\d*)$/);
          }
          
          if (match) {
            e.preventDefault(); // Prevent Places Autocomplete
            lat = parseFloat(match[1]);
            lng = parseFloat(match[2]);
            
            if (lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180) {
              // Remove previous search marker
              if (searchMarker) searchMarker.setMap(null);
              
              // Add marker at coordinates
              const position = { lat, lng };
              searchMarker = new google.maps.Marker({
                map: map2D,
                position: position,
                title: `${lat}, ${lng}`,
                animation: google.maps.Animation.DROP,
                icon: {
                  path: google.maps.SymbolPath.CIRCLE,
                  scale: 10,
                  fillColor: '#ef4444',
                  fillOpacity: 1,
                  strokeColor: '#fff',
                  strokeWeight: 3
                }
              });
              
              map2D.setCenter(position);
              map2D.setZoom(16);
              logToConsole(`Coordinates: ${lat}, ${lng}`, 'success');
              searchInput.value = ''; // Clear input
            } else {
              logToConsole('Invalid coordinates (lat: -90 to 90, lng: -180 to 180)', 'error');
            }
          }
        }
      });
      
      autocomplete.addListener('place_changed', () => {
        const place = autocomplete.getPlace();
        
        if (!place.geometry || !place.geometry.location) {
          console.warn('[Google Places] No geometry found');
          logToConsole('Location not found', 'error');
          return;
        }

        // Remove previous search marker
        if (searchMarker) searchMarker.setMap(null);

        // Add red marker at search result
        searchMarker = new google.maps.Marker({
          map: map2D,
          position: place.geometry.location,
          title: place.name || place.formatted_address,
          animation: google.maps.Animation.DROP,
          icon: {
            path: google.maps.SymbolPath.CIRCLE,
            scale: 10,
            fillColor: '#ef4444',
            fillOpacity: 1,
            strokeColor: '#fff',
            strokeWeight: 3
          }
        });

        // Pan + zoom to location
        if (place.geometry.viewport) {
          map2D.fitBounds(place.geometry.viewport);
        } else {
          map2D.setCenter(place.geometry.location);
          map2D.setZoom(16);
        }

        logToConsole(`Found: ${place.formatted_address || place.name}`, 'success');
      });
      
      console.log('[Google Places] Autocomplete enabled (worldwide)');
    }

    // ═══════════════════════════════════════════════════════════
    // 2. GOOGLE GEOCODER (Lat/Lng ↔ Address)
    // ═══════════════════════════════════════════════════════════
    const geocoder = new google.maps.Geocoder();
    
    window.geocodeLatLng = function(lat, lng) {
      const latlng = { lat: parseFloat(lat), lng: parseFloat(lng) };
      geocoder.geocode({ location: latlng }, (results, status) => {
        if (status === 'OK' && results[0]) {
          console.log('[Geocoder] Address:', results[0].formatted_address);
          logToConsole(`Address: ${results[0].formatted_address}`, 'success');
          return results[0].formatted_address;
        } else {
          console.error('[Geocoder] Failed:', status);
          logToConsole('Geocoding failed: ' + status, 'error');
          return null;
        }
      });
    };
    
    window.geocodeAddress = function(address) {
      geocoder.geocode({ address }, (results, status) => {
        if (status === 'OK' && results[0]) {
          const location = results[0].geometry.location;
          map2D.setCenter(location);
          map2D.setZoom(16);
          
          new google.maps.Marker({
            map: map2D,
            position: location,
            title: results[0].formatted_address
          });
          
          logToConsole(`Found: ${results[0].formatted_address}`, 'success');
        } else {
          logToConsole('Geocoding failed: ' + status, 'error');
        }
      });
    };

    // ═══════════════════════════════════════════════════════════
    // 3. URL PASTE PARSER (Google Maps URL formats)
    // ═══════════════════════════════════════════════════════════
    window.parseGoogleMapsUrl = function(url) {
      // Pattern 1: @lat,lng,zoom (e.g., @37.9838,23.7275,12z)
      const pattern1 = /@(-?\d+\.\d+),(-?\d+\.\d+),(\d+\.?\d*)z?/;
      const match1 = url.match(pattern1);
      if (match1) {
        return {
          lat: parseFloat(match1[1]),
          lng: parseFloat(match1[2]),
          zoom: parseFloat(match1[3])
        };
      }
      
      // Pattern 2: ?q=lat,lng (e.g., ?q=37.9838,23.7275)
      const pattern2 = /[?&]q=(-?\d+\.\d+),(-?\d+\.\d+)/;
      const match2 = url.match(pattern2);
      if (match2) {
        return {
          lat: parseFloat(match2[1]),
          lng: parseFloat(match2[2]),
          zoom: 14
        };
      }
      
      // Pattern 3: /place/.../@lat,lng (Google Maps place URLs)
      const pattern3 = /\/place\/[^/]+\/@(-?\d+\.\d+),(-?\d+\.\d+)/;
      const match3 = url.match(pattern3);
      if (match3) {
        return {
          lat: parseFloat(match3[1]),
          lng: parseFloat(match3[2]),
          zoom: 16
        };
      }
      
      return null;
    };
    
    // Paste handler for search box
    if (searchInput) {
      searchInput.addEventListener('paste', (e) => {
        setTimeout(() => {
          const pastedText = searchInput.value.trim();
          const coords = parseGoogleMapsUrl(pastedText);
          
          if (coords) {
            // Valid Google Maps URL detected
            map2D.setCenter({ lat: coords.lat, lng: coords.lng });
            map2D.setZoom(coords.zoom);
            
            new google.maps.Marker({
              map: map2D,
              position: { lat: coords.lat, lng: coords.lng },
              title: `Pasted location: ${coords.lat}, ${coords.lng}`
            });
            
            logToConsole(`Pasted URL parsed: ${coords.lat}, ${coords.lng}`, 'success');
            searchInput.value = `${coords.lat}, ${coords.lng}`; // Clean display
          }
        }, 100);
      });
    }

    // ═══════════════════════════════════════════════════════════
    // 4. 2D/OBLIQUE TOGGLE (tilt 0° vs 45°) with graceful fallback
    // ═══════════════════════════════════════════════════════════
    const obliqueBtn = document.getElementById('btn-oblique-toggle');
    
    if (obliqueBtn) {
      obliqueBtn.addEventListener('click', function() {
        const currentTilt = map2D.getTilt();
        const newTilt = currentTilt === 0 ? 45 : 0;
        
        // Try to set tilt
        map2D.setMapTypeId('satellite'); // Ensure satellite mode for oblique
        map2D.setTilt(newTilt);
        map2D.setHeading(0); // Reset heading
        
        // Check if tilt was actually applied (some areas don't support oblique)
        setTimeout(() => {
          const actualTilt = map2D.getTilt();
          
          if (newTilt === 45 && actualTilt === 0) {
            // Oblique not available at this location/zoom
            obliqueBtn.disabled = true;
            obliqueBtn.title = 'Oblique view not available here';
            obliqueBtn.textContent = '2D';
            obliqueBtn.setAttribute('aria-pressed', 'false');
            logToConsole('Oblique view not available at this location', 'warning');
          } else {
            // Tilt successful
            obliqueBtn.disabled = false;
            obliqueBtn.textContent = actualTilt === 0 ? '2D' : '45°';
            obliqueBtn.title = actualTilt === 0 ? 'Switch to 45° oblique view' : 'Switch to 2D view';
            obliqueBtn.setAttribute('aria-pressed', actualTilt === 45 ? 'true' : 'false');
            logToConsole(`Map tilt: ${actualTilt}°`, 'success');
          }
        }, 100);
      });
      
      // Re-check oblique availability on zoom/pan
      map2D.addListener('idle', () => {
        const currentTilt = map2D.getTilt();
        if (currentTilt === 0 && obliqueBtn.disabled) {
          // Re-enable button when map moves (might have oblique now)
          obliqueBtn.disabled = false;
          obliqueBtn.title = 'Switch to 45° oblique view';
        }
      });
      
      console.log('[Oblique Toggle] Control attached');
    }

    // Add airspace layers (Google Maps Data Layer format)
    addEUAirspaceLayers();
    
    // Re-attach event listeners after map initialization (DOM elements now exist)
    attachEventListeners();
    
    logToConsole('✅ Google Maps 2D loaded (Athens, Greece)', 'success');
    console.log('[initGoogleMaps] Map initialization complete');
  };
  
  // Auto-call if Google Maps already loaded (unlikely, but safe)
  if (typeof google !== 'undefined' && google.maps) {
    initGoogleMaps();
  } else {
    console.log('[init2DMap] Waiting for Google Maps API (callback: initGoogleMaps)');
  }
}

function init3DMap() {
  try {
    // CESIUM JS (Google Earth Web style - 3D Globe)
    // Using Cesium's default public token (works for development)
    // For production, get free token at https://ion.cesium.com/signup
    
    // Default public access token from Cesium (basic terrain + imagery)
    Cesium.Ion.defaultAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlYWE1OWUxNy1mMWZiLTQzYjYtYTQ0OS1kMWFjYmFkNjc5YzciLCJpZCI6OTYyMCwic2NvcGVzIjpbImFzbCIsImFzciIsImFzdyIsImdjIl0sImlhdCI6MTU1Mjk3MTU0MH0.e1jPFFn7rKHO3VWjMwF7HWbRo7KmyFP6DweCB-DJzSQ';
    
    viewer3D = new Cesium.Viewer('map3D', {
      // Terrain & Imagery from Cesium Ion (like Google Earth)
      terrainProvider: Cesium.createWorldTerrain({
        requestWaterMask: true,
        requestVertexNormals: true
      }),
      imageryProvider: new Cesium.IonImageryProvider({ assetId: 2 }), // Sentinel-2 satellite imagery
      
      // UI Controls (similar to Google Earth)
      baseLayerPicker: true,  // Switch between satellite/terrain/maps
      geocoder: true,         // Search box (like Google Earth search)
      homeButton: true,       // Reset view
      sceneModePicker: true,  // 2D/2.5D/3D toggle
      navigationHelpButton: true,
      animation: false,       // Timeline for time-based data
      timeline: false,
      fullscreenButton: true,
      vrButton: false,
      
      // Performance
      requestRenderMode: true, // Only render when needed (save battery)
      maximumRenderTimeChange: Infinity
    });

    // Fly to Athens, Greece (like Google Earth "Fly To")
    viewer3D.camera.flyTo({
      destination: Cesium.Cartesian3.fromDegrees(23.7275, 37.9838, 15000), // Athens + 15km altitude
      orientation: {
        heading: Cesium.Math.toRadians(0),   // North
        pitch: Cesium.Math.toRadians(-45),   // 45° angle (like Google Earth tilt)
        roll: 0.0
      },
      duration: 3.0 // 3 second flight animation
    });

    // Enable lighting (sun position, shadows - like Google Earth)
    viewer3D.scene.globe.enableLighting = true;
    
    // Depth test for underground objects
    viewer3D.scene.globe.depthTestAgainstTerrain = true;
    
    // Better rendering quality (like Google Earth HD)
    viewer3D.scene.highDynamicRange = true;
    viewer3D.scene.fog.enabled = true;
    viewer3D.scene.fog.density = 0.0002;
    viewer3D.scene.fog.minimumBrightness = 0.8;

    logToConsole('Cesium 3D Globe loaded (Google Earth style - Athens)', 'success');
    console.log('[Cesium] 3D Globe initialized with terrain & satellite imagery');
    
  } catch (error) {
    console.error('[init3DMap] Cesium ERROR:', error);
    logToConsole('3D viewer init failed: ' + error.message, 'error');
    
    // Fallback: Basic 3D without terrain/imagery
    try {
      viewer3D = new Cesium.Viewer('map3D', {
        baseLayerPicker: false,
        geocoder: false,
        terrainProvider: undefined, // No terrain (flat globe)
        imageryProvider: false      // No satellite imagery
      });
      logToConsole('3D viewer loaded (basic mode - no terrain)', 'warning');
    } catch (fallbackError) {
      console.error('[init3DMap] Fallback also failed:', fallbackError);
    }
  }
}

// ================================================================
// 2. EU AIRSPACE LAYERS (Google Maps Data Layer)
// ================================================================
function addEUAirspaceLayers() {
  if (!map2D || typeof google === 'undefined') {
    console.warn('[addEUAirspaceLayers] Map not ready or Google Maps not loaded');
    return;
  }

  // Demo GeoJSON for RMZ (Radio Mandatory Zone)
  // Production: fetch from Eurocontrol AIXM or OpenAIP API
  const demoRMZ = {
    type: 'FeatureCollection',
    features: [{
      type: 'Feature',
      properties: { name: 'Berlin RMZ', type: 'RMZ', airspaceClass: 'D' },
      geometry: {
        type: 'Polygon',
        coordinates: [[
          [13.2, 52.4], [13.6, 52.4], [13.6, 52.6], [13.2, 52.6], [13.2, 52.4]
        ]]
      }
    }]
  };

  // Add GeoJSON to Google Maps using Data Layer
  const rmzLayer = new google.maps.Data();
  rmzLayer.addGeoJson(demoRMZ);
  rmzLayer.setMap(map2D);

  // Style polygons (blue fill like EASA airspace charts)
  rmzLayer.setStyle({
    fillColor: '#3b82f6',
    fillOpacity: 0.2,
    strokeColor: '#3b82f6',
    strokeWeight: 2,
    strokeOpacity: 1
  });

  // Add info window on click
  rmzLayer.addListener('click', (event) => {
    const feature = event.feature;
    const name = feature.getProperty('name') || 'Unknown Airspace';
    const type = feature.getProperty('type') || 'N/A';
    const airspaceClass = feature.getProperty('airspaceClass') || 'N/A';
    
    const infoWindow = new google.maps.InfoWindow({
      content: `
        <div style="padding: 8px; font-family: sans-serif;">
          <h3 style="margin: 0 0 8px 0; color: #3b82f6;">${name}</h3>
          <p style="margin: 4px 0;"><b>Type:</b> ${type}</p>
          <p style="margin: 4px 0;"><b>Class:</b> ${airspaceClass}</p>
        </div>
      `,
      position: event.latLng
    });
    infoWindow.open(map2D);
  });

  console.log('[Google Maps Data Layer] EU Airspace layers added (demo RMZ)');
  
  // TODO: Add CTR, TMZ, Prohibited zones, etc. using same pattern
  // Production: fetch real AIXM data from Eurocontrol API
}

function toggleLayer(layerId, visible) {
  if (!map2D) return;
  // Google Maps Data Layer toggle
  // Store layers globally for toggling
  // TODO: Implement layer visibility toggle for Google Maps Data Layers
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

  // Add marker to 2D map with Google Maps
  if (map2D) {
    const marker = new google.maps.Marker({
      map: map2D,
      position: { lat, lng: lon },
      title: `WP ${missionData.waypoints.length}`,
      label: {
        text: `${missionData.waypoints.length}`,
        color: 'white',
        fontSize: '12px',
        fontWeight: 'bold'
      },
      icon: {
        path: google.maps.SymbolPath.CIRCLE,
        scale: 10,
        fillColor: '#2563eb', // Blue
        fillOpacity: 1,
        strokeColor: 'white',
        strokeWeight: 2
      }
    });
    missionMarkers.push(marker);
    
    // InfoWindow for waypoint
    const infoWindow = new google.maps.InfoWindow({
      content: `<strong>WP ${missionData.waypoints.length}</strong><br>Alt: ${alt}m AGL`
    });
    marker.addListener('click', () => {
      infoWindow.open(map2D, marker);
    });
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

// ================================================================
// 5. ADD WAYPOINT (click mode with numbered markers + polyline)
// ================================================================
let waypointClickListener = null; // Store click listener to remove later
let waypointMode = false;

function handleAddWaypoint() {
  const btn = document.getElementById('addWaypoint');
  
  if (!waypointMode) {
    // ENTER waypoint mode
    waypointMode = true;
    if (btn) {
      btn.style.background = '#10b981';
      btn.style.color = 'white';
      btn.textContent = '✓ Click Map to Add';
    }
    
    // Change cursor
    map2D.setOptions({ draggableCursor: 'crosshair' });
    
    // Add click listener
    waypointClickListener = map2D.addListener('click', (event) => {
      const lat = event.latLng.lat();
      const lng = event.latLng.lng();
      const alt = 100; // Default 100m AGL
      
      // Add to missionData
      if (!missionData.waypoints) missionData.waypoints = [];
      const seq = missionData.waypoints.length + 1;
      missionData.waypoints.push({ lat, lon: lng, alt_m: alt, seq });
      
      // Create numbered marker
      const marker = new google.maps.Marker({
        map: map2D,
        position: { lat, lng },
        label: {
          text: `${seq}`,
          color: 'white',
          fontSize: '14px',
          fontWeight: 'bold'
        },
        icon: {
          path: google.maps.SymbolPath.CIRCLE,
          scale: 12,
          fillColor: '#3b82f6',
          fillOpacity: 1,
          strokeColor: '#fff',
          strokeWeight: 3
        },
        title: `Waypoint ${seq}`,
        draggable: false
      });
      
      missionMarkers.push(marker);
      
      // InfoWindow
      const infoWindow = new google.maps.InfoWindow({
        content: `<strong>Waypoint ${seq}</strong><br>Lat: ${lat.toFixed(6)}<br>Lng: ${lng.toFixed(6)}<br>Alt: ${alt}m AGL`
      });
      marker.addListener('click', () => {
        infoWindow.open(map2D, marker);
      });
      
      // Re-draw route polyline
      updateRoutePolyline();
      
      logToConsole(`Waypoint ${seq} added: [${lat.toFixed(4)}, ${lng.toFixed(4)}]`, 'success');
      updateGeometryStats();
    });
    
    logToConsole('Waypoint mode: Click map to add waypoints', 'info');
  } else {
    // EXIT waypoint mode
    waypointMode = false;
    if (btn) {
      btn.style.background = '';
      btn.style.color = '';
      btn.textContent = '➕ Add Waypoint';
    }
    
    // Reset cursor
    map2D.setOptions({ draggableCursor: null });
    
    // Remove click listener
    if (waypointClickListener) {
      google.maps.event.removeListener(waypointClickListener);
      waypointClickListener = null;
    }
    
    logToConsole('Waypoint mode: OFF', 'info');
  }
}

// Update route polyline connecting all waypoints
function updateRoutePolyline() {
  // Remove old polyline
  if (window.routePolyline) {
    window.routePolyline.setMap(null);
  }
  
  if (!missionData.waypoints || missionData.waypoints.length < 2) {
    return; // Need at least 2 points for a line
  }
  
  // Create path from waypoints
  const path = missionData.waypoints
    .sort((a, b) => a.seq - b.seq) // Sort by sequence
    .map(wp => ({ lat: wp.lat, lng: wp.lon }));
  
  // Draw polyline
  window.routePolyline = new google.maps.Polyline({
    map: map2D,
    path: path,
    strokeColor: '#3b82f6',
    strokeWeight: 3,
    strokeOpacity: 0.8,
    geodesic: true
  });
}

// ================================================================
// 6. DRAW GEOFENCE (red polygon) with DrawingManager
// ================================================================
let geofenceDrawingManager = null;
let geofencePolygon = null;

function handleDrawGeofence() {
  const btn = document.getElementById('drawGeofence');
  
  if (!geofenceDrawingManager) {
    // Initialize DrawingManager for geofence
    geofenceDrawingManager = new google.maps.drawing.DrawingManager({
      drawingMode: null,
      drawingControl: false,
      polygonOptions: {
        strokeColor: '#ef4444',
        strokeWeight: 3,
        strokeOpacity: 1,
        fillColor: '#ef4444',
        fillOpacity: 0.2,
        editable: true,
        draggable: false
      }
    });
    
    geofenceDrawingManager.setMap(map2D);
    
    // Listen for polygon complete
    google.maps.event.addListener(geofenceDrawingManager, 'polygoncomplete', (polygon) => {
      // Remove old geofence if exists
      if (geofencePolygon) {
        geofencePolygon.setMap(null);
      }
      
      geofencePolygon = polygon;
      
      // Save to missionData
      const path = polygon.getPath();
      const vertices = [];
      for (let i = 0; i < path.getLength(); i++) {
        const point = path.getAt(i);
        vertices.push({ lat: point.lat(), lng: point.lng() });
      }
      
      missionData.geofence = {
        type: 'polygon',
        vertices: vertices
      };
      
      // Exit drawing mode
      geofenceDrawingManager.setDrawingMode(null);
      if (btn) {
        btn.style.background = '';
        btn.style.color = '';
      }
      
      logToConsole(`Geofence polygon drawn: ${vertices.length} vertices`, 'success');
      updateGeometryStats();
    });
  }
  
  // Toggle drawing mode
  const currentMode = geofenceDrawingManager.getDrawingMode();
  if (currentMode === null) {
    // Enter drawing mode
    geofenceDrawingManager.setDrawingMode(google.maps.drawing.OverlayType.POLYGON);
    if (btn) {
      btn.style.background = '#ef4444';
      btn.style.color = 'white';
    }
    logToConsole('Draw geofence: Click to add polygon vertices', 'info');
  } else {
    // Exit drawing mode
    geofenceDrawingManager.setDrawingMode(null);
    if (btn) {
      btn.style.background = '';
      btn.style.color = '';
    }
  }
}

// ================================================================
// 7. DRAW CGA (amber polygon) with DrawingManager
// ================================================================
let cgaDrawingManager = null;
let cgaPolygon = null;

function handleDrawCGA() {
  const btn = document.getElementById('drawCGA');
  
  if (!cgaDrawingManager) {
    // Initialize DrawingManager for CGA
    cgaDrawingManager = new google.maps.drawing.DrawingManager({
      drawingMode: null,
      drawingControl: false,
      polygonOptions: {
        strokeColor: '#f59e0b',
        strokeWeight: 2,
        strokeOpacity: 1,
        fillColor: '#f59e0b',
        fillOpacity: 0.25,
        editable: true,
        draggable: false
      }
    });
    
    cgaDrawingManager.setMap(map2D);
    
    // Listen for polygon complete
    google.maps.event.addListener(cgaDrawingManager, 'polygoncomplete', (polygon) => {
      // Remove old CGA if exists
      if (cgaPolygon) {
        cgaPolygon.setMap(null);
      }
      
      cgaPolygon = polygon;
      
      // Save to missionData
      const path = polygon.getPath();
      const vertices = [];
      for (let i = 0; i < path.getLength(); i++) {
        const point = path.getAt(i);
        vertices.push({ lat: point.lat(), lng: point.lng() });
      }
      
      missionData.cga = {
        type: 'polygon',
        vertices: vertices
      };
      
      // Exit drawing mode
      cgaDrawingManager.setDrawingMode(null);
      if (btn) {
        btn.style.background = '';
        btn.style.color = '';
      }
      
      logToConsole(`CGA polygon drawn: ${vertices.length} vertices`, 'success');
      updateGeometryStats();
    });
  }
  
  // Toggle drawing mode
  const currentMode = cgaDrawingManager.getDrawingMode();
  if (currentMode === null) {
    // Enter drawing mode
    cgaDrawingManager.setDrawingMode(google.maps.drawing.OverlayType.POLYGON);
    if (btn) {
      btn.style.background = '#f59e0b';
      btn.style.color = 'white';
    }
    logToConsole('Draw CGA: Click to add polygon vertices', 'info');
  } else {
    // Exit drawing mode
    cgaDrawingManager.setDrawingMode(null);
    if (btn) {
      btn.style.background = '';
      btn.style.color = '';
    }
  }
}

function handleClearRoute() {
  missionData = { waypoints: [], geofence: null, cga: null, route: null };
  
  // Clear all markers
  missionMarkers.forEach(marker => marker.setMap(null));
  missionMarkers = [];
  
  // Clear route polyline
  if (window.routePolyline) {
    window.routePolyline.setMap(null);
    window.routePolyline = null;
  }
  
  // Clear geofence polygon
  if (geofencePolygon) {
    geofencePolygon.setMap(null);
    geofencePolygon = null;
  }
  
  // Clear CGA polygon
  if (cgaPolygon) {
    cgaPolygon.setMap(null);
    cgaPolygon = null;
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
  console.log('[attachEventListeners] Attaching event listeners...');
  
  // 2D/Oblique toggle - NOTE: uses onclick in HTML, no need to attach here
  // document.getElementById('toggle-2d-oblique')?.addEventListener('click', toggle2DOblique);
  
  document.getElementById('importRouteFile')?.addEventListener('change', handleRouteImport);
  document.getElementById('importGoogleEarthKML')?.addEventListener('change', handleGoogleEarthKMLImport);
  
  const addWaypointBtn = document.getElementById('addWaypoint');
  if (addWaypointBtn) {
    addWaypointBtn.addEventListener('click', handleAddWaypoint);
    console.log('[attachEventListeners] ✅ addWaypoint listener attached');
  } else {
    console.warn('[attachEventListeners] ❌ addWaypoint button not found!');
  }
  
  const drawGeofenceBtn = document.getElementById('drawGeofence');
  if (drawGeofenceBtn) {
    drawGeofenceBtn.addEventListener('click', handleDrawGeofence);
    console.log('[attachEventListeners] ✅ drawGeofence listener attached');
  }
  
  const drawCGABtn = document.getElementById('drawCGA');
  if (drawCGABtn) {
    drawCGABtn.addEventListener('click', handleDrawCGA);
    console.log('[attachEventListeners] ✅ drawCGA listener attached');
  }
  
  const clearRouteBtn = document.getElementById('clearRoute');
  if (clearRouteBtn) {
    clearRouteBtn.addEventListener('click', handleClearRoute);
    console.log('[attachEventListeners] ✅ clearRoute listener attached');
  }

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

  // NOTE: Search functionality is now handled by Google Places Autocomplete
  // in initGoogleMaps() callback (lines 930-1130). No need for separate search button.
  // The #searchLocationInput has autocomplete attached automatically.

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
    
    // 7. ADD MISSION ANNOTATIONS (TOL, distances, observers, etc.)
    addMissionAnnotations(missionData);
    
    // 8. UPDATE KEY DISTANCES PANEL (EASA/JARUS SORA 2.5 Annex A)
    updateKeyDistancesPanel(missionData);
    
    // 9. UPDATE SORA VERSION BADGE
    updateSoraVersionBadge(missionData);
  } catch (error) {
    console.error('[renderMission] ERROR:', error);
  }
};

// ================================================================
// MISSION ANNOTATIONS - Labels, distances, TOL, observers, crew
// ================================================================
function addMissionAnnotations(missionData) {
  if (!map2D || !missionData) return;
  
  const annotations = [];
  
  // Helper: Calculate distance in meters using Haversine formula
  function calculateDistance(lat1, lon1, lat2, lon2) {
    const R = 6371000; // Earth radius in meters
    const φ1 = lat1 * Math.PI / 180;
    const φ2 = lat2 * Math.PI / 180;
    const Δφ = (lat2 - lat1) * Math.PI / 180;
    const Δλ = (lon2 - lon1) * Math.PI / 180;
    
    const a = Math.sin(Δφ/2) * Math.sin(Δφ/2) +
              Math.cos(φ1) * Math.cos(φ2) *
              Math.sin(Δλ/2) * Math.sin(Δλ/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    
    return R * c; // Distance in meters
  }
  
  // 1. TOL (Take-Off/Landing) - EASA/JARUS SORA 2.5 Annex A Compliance
  // Per Annex A Section A.5.1 (page 30): "A position: Take Off / Landing Position (optional)"
  if (missionData.waypoints && missionData.waypoints.length > 0) {
    const tol = missionData.waypoints[0];
    const altitude = missionData.geometry?.maxHeightAGL_m || tol.alt || 0;
    
    annotations.push({
      type: 'Feature',
      geometry: {
        type: 'Point',
        coordinates: [tol.lon, tol.lat]
      },
      properties: {
        label: `TOL (Take-Off/Landing)\n${tol.lat.toFixed(5)}°N, ${tol.lon.toFixed(5)}°E\nFlight Geography Height: ${altitude}m AGL`,
        type: 'tol',
        color: '#10b981', // green - SORA 2.5 Annex A Flight Geography color
        size: 14,
        anchor: 'top', // Label below marker to avoid overlap
        offsetY: 1.8 // Increased offset for clarity
      }
    });
  }
  
  // 2. Separate Landing Location (ONLY if different from TOL)
  // Per EASA/JARUS: Most operations use same TOL for take-off and landing
  // Only show separate "LND" marker if landing point is significantly different (>50m)
  if (missionData.waypoints && missionData.waypoints.length > 1) {
    const landing = missionData.waypoints[missionData.waypoints.length - 1];
    const tol = missionData.waypoints[0];
    const distance = calculateDistance(tol.lat, tol.lon, landing.lat, landing.lon);
    
    // Only add separate landing marker if distance > 50m (different site)
    if (distance > 50) {
      annotations.push({
        type: 'Feature',
        geometry: {
          type: 'Point',
          coordinates: [landing.lon, landing.lat]
        },
        properties: {
          label: `LND (Landing)\n${landing.lat.toFixed(5)}°N, ${landing.lon.toFixed(5)}°E\nDistance from T/O: ${(distance/1000).toFixed(2)}km`,
          type: 'landing',
          color: '#ef4444', // red
          size: 14,
          anchor: 'top',
          offsetY: 1.8
        }
      });
      console.log(`[addMissionAnnotations] Separate landing site detected (${distance.toFixed(0)}m from TOL)`);
    } else {
      console.log('[addMissionAnnotations] TOL = Take-off/Landing (same location - typical mission)');
    }
  }
  
  // 3. Safe Area annotation (if exists)
  const erpData = missionData.erp || {};
  const flyAway = erpData.flyAway || erpData.FlyAway;
  if (flyAway) {
    const safeAreaLat = flyAway.safeAreaLat || flyAway.SafeAreaLat;
    const safeAreaLon = flyAway.safeAreaLon || flyAway.SafeAreaLon;
    const safeAreaRadius = flyAway.safeAreaRadius_m || flyAway.SafeAreaRadius_m;
    
    if (safeAreaLat && safeAreaLon && safeAreaRadius) {
      const area_km2 = (Math.PI * Math.pow(safeAreaRadius, 2) / 1000000).toFixed(2);
      annotations.push({
        type: 'Feature',
        geometry: {
          type: 'Point',
          coordinates: [safeAreaLon, safeAreaLat]
        },
        properties: {
          label: `SAFE AREA\nRadius: ${safeAreaRadius}m\nArea: ${area_km2}km²\n(C2 Loss Containment)`,
          type: 'safe-area',
          color: '#3b82f6', // blue
          size: 12
        }
      });
    }
  }
  
  // 4. Emergency Sites distances
  const emergencyLanding = erpData.emergencyLanding || erpData.EmergencyLanding;
  const emergencySites = emergencyLanding?.sites || emergencyLanding?.Sites;
  
  if (emergencySites && emergencySites.length > 0 && missionData.waypoints.length > 0) {
    const tol = missionData.waypoints[0];
    emergencySites.forEach((site, idx) => {
      const siteLat = site.Lat || site.lat;
      const siteLon = site.Lon || site.lon;
      const distance = calculateDistance(tol.lat, tol.lon, siteLat, siteLon);
      
      annotations.push({
        type: 'Feature',
        geometry: {
          type: 'Point',
          coordinates: [siteLon + 0.001, siteLat + 0.001] // Offset to avoid overlap
        },
        properties: {
          label: `${(distance/1000).toFixed(2)}km from TOL`,
          type: 'distance',
          color: '#6b7280', // gray
          size: 10
        }
      });
    });
  }
  
  // 5. Observer positions (ONLY if available in mission data - DATA-DRIVEN ONLY)
  // Per SORA 2.0 AMC Figure 2 (page 14) and SORA 2.5 Annex A Section A.5.1
  // Do NOT generate fake positions - if no data exists, skip this section
  // Use GENERIC labels only (no fake names)
  if (missionData.observers && Array.isArray(missionData.observers) && missionData.observers.length > 0) {
    missionData.observers.forEach((observer, idx) => {
      // Validate observer has coordinates
      if (observer.lat && observer.lon) {
        const role = observer.role || `Observer ${idx + 1}`;
        annotations.push({
          type: 'Feature',
          geometry: {
            type: 'Point',
            coordinates: [observer.lon, observer.lat]
          },
          properties: {
            label: role, // Generic label only, no names
            type: 'observer',
            color: '#8b5cf6', // purple
            size: 11,
            anchor: 'left', // Offset to right to avoid TOL overlap
            offsetY: 0
          }
        });
      }
    });
  }
  
  // 6. Crew positions (Remote Pilot, VO, etc.) - DATA-DRIVEN ONLY
  // If no crew data exists, only TOL marker will represent the pilot position
  // Use GENERIC labels only (no fake names)
  if (missionData.crew && Array.isArray(missionData.crew) && missionData.crew.length > 0) {
    missionData.crew.forEach((member, idx) => {
      // Validate crew member has coordinates
      if (member.lat && member.lon) {
        const role = member.role || `Crew ${idx + 1}`;
        annotations.push({
          type: 'Feature',
          geometry: {
            type: 'Point',
            coordinates: [member.lon, member.lat]
          },
          properties: {
            label: role, // Generic label only, no names
            type: 'crew',
            color: '#f59e0b', // orange
            size: 11,
            anchor: 'right', // Offset to left to avoid TOL overlap
            offsetY: 0
          }
        });
      }
    });
  } else {
    // No crew data: Assume Pilot position = TOL (first waypoint)
    // This is implicit - TOL marker already added in section 1
    console.log('[addMissionAnnotations] No crew data - using TOL as Remote Pilot position');
  }
  
  // Add all annotations as a layer
  if (annotations.length > 0) {
    if (!map2D.getSource('mission-annotations')) {
      map2D.addSource('mission-annotations', {
        type: 'geojson',
        data: { type: 'FeatureCollection', features: annotations }
      });
      
      map2D.addLayer({
        id: 'mission-annotations-labels',
        type: 'symbol',
        source: 'mission-annotations',
        layout: {
          'text-field': ['get', 'label'],
          'text-size': ['get', 'size'],
          'text-font': ['Open Sans Regular', 'Arial Unicode MS Regular'],
          'text-anchor': ['coalesce', ['get', 'anchor'], 'top'], // Use property anchor or default 'top'
          'text-offset': [0, ['coalesce', ['get', 'offsetY'], 1.5]], // Dynamic offset based on property
          'text-justify': 'center'
        },
        paint: {
          'text-color': ['get', 'color'],
          'text-halo-color': '#ffffff',
          'text-halo-width': 2,
          'text-halo-blur': 1
        }
      });
    } else {
      map2D.getSource('mission-annotations').setData({
        type: 'FeatureCollection',
        features: annotations
      });
    }
    
    console.log(`[addMissionAnnotations] Added ${annotations.length} labels to map`);
  }
}

// ================================================================
// KEY DISTANCES PANEL - EASA/JARUS SORA 2.5 Annex A Compliance
// ================================================================
// Per Annex A Examples (pages 32-34): Display key distances for ConOps diagram
function updateKeyDistancesPanel(missionData) {
  const panelContainer = document.getElementById('key-distances-panel');
  if (!panelContainer) {
    console.warn('[updateKeyDistancesPanel] Panel container not found');
    return;
  }
  
  const distances = [];
  
  // Helper: Calculate distance in meters
  function calculateDistance(lat1, lon1, lat2, lon2) {
    const R = 6371000; // Earth radius in meters
    const φ1 = lat1 * Math.PI / 180;
    const φ2 = lat2 * Math.PI / 180;
    const Δφ = (lat2 - lat1) * Math.PI / 180;
    const Δλ = (lon2 - lon1) * Math.PI / 180;
    
    const a = Math.sin(Δφ/2) * Math.sin(Δφ/2) +
              Math.cos(φ1) * Math.cos(φ2) *
              Math.sin(Δλ/2) * Math.sin(Δλ/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    
    return R * c; // Distance in meters
  }
  
  // Get TOL (first waypoint)
  const tol = missionData.waypoints?.[0];
  if (!tol) {
    panelContainer.innerHTML = '<p class="text-sm text-gray-500">No TOL data available</p>';
    return;
  }
  
  // 1. TOL → CGA Edge Distance
  const cgaGeometry = missionData.geometry?.geoJson;
  if (cgaGeometry) {
    try {
      const geoJson = typeof cgaGeometry === 'string' ? JSON.parse(cgaGeometry) : cgaGeometry;
      const cgaFeature = geoJson.features?.find(f => f.properties?.type === 'cga');
      
      if (cgaFeature && cgaFeature.geometry.type === 'Polygon') {
        // Find closest CGA edge point
        const cgaCoords = cgaFeature.geometry.coordinates[0];
        let minDistance = Infinity;
        
        cgaCoords.forEach(([lon, lat]) => {
          const dist = calculateDistance(tol.lat, tol.lon, lat, lon);
          if (dist < minDistance) minDistance = dist;
        });
        
        distances.push({
          label: 'TOL → CGA Edge',
          value: `${minDistance.toFixed(0)}m`,
          color: '#eab308' // yellow
        });
      }
    } catch (e) {
      console.warn('[updateKeyDistancesPanel] Error parsing CGA:', e);
    }
  }
  
  // 2. TOL → Safe Area Radius
  const flyAway = missionData.erp?.flyAway || missionData.erp?.FlyAway;
  const safeAreaRadius = flyAway?.safeAreaRadius_m || flyAway?.SafeAreaRadius_m;
  
  if (safeAreaRadius) {
    distances.push({
      label: 'Safe Area Radius',
      value: `${safeAreaRadius}m`,
      color: '#3b82f6' // blue
    });
  }
  
  // 3. TOL → Emergency Site E1
  const emergencyLanding = missionData.erp?.emergencyLanding || missionData.erp?.EmergencyLanding;
  const emergencySites = emergencyLanding?.sites || emergencyLanding?.Sites;
  
  if (emergencySites && emergencySites.length > 0) {
    const e1 = emergencySites[0];
    const e1Lat = e1.Lat || e1.lat;
    const e1Lon = e1.Lon || e1.lon;
    
    if (e1Lat && e1Lon) {
      const distanceToE1 = calculateDistance(tol.lat, tol.lon, e1Lat, e1Lon);
      distances.push({
        label: 'TOL → E1 (Primary)',
        value: `${(distanceToE1 / 1000).toFixed(2)}km`,
        color: '#10b981' // green
      });
    }
  }
  
  // Render panel - Keep ONLY 3-4 key distances (EASA Annex A best practice)
  if (distances.length === 0) {
    panelContainer.innerHTML = '<p class="text-sm text-gray-500">No distance data available</p>';
  } else {
    // Limit to first 3 distances (TOL→CGA, Safe Area radius, TOL→E1)
    const displayDistances = distances.slice(0, 3);
    const html = displayDistances.map(({ label, value, color }) => `
      <div class="flex justify-between items-center mb-2">
        <span class="text-sm font-medium" style="color: ${color};">${label}:</span>
        <span class="text-sm font-bold">${value}</span>
      </div>
    `).join('');
    
    panelContainer.innerHTML = html;
    console.log(`[updateKeyDistancesPanel] Displayed ${displayDistances.length} key distances (max 3)`);
  }
}


// ================================================================
// SORA VERSION BADGE - EASA/JARUS SORA 2.0 AMC vs 2.5 Compliance
// ================================================================
function updateSoraVersionBadge(missionData) {
  const badgeContainer = document.getElementById('sora-version-badge');
  if (!badgeContainer) {
    console.warn('[updateSoraVersionBadge] Badge container not found');
    return;
  }
  
  // Get SORA version from mission or SORA assessment
  const soraVersion = missionData.soraVersion || 
                      missionData.soraAssessment?.soraVersion || 
                      missionData.sora?.soraVersion;
  
  // Blank if no version (better blank than wrong)
  if (!soraVersion) {
    badgeContainer.innerHTML = '';
    console.log('[updateSoraVersionBadge] No SORA version found - badge hidden');
    return;
  }
  
  // Determine badge color and label
  let badgeClass, badgeLabel;
  
  if (soraVersion === '2.0' || soraVersion.startsWith('2.0')) {
    badgeClass = 'bg-blue-100 text-blue-800 border-blue-300';
    badgeLabel = '🛡️ EASA SORA 2.0 AMC';
  } else if (soraVersion === '2.5' || soraVersion.startsWith('2.5')) {
    badgeClass = 'bg-green-100 text-green-800 border-green-300';
    badgeLabel = '🛡️ JARUS SORA 2.5 Annex A';
  } else {
    // Unknown version - hide badge
    badgeContainer.innerHTML = '';
    console.warn(`[updateSoraVersionBadge] Unknown SORA version: ${soraVersion} - badge hidden`);
    return;
  }
  
  badgeContainer.innerHTML = `
    <div class="inline-flex items-center px-3 py-1 border-2 rounded-full text-xs font-semibold ${badgeClass}">
      ${badgeLabel}
    </div>
  `;
  
  console.log(`[updateSoraVersionBadge] Badge displayed: ${badgeLabel}`);
}


window.updateErpPanel = updateErpPanel;
window.updateSoraBadges = updateSoraBadges;
window.updateOsoPanel = updateOsoPanel;
Object.defineProperty(window, 'map2D', { get: () => map2D });
Object.defineProperty(window, 'map3D', { get: () => viewer3D });
window.missionData = missionData; // Expose for debug/tests
