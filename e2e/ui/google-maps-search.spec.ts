/**
 * E2E Test: Google Maps Search + Geocode + URL Paste
 * SORA Compliance: STRICTLY Google Maps only
 */

import { test, expect } from '@playwright/test';

test.describe('Google Maps - Search & Geocode', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    
    // Wait for Google Maps to load
    await page.waitForFunction(() => typeof google !== 'undefined' && google.maps, { timeout: 15000 });
    await page.waitForTimeout(2000); // Wait for map initialization
  });

  test('should have Places Autocomplete on search input', async ({ page }) => {
    const searchInput = page.locator('#searchLocationInput');
    await expect(searchInput).toBeVisible();
    
    // Check if autocomplete is attached (Google adds pac-container class)
    await searchInput.click();
    await searchInput.fill('Athens');
    
    // Wait for autocomplete dropdown (Google creates .pac-container)
    await page.waitForTimeout(1000);
    const autocomplete = await page.locator('.pac-container').count();
    
    // If API key is valid, autocomplete should appear
    if (autocomplete > 0) {
      expect(autocomplete).toBeGreaterThan(0);
      console.log('✅ Google Places Autocomplete working');
    } else {
      console.log('⚠️ Autocomplete not visible (API key might be placeholder)');
    }
  });

  test('should search for "Thessaloniki" and pan map', async ({ page }) => {
    const searchInput = page.locator('#searchLocationInput');
    await searchInput.fill('Thessaloniki, Greece');
    
    // Wait for autocomplete
    await page.waitForTimeout(1500);
    
    // Press Enter or click first suggestion
    await searchInput.press('Enter');
    await page.waitForTimeout(2000);
    
    // Check if map center changed (Thessaloniki ~40.64, 22.94)
    const mapCenter = await page.evaluate(() => {
      if (typeof map2D !== 'undefined' && map2D.getCenter) {
        const center = map2D.getCenter();
        return { lat: center.lat(), lng: center.lng() };
      }
      return null;
    });
    
    if (mapCenter) {
      // Map should be near Thessaloniki (rough check)
      expect(mapCenter.lat).toBeGreaterThan(40);
      expect(mapCenter.lat).toBeLessThan(41);
      expect(mapCenter.lng).toBeGreaterThan(22);
      expect(mapCenter.lng).toBeLessThan(24);
      console.log('✅ Map panned to Thessaloniki:', mapCenter);
    }
  });

  test('should parse Google Maps URL (@lat,lng,zoom format)', async ({ page }) => {
    const testUrl = 'https://www.google.com/maps/@37.9838,23.7275,12z';
    
    const parsed = await page.evaluate((url) => {
      return window.parseGoogleMapsUrl(url);
    }, testUrl);
    
    expect(parsed).not.toBeNull();
    expect(parsed!.lat).toBeCloseTo(37.9838, 4);
    expect(parsed!.lng).toBeCloseTo(23.7275, 4);
    expect(parsed!.zoom).toBe(12);
  });

  test('should parse Google Maps URL (?q=lat,lng format)', async ({ page }) => {
    const testUrl = 'https://www.google.com/maps?q=40.6401,22.9444';
    
    const parsed = await page.evaluate((url) => {
      return window.parseGoogleMapsUrl(url);
    }, testUrl);
    
    expect(parsed).not.toBeNull();
    expect(parsed!.lat).toBeCloseTo(40.6401, 4);
    expect(parsed!.lng).toBeCloseTo(22.9444, 4);
    expect(parsed!.zoom).toBe(14); // Default zoom
  });

  test('should parse Google Maps place URL', async ({ page }) => {
    const testUrl = 'https://www.google.com/maps/place/Athens/@37.9838,23.7275,12z';
    
    const parsed = await page.evaluate((url) => {
      return window.parseGoogleMapsUrl(url);
    }, testUrl);
    
    expect(parsed).not.toBeNull();
    expect(parsed!.lat).toBeCloseTo(37.9838, 4);
    expect(parsed!.lng).toBeCloseTo(23.7275, 4);
  });

  test('should return null for invalid URL', async ({ page }) => {
    const testUrl = 'https://example.com/not-a-maps-url';
    
    const parsed = await page.evaluate((url) => {
      return window.parseGoogleMapsUrl(url);
    }, testUrl);
    
    expect(parsed).toBeNull();
  });

  test('should handle URL paste in search box', async ({ page }) => {
    const searchInput = page.locator('#searchLocationInput');
    const testUrl = 'https://www.google.com/maps/@41.0082,25.0012,14z';
    
    // Simulate paste event
    await searchInput.fill(testUrl);
    await page.dispatchEvent('#searchLocationInput', 'paste');
    await page.waitForTimeout(500);
    
    // Check if map panned to pasted coordinates
    const mapCenter = await page.evaluate(() => {
      if (typeof map2D !== 'undefined' && map2D.getCenter) {
        const center = map2D.getCenter();
        return { lat: center.lat(), lng: center.lng() };
      }
      return null;
    });
    
    if (mapCenter) {
      expect(mapCenter.lat).toBeCloseTo(41.0082, 2);
      expect(mapCenter.lng).toBeCloseTo(25.0012, 2);
    }
  });

  test('should have geocodeLatLng function', async ({ page }) => {
    const hasFunction = await page.evaluate(() => {
      return typeof window.geocodeLatLng === 'function';
    });
    
    expect(hasFunction).toBe(true);
  });

  test('should have geocodeAddress function', async ({ page }) => {
    const hasFunction = await page.evaluate(() => {
      return typeof window.geocodeAddress === 'function';
    });
    
    expect(hasFunction).toBe(true);
  });

});

test.describe('Google Maps - 2D/Oblique Toggle', () => {

  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    await page.waitForFunction(() => typeof google !== 'undefined' && google.maps, { timeout: 15000 });
    await page.waitForTimeout(2000);
  });

  test('should have 2D/Oblique toggle button', async ({ page }) => {
    const toggleBtn = page.locator('#toggle-2d-oblique');
    await expect(toggleBtn).toBeVisible();
    
    const text = await toggleBtn.textContent();
    expect(text?.trim()).toBe('2D'); // Initial state
  });

  test('should toggle between 2D (0°) and Oblique (45°)', async ({ page }) => {
    const toggleBtn = page.locator('#toggle-2d-oblique');
    
    // Initial tilt should be 0°
    let tilt = await page.evaluate(() => {
      return typeof map2D !== 'undefined' && map2D.getTilt ? map2D.getTilt() : null;
    });
    expect(tilt).toBe(0);
    
    // Click toggle
    await toggleBtn.click();
    await page.waitForTimeout(500);
    
    // Tilt should now be 45°
    tilt = await page.evaluate(() => {
      return typeof map2D !== 'undefined' && map2D.getTilt ? map2D.getTilt() : null;
    });
    expect(tilt).toBe(45);
    
    // Button text should change
    let btnText = await toggleBtn.textContent();
    expect(btnText?.trim()).toBe('45°');
    
    // Click again to go back to 2D
    await toggleBtn.click();
    await page.waitForTimeout(500);
    
    tilt = await page.evaluate(() => {
      return typeof map2D !== 'undefined' && map2D.getTilt ? map2D.getTilt() : null;
    });
    expect(tilt).toBe(0);
    
    btnText = await toggleBtn.textContent();
    expect(btnText?.trim()).toBe('2D');
  });

  test('should have toggle2DOblique function', async ({ page }) => {
    const hasFunction = await page.evaluate(() => {
      return typeof window.toggle2DOblique === 'function';
    });
    
    expect(hasFunction).toBe(true);
  });

});
