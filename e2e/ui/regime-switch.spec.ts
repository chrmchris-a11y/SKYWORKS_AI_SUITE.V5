import { test, expect } from '@playwright/test';

/**
 * E2E Test: Regime Switch (SORA 2.0 ↔ SORA 2.5)
 * Validates that switching between SORA 2.0 AMC and SORA 2.5 JARUS
 * produces different CV/GRB geometries and updates badge correctly.
 */

test.describe('Phase 6: Regime Switch (SORA 2.0 ↔ 2.5)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    await page.waitForLoadState('networkidle');
    
    // Wait for Google Maps to initialize
    await page.waitForFunction(() => {
      return typeof window !== 'undefined' && 
             typeof (window as any).map2D !== 'undefined' &&
             (window as any).map2D !== null;
    }, { timeout: 10000 });
  });

  test('should show different CV/GRB bounds for SORA 2.0 vs 2.5', async ({ page }) => {
    // Set SORA 2.0 and create mission
    await page.selectOption('#regime-select', 'SORA_20_AMC');
    await page.waitForTimeout(500);
    
    // Set center coordinates to Akrotiri, Limassol
    await page.evaluate(() => {
      if ((window as any).map2D) {
        (window as any).map2D.setCenter({ lat: 34.5937, lng: 32.9980 });
        (window as any).map2D.setZoom(14);
      }
    });
    
    await page.click('#btn-create-mission');
    await page.waitForTimeout(1000);
    
    // Capture SORA 2.0 CV/GRB bounds
    const sora20Bounds = await page.evaluate(() => {
      const cv = (window as any).missionOverlays?.cv;
      const grb = (window as any).missionOverlays?.grb;
      
      if (!cv || !grb) return null;
      
      const cvBounds = new google.maps.LatLngBounds();
      const grbBounds = new google.maps.LatLngBounds();
      
      cv.getPath().getArray().forEach((latLng: any) => cvBounds.extend(latLng));
      grb.getPath().getArray().forEach((latLng: any) => grbBounds.extend(latLng));
      
      return {
        cv: {
          north: cvBounds.getNorthEast().lat(),
          south: cvBounds.getSouthWest().lat(),
          east: cvBounds.getNorthEast().lng(),
          west: cvBounds.getSouthWest().lng()
        },
        grb: {
          north: grbBounds.getNorthEast().lat(),
          south: grbBounds.getSouthWest().lat(),
          east: grbBounds.getNorthEast().lng(),
          west: grbBounds.getSouthWest().lng()
        }
      };
    });
    
    expect(sora20Bounds).not.toBeNull();
    
    // Switch to SORA 2.5 and create mission
    await page.selectOption('#regime-select', 'SORA_25_JARUS');
    await page.waitForTimeout(500);
    
    // Set different CV/GRB parameters for SORA 2.5
    await page.selectOption('#cv-method', 'percent');
    await page.fill('#cv-value', '30'); // 30% instead of default 20%
    await page.selectOption('#grb-method', 'fixed');
    await page.fill('#grb-value', '200'); // 200m fixed buffer
    
    await page.click('#btn-create-mission');
    await page.waitForTimeout(1000);
    
    // Capture SORA 2.5 CV/GRB bounds
    const sora25Bounds = await page.evaluate(() => {
      const cv = (window as any).missionOverlays?.cv;
      const grb = (window as any).missionOverlays?.grb;
      
      if (!cv || !grb) return null;
      
      const cvBounds = new google.maps.LatLngBounds();
      const grbBounds = new google.maps.LatLngBounds();
      
      cv.getPath().getArray().forEach((latLng: any) => cvBounds.extend(latLng));
      grb.getPath().getArray().forEach((latLng: any) => grbBounds.extend(latLng));
      
      return {
        cv: {
          north: cvBounds.getNorthEast().lat(),
          south: cvBounds.getSouthWest().lat(),
          east: cvBounds.getNorthEast().lng(),
          west: cvBounds.getSouthWest().lng()
        },
        grb: {
          north: grbBounds.getNorthEast().lat(),
          south: grbBounds.getSouthWest().lat(),
          east: grbBounds.getNorthEast().lng(),
          west: grbBounds.getSouthWest().lng()
        }
      };
    });
    
    expect(sora25Bounds).not.toBeNull();
    
    // Verify bounds are different (with null checks)
    if (sora20Bounds && sora25Bounds) {
      expect(sora20Bounds.cv.north).not.toEqual(sora25Bounds.cv.north);
      expect(sora20Bounds.grb.north).not.toEqual(sora25Bounds.grb.north);
    }
  });

  test('should update regime badge correctly', async ({ page }) => {
    const badge = page.locator('#regime-badge');
    
    // Test SORA 2.0
    await page.selectOption('#regime-select', 'SORA_20_AMC');
    await page.waitForTimeout(300);
    await expect(badge).toContainText('EASA SORA 2.0 AMC');
    
    // Test SORA 2.5
    await page.selectOption('#regime-select', 'SORA_25_JARUS');
    await page.waitForTimeout(300);
    await expect(badge).toContainText('JARUS SORA 2.5 Annex A');
    
    // Test PDRA S-01
    await page.selectOption('#regime-select', 'PDRA_S01');
    await page.waitForTimeout(300);
    await expect(badge).toContainText('PDRA S-01');
    
    // Test STS-01
    await page.selectOption('#regime-select', 'STS_01');
    await page.waitForTimeout(300);
    await expect(badge).toContainText('STS-01');
  });

  test('should hide CV/GRB legend for PDRA/STS regimes', async ({ page }) => {
    const legendCv = page.locator('#legend-cv');
    const legendGrb = page.locator('#legend-grb');
    
    // SORA regimes: CV/GRB visible
    await page.selectOption('#regime-select', 'SORA_25_JARUS');
    await page.waitForTimeout(300);
    await expect(legendCv).toBeVisible();
    await expect(legendGrb).toBeVisible();
    
    // PDRA: CV/GRB hidden
    await page.selectOption('#regime-select', 'PDRA_S01');
    await page.waitForTimeout(300);
    await expect(legendCv).not.toBeVisible();
    await expect(legendGrb).not.toBeVisible();
    
    // STS: CV/GRB hidden
    await page.selectOption('#regime-select', 'STS_01');
    await page.waitForTimeout(300);
    await expect(legendCv).not.toBeVisible();
    await expect(legendGrb).not.toBeVisible();
  });
});
