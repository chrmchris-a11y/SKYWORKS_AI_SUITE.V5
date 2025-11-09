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
// ================================================================

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
  logToConsole('Airspace Maps initialized (2D mode)', 'success');
});

function init2DMap() {
  // MapLibre GL JS (OSM raster tiles)
  map2D = new maplibregl.Map({
    container: 'map2D',
    style: {
      version: 8,
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
  // CesiumJS (requires ion token for terrain/imagery)
  // For demo, using default Cesium ion assets
  Cesium.Ion.defaultAccessToken = 'YOUR_CESIUM_ION_TOKEN'; // Replace with production token
  
  viewer3D = new Cesium.Viewer('map3D', {
    terrainProvider: Cesium.createWorldTerrain(),
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
  // Production: use toGeoJSON library or DOMParser
  logToConsole('KML import: basic parser (use toGeoJSON for production)', 'warning');
  
  const parser = new DOMParser();
  const xml = parser.parseFromString(kmlText, 'text/xml');
  const placemarks = xml.getElementsByTagName('Placemark');

  Array.from(placemarks).forEach(pm => {
    const coords = pm.getElementsByTagName('coordinates')[0]?.textContent.trim();
    if (!coords) return;

    // Parse "lon,lat,alt lon,lat,alt ..."
    const points = coords.split(/\s+/).map(p => {
      const [lon, lat, alt] = p.split(',').map(Number);
      return { lat, lon, alt: alt || 0 };
    });

    points.forEach(p => addWaypoint(p.lat, p.lon, p.alt));
  });

  renderMissionGeometry();
  updateGeometryStats();
  logToConsole(`KML imported: ${missionData.waypoints.length} waypoints`, 'success');
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
  const center = map2D ? map2D.getCenter() : { lng: 13.4050, lat: 52.5200 };
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

function renderMissionGeometry() {
  // Render waypoints, route, geofence, CGA on 2D map
  if (!map2D) return;

  // Route (LineString)
  if (missionData.waypoints.length > 1) {
    const coords = missionData.waypoints.map(wp => [wp.lon, wp.lat]);
    const routeGeoJSON = {
      type: 'Feature',
      geometry: { type: 'LineString', coordinates: coords }
    };

    if (map2D.getSource('mission-route')) {
      map2D.getSource('mission-route').setData(routeGeoJSON);
    } else {
      map2D.addSource('mission-route', { type: 'geojson', data: routeGeoJSON });
      map2D.addLayer({
        id: 'mission-route',
        type: 'line',
        source: 'mission-route',
        paint: {
          'line-color': '#2563eb',
          'line-width': 3
        }
      });
    }
  }

  // CGA (Polygon)
  if (missionData.cga) {
    if (map2D.getSource('mission-cga')) {
      map2D.getSource('mission-cga').setData(missionData.cga);
    } else {
      map2D.addSource('mission-cga', { type: 'geojson', data: missionData.cga });
      map2D.addLayer({
        id: 'mission-cga',
        type: 'fill',
        source: 'mission-cga',
        paint: {
          'fill-color': '#10b981',
          'fill-opacity': 0.2
        }
      });
    }
  }
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
// ================================================================
function updateSORABadges() {
  // If no waypoints, clear badges
  if (missionData.waypoints.length === 0) {
    document.getElementById('kpi_igrc').textContent = '–';
    document.getElementById('kpi_fgrc').textContent = '–';
    document.getElementById('kpi_iarc').textContent = '–';
    document.getElementById('kpi_rarc').textContent = '–';
    document.getElementById('sailBadge').textContent = 'SAIL: –';
    return;
  }

  // Extract mission parameters from geometry
  const params = extractSORAParams();

  // Calculate SORA 2.5 (default for Phase 6)
  // User can toggle to SORA 2.0 via future UI switch
  const soraVersion = '2.5'; // TODO: Add version selector in UI
  const results = soraVersion === '2.5' 
    ? calculateSORA25(params) 
    : calculateSORA20(params);

  // Update badges
  document.getElementById('kpi_igrc').textContent = results.initialGRC;
  document.getElementById('kpi_fgrc').textContent = results.finalGRC;
  document.getElementById('kpi_iarc').textContent = results.initialARC;
  document.getElementById('kpi_rarc').textContent = results.residualARC;
  document.getElementById('sailBadge').textContent = `SAIL: ${results.SAIL || '–'}`;
  document.getElementById('sailBadge').className = `sail-badge sail-${results.SAIL?.toLowerCase() || 'none'}`;

  // Log to validation console
  logToConsole(`SORA ${soraVersion}: GRC=${results.finalGRC}, ARC=${results.residualARC}, SAIL=${results.SAIL}`, 'success');
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
    
    // Mitigations (SORA 2.5 format: lowercase m1a/m1b/m1c)
    m1a: 'None', m1b: 'None', m1c: 'None', m2: 'None',
    tmpr_low: false, tmpr_medium: false, tmpr_high: false,
    tactical_low: false, tactical_medium: false, tactical_high: false,
    
    // Strategic mitigations
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

  return params;
}


// ================================================================
// 5. 2D/3D TOGGLE
// ================================================================
function toggle2D3D() {
  if (currentMode === '2D') {
    // Switch to 3D
    document.getElementById('map2D').style.display = 'none';
    document.getElementById('map3D').style.display = 'block';
    document.getElementById('toggle2D3D').textContent = 'Switch to 2D 🗺️';
    
    if (!viewer3D) {
      init3DMap();
    }
    
    currentMode = '3D';
    logToConsole('Switched to 3D viewer (CesiumJS)', 'success');
  } else {
    // Switch to 2D
    document.getElementById('map3D').style.display = 'none';
    document.getElementById('map2D').style.display = 'block';
    document.getElementById('toggle2D3D').textContent = 'Switch to 3D 🌍';
    
    currentMode = '2D';
    logToConsole('Switched to 2D map (MapLibre GL JS)', 'success');
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
  a.click();
  URL.revokeObjectURL(url);
}

// ================================================================
// 8. EVENT LISTENERS
// ================================================================
function attachEventListeners() {
  document.getElementById('toggle2D3D').addEventListener('click', toggle2D3D);
  document.getElementById('importRouteFile').addEventListener('change', handleRouteImport);
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
