import { test, expect } from '@playwright/test';

/**
 * E2E Test: PDRA/STS Forms
 * Validates that switching to PDRA or STS regimes shows only
 * the correct fields for that specific regulation.
 */

test.describe('Phase 6: PDRA/STS Forms', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:5210/app/Pages/ui/airspace-maps.html');
    await page.waitForLoadState('networkidle');
    
    // Wait for Google Maps
    await page.waitForFunction(() => {
      return typeof window !== 'undefined' && 
             typeof (window as any).map2D !== 'undefined' &&
             (window as any).map2D !== null;
    }, { timeout: 10000 });
  });

  test('PDRA S-01: should show only PDRA fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'PDRA_S01');
    await page.waitForTimeout(500);
    
    // PDRA fields should be visible
    await expect(page.locator('#pdra-fields')).toBeVisible();
    await expect(page.locator('#pdra-environment')).toBeVisible();
    await expect(page.locator('#pdra-vlos')).toBeVisible();
    await expect(page.locator('#pdra-max-alt-m')).toBeVisible();
    await expect(page.locator('#pdra-geo-awareness')).toBeVisible();
    await expect(page.locator('#pdra-remote-id')).toBeVisible();
    await expect(page.locator('#pdra-competency')).toBeVisible();
    await expect(page.locator('#pdra-containment')).toBeVisible();
    await expect(page.locator('#btn-pdra-pdf')).toBeVisible();
    
    // SORA fields should be hidden
    await expect(page.locator('#sora25-fields')).not.toBeVisible();
    await expect(page.locator('#sora20-fields')).not.toBeVisible();
    
    // STS fields should be hidden
    await expect(page.locator('#sts-fields')).not.toBeVisible();
  });

  test('PDRA S-02: should show PDRA fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'PDRA_S02');
    await page.waitForTimeout(500);
    
    await expect(page.locator('#pdra-fields')).toBeVisible();
    await expect(page.locator('#sora25-fields')).not.toBeVisible();
    await expect(page.locator('#sora20-fields')).not.toBeVisible();
    await expect(page.locator('#sts-fields')).not.toBeVisible();
  });

  test('PDRA G-01: should show PDRA fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'PDRA_G01');
    await page.waitForTimeout(500);
    
    await expect(page.locator('#pdra-fields')).toBeVisible();
    await expect(page.locator('#sora25-fields')).not.toBeVisible();
    await expect(page.locator('#sora20-fields')).not.toBeVisible();
    await expect(page.locator('#sts-fields')).not.toBeVisible();
  });

  test('PDRA G-02: should show PDRA fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'PDRA_G02');
    await page.waitForTimeout(500);
    
    await expect(page.locator('#pdra-fields')).toBeVisible();
  });

  test('PDRA G-03: should show PDRA fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'PDRA_G03');
    await page.waitForTimeout(500);
    
    await expect(page.locator('#pdra-fields')).toBeVisible();
  });

  test('STS-01: should show only STS fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'STS_01');
    await page.waitForTimeout(500);
    
    // STS fields should be visible
    await expect(page.locator('#sts-fields')).toBeVisible();
    await expect(page.locator('#sts-uascategory')).toBeVisible();
    await expect(page.locator('#sts-class')).toBeVisible();
    await expect(page.locator('#sts-alt-limit')).toBeVisible();
    await expect(page.locator('#sts-training')).toBeVisible();
    await expect(page.locator('#sts-equipments')).toBeVisible();
    await expect(page.locator('#btn-sts-pdf')).toBeVisible();
    
    // SORA fields should be hidden
    await expect(page.locator('#sora25-fields')).not.toBeVisible();
    await expect(page.locator('#sora20-fields')).not.toBeVisible();
    
    // PDRA fields should be hidden
    await expect(page.locator('#pdra-fields')).not.toBeVisible();
  });

  test('STS-02: should show STS fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'STS_02');
    await page.waitForTimeout(500);
    
    await expect(page.locator('#sts-fields')).toBeVisible();
    await expect(page.locator('#sora25-fields')).not.toBeVisible();
    await expect(page.locator('#sora20-fields')).not.toBeVisible();
    await expect(page.locator('#pdra-fields')).not.toBeVisible();
  });

  test('SORA 2.5: should show only SORA 2.5 fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'SORA_25_JARUS');
    await page.waitForTimeout(500);
    
    // SORA 2.5 fields should be visible
    await expect(page.locator('#sora25-fields')).toBeVisible();
    await expect(page.locator('#cv-method')).toBeVisible();
    await expect(page.locator('#cv-value')).toBeVisible();
    await expect(page.locator('#grb-method')).toBeVisible();
    await expect(page.locator('#grb-value')).toBeVisible();
    await expect(page.locator('#m1a')).toBeVisible();
    await expect(page.locator('#m1b')).toBeVisible();
    await expect(page.locator('#m1c')).toBeVisible();
    await expect(page.locator('#m2')).toBeVisible();
    
    // Other regime fields should be hidden
    await expect(page.locator('#sora20-fields')).not.toBeVisible();
    await expect(page.locator('#pdra-fields')).not.toBeVisible();
    await expect(page.locator('#sts-fields')).not.toBeVisible();
  });

  test('SORA 2.0: should show only SORA 2.0 fields', async ({ page }) => {
    await page.selectOption('#regime-select', 'SORA_20_AMC');
    await page.waitForTimeout(500);
    
    // SORA 2.0 fields should be visible
    await expect(page.locator('#sora20-fields')).toBeVisible();
    await expect(page.locator('#igrp-area-type')).toBeVisible();
    await expect(page.locator('#arc-method')).toBeVisible();
    await expect(page.locator('#m1a-20')).toBeVisible();
    await expect(page.locator('#m1b-20')).toBeVisible();
    await expect(page.locator('#m1c-20')).toBeVisible();
    await expect(page.locator('#m2-20')).toBeVisible();
    
    // Other regime fields should be hidden
    await expect(page.locator('#sora25-fields')).not.toBeVisible();
    await expect(page.locator('#pdra-fields')).not.toBeVisible();
    await expect(page.locator('#sts-fields')).not.toBeVisible();
  });

  test('PDRA PDF button should trigger alert', async ({ page }) => {
    await page.selectOption('#regime-select', 'PDRA_S01');
    await page.waitForTimeout(500);
    
    page.on('dialog', async dialog => {
      expect(dialog.message()).toContain('PDRA Compliance Table');
      await dialog.accept();
    });
    
    await page.click('#btn-pdra-pdf');
  });

  test('STS PDF button should trigger alert', async ({ page }) => {
    await page.selectOption('#regime-select', 'STS_01');
    await page.waitForTimeout(500);
    
    page.on('dialog', async dialog => {
      expect(dialog.message()).toContain('STS Compliance Table');
      await dialog.accept();
    });
    
    await page.click('#btn-sts-pdf');
  });
});
