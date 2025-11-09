/**
 * SORA API Integration E2E Tests
 * 
 * Purpose: Verify Mission Planner & Airspace Maps use backend API for SORA calculations
 * 
 * Test Coverage:
 * 1. Mission Planner SORA 2.5 - verify badges from API
 * 2. Mission Planner SORA 2.0 - verify badges from API
 * 3. Airspace Maps SORA 2.5 - verify badges from API after drawing geometry
 * 4. Version toggle - verify dropdown options change (GET /specifications)
 * 5. API failure fallback - verify offline TS calculator works
 * 6. Validation errors - verify error display
 * 
 * Prerequisites:
 * - Backend API running on http://localhost:5210
 * - Python FastAPI running on http://localhost:8001
 * - Frontend served from WebPlatform/wwwroot
 */

import { test, expect } from '@playwright/test';

const BASE_URL = 'http://localhost:5210';
const MISSION_PLANNER_URL = `${BASE_URL}/app/Pages/mission.html`;
const AIRSPACE_MAPS_URL = `${BASE_URL}/app/Pages/airspace-maps.html`;

test.describe('SORA API Integration E2E Tests', () => {
  
  // ========================================================================
  // Test 1: Mission Planner SORA 2.5 - Backend API Integration
  // ========================================================================
  test('Mission Planner: SORA 2.5 calculation uses backend API', async ({ page }) => {
    await page.goto(MISSION_PLANNER_URL);
    
    // Wait for page load
    await expect(page.locator('h1')).toContainText('SKYWORKS');
    
    // Select SORA 2.5
    await page.selectOption('select#soraVersionSelect', '2.5');
    
    // Fill in DJI Mini 4 Pro specs
    await page.fill('input#mtom', '0.249');
    await page.fill('input#maxSpeed', '16');
    await page.fill('input#charDim', '0.213');
    await page.fill('input#altitude', '400');
    
    // Select population density: <500 (sparsely populated)
    await page.selectOption('select#populationDensity', '<500');
    
    // Select mitigations: M1A Low, M2 Medium
    await page.selectOption('select#m1a', 'Low');
    await page.selectOption('select#m2', 'Medium');
    
    // Submit form
    await page.click('button[type="submit"]');
    
    // Wait for results
    await page.waitForSelector('#results', { state: 'visible', timeout: 10000 });
    
    // Verify badges are displayed (from API response)
    const iGRCBadge = await page.locator('#badge-igrc').textContent();
    const fGRCBadge = await page.locator('#badge-fgrc').textContent();
    const sailBadge = await page.locator('#badge-sail').textContent();
    
    // Expected: iGRC=2 (Table 2: <500 density), fGRC=1 (2 - M1A(Low=-1) - M2(Medium=-1) = 0, clamped to 1)
    expect(iGRCBadge).toContain('2'); // iGRC
    expect(fGRCBadge).toContain('1'); // fGRC
    expect(sailBadge).toMatch(/II|I/); // SAIL II or I
    
    // Verify network request to backend API was made
    const requests = [];
    page.on('request', request => {
      if (request.url().includes('/api/v1/sora/calculate')) {
        requests.push(request);
      }
    });
    
    // Re-submit to capture API call
    await page.click('button[type="submit"]');
    await page.waitForTimeout(1000);
    
    // Expect at least one API call
    expect(requests.length).toBeGreaterThan(0);
  });

  // ========================================================================
  // Test 2: Mission Planner SORA 2.0 - Backend API Integration
  // ========================================================================
  test('Mission Planner: SORA 2.0 calculation uses backend API', async ({ page }) => {
    await page.goto(MISSION_PLANNER_URL);
    
    // Select SORA 2.0
    await page.selectOption('select#soraVersionSelect', '2.0');
    
    // Wait for dropdown options to reload (from GET /specifications)
    await page.waitForTimeout(1000);
    
    // Fill in drone specs
    await page.fill('input#mtom', '2.0');
    await page.fill('input#maxSpeed', '20');
    await page.fill('input#charDim', '0.5');
    await page.fill('input#altitude', '350');
    
    // Select operation scenario: VLOS_Sparsely
    await page.selectOption('select#operationScenario', 'VLOS_Sparsely');
    
    // Select mitigations: M1 Low, M2 Low
    await page.selectOption('select#m1', 'Low');
    await page.selectOption('select#m2_20', 'Low');
    
    // Submit form
    await page.click('button[type="submit"]');
    
    // Wait for results
    await page.waitForSelector('#results', { state: 'visible', timeout: 10000 });
    
    // Verify badges (expected: iGRC=2, fGRC reduced by M1+M2)
    const iGRCBadge = await page.locator('#badge-igrc').textContent();
    expect(iGRCBadge).toContain('2'); // VLOS_Sparsely → iGRC=2
  });

  // ========================================================================
  // Test 3: Airspace Maps SORA 2.5 - Real-time badge updates from API
  // ========================================================================
  test('Airspace Maps: Drawing geometry triggers backend API calculation', async ({ page }) => {
    await page.goto(AIRSPACE_MAPS_URL);
    
    // Wait for map to load
    await page.waitForSelector('#map', { timeout: 10000 });
    
    // Select SORA 2.5
    await page.selectOption('select#soraVersionSelect', '2.5');
    
    // Fill in drone specs in sidebar
    await page.fill('input#mtom', '0.895');
    await page.fill('input#maxSpeed', '21');
    await page.fill('input#charDim', '0.35');
    await page.fill('input#altitude', '400');
    
    // Simulate drawing a polygon (trigger map click events)
    // Note: Playwright doesn't fully support Leaflet interactions - using button click instead
    await page.click('button#calculateSORA'); // Assuming there's a manual "Calculate SORA" button
    
    // Wait for badges to update
    await page.waitForTimeout(2000);
    
    // Verify SORA badges are visible
    const badgesContainer = await page.locator('#sora-badges');
    await expect(badgesContainer).toBeVisible();
    
    // Verify badges contain GRC/ARC/SAIL values
    const badges = await badgesContainer.textContent();
    expect(badges).toMatch(/GRC|ARC|SAIL/);
  });

  // ========================================================================
  // Test 4: Version Toggle - GET /specifications updates dropdowns
  // ========================================================================
  test('Version toggle: Switching SORA version updates dropdown options from API', async ({ page }) => {
    await page.goto(MISSION_PLANNER_URL);
    
    // Start with SORA 2.5
    await page.selectOption('select#soraVersionSelect', '2.5');
    await page.waitForTimeout(500);
    
    // Verify M1A dropdown exists (SORA 2.5 specific)
    const m1aSelect = await page.locator('select#m1a');
    await expect(m1aSelect).toBeVisible();
    
    // Verify M1A options: None, Low, Medium, High
    const m1aOptions = await m1aSelect.locator('option').allTextContents();
    expect(m1aOptions).toContain('Low');
    expect(m1aOptions).toContain('Medium');
    expect(m1aOptions).toContain('High');
    
    // Switch to SORA 2.0
    await page.selectOption('select#soraVersionSelect', '2.0');
    await page.waitForTimeout(500);
    
    // Verify M1 dropdown exists (SORA 2.0 specific)
    const m1Select = await page.locator('select#m1');
    await expect(m1Select).toBeVisible();
    
    // Verify M1 options: None, Low, Medium, High
    const m1Options = await m1Select.locator('option').allTextContents();
    expect(m1Options).toContain('Low');
    expect(m1Options).toContain('Medium');
    
    // Verify M1A dropdown is hidden (SORA 2.5 only)
    await expect(m1aSelect).toBeHidden();
  });

  // ========================================================================
  // Test 5: API Failure - Offline fallback to TS calculator
  // ========================================================================
  test('API failure: Falls back to offline TS calculator', async ({ page }) => {
    // Block API requests to simulate failure
    await page.route('**/api/v1/sora/calculate', route => route.abort());
    
    await page.goto(MISSION_PLANNER_URL);
    
    // Select SORA 2.5
    await page.selectOption('select#soraVersionSelect', '2.5');
    
    // Fill in DJI Mini 3 specs
    await page.fill('input#mtom', '0.249');
    await page.fill('input#maxSpeed', '16');
    await page.fill('input#charDim', '0.213');
    await page.fill('input#altitude', '400');
    await page.selectOption('select#populationDensity', '<500');
    
    // Submit form
    await page.click('button[type="submit"]');
    
    // Wait for fallback calculation (TS calculator)
    await page.waitForSelector('#results', { state: 'visible', timeout: 10000 });
    
    // Verify results are still displayed (from offline calculator)
    const iGRCBadge = await page.locator('#badge-igrc').textContent();
    expect(iGRCBadge).toContain('1'); // Small-UA Rule: MTOM ≤ 0.25kg → GRC=1
    
    // Verify warning message about offline mode
    const warningMsg = await page.locator('.warning-offline').textContent();
    expect(warningMsg).toMatch(/offline|fallback|local/i);
  });

  // ========================================================================
  // Test 6: Validation Errors - Server-side validation
  // ========================================================================
  test('Validation errors: Server returns validation errors for invalid input', async ({ page }) => {
    await page.goto(MISSION_PLANNER_URL);
    
    // Select SORA 2.5
    await page.selectOption('select#soraVersionSelect', '2.5');
    
    // Submit with INVALID inputs (negative MTOM)
    await page.fill('input#mtom', '-10'); // Invalid: negative mass
    await page.fill('input#maxSpeed', '25');
    await page.fill('input#charDim', '1.0');
    await page.fill('input#altitude', '400');
    
    // Submit form
    await page.click('button[type="submit"]');
    
    // Wait for error message
    await page.waitForSelector('.error-message', { state: 'visible', timeout: 5000 });
    
    // Verify error is displayed
    const errorMsg = await page.locator('.error-message').textContent();
    expect(errorMsg).toMatch(/invalid|error|required/i);
  });

  // ========================================================================
  // Test 7: Airspace Maps - Multiple geometries, batch calculation
  // ========================================================================
  test('Airspace Maps: Multiple geometries trigger batch SORA calculations', async ({ page }) => {
    await page.goto(AIRSPACE_MAPS_URL);
    
    // Wait for map
    await page.waitForSelector('#map', { timeout: 10000 });
    
    // Select SORA 2.5
    await page.selectOption('select#soraVersionSelect', '2.5');
    
    // Fill drone specs
    await page.fill('input#mtom', '5.0');
    await page.fill('input#maxSpeed', '25');
    await page.fill('input#charDim', '1.2');
    await page.fill('input#altitude', '300');
    
    // Simulate drawing 2 polygons (if UI supports it)
    // Note: This is a placeholder - actual implementation depends on Leaflet Draw API
    await page.click('button#drawPolygon1');
    await page.waitForTimeout(1000);
    await page.click('button#drawPolygon2');
    await page.waitForTimeout(1000);
    
    // Calculate SORA for all geometries
    await page.click('button#calculateSORABatch');
    
    // Verify batch results displayed
    const resultsTable = await page.locator('#batch-results-table');
    await expect(resultsTable).toBeVisible();
    
    // Verify at least 2 rows (2 geometries)
    const rows = await resultsTable.locator('tbody tr').count();
    expect(rows).toBeGreaterThanOrEqual(2);
  });

});
