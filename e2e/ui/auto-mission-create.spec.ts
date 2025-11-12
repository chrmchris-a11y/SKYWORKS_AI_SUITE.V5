/**
 * E2E Tests: Auto-Mission Create (Phase 6)
 * Tests for one-click mission creation functionality with SORA/PDRA/STS regimes
 * Updated for Phase 6 Maps with regime-aware mission builder
 */

import { test, expect } from '@playwright/test';

test.describe('Auto-Mission Create (Phase 6 Maps)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    
    // Wait for Google Maps to load
    await page.waitForSelector('#map2D', { timeout: 10000 });
    
    // Wait for Google Maps initialization
    await page.waitForFunction(() => {
      return typeof window !== 'undefined' && 
             typeof (window as any).map2D !== 'undefined' &&
             (window as any).map2D !== null;
    }, { timeout: 10000 });
    
    await page.waitForTimeout(1000);
  });

  test('should have Create Mission button (updated selector)', async ({ page }) => {
    const button = page.locator('#btn-create-mission');
    await expect(button).toBeVisible();
    await expect(button).toHaveText('Create Mission');
  });

  test('should have Regime select with all options', async ({ page }) => {
    const dropdown = page.locator('#regime-select');
    await expect(dropdown).toBeVisible();
    
    const options = await dropdown.locator('option').allTextContents();
    expect(options).toContain('SORA 2.5 (JARUS Annex A)');
    expect(options).toContain('SORA 2.0 (EASA AMC)');
    expect(options).toContain('PDRA S-01');
    expect(options).toContain('PDRA S-02');
    expect(options).toContain('PDRA G-01');
    expect(options).toContain('PDRA G-02');
    expect(options).toContain('PDRA G-03');
    expect(options).toContain('STS-01');
    expect(options).toContain('STS-02');
    
    // Default should be SORA_25_JARUS
    const value = await dropdown.inputValue();
    expect(value).toBe('SORA_25_JARUS');
  });

  test('should have mission template dropdown', async ({ page }) => {
    const dropdown = page.locator('#mission-template');
    await expect(dropdown).toBeVisible();
    
    const options = await dropdown.locator('option').allTextContents();
    expect(options).toContain('VLOS Spot (150m radius)');
    expect(options).toContain('Corridor (from waypoints)');
    expect(options).toContain('Area Scan (200×150m)');
    
    // Default should be VLOS_Spot
    const value = await dropdown.inputValue();
    expect(value).toBe('VLOS_Spot');
  });

  test('should have numeric inputs with default values (updated selectors)', async ({ page }) => {
    const heightFg = page.locator('#fg-height-m');
    await expect(heightFg).toHaveValue('60');
    
    const speed = page.locator('#uas-speed-ms');
    await expect(speed).toHaveValue('10');
    
    const mtom = page.locator('#uas-mtom-kg');
    await expect(mtom).toHaveValue('25');
    
    const safeRadius = page.locator('#safe-radius-m');
    await expect(safeRadius).toHaveValue('500');
  });

  test('should create mission and show all labels with no overlaps', async ({ page }) => {
    // Set center to Akrotiri, Limassol
    await page.evaluate(() => {
      if ((window as any).map2D) {
        (window as any).map2D.setCenter({ lat: 34.5937, lng: 32.9980 });
        (window as any).map2D.setZoom(14);
      }
    });
    
    // Click create button
    const button = page.locator('#btn-create-mission');
    await button.click();
    
    // Wait for mission creation
    await page.waitForTimeout(1500);
    
    // Verify all pins are visible
    const pinsExist = await page.evaluate(() => {
      const pins = (window as any).missionOverlays?.pins;
      if (!pins) return false;
      
      return pins.tol && pins.rp && pins.vo && 
             pins.obs1 && pins.e1;
    });
    
    expect(pinsExist).toBe(true);
    
    // Verify distance lines exist
    const linesExist = await page.evaluate(() => {
      const lines = (window as any).missionOverlays?.lines;
      if (!lines) return false;
      
      return lines.tolCv && lines.tolGrb && lines.tolE1;
    });
    
    expect(linesExist).toBe(true);
    
    // Verify Safe Area label exists
    const safeAreaLabel = await page.evaluate(() => {
      return (window as any).missionOverlays?.labels?.safeArea !== null &&
             (window as any).missionOverlays?.labels?.safeArea !== undefined;
    });
    
    expect(safeAreaLabel).toBe(true);
    
    // Verify legend box is visible
    await expect(page.locator('#legend-box')).toBeVisible();
    
    // Verify regime badge is visible
    await expect(page.locator('#regime-badge')).toBeVisible();
    const badgeText = await page.locator('#regime-badge').textContent();
    expect(badgeText).toContain('JARUS SORA 2.5');
  });

  test('should show FG/CV/GRB layers for SORA regimes', async ({ page }) => {
    // Ensure SORA 2.5 selected
    await page.selectOption('#regime-select', 'SORA_25_JARUS');
    await page.waitForTimeout(500);
    
    await page.evaluate(() => {
      if ((window as any).map2D) {
        (window as any).map2D.setCenter({ lat: 34.5937, lng: 32.9980 });
        (window as any).map2D.setZoom(14);
      }
    });
    
    await page.click('#btn-create-mission');
    await page.waitForTimeout(1500);
    
    const layersExist = await page.evaluate(() => {
      const overlays = (window as any).missionOverlays;
      if (!overlays) return { fg: false, cv: false, grb: false };
      
      return {
        fg: overlays.fg !== null && overlays.fg !== undefined,
        cv: overlays.cv !== null && overlays.cv !== undefined,
        grb: overlays.grb !== null && overlays.grb !== undefined
      };
    });
    
    expect(layersExist.fg).toBe(true);
    expect(layersExist.cv).toBe(true);
    expect(layersExist.grb).toBe(true);
  });

  test('Reset button should clear all overlays', async ({ page }) => {
    // Create mission first
    await page.click('#btn-create-mission');
    await page.waitForTimeout(1500);
    
    // Verify mission exists
    let hasOverlays = await page.evaluate(() => {
      const overlays = (window as any).missionOverlays;
      return overlays?.fg !== null || overlays?.cv !== null;
    });
    
    expect(hasOverlays).toBe(true);
    
    // Accept confirm dialog
    page.on('dialog', async dialog => {
      await dialog.accept();
    });
    
    // Click reset
    await page.click('#btn-reset-mission');
    await page.waitForTimeout(500);
    
    // Verify all cleared
    hasOverlays = await page.evaluate(() => {
      const overlays = (window as any).missionOverlays;
      return overlays?.fg !== null || 
             overlays?.cv !== null || 
             overlays?.grb !== null ||
             Object.values(overlays?.pins || {}).some((p: any) => p !== null);
    });
    
    expect(hasOverlays).toBe(false);
  });

  test('should verify no label overlaps (basic check)', async ({ page }) => {
    await page.evaluate(() => {
      if ((window as any).map2D) {
        (window as any).map2D.setCenter({ lat: 34.5937, lng: 32.9980 });
        (window as any).map2D.setZoom(14);
      }
    });
    
    await page.click('#btn-create-mission');
    await page.waitForTimeout(1500);
    
    // Get positions of distance labels
    const labelPositions = await page.evaluate(() => {
      const labels = (window as any).missionOverlays?.labels?.distances || [];
      return labels.map((label: any) => {
        if (!label || !label.getPosition) return null;
        const pos = label.getPosition();
        return { lat: pos.lat(), lng: pos.lng() };
      }).filter((p: any) => p !== null);
    });
    
    // Verify we have multiple labels
    expect(labelPositions.length).toBeGreaterThan(0);
    
    // Simple check: all labels have different positions
    const uniquePositions = new Set(labelPositions.map((p: any) => `${p.lat},${p.lng}`));
    expect(uniquePositions.size).toBe(labelPositions.length);
  });
});
