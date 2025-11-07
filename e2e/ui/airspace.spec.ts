/**
 * E2E Tests: Phase 6 - Airspace Maps
 * Tests: 2D/3D toggle, layer visibility, route import, export KML
 */

import { test, expect } from '@playwright/test';

test.describe('Airspace Maps - Phase 6', () => {
  
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
  });

  test('should load map in 2D mode by default', async ({ page }) => {
    // Wait for MapLibre GL JS to initialize
    await page.waitForSelector('#map2D', { state: 'visible' });
    
    // 3D map should be hidden
    const map3D = page.locator('#map3D');
    await expect(map3D).toHaveCSS('display', 'none');
    
    // Toggle button should say "Switch to 3D"
    const toggleBtn = page.locator('#toggle2D3D');
    await expect(toggleBtn).toContainText('Switch to 3D');
  });

  test('should toggle between 2D and 3D', async ({ page }) => {
    const toggleBtn = page.locator('#toggle2D3D');
    const map2D = page.locator('#map2D');
    const map3D = page.locator('#map3D');
    
    // Initially in 2D
    await expect(map2D).toBeVisible();
    await expect(map3D).toHaveCSS('display', 'none');
    
    // Toggle to 3D
    await toggleBtn.click();
    await page.waitForTimeout(500); // Wait for transition
    
    // Now 3D visible, 2D hidden
    await expect(map3D).toBeVisible();
    await expect(map2D).toHaveCSS('display', 'none');
    await expect(toggleBtn).toContainText('Switch to 2D');
    
    // Toggle back to 2D
    await toggleBtn.click();
    await page.waitForTimeout(500);
    
    await expect(map2D).toBeVisible();
    await expect(map3D).toHaveCSS('display', 'none');
    await expect(toggleBtn).toContainText('Switch to 3D');
  });

  test('should have all EU airspace layer checkboxes', async ({ page }) => {
    const layers = [
      'layer_rmz', 'layer_tmz', 'layer_ctr', 'layer_tma', 'layer_atz',
      'layer_prohibited', 'layer_restricted', 'layer_danger',
      'layer_tsa', 'layer_tra', 'layer_cba', 'layer_uas',
      'layer_population', 'layer_buildings'
    ];
    
    for (const layerId of layers) {
      const checkbox = page.locator(`#${layerId}`);
      await expect(checkbox).toBeVisible();
      await expect(checkbox).toHaveAttribute('type', 'checkbox');
    }
  });

  test('should check default layer visibility (RMZ, TMZ, CTR, Prohibited, Restricted, UAS)', async ({ page }) => {
    // Default checked: RMZ, TMZ, CTR, Prohibited, Restricted, UAS
    await expect(page.locator('#layer_rmz')).toBeChecked();
    await expect(page.locator('#layer_tmz')).toBeChecked();
    await expect(page.locator('#layer_ctr')).toBeChecked();
    await expect(page.locator('#layer_prohibited')).toBeChecked();
    await expect(page.locator('#layer_restricted')).toBeChecked();
    await expect(page.locator('#layer_uas')).toBeChecked();
    
    // Default unchecked: TMA, ATZ, Danger, TSA, TRA, CBA, Population, Buildings
    await expect(page.locator('#layer_tma')).not.toBeChecked();
    await expect(page.locator('#layer_atz')).not.toBeChecked();
    await expect(page.locator('#layer_danger')).not.toBeChecked();
    await expect(page.locator('#layer_tsa')).not.toBeChecked();
    await expect(page.locator('#layer_tra')).not.toBeChecked();
    await expect(page.locator('#layer_cba')).not.toBeChecked();
    await expect(page.locator('#layer_population')).not.toBeChecked();
    await expect(page.locator('#layer_buildings')).not.toBeChecked();
  });

  test('should add waypoint when clicking "Add Waypoint"', async ({ page }) => {
    const addWpBtn = page.locator('#addWaypoint');
    const waypointStat = page.locator('#stat_waypoints');
    
    // Initially 0 waypoints
    await expect(waypointStat).toHaveText('0');
    
    // Add waypoint
    await addWpBtn.click();
    await page.waitForTimeout(300);
    
    // Now 1 waypoint
    await expect(waypointStat).toHaveText('1');
    
    // Add another
    await addWpBtn.click();
    await page.waitForTimeout(300);
    
    await expect(waypointStat).toHaveText('2');
  });

  test('should draw geofence (requires waypoints)', async ({ page }) => {
    const geofenceBtn = page.locator('#drawGeofence');
    const geofenceStat = page.locator('#stat_geofence');
    
    // Initially no geofence
    await expect(geofenceStat).toHaveText('None');
    
    // Add waypoint first
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(300);
    
    // Draw geofence
    await geofenceBtn.click();
    await page.waitForTimeout(300);
    
    // Geofence should be drawn
    await expect(geofenceStat).toContainText('Cylinder: 500m radius, 150m height');
  });

  test('should draw CGA (requires 2+ waypoints)', async ({ page }) => {
    const cgaBtn = page.locator('#drawCGA');
    const cgaAreaStat = page.locator('#stat_cga_area');
    
    // Initially 0 m²
    await expect(cgaAreaStat).toHaveText('0 m²');
    
    // Add 2 waypoints
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(200);
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(200);
    
    // Draw CGA
    await cgaBtn.click();
    await page.waitForTimeout(300);
    
    // CGA area should be > 0
    const areaText = await cgaAreaStat.textContent();
    const areaMatch = areaText?.match(/(\d+)\s*m²/);
    expect(parseInt(areaMatch?.[1] || '0')).toBeGreaterThan(0);
  });

  test('should clear route when clicking "Clear Route"', async ({ page }) => {
    // Add waypoints
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(200);
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(200);
    
    // Verify waypoints added
    await expect(page.locator('#stat_waypoints')).toHaveText('2');
    
    // Clear route
    await page.locator('#clearRoute').click();
    await page.waitForTimeout(300);
    
    // All stats reset
    await expect(page.locator('#stat_waypoints')).toHaveText('0');
    await expect(page.locator('#stat_path_length')).toHaveText('0 m');
    await expect(page.locator('#stat_geofence')).toHaveText('None');
    await expect(page.locator('#stat_cga_area')).toHaveText('0 m²');
  });

  test('should have export buttons (GeoJSON, KML, CSV, PNG, Mission Pack)', async ({ page }) => {
    await expect(page.locator('#exportGeoJSON')).toBeVisible();
    await expect(page.locator('#exportKML')).toBeVisible();
    await expect(page.locator('#exportCSV')).toBeVisible();
    await expect(page.locator('#exportPNG')).toBeVisible();
    await expect(page.locator('#exportMissionPack')).toBeVisible();
  });

  test('should export GeoJSON (download triggered)', async ({ page }) => {
    // Add waypoint
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(300);
    
    // Listen for download
    const downloadPromise = page.waitForEvent('download');
    await page.locator('#exportGeoJSON').click();
    
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toBe('mission.geojson');
  });

  test('should export KML (download triggered)', async ({ page }) => {
    // Add waypoint
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(300);
    
    // Listen for download
    const downloadPromise = page.waitForEvent('download');
    await page.locator('#exportKML').click();
    
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toBe('mission.kml');
  });

  test('should export CSV (download triggered)', async ({ page }) => {
    // Add waypoint
    await page.locator('#addWaypoint').click();
    await page.waitForTimeout(300);
    
    // Listen for download
    const downloadPromise = page.waitForEvent('download');
    await page.locator('#exportCSV').click();
    
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toBe('mission.csv');
  });

  test('should display SORA compliance badges (default state)', async ({ page }) => {
    await expect(page.locator('#kpi_igrc')).toHaveText('–');
    await expect(page.locator('#kpi_fgrc')).toHaveText('–');
    await expect(page.locator('#kpi_iarc')).toHaveText('–');
    await expect(page.locator('#kpi_rarc')).toHaveText('–');
    await expect(page.locator('#sailBadge')).toContainText('SAIL: –');
  });

  test('should display ERP/TMPR checkboxes', async ({ page }) => {
    await expect(page.locator('#erp_emergency_sites')).toBeVisible();
    await expect(page.locator('#erp_recovery_routes')).toBeVisible();
    await expect(page.locator('#erp_muster_points')).toBeVisible();
    await expect(page.locator('#erp_comms_backup')).toBeVisible();
  });

  test('should have file import input', async ({ page }) => {
    const fileInput = page.locator('#importRouteFile');
    await expect(fileInput).toBeVisible();
    await expect(fileInput).toHaveAttribute('accept', '.geojson,.kml,.csv');
  });

  test('should log to validation console', async ({ page }) => {
    const console = page.locator('#validationConsole');
    
    // Wait for init message
    await page.waitForTimeout(500);
    
    const consoleText = await console.textContent();
    expect(consoleText).toContain('Airspace Maps initialized (2D mode)');
  });

  test('should NOT contain banned tokens (ACE, Mode-S veil)', async ({ page }) => {
    const bodyText = await page.textContent('body');
    
    // Ban tokens check
    expect(bodyText).not.toContain('ACE');
    expect(bodyText).not.toContain('Mode-S veil');
    
    // Should contain EU airspace terms
    expect(bodyText).toContain('RMZ');
    expect(bodyText).toContain('TMZ');
    expect(bodyText).toContain('CTR');
    expect(bodyText).toContain('UAS Geo Zones');
  });

});
