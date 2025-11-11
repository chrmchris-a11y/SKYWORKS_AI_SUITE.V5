/**
 * E2E Test: Google Maps Initialization
 * Verifies Google Maps script loads and map bootstraps correctly
 * 
 * SORA Compliance: STRICTLY Google Maps only (NO OSM/Nominatim/MapLibre/Leaflet/Cesium)
 */

import { test, expect } from '@playwright/test';

test.describe('Google Maps Initialization', () => {
  
  test.beforeEach(async ({ page }) => {
    // Navigate to airspace maps page
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
  });

  test('should load maps.config.json successfully', async ({ page }) => {
    // Wait for config loader to execute
    await page.waitForTimeout(1000);
    
    // Check console for config load message
    const logs = [];
    page.on('console', msg => logs.push(msg.text()));
    
    await page.waitForTimeout(500);
    
    const configLog = logs.find(log => log.includes('[Config Loader] Config loaded'));
    expect(configLog).toBeTruthy();
  });

  test('should show blocking error if API key is placeholder', async ({ page }) => {
    // If API key is still placeholder, blocking error should appear
    const errorOverlay = await page.locator('#google-maps-error-overlay').count();
    
    if (errorOverlay > 0) {
      // Error overlay visible (expected if key not configured)
      const errorTitle = await page.locator('#google-maps-error-overlay h2').textContent();
      expect(errorTitle).toContain('Google Maps API Key Required');
      
      console.log('⚠️ Test detected placeholder API key - blocking error shown (EXPECTED)');
      test.skip(); // Skip remaining tests if key not configured
    }
  });

  test('should inject Google Maps script tag', async ({ page }) => {
    // Wait for script injection
    await page.waitForTimeout(2000);
    
    // Check for Google Maps script in <head>
    const googleScript = await page.locator('script[src*="maps.googleapis.com"]').count();
    expect(googleScript).toBeGreaterThan(0);
  });

  test('should load Google Maps JavaScript API', async ({ page }) => {
    // Wait for Google Maps to load
    await page.waitForFunction(() => typeof google !== 'undefined' && google.maps, { timeout: 10000 });
    
    // Verify google.maps object exists
    const hasMapsAPI = await page.evaluate(() => {
      return typeof google !== 'undefined' && 
             typeof google.maps !== 'undefined' &&
             typeof google.maps.Map !== 'undefined';
    });
    
    expect(hasMapsAPI).toBe(true);
  });

  test('should load required Google Maps libraries (places, geocoding)', async ({ page }) => {
    await page.waitForFunction(() => typeof google !== 'undefined' && google.maps, { timeout: 10000 });
    
    const hasLibraries = await page.evaluate(() => {
      return {
        places: typeof google.maps.places !== 'undefined' && typeof google.maps.places.Autocomplete !== 'undefined',
        geocoder: typeof google.maps.Geocoder !== 'undefined',
        marker: typeof google.maps.Marker !== 'undefined'
      };
    });
    
    expect(hasLibraries.places).toBe(true);
    expect(hasLibraries.geocoder).toBe(true);
    expect(hasLibraries.marker).toBe(true);
  });

  test('should pass healthcheck', async ({ page }) => {
    await page.waitForFunction(() => typeof google !== 'undefined' && google.maps, { timeout: 10000 });
    
    // Call healthcheck function
    const healthOK = await page.evaluate(() => {
      return window.checkGoogleMapsHealth();
    });
    
    expect(healthOK).toBe(true);
  });

  test('should initialize Google Map in #map2D container', async ({ page }) => {
    await page.waitForFunction(() => typeof google !== 'undefined' && google.maps, { timeout: 10000 });
    
    // Wait for initGoogleMaps callback (if implemented)
    await page.waitForTimeout(3000);
    
    // Check if map container has Google Maps elements
    const hasGoogleUI = await page.locator('#map2D img[src*="gstatic.com"]').count();
    expect(hasGoogleUI).toBeGreaterThan(0); // Google Maps UI elements present
  });

  test('should NOT load OSM/MapLibre/Leaflet/Cesium', async ({ page }) => {
    // Verify NO other mapping libraries are loaded
    const hasMapLibre = await page.evaluate(() => typeof maplibregl !== 'undefined');
    const hasLeaflet = await page.evaluate(() => typeof L !== 'undefined');
    const hasCesium = await page.evaluate(() => typeof Cesium !== 'undefined');
    
    expect(hasMapLibre).toBe(false);
    expect(hasLeaflet).toBe(false);
    expect(hasCesium).toBe(false);
  });

  test('should display Google Maps watermark/logo', async ({ page }) => {
    await page.waitForFunction(() => typeof google !== 'undefined' && google.maps, { timeout: 10000 });
    await page.waitForTimeout(3000);
    
    // Google Maps always shows its logo (bottom-left typically)
    const googleLogo = await page.locator('a[href*="google.com/maps"]').count();
    expect(googleLogo).toBeGreaterThan(0);
  });

});
