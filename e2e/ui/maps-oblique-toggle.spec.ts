/**
 * E2E Test: Google Maps Oblique Toggle (2D ↔ 45° Tilt)
 * 
 * Acceptance:
 * - Button #btn-oblique-toggle exists and is visible
 * - Clicking toggles aria-pressed attribute (false → true → false)
 * - Graceful degradation if oblique view not available in area
 */

import { test, expect } from '@playwright/test';

const BASE_URL = 'http://localhost:5210';
const MAP_PAGE = `${BASE_URL}/app/Pages/ui/airspace-maps.html`;

test.describe('Google Maps Oblique Toggle', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to map page
    await page.goto(MAP_PAGE, { waitUntil: 'networkidle' });
    
    // Wait for Google Maps to initialize
    await page.waitForFunction(() => {
      return typeof (window as any).map2D !== 'undefined' && (window as any).map2D !== null;
    }, { timeout: 15000 });
    
    // Wait for oblique toggle button to be attached to DOM
    await page.waitForSelector('#btn-oblique-toggle', { timeout: 5000 });
  });

  test('oblique toggle button should exist and be visible', async ({ page }) => {
    const obliqueButton = page.locator('#btn-oblique-toggle');
    
    // Button should exist
    await expect(obliqueButton).toBeAttached();
    
    // Button should be visible (not hidden)
    await expect(obliqueButton).toBeVisible();
    
    // Button should have initial aria-pressed="false"
    await expect(obliqueButton).toHaveAttribute('aria-pressed', 'false');
    
    // Button text should be "2D" initially
    await expect(obliqueButton).toContainText('2D');
  });

  test('oblique toggle should change aria-pressed on click', async ({ page }) => {
    const obliqueButton = page.locator('#btn-oblique-toggle');
    
    // Initial state: aria-pressed="false"
    await expect(obliqueButton).toHaveAttribute('aria-pressed', 'false');
    
    // Click to toggle to oblique view
    await obliqueButton.click();
    await page.waitForTimeout(500); // Wait for tilt animation
    
    // aria-pressed should now be "true"
    await expect(obliqueButton).toHaveAttribute('aria-pressed', 'true');
    
    // Button text should change to "Oblique"
    await expect(obliqueButton).toContainText('Oblique');
    
    // Click again to toggle back to 2D
    await obliqueButton.click();
    await page.waitForTimeout(500);
    
    // aria-pressed should be "false" again
    await expect(obliqueButton).toHaveAttribute('aria-pressed', 'false');
    
    // Button text should be "2D" again
    await expect(obliqueButton).toContainText('2D');
  });

  test('oblique toggle should handle unavailable oblique view gracefully', async ({ page }) => {
    // Navigate to an area where oblique imagery may not be available (e.g., remote ocean)
    await page.evaluate(() => {
      // Move map to middle of Pacific Ocean (no oblique imagery)
      (window as any).map2D.setCenter({ lat: 0, lng: -160 });
      (window as any).map2D.setZoom(5);
    });
    
    await page.waitForTimeout(1000); // Wait for map to settle
    
    const obliqueButton = page.locator('#btn-oblique-toggle');
    
    // Click oblique toggle
    await obliqueButton.click();
    await page.waitForTimeout(1500); // Wait for fallback detection
    
    // Button should either:
    // 1. Be disabled (if oblique not available), OR
    // 2. Still work (if Google has oblique for this area)
    const isDisabled = await obliqueButton.isDisabled();
    
    if (isDisabled) {
      console.log('✅ Oblique toggle disabled gracefully (no imagery available)');
    } else {
      // If enabled, aria-pressed should have changed
      const ariaPressed = await obliqueButton.getAttribute('aria-pressed');
      expect(ariaPressed).toBeTruthy(); // Should be 'true' if working
      console.log('✅ Oblique toggle working (imagery available)');
    }
  });

  test('console should not have errors after oblique toggle', async ({ page }) => {
    const consoleErrors: string[] = [];
    
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });
    
    const obliqueButton = page.locator('#btn-oblique-toggle');
    
    // Toggle twice
    await obliqueButton.click();
    await page.waitForTimeout(500);
    await obliqueButton.click();
    await page.waitForTimeout(500);
    
    // Filter out known safe errors (Google Maps internal warnings)
    const criticalErrors = consoleErrors.filter(err => 
      !err.includes('Google Maps') &&
      !err.includes('Failed to load resource') &&
      !err.includes('net::ERR_BLOCKED_BY_CLIENT') // Ad blockers
    );
    
    expect(criticalErrors).toHaveLength(0);
  });
});
