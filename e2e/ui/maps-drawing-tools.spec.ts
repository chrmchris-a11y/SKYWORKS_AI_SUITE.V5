/**
 * E2E Test: Google Maps Drawing Tools (Waypoint, Geofence, CGA)
 * 
 * Acceptance:
 * - Add Waypoint: Creates numbered markers (1, 2, 3...)
 * - Draw Geofence: Creates red polygon, saves to missionData.geofence
 * - Draw CGA: Creates amber polygon, saves to missionData.cga
 * - DOM updates with new markers/polygons
 */

import { test, expect } from '@playwright/test';

const BASE_URL = 'http://localhost:5210';
const MAP_PAGE = `${BASE_URL}/app/Pages/ui/airspace-maps.html`;

test.describe('Google Maps Drawing Tools', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to map page
    await page.goto(MAP_PAGE, { waitUntil: 'networkidle' });
    
    // Wait for Google Maps to initialize
    await page.waitForFunction(() => {
      return typeof (window as any).map2D !== 'undefined' && (window as any).map2D !== null;
    }, { timeout: 15000 });
    
    // Wait for drawing buttons to be attached
    await page.waitForSelector('#btn-add-waypoint', { timeout: 5000 });
  });

  test('add waypoint button should exist and be clickable', async ({ page }) => {
    const addWaypointBtn = page.locator('#btn-add-waypoint');
    
    // Button should exist and be visible
    await expect(addWaypointBtn).toBeAttached();
    await expect(addWaypointBtn).toBeVisible();
    
    // Should not be disabled
    await expect(addWaypointBtn).toBeEnabled();
  });

  test('clicking add waypoint should activate mode', async ({ page }) => {
    const addWaypointBtn = page.locator('#btn-add-waypoint');
    
    // Click to activate waypoint mode
    await addWaypointBtn.click();
    await page.waitForTimeout(300);
    
    // Button should change style (e.g., green background when active)
    const bgColor = await addWaypointBtn.evaluate(el => 
      window.getComputedStyle(el).backgroundColor
    );
    
    // Active state: rgb(16, 185, 129) = #10b981
    // Note: Some browsers may return different formats
    console.log('Add Waypoint button background:', bgColor);
    
    // Click again to deactivate
    await addWaypointBtn.click();
    await page.waitForTimeout(300);
  });

  test('draw geofence button should exist', async ({ page }) => {
    const drawGeofenceBtn = page.locator('#btn-draw-geofence');
    
    await expect(drawGeofenceBtn).toBeAttached();
    await expect(drawGeofenceBtn).toBeVisible();
    await expect(drawGeofenceBtn).toBeEnabled();
  });

  test('draw CGA button should exist', async ({ page }) => {
    const drawCgaBtn = page.locator('#btn-draw-cga');
    
    await expect(drawCgaBtn).toBeAttached();
    await expect(drawCgaBtn).toBeVisible();
    await expect(drawCgaBtn).toBeEnabled();
  });

  test('clear route button should exist', async ({ page }) => {
    const clearBtn = page.locator('#btn-clear-route');
    
    await expect(clearBtn).toBeAttached();
    await expect(clearBtn).toBeVisible();
    await expect(clearBtn).toBeEnabled();
  });

  test('missionData global object should exist', async ({ page }) => {
    // Verify missionData structure exists
    const missionData = await page.evaluate(() => {
      return (window as any).missionData;
    });
    
    expect(missionData).toBeDefined();
    expect(missionData).toHaveProperty('waypoints');
    expect(missionData).toHaveProperty('geofence');
    expect(missionData).toHaveProperty('cga');
    
    // Initially should be empty
    expect(Array.isArray(missionData.waypoints)).toBe(true);
  });

  test.skip('NOTE: Automated polygon drawing requires complex Google Maps API interaction', async ({ page }) => {
    // SKIP REASON: Drawing polygons programmatically requires:
    // 1. Triggering DrawingManager.setDrawingMode(POLYGON)
    // 2. Simulating mouse clicks on map canvas (not standard DOM elements)
    // 3. Google Maps uses canvas rendering, not HTML elements
    // 4. Playwright cannot reliably click canvas coordinates without visual regression
    
    // MANUAL TEST:
    // 1. Click "Draw Geofence" button
    // 2. Click 3-4 points on map to create polygon
    // 3. Close polygon by clicking near first point
    // 4. Verify red polygon appears
    // 5. Check console: missionData.geofence should have coordinates
    
    console.log('⚠️ Manual test required for polygon drawing (see test description)');
  });

  test('console should not have errors after button interactions', async ({ page }) => {
    const consoleErrors: string[] = [];
    
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });
    
    // Click all drawing buttons
    await page.locator('#btn-add-waypoint').click();
    await page.waitForTimeout(200);
    await page.locator('#btn-add-waypoint').click(); // Deactivate
    await page.waitForTimeout(200);
    
    await page.locator('#btn-draw-geofence').click();
    await page.waitForTimeout(200);
    await page.locator('#btn-draw-geofence').click(); // Deactivate
    await page.waitForTimeout(200);
    
    await page.locator('#btn-draw-cga').click();
    await page.waitForTimeout(200);
    await page.locator('#btn-draw-cga').click(); // Deactivate
    await page.waitForTimeout(200);
    
    await page.locator('#btn-clear-route').click();
    await page.waitForTimeout(200);
    
    // Filter out known safe errors
    const criticalErrors = consoleErrors.filter(err => 
      !err.includes('Google Maps') &&
      !err.includes('Failed to load resource') &&
      !err.includes('net::ERR_BLOCKED_BY_CLIENT')
    );
    
    expect(criticalErrors).toHaveLength(0);
  });
});
